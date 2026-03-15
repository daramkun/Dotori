using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotori.PackageManager.Config;

namespace Dotori.PackageManager;

/// <summary>HTTP 기반 레지스트리 API 클라이언트. NativeAOT 호환 (소스 제네레이터 사용).</summary>
public sealed class RegistryClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public RegistryClient(string registryUrl, string? token = null)
    {
        _baseUrl = registryUrl.TrimEnd('/');
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("dotori/1.0");
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    public static RegistryClient FromConfig(string? registryUrl = null)
    {
        var config = DotoriConfigManager.Load();
        var reg = config.GetRegistry(registryUrl);
        return new RegistryClient(reg.Url, reg.Token);
    }

    /// <summary>owner/name 패키지의 모든 버전 목록을 반환합니다.</summary>
    public async Task<List<string>> GetVersionsAsync(string owner, string name, CancellationToken ct = default)
    {
        try
        {
            var dto = await _http.GetFromJsonAsync(
                $"{_baseUrl}/api/v1/packages/{owner}/{name}",
                RegistryJsonContext.Default.PackageListResponseDto, ct);

            if (dto is null) return [];
            return dto.Versions
                .Where(v => !v.Yanked)
                .Select(v => v.Version)
                .ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    /// <summary>특정 버전의 메타데이터를 반환합니다.</summary>
    public async Task<PackageVersionInfo?> GetMetadataAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        try
        {
            var dto = await _http.GetFromJsonAsync(
                $"{_baseUrl}/api/v1/packages/{owner}/{name}/{version}",
                RegistryJsonContext.Default.PackageVersionDto, ct);

            if (dto is null) return null;
            return new PackageVersionInfo(dto.Owner, dto.Name, dto.Version, dto.Hash, dto.Yanked);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <summary>패키지 아카이브를 다운로드하여 스트림으로 반환합니다.</summary>
    public async Task<Stream> DownloadAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(
            $"{_baseUrl}/api/v1/packages/{owner}/{name}/{version}/download",
            HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync(ct);
    }

    /// <summary>패키지를 레지스트리에 발행합니다.</summary>
    public async Task PublishAsync(Stream archive, string filename, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(archive);
        content.Add(streamContent, "archive", filename);

        var resp = await _http.PostAsync($"{_baseUrl}/api/v1/packages/publish", content, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new RegistryException($"Publish failed ({(int)resp.StatusCode}): {body}");
        }
    }

    /// <summary>패키지를 검색합니다.</summary>
    public async Task<PackageSearchResult> SearchAsync(string query, int page = 1, int perPage = 20, CancellationToken ct = default)
    {
        try
        {
            var dto = await _http.GetFromJsonAsync(
                $"{_baseUrl}/api/v1/packages/search?q={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}",
                RegistryJsonContext.Default.PackageSearchResultDto, ct);

            if (dto is null) return new PackageSearchResult(0, []);
            return new PackageSearchResult(dto.Total, dto.Items.Select(i =>
                new PackageInfo(i.Owner, i.Name, i.LatestVersion, i.Description, i.TotalDownloads)).ToList());
        }
        catch (HttpRequestException)
        {
            return new PackageSearchResult(0, []);
        }
    }

    public void Dispose() => _http.Dispose();
}

// --- 공개 모델 ---

public sealed record PackageVersionInfo(string Owner, string Name, string Version, string Hash, bool Yanked);
public sealed record PackageInfo(string Owner, string Name, string? LatestVersion, string? Description, long TotalDownloads);
public sealed record PackageSearchResult(int Total, List<PackageInfo> Items);

// --- JSON DTOs (소스 제네레이터) ---

internal sealed class PackageVersionDto
{
    [JsonPropertyName("owner")]         public string Owner { get; init; } = "";
    [JsonPropertyName("name")]          public string Name { get; init; } = "";
    [JsonPropertyName("version")]       public string Version { get; init; } = "";
    [JsonPropertyName("hash")]          public string Hash { get; init; } = "";
    [JsonPropertyName("yanked")]        public bool Yanked { get; init; }
    [JsonPropertyName("downloadCount")] public long DownloadCount { get; init; }
}

internal sealed class PackageListResponseDto
{
    [JsonPropertyName("owner")]          public string Owner { get; init; } = "";
    [JsonPropertyName("name")]           public string Name { get; init; } = "";
    [JsonPropertyName("latestVersion")]  public string? LatestVersion { get; init; }
    [JsonPropertyName("description")]    public string? Description { get; init; }
    [JsonPropertyName("totalDownloads")] public long TotalDownloads { get; init; }
    [JsonPropertyName("versions")]       public List<PackageVersionDto> Versions { get; init; } = [];
}

internal sealed class PackageSearchItemDto
{
    [JsonPropertyName("owner")]          public string Owner { get; init; } = "";
    [JsonPropertyName("name")]           public string Name { get; init; } = "";
    [JsonPropertyName("latestVersion")]  public string? LatestVersion { get; init; }
    [JsonPropertyName("description")]    public string? Description { get; init; }
    [JsonPropertyName("totalDownloads")] public long TotalDownloads { get; init; }
}

internal sealed class PackageSearchResultDto
{
    [JsonPropertyName("total")]   public int Total { get; init; }
    [JsonPropertyName("page")]    public int Page { get; init; }
    [JsonPropertyName("perPage")] public int PerPage { get; init; }
    [JsonPropertyName("items")]   public List<PackageSearchItemDto> Items { get; init; } = [];
}

[JsonSerializable(typeof(PackageVersionDto))]
[JsonSerializable(typeof(PackageListResponseDto))]
[JsonSerializable(typeof(PackageSearchResultDto))]
internal partial class RegistryJsonContext : JsonSerializerContext { }

public sealed class RegistryException(string message) : Exception(message);
