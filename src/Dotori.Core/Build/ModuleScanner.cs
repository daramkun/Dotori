using System.Diagnostics;
using System.Text.Json;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// Scans C++ module source files for import/export declarations using P1689 format.
///
/// P1689 (Modules Dependency Format) output looks like:
/// {
///   "version": 1,
///   "rules": [
///     {
///       "primary-output": "MyLib.pcm",
///       "provides": [{ "logical-name": "MyLib", "is-interface": true }],
///       "requires": [{ "logical-name": "std.core" }]
///     }
///   ]
/// }
///
/// MSVC:  cl.exe /scanDependencies <out.json> <source.cppm>
/// Clang: clang-scan-deps -format=p1689 -- clang++ ... <source.cppm>
/// </summary>
public static class ModuleScanner
{
    /// <summary>
    /// Represents a single module's dependency info from P1689 output.
    /// </summary>
    public sealed class ModuleDep
    {
        /// <summary>The source file this info was scanned from.</summary>
        public required string SourceFile  { get; init; }
        /// <summary>The logical module name this file provides (null if it's not an interface unit).</summary>
        public string?         Provides    { get; init; }
        /// <summary>Logical module names this file imports.</summary>
        public IReadOnlyList<string> Requires { get; init; } = Array.Empty<string>();
    }

    // ─── MSVC scan ────────────────────────────────────────────────────────────

    /// <summary>
    /// Scan a module interface file using MSVC's /scanDependencies flag.
    /// </summary>
    public static async Task<ModuleDep?> ScanMsvcAsync(
        string compilerPath,
        string sourceFile,
        IReadOnlyList<string> compileFlags,
        CancellationToken ct = default)
    {
        var tempJson = Path.GetTempFileName() + ".json";
        try
        {
            // Build args: /scanDependencies <out.json> <flags> <source>
            var flagsStr = string.Join(" ", compileFlags
                .Where(f => !f.StartsWith("/Fo") && f != "/c")
                .Select(f => f));
            var args = $"/scanDependencies \"{tempJson}\" {flagsStr} \"{sourceFile}\"";

            var output = await RunProcessAsync(compilerPath, args, ct);
            if (output is null) return null;

            if (!File.Exists(tempJson)) return null;
            var json = await File.ReadAllTextAsync(tempJson, ct);
            return ParseP1689(sourceFile, json);
        }
        finally
        {
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }

    // ─── Clang scan ──────────────────────────────────────────────────────────

    /// <summary>
    /// Scan a module interface file using clang-scan-deps (P1689 format).
    /// </summary>
    public static async Task<ModuleDep?> ScanClangAsync(
        string compilerPath,
        string sourceFile,
        IReadOnlyList<string> compileFlags,
        CancellationToken ct = default)
    {
        // Try to find clang-scan-deps alongside the compiler
        var scanDepsPath = FindClangScanDeps(compilerPath);
        if (scanDepsPath is null)
        {
            // Fallback: use simple text scan
            return ScanByText(sourceFile);
        }

        var flagsStr = string.Join(" ", compileFlags
            .Where(f => f != "-c")
            .Select(f => f));
        var args = $"-format=p1689 -- \"{compilerPath}\" {flagsStr} \"{sourceFile}\"";

        var output = await RunProcessAsync(scanDepsPath, args, ct);
        if (output is null) return ScanByText(sourceFile);

        return ParseP1689(sourceFile, output);
    }

    // ─── Text-based fallback scan ─────────────────────────────────────────────

    /// <summary>
    /// Simple regex-free text scan for export module / import declarations.
    /// Used when clang-scan-deps is not available.
    /// This is a best-effort scan; it may miss some cases.
    /// </summary>
    public static ModuleDep ScanByText(string sourceFile)
    {
        string? provides = null;
        var requires = new List<string>();

        try
        {
            foreach (var line in File.ReadLines(sourceFile))
            {
                var trimmed = line.TrimStart();

                // export module <name>;
                if (trimmed.StartsWith("export") && trimmed.Contains("module"))
                {
                    var name = ExtractModuleName(trimmed, "module");
                    if (name is not null) provides = name;
                }
                // module <name>;  (module implementation unit)
                else if (trimmed.StartsWith("module") &&
                         !trimmed.StartsWith("module;") &&
                         !trimmed.StartsWith("module :"))
                {
                    // Don't extract as provides — implementation units share the name
                    var name = ExtractModuleName(trimmed, "module");
                    if (name is not null && provides is null)
                        provides = name; // Only set if we haven't found an export module
                }
                // import <name>;
                else if (trimmed.StartsWith("import"))
                {
                    var name = ExtractModuleName(trimmed, "import");
                    // Skip <header> imports and "import :" (partition imports)
                    if (name is not null && !name.StartsWith('<') && !name.StartsWith(':'))
                        requires.Add(name);
                }
            }
        }
        catch (IOException)
        {
            // File may not exist or be unreadable — return empty dep
        }

        return new ModuleDep
        {
            SourceFile = sourceFile,
            Provides   = provides,
            Requires   = requires,
        };
    }

    // ─── Batch scan ───────────────────────────────────────────────────────────

    /// <summary>
    /// Scan all module files in parallel.
    /// Returns a list of <see cref="ModuleDep"/> (one per file).
    /// </summary>
    public static async Task<IReadOnlyList<ModuleDep>> ScanAllAsync(
        string compilerPath,
        CompilerKind kind,
        IReadOnlyList<string> moduleFiles,
        IReadOnlyList<string> compileFlags,
        CancellationToken ct = default)
    {
        var tasks = moduleFiles.Select(f => ScanFileAsync(compilerPath, kind, f, compileFlags, ct));
        var results = await Task.WhenAll(tasks);
        return results.OfType<ModuleDep>().ToList();
    }

    private static Task<ModuleDep?> ScanFileAsync(
        string compilerPath,
        CompilerKind kind,
        string sourceFile,
        IReadOnlyList<string> compileFlags,
        CancellationToken ct)
    {
        return kind == CompilerKind.Msvc
            ? ScanMsvcAsync(compilerPath, sourceFile, compileFlags, ct)
            : ScanClangAsync(compilerPath, sourceFile, compileFlags, ct);
    }

    // ─── P1689 JSON parsing ───────────────────────────────────────────────────

    internal static ModuleDep? ParseP1689(string sourceFile, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("rules", out var rules)) return null;

            foreach (var rule in rules.EnumerateArray())
            {
                string? provides = null;
                var requires = new List<string>();

                if (rule.TryGetProperty("provides", out var providesArr))
                {
                    foreach (var p in providesArr.EnumerateArray())
                    {
                        if (p.TryGetProperty("logical-name", out var name))
                        {
                            provides = name.GetString();
                            break;
                        }
                    }
                }

                if (rule.TryGetProperty("requires", out var requiresArr))
                {
                    foreach (var r in requiresArr.EnumerateArray())
                    {
                        if (r.TryGetProperty("logical-name", out var name))
                        {
                            var n = name.GetString();
                            if (n is not null) requires.Add(n);
                        }
                    }
                }

                return new ModuleDep
                {
                    SourceFile = sourceFile,
                    Provides   = provides,
                    Requires   = requires,
                };
            }
        }
        catch (JsonException)
        {
            // Malformed JSON — fall back to text scan
            return ScanByText(sourceFile);
        }

        return null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string? FindClangScanDeps(string compilerPath)
    {
        var dir  = Path.GetDirectoryName(compilerPath);
        if (dir is null) return null;

        // Try standard name alongside the compiler
        var candidate = Path.Combine(dir, "clang-scan-deps");
        if (File.Exists(candidate)) return candidate;

        candidate = Path.Combine(dir, "clang-scan-deps.exe");
        if (File.Exists(candidate)) return candidate;

        return null;
    }

    private static async Task<string?> RunProcessAsync(string exe, string args, CancellationToken ct)
    {
        try
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };

            using var proc = Process.Start(psi);
            if (proc is null) return null;

            var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            return proc.ExitCode == 0 ? stdout : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? ExtractModuleName(string line, string keyword)
    {
        var idx = line.IndexOf(keyword, StringComparison.Ordinal);
        if (idx < 0) return null;

        var rest = line[(idx + keyword.Length)..].TrimStart();

        // Strip trailing semicolon and whitespace
        var semi = rest.IndexOf(';');
        if (semi >= 0) rest = rest[..semi];

        // Strip any attributes like [[...]]
        if (rest.StartsWith("[["))
        {
            var end = rest.IndexOf("]]");
            if (end >= 0) rest = rest[(end + 2)..].TrimStart();
        }

        rest = rest.Trim();
        return string.IsNullOrEmpty(rest) ? null : rest;
    }
}
