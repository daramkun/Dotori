namespace Dotori.Core.Parsing;

// ─── Source location ───────────────────────────────────────────────────────

public readonly record struct SourceLocation(string File, int Line, int Column)
{
    public override string ToString() => $"{File}:{Line}:{Column}";
}

// ─── Top-level ─────────────────────────────────────────────────────────────

public sealed class DotoriFile
{
    public required string FilePath { get; init; }
    public ProjectDecl? Project { get; init; }
    public PackageDecl? Package { get; init; }
}

// ─── Project declaration ───────────────────────────────────────────────────

public sealed class ProjectDecl
{
    public required SourceLocation Location { get; init; }
    public required string Name { get; init; }
    public List<ProjectItem> Items { get; } = new();
}

/// <summary>Base class for all items that can appear inside a project block.</summary>
public abstract class ProjectItem
{
    public required SourceLocation Location { get; init; }
}

// Props
public sealed class ProjectTypeProp(ProjectType value) : ProjectItem
{
    public ProjectType Value { get; } = value;
}
public sealed class StdProp(CxxStd value) : ProjectItem
{
    public CxxStd Value { get; } = value;
}
public sealed class DescriptionProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}
public sealed class OptimizeProp(OptimizeLevel value) : ProjectItem
{
    public OptimizeLevel Value { get; } = value;
}
public sealed class DebugInfoProp(DebugInfoLevel value) : ProjectItem
{
    public DebugInfoLevel Value { get; } = value;
}
public sealed class RuntimeLinkProp(RuntimeLink value) : ProjectItem
{
    public RuntimeLink Value { get; } = value;
}
public sealed class LibcProp(LibcType value) : ProjectItem
{
    public LibcType Value { get; } = value;
}
public sealed class StdlibProp(StdlibType value) : ProjectItem
{
    public StdlibType Value { get; } = value;
}
public sealed class LtoProp(bool value) : ProjectItem
{
    public bool Value { get; } = value;
}
public sealed class WarningsProp(WarningLevel value) : ProjectItem
{
    public WarningLevel Value { get; } = value;
}
public sealed class WarningsAsErrorsProp(bool value) : ProjectItem
{
    public bool Value { get; } = value;
}
public sealed class AndroidApiLevelProp(int value) : ProjectItem
{
    public int Value { get; } = value;
}
public sealed class MacosMinProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}
public sealed class IosMinProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}
public sealed class TvosMinProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}
public sealed class WatchosMinProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}
public sealed class EmscriptenFlagsProp(IReadOnlyList<string> flags) : ProjectItem
{
    public IReadOnlyList<string> Flags { get; } = flags;
}

// Blocks
public sealed class SourcesBlock(bool isModules) : ProjectItem
{
    public bool IsModules { get; } = isModules;
    public List<SourceItem> Items { get; } = new();
    /// <summary>Only meaningful when IsModules=true. null = inherit default (true).</summary>
    public bool? ExportMap { get; set; }
}
public sealed class SourceItem(bool isInclude, string glob)
{
    public bool IsInclude { get; } = isInclude;
    public string Glob { get; } = glob;
}

public sealed class HeadersBlock : ProjectItem
{
    public List<HeaderItem> Items { get; } = new();
}
public sealed class HeaderItem(bool isPublic, string path)
{
    public bool IsPublic { get; } = isPublic;
    public string Path { get; } = path;
}

public sealed class DefinesBlock : ProjectItem
{
    public List<string> Values { get; } = new();
}
public sealed class LinksBlock : ProjectItem
{
    public List<string> Values { get; } = new();
}
public sealed class FrameworksBlock : ProjectItem
{
    public List<string> Values { get; } = new();
}

/// <summary>
/// Paths to .framework or .xcframework bundles (Apple only).
/// At build time each entry is resolved to a framework search dir (-F)
/// and framework name (-framework), then merged into the model.
/// </summary>
public sealed class FrameworkPathsBlock : ProjectItem
{
    public List<string> Paths { get; } = new();
}

public sealed class CompileFlagsBlock : ProjectItem
{
    public List<string> Values { get; } = new();
}
public sealed class LinkFlagsBlock : ProjectItem
{
    public List<string> Values { get; } = new();
}

/// <summary>Windows resource files (.rc) to compile with rc.exe.</summary>
public sealed class ResourcesBlock : ProjectItem
{
    public List<string> Paths { get; } = new();
}

/// <summary>Windows application manifest file (.manifest) to embed after linking.</summary>
public sealed class ManifestProp(string value) : ProjectItem
{
    public string Value { get; } = value;
}

public sealed class DependenciesBlock : ProjectItem
{
    public List<DependencyItem> Items { get; } = new();
}
public sealed class DependencyItem(string name, DependencyValue value)
{
    public string Name { get; } = name;
    public DependencyValue Value { get; } = value;
}
public abstract class DependencyValue { }
public sealed class VersionDependency(string version) : DependencyValue
{
    public string Version { get; } = version;
}
public sealed class ComplexDependency : DependencyValue
{
    public string? Git { get; set; }
    public string? Tag { get; set; }
    public string? Commit { get; set; }
    public string? Path { get; set; }
    public string? Version { get; set; }
}

public sealed class PchBlock : ProjectItem
{
    public string? Header { get; set; }
    public string? Source { get; set; }
    public bool? Modules { get; set; }
}

public sealed class UnityBuildBlock : ProjectItem
{
    public bool? Enabled { get; set; }
    public int? BatchSize { get; set; }
    public List<string> Exclude { get; } = new();
}

public sealed class OutputBlock : ProjectItem
{
    public string? Binaries  { get; set; }  // exe, dll/so/dylib copy dir
    public string? Libraries { get; set; }  // .lib/.a copy dir
    public string? Symbols   { get; set; }  // .pdb/.dSYM copy dir
}

public sealed class PreBuildBlock : ProjectItem
{
    public List<string> Commands { get; } = new();
}

public sealed class PostBuildBlock : ProjectItem
{
    public List<string> Commands { get; } = new();
}

public sealed class ConditionBlock(ConditionExpr condition) : ProjectItem
{
    public ConditionExpr Condition { get; } = condition;
    public List<ProjectItem> Items { get; } = new();
}

// ─── Conditions ────────────────────────────────────────────────────────────

/// <summary>Condition is a dot-joined sequence of atoms, e.g. [windows.release].</summary>
public sealed class ConditionExpr(IReadOnlyList<string> atoms)
{
    public IReadOnlyList<string> Atoms { get; } = atoms;

    public override string ToString() => string.Join(".", Atoms);

    /// <summary>Specificity = number of atoms (more atoms = higher priority).</summary>
    public int Specificity => Atoms.Count;
}

// ─── Package declaration ───────────────────────────────────────────────────

public sealed class PackageDecl
{
    public required SourceLocation Location { get; init; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? License { get; set; }
    public string? Homepage { get; set; }
    public List<string> Authors { get; } = new();
    public Dictionary<string, string> Exports { get; } = new();
}

// ─── Enums ─────────────────────────────────────────────────────────────────

public enum ProjectType { Executable, StaticLibrary, SharedLibrary, HeaderOnly }
public enum CxxStd { Cxx17, Cxx20, Cxx23 }
public enum OptimizeLevel { None, Size, Speed, Full }
public enum DebugInfoLevel { None, Minimal, Full }
public enum RuntimeLink { Static, Dynamic }
public enum LibcType { Glibc, Musl }
public enum StdlibType { LibCxx, LibStdCxx }
public enum WarningLevel { None, Default, All, Extra }
