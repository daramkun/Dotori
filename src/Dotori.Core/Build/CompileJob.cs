namespace Dotori.Core.Build;

/// <summary>A single file compile job.</summary>
public sealed class CompileJob
{
    public required string SourceFile  { get; init; }
    public required string OutputFile  { get; init; }
    public required string[] Args      { get; init; }
}

/// <summary>A link job (produces an executable or library).</summary>
public sealed class LinkJob
{
    public required string[] InputFiles  { get; init; }  // .obj / .o files
    public required string   OutputFile  { get; init; }
    public required string[] Args        { get; init; }
}
