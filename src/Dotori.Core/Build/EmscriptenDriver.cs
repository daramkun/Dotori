using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// Generates compile/link jobs for the Emscripten toolchain (emcc).
/// </summary>
public static class EmscriptenDriver
{
    public static IReadOnlyList<string> CompileFlags(
        FlatProjectModel model,
        string objDir)
    {
        var flags = new List<string>();

        // C++ standard
        flags.Add(model.Std switch
        {
            CxxStd.Cxx17 => "-std=c++17",
            CxxStd.Cxx20 => "-std=c++20",
            _             => "-std=c++23",
        });

        // Optimization
        flags.Add(model.Optimize switch
        {
            OptimizeLevel.None  => "-O0",
            OptimizeLevel.Size  => "-Os",
            OptimizeLevel.Speed => "-O2",
            OptimizeLevel.Full  => "-O3",
            _                   => "-O0",
        });

        if (model.Lto) flags.Add("-flto");

        flags.Add(model.DebugInfo switch
        {
            DebugInfoLevel.Full    => "-g",
            DebugInfoLevel.Minimal => "-gline-tables-only",
            _                      => string.Empty,
        });

        foreach (var d in model.Defines)
            flags.Add($"-D{d}");

        foreach (var h in model.Headers)
            flags.Add($"-I\"{h.Path}\"");

        flags.Add("-c");

        return flags.Where(f => !string.IsNullOrEmpty(f)).ToList();
    }

    public static IReadOnlyList<string> LinkFlags(
        FlatProjectModel model,
        string outputFile)
    {
        var flags = new List<string>();

        flags.Add($"-o \"{outputFile}\"");

        if (model.Lto) flags.Add("-flto");

        // Emscripten-specific flags
        foreach (var ef in model.EmscriptenFlags)
            flags.Add(ef);

        foreach (var lib in model.Links)
            flags.Add($"-l{lib}");

        return flags;
    }

    public static CompileJob MakeCompileJob(
        string sourceFile,
        string objDir,
        IReadOnlyList<string> commonFlags)
    {
        var objFile = Path.Combine(objDir,
            Path.GetFileNameWithoutExtension(sourceFile) + ".o");

        var args = new List<string>(commonFlags) { $"\"{sourceFile}\"", $"-o \"{objFile}\"" };

        return new CompileJob
        {
            SourceFile = sourceFile,
            OutputFile = objFile,
            Args       = args.ToArray(),
        };
    }

    public static LinkJob MakeLinkJob(
        IEnumerable<string> objFiles,
        string outputFile,
        IReadOnlyList<string> linkFlags)
    {
        var args = new List<string>(linkFlags);
        foreach (var obj in objFiles) args.Add($"\"{obj}\"");

        return new LinkJob
        {
            InputFiles = objFiles.ToArray(),
            OutputFile = outputFile,
            Args       = args.ToArray(),
        };
    }
}
