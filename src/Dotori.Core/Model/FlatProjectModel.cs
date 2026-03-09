using Dotori.Core.Parsing;

namespace Dotori.Core.Model;

/// <summary>
/// Flattened project configuration after merging all applicable condition blocks
/// for a given <see cref="TargetContext"/>.
/// Condition specificity rule: more-specific conditions override less-specific ones.
/// e.g. [windows.release] > [windows] > (top-level)
/// </summary>
public sealed class FlatProjectModel
{
    public required string Name        { get; init; }
    public required string ProjectDir  { get; init; }  // directory containing .dotori
    public required string DotoriPath  { get; init; }  // absolute path to .dotori file

    // Core props
    public ProjectType   Type         { get; set; } = ProjectType.Executable;
    public CxxStd        Std          { get; set; } = CxxStd.Cxx23;
    public string?       Description  { get; set; }
    public OptimizeLevel Optimize     { get; set; } = OptimizeLevel.None;
    public DebugInfoLevel DebugInfo   { get; set; } = DebugInfoLevel.None;
    public RuntimeLink   RuntimeLink  { get; set; } = RuntimeLink.Static;
    public LibcType?     Libc         { get; set; }
    public StdlibType?   Stdlib       { get; set; }
    public bool          Lto          { get; set; } = false;
    public WarningLevel  Warnings     { get; set; } = WarningLevel.Default;
    public bool          WarningsAsErrors { get; set; } = false;
    public int?          AndroidApiLevel  { get; set; }
    public string?       MacosMin     { get; set; }
    public string?       IosMin       { get; set; }
    public string?       TvosMin      { get; set; }
    public string?       WatchosMin   { get; set; }
    public List<string>  EmscriptenFlags { get; } = new();

    // Accumulated lists (merged across all applicable condition blocks)
    public List<SourceItem>     Sources      { get; } = new();
    public List<SourceItem>     Modules      { get; } = new();
    public List<HeaderItem>     Headers      { get; } = new();
    public List<string>         Defines      { get; } = new();
    public List<string>         Links        { get; } = new();
    public List<string>         Frameworks   { get; } = new();
    public List<DependencyItem> Dependencies { get; } = new();

    // Optional blocks (last one wins)
    public PchConfig?         Pch         { get; set; }
    public UnityBuildConfig?  UnityBuild  { get; set; }
}

public sealed class PchConfig
{
    public string? Header  { get; set; }
    public string? Source  { get; set; }
    public bool?   Modules { get; set; }
}

public sealed class UnityBuildConfig
{
    public bool        Enabled   { get; set; } = false;
    public int         BatchSize { get; set; } = 8;
    public List<string> Exclude  { get; } = new();
}
