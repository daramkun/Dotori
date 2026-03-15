namespace Dotori.Registry.Auth;

public sealed class OAuthProviderRegistry(IEnumerable<IOAuthProvider> providers, IConfiguration config)
{
    private readonly Dictionary<string, IOAuthProvider> _map =
        providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    public IOAuthProvider? Get(string name)
    {
        if (!_map.TryGetValue(name, out var provider))
            return null;

        // 설정에서 비활성화된 provider는 null 반환
        var enabled = config[$"OAuth:Providers:{name}:Enabled"];
        if (!string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
            return null;

        return provider;
    }

    public IEnumerable<string> EnabledProviders =>
        _map.Keys.Where(name =>
        {
            var enabled = config[$"OAuth:Providers:{name}:Enabled"];
            return string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase);
        });
}
