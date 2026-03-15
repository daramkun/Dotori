namespace Dotori.PackageManager;

/// <summary>
/// PubGrub IPackageSource 구현: 레지스트리 API로 버전/의존성 정보를 조회합니다.
/// git 패키지는 Phase1PackageSource와 동일하게 처리합니다.
/// 레지스트리에 연결할 수 없으면 기존 lock 파일 정보로 폴백합니다.
/// </summary>
internal sealed class RegistryPackageSource : IPackageSource
{
    private readonly LockFile _existingLock;
    private readonly RegistryClient? _client;
    private readonly string _registryUrl;
    private readonly CancellationToken _ct;

    private readonly Dictionary<string, (string url, string tagOrCommit)> _gitInfo = new();
    private readonly Dictionary<string, IReadOnlyDictionary<string, VersionConstraint>> _depCache = new();
    private readonly Dictionary<string, List<SemanticVersion>> _versionCache = new(StringComparer.OrdinalIgnoreCase);

    public RegistryPackageSource(LockFile existingLock, string registryUrl, RegistryClient? client, CancellationToken ct)
    {
        _existingLock = existingLock;
        _registryUrl = registryUrl;
        _client = client;
        _ct = ct;
    }

    public void RegisterGit(string name, string url, string tagOrCommit) =>
        _gitInfo[name] = (url, tagOrCommit);

    public bool TryGetGitInfo(string name, out string? url, out string? tagOrCommit)
    {
        if (_gitInfo.TryGetValue(name, out var info))
        {
            url = info.url;
            tagOrCommit = info.tagOrCommit;
            return true;
        }
        url = tagOrCommit = null;
        return false;
    }

    public bool TryGetRegistryInfo(string name, out string registryUrl)
    {
        registryUrl = _registryUrl;
        // name이 "owner/package" 형식인 경우 레지스트리 패키지
        return name.Contains('/') && !_gitInfo.ContainsKey(name);
    }

    public async Task<IReadOnlyList<SemanticVersion>> GetVersionsAsync(string package, CancellationToken ct)
    {
        if (_versionCache.TryGetValue(package, out var cached))
            return cached;

        var versions = new List<SemanticVersion>();

        // 1. 기존 lock 파일에서 버전 수집
        foreach (var entry in _existingLock.Packages)
        {
            if (entry.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                SemanticVersion.TryParse(entry.Version, out var v))
            {
                versions.Add(v);
            }
        }

        // 2. git 패키지이면 고정 버전만
        if (_gitInfo.TryGetValue(package, out var gitInfo))
        {
            var tagOrCommit = gitInfo.tagOrCommit;
            var versionStr = tagOrCommit.StartsWith('v')
                ? tagOrCommit[1..]
                : tagOrCommit[..Math.Min(8, tagOrCommit.Length)];

            if (SemanticVersion.TryParse(versionStr, out var gitVer) && !versions.Contains(gitVer))
                versions.Add(gitVer);
        }
        // 3. owner/name 형식이면 레지스트리에서 버전 목록 조회
        else if (package.Contains('/') && _client is not null)
        {
            var slash = package.IndexOf('/');
            var owner = package[..slash];
            var name = package[(slash + 1)..];

            try
            {
                var remoteVersions = await _client.GetVersionsAsync(owner, name, ct);
                foreach (var vs in remoteVersions)
                {
                    if (SemanticVersion.TryParse(vs, out var rv) && !versions.Contains(rv))
                        versions.Add(rv);
                }
            }
            catch (Exception)
            {
                // 레지스트리 연결 실패 → lock 파일 폴백
            }
        }

        versions.Sort((a, b) => b.CompareTo(a));
        _versionCache[package] = versions;
        return versions;
    }

    public async Task<IReadOnlyDictionary<string, VersionConstraint>> GetDependenciesAsync(
        string package, SemanticVersion version, CancellationToken ct)
    {
        var key = $"{package}@{version}";
        if (_depCache.TryGetValue(key, out var cached))
            return cached;

        // lock 파일에서 먼저 확인
        var lockEntry = _existingLock.Packages.FirstOrDefault(
            p => p.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                 SemanticVersion.TryParse(p.Version, out var v) && v == version);

        if (lockEntry is not null && lockEntry.Deps.Count > 0)
        {
            var deps = new Dictionary<string, VersionConstraint>();
            foreach (var dep in lockEntry.Deps)
            {
                var atIdx = dep.LastIndexOf('@');
                if (atIdx > 0)
                    deps[dep[..atIdx]] = VersionConstraint.Parse(dep[(atIdx + 1)..]);
            }
            _depCache[key] = deps;
            return deps;
        }

        // 레지스트리에서 메타데이터 조회 (manifest 파싱으로 의존성 추출)
        if (package.Contains('/') && _client is not null)
        {
            var slash = package.IndexOf('/');
            var owner = package[..slash];
            var name = package[(slash + 1)..];

            try
            {
                var meta = await _client.GetMetadataAsync(owner, name, version.ToString(), ct);
                if (meta is not null)
                {
                    // 현재 서버 응답에서 manifest 파싱은 추후 구현
                    // 일단 빈 의존성 반환
                }
            }
            catch (Exception)
            {
                // 폴백
            }
        }

        var empty = new Dictionary<string, VersionConstraint>();
        _depCache[key] = empty;
        return empty;
    }
}
