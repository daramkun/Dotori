namespace Dotori.PackageManager.Config;

public sealed class RegistryConfig
{
    public string Url { get; set; } = "https://registry.dotori.dev";
    public string? Token { get; set; }
    // 프록시 모드: upstream 레지스트리 URL
    public string? Upstream { get; set; }
}

public sealed class DotoriConfig
{
    public string DefaultRegistry { get; set; } = "https://registry.dotori.dev";

    // key = registry URL
    public Dictionary<string, RegistryConfig> Registries { get; } = new(StringComparer.OrdinalIgnoreCase);

    public RegistryConfig GetRegistry(string? url = null)
    {
        var key = url ?? DefaultRegistry;
        if (Registries.TryGetValue(key, out var cfg))
            return cfg;
        return new RegistryConfig { Url = key };
    }
}
