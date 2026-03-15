using Dotori.Core.Parsing;
using Dotori.LanguageServer.Protocol;

namespace Dotori.LanguageServer.Providers;

/// <summary>
/// Converts DSL parser errors and semantic issues into LSP Diagnostics.
/// </summary>
public static class DiagnosticsProvider
{
    /// <summary>
    /// Parse the given .dotori source text and return any diagnostics.
    /// </summary>
    public static List<LspDiagnostic> Analyze(string source, string filePath)
    {
        var diagnostics = new List<LspDiagnostic>();

        DotoriFile? file = null;
        try
        {
            file = DotoriParser.ParseSource(source, filePath);
        }
        catch (ParseException ex)
        {
            diagnostics.Add(ToDiagnostic(ex.Location, ex.Message, 1));
            return diagnostics;
        }
        catch (LexerException ex)
        {
            diagnostics.Add(ToDiagnostic(ex.Location, ex.Message, 1));
            return diagnostics;
        }

        // Semantic checks
        if (file.Project is not null)
            AddSemanticDiagnostics(file.Project, filePath, diagnostics);

        return diagnostics;
    }

    private static void AddSemanticDiagnostics(
        ProjectDecl project,
        string dotoriFilePath,
        List<LspDiagnostic> diagnostics)
    {
        var projectDir = Path.GetDirectoryName(dotoriFilePath) ?? ".";

        // Check path dependencies: does the target .dotori exist?
        foreach (var item in FlattenItems(project.Items))
        {
            if (item is not DependenciesBlock deps) continue;
            foreach (var dep in deps.Items)
            {
                if (dep.Value is ComplexDependency cd && cd.Path is { } pathVal)
                {
                    var resolved = Path.IsPathRooted(pathVal)
                        ? pathVal
                        : Path.GetFullPath(Path.Combine(projectDir, pathVal));
                    var targetDotori = Path.Combine(resolved, ".dotori");
                    if (!File.Exists(targetDotori))
                    {
                        // Use the location from the dependencies block
                        diagnostics.Add(new LspDiagnostic
                        {
                            Range = ZeroRange(deps.Location),
                            Severity = 2, // Warning
                            Source = "dotori",
                            Message = $"path dependency '{dep.Name}': no .dotori found at '{resolved}'",
                        });
                    }
                }
            }
        }

        // Check compile-flags/link-flags without condition block
        CheckUnguardedFlags(project, diagnostics);
    }

    private static void CheckUnguardedFlags(ProjectDecl project, List<LspDiagnostic> diagnostics)
    {
        // Top-level items (not inside a condition block) are "unguarded"
        foreach (var item in project.Items)
        {
            if (item is CompileFlagsBlock cfb && cfb.Values.Count > 0)
            {
                diagnostics.Add(new LspDiagnostic
                {
                    Range = ZeroRange(cfb.Location),
                    Severity = 2, // Warning
                    Source = "dotori",
                    Message = "compile-flags used without a compiler condition (e.g. [msvc], [clang]) — may reduce portability",
                });
            }
            if (item is LinkFlagsBlock lfb && lfb.Values.Count > 0)
            {
                diagnostics.Add(new LspDiagnostic
                {
                    Range = ZeroRange(lfb.Location),
                    Severity = 2, // Warning
                    Source = "dotori",
                    Message = "link-flags used without a compiler condition (e.g. [msvc], [clang]) — may reduce portability",
                });
            }
        }
    }

    /// <summary>Flatten all items recursively (including from condition blocks).</summary>
    private static IEnumerable<ProjectItem> FlattenItems(IEnumerable<ProjectItem> items)
    {
        foreach (var item in items)
        {
            yield return item;
            if (item is ConditionBlock cb)
                foreach (var child in FlattenItems(cb.Items))
                    yield return child;
        }
    }

    private static LspDiagnostic ToDiagnostic(SourceLocation loc, string message, int severity)
    {
        return new LspDiagnostic
        {
            Range = new LspRange
            {
                // LSP uses 0-indexed lines and columns; our parser uses 1-indexed
                Start = new LspPosition { Line = Math.Max(0, loc.Line - 1), Character = Math.Max(0, loc.Column - 1) },
                End   = new LspPosition { Line = Math.Max(0, loc.Line - 1), Character = Math.Max(0, loc.Column) },
            },
            Severity = severity,
            Source   = "dotori",
            Message  = message,
        };
    }

    private static LspRange ZeroRange(SourceLocation loc)
    {
        int line = Math.Max(0, loc.Line - 1);
        int col  = Math.Max(0, loc.Column - 1);
        return new LspRange
        {
            Start = new LspPosition { Line = line, Character = col },
            End   = new LspPosition { Line = line, Character = col + 1 },
        };
    }
}
