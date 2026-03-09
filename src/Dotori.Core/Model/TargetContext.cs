namespace Dotori.Core.Model;

/// <summary>
/// Fully-resolved build context: target platform + config + compiler + runtime.
/// Used to evaluate which condition blocks apply.
/// </summary>
public sealed class TargetContext
{
    public required string Platform { get; init; }   // windows, linux, macos, ios, tvos, watchos, android, wasm, uwp
    public required string Config   { get; init; }   // debug, release, or custom
    public required string Compiler { get; init; }   // msvc, clang
    public required string Runtime  { get; init; }   // static, dynamic
    public string? Libc             { get; init; }   // glibc, musl (Linux only)
    public string? Stdlib           { get; init; }   // libc++, libstdc++
    public string? WasmBackend      { get; init; }   // emscripten, bare (WASM only)

    /// <summary>Returns all active atom names for condition matching.</summary>
    public IReadOnlySet<string> ActiveAtoms()
    {
        var atoms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Platform,
            Config,
            Compiler,
            Runtime,
        };
        if (Libc    != null) atoms.Add(Libc);
        if (Stdlib  != null) atoms.Add(NormalizeStdlib(Stdlib));
        if (WasmBackend != null) atoms.Add(WasmBackend);
        return atoms;
    }

    private static string NormalizeStdlib(string s) => s switch
    {
        "libc++"    => "libcxx",
        "libstdc++" => "libstdcxx",
        _ => s,
    };

    public static TargetContext Default => new()
    {
        Platform = "linux",
        Config   = "debug",
        Compiler = "clang",
        Runtime  = "static",
    };
}
