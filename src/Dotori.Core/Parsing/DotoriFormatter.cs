using System.Text;

namespace Dotori.Core.Parsing;

/// <summary>
/// Formats a parsed <see cref="DotoriFile"/> AST back into canonical .dotori source text.
/// Comments in the original file are not preserved (they are stripped during lexing).
/// </summary>
public static class DotoriFormatter
{
    /// <summary>
    /// Formats the given <see cref="DotoriFile"/> and returns the canonical source text.
    /// </summary>
    public static string Format(DotoriFile file)
    {
        var sb = new StringBuilder();
        bool hasProject = file.Project is not null;

        if (file.Project is not null)
            FormatProject(sb, file.Project);

        if (file.Package is not null)
        {
            if (hasProject)
                sb.AppendLine();
            FormatPackage(sb, file.Package);
        }

        return sb.ToString();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static string I(int level) => new string(' ', level * 4);

    private static string QuoteString(string s)
    {
        var escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    private static bool IsProp(ProjectItem item) => item is
        ProjectTypeProp or StdProp or DescriptionProp or OptimizeProp or
        DebugInfoProp or RuntimeLinkProp or LibcProp or StdlibProp or
        LtoProp or WarningsProp or WarningsAsErrorsProp or
        AndroidApiLevelProp or MacosMinProp or IosMinProp or
        TvosMinProp or WatchosMinProp or ManifestProp or
        EmscriptenFlagsProp;

    // ─── project block ──────────────────────────────────────────────────────

    private static void FormatProject(StringBuilder sb, ProjectDecl project)
    {
        sb.AppendLine($"project {project.Name} {{");
        AppendProjectItems(sb, project.Items, 1);
        sb.AppendLine("}");
    }

    private static void AppendProjectItems(StringBuilder sb, List<ProjectItem> items, int indent)
    {
        bool prevWasProp = false;
        bool firstItem   = true;

        foreach (var item in items)
        {
            bool isProp = IsProp(item);

            if (!firstItem)
            {
                // No blank line between consecutive props; blank line otherwise
                if (!(isProp && prevWasProp))
                    sb.AppendLine();
            }

            FormatProjectItem(sb, item, indent);
            prevWasProp = isProp;
            firstItem   = false;
        }
    }

    private static void FormatProjectItem(StringBuilder sb, ProjectItem item, int indent)
    {
        switch (item)
        {
            case ProjectTypeProp p:
                sb.AppendLine($"{I(indent)}type = {FormatProjectType(p.Value)}");
                break;
            case StdProp p:
                sb.AppendLine($"{I(indent)}std = {FormatCxxStd(p.Value)}");
                break;
            case DescriptionProp p:
                sb.AppendLine($"{I(indent)}description = {QuoteString(p.Value)}");
                break;
            case OptimizeProp p:
                sb.AppendLine($"{I(indent)}optimize = {FormatOptimize(p.Value)}");
                break;
            case DebugInfoProp p:
                sb.AppendLine($"{I(indent)}debug-info = {FormatDebugInfo(p.Value)}");
                break;
            case RuntimeLinkProp p:
                sb.AppendLine($"{I(indent)}runtime-link = {FormatRuntimeLink(p.Value)}");
                break;
            case LibcProp p:
                sb.AppendLine($"{I(indent)}libc = {FormatLibc(p.Value)}");
                break;
            case StdlibProp p:
                sb.AppendLine($"{I(indent)}stdlib = {FormatStdlib(p.Value)}");
                break;
            case LtoProp p:
                sb.AppendLine($"{I(indent)}lto = {(p.Value ? "true" : "false")}");
                break;
            case WarningsProp p:
                sb.AppendLine($"{I(indent)}warnings = {FormatWarnings(p.Value)}");
                break;
            case WarningsAsErrorsProp p:
                sb.AppendLine($"{I(indent)}warnings-as-errors = {(p.Value ? "true" : "false")}");
                break;
            case AndroidApiLevelProp p:
                sb.AppendLine($"{I(indent)}android-api-level = {p.Value}");
                break;
            case MacosMinProp p:
                sb.AppendLine($"{I(indent)}macos-min = {QuoteString(p.Value)}");
                break;
            case IosMinProp p:
                sb.AppendLine($"{I(indent)}ios-min = {QuoteString(p.Value)}");
                break;
            case TvosMinProp p:
                sb.AppendLine($"{I(indent)}tvos-min = {QuoteString(p.Value)}");
                break;
            case WatchosMinProp p:
                sb.AppendLine($"{I(indent)}watchos-min = {QuoteString(p.Value)}");
                break;
            case ManifestProp p:
                sb.AppendLine($"{I(indent)}manifest = {QuoteString(p.Value)}");
                break;
            case EmscriptenFlagsProp p:
                sb.AppendLine($"{I(indent)}emscripten-flags {{");
                foreach (var f in p.Flags)
                    sb.AppendLine($"{I(indent + 1)}{QuoteString(f)}");
                sb.AppendLine($"{I(indent)}}}");
                break;
            case SourcesBlock b:
                FormatSourcesBlock(sb, b, indent);
                break;
            case HeadersBlock b:
                FormatHeadersBlock(sb, b, indent);
                break;
            case DefinesBlock b:
                FormatStringValuesBlock(sb, "defines", b.Values, indent);
                break;
            case LinksBlock b:
                FormatStringValuesBlock(sb, "links", b.Values, indent);
                break;
            case FrameworksBlock b:
                FormatStringValuesBlock(sb, "frameworks", b.Values, indent);
                break;
            case FrameworkPathsBlock b:
                FormatStringListBlock(sb, "framework-paths", b.Paths, indent);
                break;
            case CompileFlagsBlock b:
                FormatStringValuesBlock(sb, "compile-flags", b.Values, indent);
                break;
            case LinkFlagsBlock b:
                FormatStringValuesBlock(sb, "link-flags", b.Values, indent);
                break;
            case ResourcesBlock b:
                FormatStringListBlock(sb, "resources", b.Paths, indent);
                break;
            case DependenciesBlock b:
                FormatDependenciesBlock(sb, b, indent);
                break;
            case PchBlock b:
                FormatPchBlock(sb, b, indent);
                break;
            case UnityBuildBlock b:
                FormatUnityBuildBlock(sb, b, indent);
                break;
            case OutputBlock b:
                FormatOutputBlock(sb, b, indent);
                break;
            case PreBuildBlock b:
                FormatStringValuesBlock(sb, "pre-build", b.Commands, indent);
                break;
            case PostBuildBlock b:
                FormatStringValuesBlock(sb, "post-build", b.Commands, indent);
                break;
            case ConditionBlock b:
                FormatConditionBlock(sb, b, indent);
                break;
        }
    }

    // ─── Individual block formatters ─────────────────────────────────────────

    private static void FormatSourcesBlock(StringBuilder sb, SourcesBlock block, int indent)
    {
        string name = block.IsModules ? "modules" : "sources";
        sb.AppendLine($"{I(indent)}{name} {{");
        foreach (var item in block.Items)
        {
            string keyword = item.IsInclude ? "include" : "exclude";
            sb.AppendLine($"{I(indent + 1)}{keyword} {QuoteString(item.Glob)}");
        }
        if (block.IsModules && block.ExportMap.HasValue)
            sb.AppendLine($"{I(indent + 1)}export-map = {(block.ExportMap.Value ? "true" : "false")}");
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatHeadersBlock(StringBuilder sb, HeadersBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}headers {{");
        foreach (var item in block.Items)
        {
            string keyword = item.IsPublic ? "public" : "private";
            sb.AppendLine($"{I(indent + 1)}{keyword} {QuoteString(item.Path)}");
        }
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatStringValuesBlock(StringBuilder sb, string name, IReadOnlyList<string> values, int indent)
    {
        sb.AppendLine($"{I(indent)}{name} {{");
        foreach (var v in values)
            sb.AppendLine($"{I(indent + 1)}{QuoteString(v)}");
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatStringListBlock(StringBuilder sb, string name, IReadOnlyList<string> paths, int indent)
    {
        sb.AppendLine($"{I(indent)}{name} {{");
        foreach (var p in paths)
            sb.AppendLine($"{I(indent + 1)}{QuoteString(p)}");
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatDependenciesBlock(StringBuilder sb, DependenciesBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}dependencies {{");
        foreach (var dep in block.Items)
        {
            switch (dep.Value)
            {
                case VersionDependency v:
                    sb.AppendLine($"{I(indent + 1)}{dep.Name} = {QuoteString(v.Version)}");
                    break;
                case ComplexDependency c:
                    var parts = new List<string>();
                    if (c.Git     is not null) parts.Add($"git = {QuoteString(c.Git)}");
                    if (c.Tag     is not null) parts.Add($"tag = {QuoteString(c.Tag)}");
                    if (c.Commit  is not null) parts.Add($"commit = {QuoteString(c.Commit)}");
                    if (c.Path    is not null) parts.Add($"path = {QuoteString(c.Path)}");
                    if (c.Version is not null) parts.Add($"version = {QuoteString(c.Version)}");
                    sb.AppendLine($"{I(indent + 1)}{dep.Name} = {{ {string.Join(", ", parts)} }}");
                    break;
            }
        }
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatPchBlock(StringBuilder sb, PchBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}pch {{");
        if (block.Header  is not null) sb.AppendLine($"{I(indent + 1)}header = {QuoteString(block.Header)}");
        if (block.Source  is not null) sb.AppendLine($"{I(indent + 1)}source = {QuoteString(block.Source)}");
        if (block.Modules.HasValue)    sb.AppendLine($"{I(indent + 1)}modules = {(block.Modules.Value ? "true" : "false")}");
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatUnityBuildBlock(StringBuilder sb, UnityBuildBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}unity-build {{");
        if (block.Enabled.HasValue)   sb.AppendLine($"{I(indent + 1)}enabled = {(block.Enabled.Value ? "true" : "false")}");
        if (block.BatchSize.HasValue) sb.AppendLine($"{I(indent + 1)}batch-size = {block.BatchSize.Value}");
        if (block.Exclude.Count > 0)
        {
            sb.AppendLine($"{I(indent + 1)}exclude {{");
            foreach (var e in block.Exclude)
                sb.AppendLine($"{I(indent + 2)}{QuoteString(e)}");
            sb.AppendLine($"{I(indent + 1)}}}");
        }
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatOutputBlock(StringBuilder sb, OutputBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}output {{");
        if (block.Binaries  is not null) sb.AppendLine($"{I(indent + 1)}binaries = {QuoteString(block.Binaries)}");
        if (block.Libraries is not null) sb.AppendLine($"{I(indent + 1)}libraries = {QuoteString(block.Libraries)}");
        if (block.Symbols   is not null) sb.AppendLine($"{I(indent + 1)}symbols = {QuoteString(block.Symbols)}");
        sb.AppendLine($"{I(indent)}}}");
    }

    private static void FormatConditionBlock(StringBuilder sb, ConditionBlock block, int indent)
    {
        sb.AppendLine($"{I(indent)}[{block.Condition}] {{");
        AppendProjectItems(sb, block.Items, indent + 1);
        sb.AppendLine($"{I(indent)}}}");
    }

    // ─── package block ──────────────────────────────────────────────────────

    private static void FormatPackage(StringBuilder sb, PackageDecl package)
    {
        sb.AppendLine("package {");
        if (package.Name        is not null) sb.AppendLine($"    name = {QuoteString(package.Name)}");
        if (package.Version     is not null) sb.AppendLine($"    version = {QuoteString(package.Version)}");
        if (package.Description is not null) sb.AppendLine($"    description = {QuoteString(package.Description)}");
        if (package.License     is not null) sb.AppendLine($"    license = {QuoteString(package.License)}");
        if (package.Homepage    is not null) sb.AppendLine($"    homepage = {QuoteString(package.Homepage)}");
        if (package.Authors.Count > 0)
        {
            sb.AppendLine("    authors {");
            foreach (var a in package.Authors)
                sb.AppendLine($"        {QuoteString(a)}");
            sb.AppendLine("    }");
        }
        if (package.Exports.Count > 0)
        {
            sb.AppendLine("    exports {");
            foreach (var (k, v) in package.Exports)
                sb.AppendLine($"        {k} = {QuoteString(v)}");
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
    }

    // ─── Enum formatters ────────────────────────────────────────────────────

    private static string FormatProjectType(ProjectType v) => v switch
    {
        ProjectType.Executable    => "executable",
        ProjectType.StaticLibrary => "static-library",
        ProjectType.SharedLibrary => "shared-library",
        ProjectType.HeaderOnly    => "header-only",
        _                         => v.ToString().ToLowerInvariant(),
    };

    private static string FormatCxxStd(CxxStd v) => v switch
    {
        CxxStd.Cxx17 => "c++17",
        CxxStd.Cxx20 => "c++20",
        CxxStd.Cxx23 => "c++23",
        _            => "c++23",
    };

    private static string FormatOptimize(OptimizeLevel v) => v switch
    {
        OptimizeLevel.None  => "none",
        OptimizeLevel.Size  => "size",
        OptimizeLevel.Speed => "speed",
        OptimizeLevel.Full  => "full",
        _                   => "none",
    };

    private static string FormatDebugInfo(DebugInfoLevel v) => v switch
    {
        DebugInfoLevel.None    => "none",
        DebugInfoLevel.Minimal => "minimal",
        DebugInfoLevel.Full    => "full",
        _                      => "none",
    };

    private static string FormatRuntimeLink(RuntimeLink v) => v switch
    {
        RuntimeLink.Static  => "static",
        RuntimeLink.Dynamic => "dynamic",
        _                   => "static",
    };

    private static string FormatLibc(LibcType v) => v switch
    {
        LibcType.Glibc => "glibc",
        LibcType.Musl  => "musl",
        _              => "glibc",
    };

    private static string FormatStdlib(StdlibType v) => v switch
    {
        StdlibType.LibCxx    => "libc++",
        StdlibType.LibStdCxx => "libstdc++",
        _                    => "libstdc++",
    };

    private static string FormatWarnings(WarningLevel v) => v switch
    {
        WarningLevel.None    => "none",
        WarningLevel.Default => "default",
        WarningLevel.All     => "all",
        WarningLevel.Extra   => "extra",
        _                    => "default",
    };
}
