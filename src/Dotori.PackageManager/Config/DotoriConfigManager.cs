namespace Dotori.PackageManager.Config;

/// <summary>
/// ~/.dotori/config.toml 을 수동 파싱/저장합니다. NativeAOT 호환.
///
/// 지원 형식:
/// [registry]
/// default = "https://registry.dotori.dev"
///
/// [registry."https://registry.dotori.dev"]
/// token = "dt_live_..."
/// upstream = "https://..."
/// </summary>
public static class DotoriConfigManager
{
    private static string ConfigPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotori", "config.toml");

    public static DotoriConfig Load()
    {
        var config = new DotoriConfig();
        if (!File.Exists(ConfigPath))
            return config;

        var text = File.ReadAllText(ConfigPath);
        Parse(text, config);
        return config;
    }

    public static void Save(DotoriConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        File.WriteAllText(ConfigPath, Serialize(config));
    }

    public static void SetToken(string registryUrl, string token)
    {
        var config = Load();
        if (!config.Registries.TryGetValue(registryUrl, out var reg))
        {
            reg = new RegistryConfig { Url = registryUrl };
            config.Registries[registryUrl] = reg;
        }
        reg.Token = token;
        Save(config);
    }

    public static void RemoveToken(string registryUrl)
    {
        var config = Load();
        if (config.Registries.TryGetValue(registryUrl, out var reg))
        {
            reg.Token = null;
            Save(config);
        }
    }

    private static void Parse(string text, DotoriConfig config)
    {
        string? currentSection = null;
        string? currentRegistryUrl = null;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith('#') || string.IsNullOrEmpty(line))
                continue;

            // 섹션 헤더
            if (line.StartsWith('['))
            {
                var header = line.TrimStart('[').TrimEnd(']').Trim();
                if (header == "registry")
                {
                    currentSection = "registry-default";
                    currentRegistryUrl = null;
                }
                else if (header.StartsWith("registry.\"") && header.EndsWith("\""))
                {
                    currentSection = "registry-entry";
                    currentRegistryUrl = header["registry.\"".Length..^1];
                    if (!config.Registries.ContainsKey(currentRegistryUrl))
                        config.Registries[currentRegistryUrl] = new RegistryConfig { Url = currentRegistryUrl };
                }
                else if (header.StartsWith("registries."))
                {
                    currentSection = "registry-entry";
                    // [registries.alias] 형식도 url 키로 처리
                    var alias = header["registries.".Length..].Trim('"');
                    currentRegistryUrl = alias;
                    if (!config.Registries.ContainsKey(alias))
                        config.Registries[alias] = new RegistryConfig { Url = alias };
                }
                else
                {
                    currentSection = null;
                    currentRegistryUrl = null;
                }
                continue;
            }

            // key = "value" 파싱
            var eqIdx = line.IndexOf('=');
            if (eqIdx < 0) continue;
            var key = line[..eqIdx].Trim();
            var val = line[(eqIdx + 1)..].Trim().Trim('"');

            switch (currentSection)
            {
                case "registry-default" when key == "default":
                    config.DefaultRegistry = val;
                    break;
                case "registry-entry" when currentRegistryUrl is not null:
                    var reg = config.Registries[currentRegistryUrl];
                    switch (key)
                    {
                        case "url":      reg.Url = val;      break;
                        case "token":    reg.Token = val;    break;
                        case "upstream": reg.Upstream = val; break;
                    }
                    break;
            }
        }
    }

    private static string Serialize(DotoriConfig config)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[registry]");
        sb.AppendLine($"default = \"{config.DefaultRegistry}\"");

        foreach (var (url, reg) in config.Registries)
        {
            sb.AppendLine();
            sb.AppendLine($"[registry.\"{url}\"]");
            if (reg.Token is not null)
                sb.AppendLine($"token = \"{reg.Token}\"");
            if (reg.Upstream is not null)
                sb.AppendLine($"upstream = \"{reg.Upstream}\"");
        }

        return sb.ToString();
    }
}
