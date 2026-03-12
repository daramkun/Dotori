using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dotori.Core.Build;

/// <summary>A single entry in module-map.json.</summary>
public sealed class ModuleMapEntry
{
    [JsonPropertyName("logical-name")]
    public required string LogicalName { get; init; }

    [JsonPropertyName("source-file")]
    public required string SourceFile  { get; init; }

    [JsonPropertyName("bmi-path")]
    public required string BmiPath     { get; init; }
}

/// <summary>Root object of module-map.json.</summary>
public sealed class ModuleMap
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("target")]
    public required string Target { get; init; }

    [JsonPropertyName("config")]
    public required string Config { get; init; }

    [JsonPropertyName("modules")]
    public required List<ModuleMapEntry> Modules { get; init; }
}

[JsonSerializable(typeof(ModuleMap))]
[JsonSerializable(typeof(ModuleMapEntry))]
[JsonSerializable(typeof(List<ModuleMapEntry>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class ModuleMapJsonContext : JsonSerializerContext { }

/// <summary>
/// Writes a module-map.json file alongside the generated BMI files.
/// This allows IDE tools and dependent packages to locate BMIs without
/// re-scanning source files.
/// </summary>
public static class ModuleMapWriter
{
    /// <summary>
    /// Build a <see cref="ModuleMap"/> from a completed list of BMI compile jobs
    /// and write it to <c>.dotori-cache/obj/&lt;target&gt;-&lt;config&gt;/bmi/module-map.json</c>.
    /// </summary>
    public static void Write(
        IReadOnlyList<CompileJob> moduleJobs,
        string                   targetId,
        string                   config,
        string                   bmiDir)
    {
        var entries = new List<ModuleMapEntry>(moduleJobs.Count);

        foreach (var job in moduleJobs)
        {
            var dep = ModuleScanner.ScanByText(job.SourceFile);
            if (dep.Provides is null) continue;

            entries.Add(new ModuleMapEntry
            {
                LogicalName = dep.Provides,
                SourceFile  = job.SourceFile,
                BmiPath     = job.OutputFile,
            });
        }

        if (entries.Count == 0) return;

        var map = new ModuleMap
        {
            Target  = targetId,
            Config  = config,
            Modules = entries,
        };

        Directory.CreateDirectory(bmiDir);
        var mapPath = Path.Combine(bmiDir, "module-map.json");
        var json    = JsonSerializer.Serialize(map, ModuleMapJsonContext.Default.ModuleMap);
        File.WriteAllText(mapPath, json);
    }
}
