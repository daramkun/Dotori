using Dotori.Core.Parsing;

namespace Dotori.Core.Model;

/// <summary>
/// Flattens a <see cref="ProjectDecl"/> into a <see cref="FlatProjectModel"/>
/// by applying all condition blocks that match the given <see cref="TargetContext"/>,
/// in order of increasing specificity.
/// </summary>
public static class ProjectFlattener
{
    public static FlatProjectModel Flatten(
        ProjectDecl decl,
        string dotoriPath,
        TargetContext context)
    {
        var model = new FlatProjectModel
        {
            Name       = decl.Name,
            ProjectDir = Path.GetDirectoryName(dotoriPath)!,
            DotoriPath = dotoriPath,
        };

        // Collect (specificity, items) pairs; sort ascending so lower specificity
        // is applied first and higher specificity can override.
        var layers = new List<(int Specificity, List<ProjectItem> Items)>
        {
            (0, decl.Items),
        };

        CollectConditionLayers(decl.Items, context, layers);
        layers.Sort((a, b) => a.Specificity.CompareTo(b.Specificity));

        foreach (var (_, items) in layers)
            ApplyItems(model, items);

        RuntimeEnforcer.Enforce(model, context.Platform);
        return model;
    }

    // ─── Condition evaluation ──────────────────────────────────────────────

    /// <summary>
    /// Recursively collect all condition blocks that match the context,
    /// recording their specificity.
    /// </summary>
    private static void CollectConditionLayers(
        List<ProjectItem> items,
        TargetContext context,
        List<(int, List<ProjectItem>)> layers)
    {
        var activeAtoms = context.ActiveAtoms();

        foreach (var item in items)
        {
            if (item is not ConditionBlock cb) continue;

            // A condition matches if ALL its atoms are in the active set.
            if (cb.Condition.Atoms.All(a =>
                    activeAtoms.Contains(a, StringComparer.OrdinalIgnoreCase)))
            {
                layers.Add((cb.Condition.Specificity, cb.Items));
                // Recurse: nested condition blocks inside a matched block
                CollectConditionLayers(cb.Items, context, layers);
            }
        }
    }

    // ─── Item application ──────────────────────────────────────────────────

    private static void ApplyItems(FlatProjectModel model, List<ProjectItem> items)
    {
        foreach (var item in items)
        {
            // Skip condition blocks — they are handled by CollectConditionLayers
            if (item is ConditionBlock) continue;

            switch (item)
            {
                case ProjectTypeProp p:    model.Type        = p.Value; break;
                case StdProp p:            model.Std         = p.Value; break;
                case DescriptionProp p:    model.Description = p.Value; break;
                case OptimizeProp p:       model.Optimize    = p.Value; break;
                case DebugInfoProp p:      model.DebugInfo   = p.Value; break;
                case RuntimeLinkProp p:    model.RuntimeLink = p.Value; break;
                case LibcProp p:           model.Libc        = p.Value; break;
                case StdlibProp p:         model.Stdlib      = p.Value; break;
                case LtoProp p:            model.Lto         = p.Value; break;
                case WarningsProp p:       model.Warnings    = p.Value; break;
                case WarningsAsErrorsProp p: model.WarningsAsErrors = p.Value; break;
                case AndroidApiLevelProp p:  model.AndroidApiLevel  = p.Value; break;
                case MacosMinProp p:       model.MacosMin    = p.Value; break;
                case IosMinProp p:         model.IosMin      = p.Value; break;
                case TvosMinProp p:        model.TvosMin     = p.Value; break;
                case WatchosMinProp p:     model.WatchosMin  = p.Value; break;

                case EmscriptenFlagsProp p:
                    model.EmscriptenFlags.AddRange(p.Flags);
                    break;

                case SourcesBlock b when !b.IsModules:
                    model.Sources.AddRange(b.Items);
                    break;

                case SourcesBlock b when b.IsModules:
                    model.Modules.AddRange(b.Items);
                    break;

                case HeadersBlock b:
                    model.Headers.AddRange(b.Items);
                    break;

                case DefinesBlock b:
                    model.Defines.AddRange(b.Values);
                    break;

                case LinksBlock b:
                    model.Links.AddRange(b.Values);
                    break;

                case FrameworksBlock b:
                    model.Frameworks.AddRange(b.Values);
                    break;

                case DependenciesBlock b:
                    // Merge: later entries with same name overwrite earlier ones
                    foreach (var dep in b.Items)
                    {
                        var idx = model.Dependencies.FindIndex(d => d.Name == dep.Name);
                        if (idx >= 0) model.Dependencies[idx] = dep;
                        else model.Dependencies.Add(dep);
                    }
                    break;

                case PchBlock b:
                    model.Pch ??= new PchConfig();
                    if (b.Header  != null) model.Pch.Header  = b.Header;
                    if (b.Source  != null) model.Pch.Source  = b.Source;
                    if (b.Modules != null) model.Pch.Modules = b.Modules;
                    break;

                case UnityBuildBlock b:
                    model.UnityBuild ??= new UnityBuildConfig();
                    if (b.Enabled   != null) model.UnityBuild.Enabled   = b.Enabled.Value;
                    if (b.BatchSize != null) model.UnityBuild.BatchSize  = b.BatchSize.Value;
                    model.UnityBuild.Exclude.AddRange(b.Exclude);
                    break;
            }
        }
    }
}
