using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

public sealed partial class BuildPlanner
{
    // ─── External assembler jobs ──────────────────────────────────────────────

    /// <summary>
    /// Plan compile jobs for assembler source files (NASM/YASM/GAS/MASM).
    /// Returns an empty list when no <c>assembler { }</c> block is declared.
    /// The returned jobs must be executed with <see cref="GetAssemblerPath"/> as the tool.
    /// </summary>
    public IReadOnlyList<CompileJob> PlanAssemblerJobs()
    {
        if (_model.Assembler is null) return [];

        var config = _model.Assembler;
        var tool   = ResolveAssemblerTool(config.Tool);
        var asmPath = GetAssemblerPathForTool(tool);

        if (asmPath is null)
        {
            Console.Error.WriteLine(
                $"Warning: assembler '{AssemblerToolName(tool)}' not found in PATH, skipping assembly sources.");
            return [];
        }

        var format = config.Format ?? AutoDetectFormat(tool, _targetId);

        var sources = GlobExpander.Expand(
            _model.ProjectDir,
            config.Items.Where(s =>  s.IsInclude).Select(s => s.Glob),
            config.Items.Where(s => !s.IsInclude).Select(s => s.Glob));

        var jobs = new List<CompileJob>();
        foreach (var src in sources)
        {
            var abs = PathUtils.MakeAbsolute(_model.ProjectDir, src);
            if (!File.Exists(abs))
            {
                Console.Error.WriteLine(
                    $"Warning: assembler source not found, skipping: {abs}");
                continue;
            }

            var job = tool switch
            {
                AssemblerTool.Nasm => NasmDriver.MakeJob(abs, _cacheDir, format, config),
                AssemblerTool.Yasm => NasmDriver.MakeJob(abs, _cacheDir, format, config),
                AssemblerTool.Gas  => GasDriver.MakeJob(abs, _cacheDir, config),
                AssemblerTool.Masm => MasmDriver.MakeJob(abs, _cacheDir, config),
                _                  => null,
            };

            if (job is not null)
                jobs.Add(job);
        }
        return jobs;
    }

    /// <summary>
    /// Returns the assembler executable path for the resolved tool,
    /// or null if the tool is not installed.
    /// </summary>
    public string? GetAssemblerPath()
    {
        if (_model.Assembler is null) return null;
        var tool = ResolveAssemblerTool(_model.Assembler.Tool);
        return GetAssemblerPathForTool(tool);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private AssemblerTool ResolveAssemblerTool(AssemblerTool declared) =>
        declared != AssemblerTool.Auto
            ? declared
            : _toolchain.Kind == CompilerKind.Msvc
                ? AssemblerTool.Masm
                : AssemblerTool.Gas;

    private string? GetAssemblerPathForTool(AssemblerTool tool)
    {
        var paths = _toolchain.Assembler;
        return tool switch
        {
            AssemblerTool.Nasm => paths?.NasmPath,
            AssemblerTool.Yasm => paths?.YasmPath,
            AssemblerTool.Gas  => paths?.GasPath,
            AssemblerTool.Masm => paths?.MasmPath,
            _                  => null,
        };
    }

    private static string AssemblerToolName(AssemblerTool tool) => tool switch
    {
        AssemblerTool.Nasm => "nasm",
        AssemblerTool.Yasm => "yasm",
        AssemblerTool.Gas  => "as",
        AssemblerTool.Masm => "masm (ml64.exe)",
        _                  => "auto",
    };

    /// <summary>
    /// Auto-detects the NASM/YASM output format from the target identifier.
    /// GAS and MASM determine their format automatically; returns null for those.
    /// </summary>
    private static string? AutoDetectFormat(AssemblerTool tool, string targetId)
    {
        if (tool is AssemblerTool.Gas or AssemblerTool.Masm) return null;

        return targetId.ToLowerInvariant() switch
        {
            var t when t.StartsWith("windows-") || t.StartsWith("uwp-") => "win64",
            var t when t.StartsWith("linux-")                           => "elf64",
            var t when t.StartsWith("macos-")                           => "macho64",
            var t when t.StartsWith("android-")                        => "elf64",
            _                                                           => null,
        };
    }
}
