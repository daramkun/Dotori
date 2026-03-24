using System.Text.RegularExpressions;

namespace Dotori.Core.Grammar;

/// <summary>
/// <c>{{KEY}}</c> 형태의 플레이스홀더를 값으로 치환하는 경량 템플릿 엔진.
/// NativeAOT 완전 호환 (순수 string.Replace 사용).
/// </summary>
internal static partial class TemplateEngine
{
    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();

    /// <summary>
    /// 템플릿 문자열의 <c>{{KEY}}</c> 플레이스홀더를 <paramref name="vars"/> 딕셔너리의 값으로 치환합니다.
    /// 매핑되지 않은 플레이스홀더는 빈 문자열로 치환합니다.
    /// </summary>
    public static string Render(string template, IReadOnlyDictionary<string, string> vars) =>
        PlaceholderRegex().Replace(template, m =>
            vars.TryGetValue(m.Groups[1].Value, out var val) ? val : string.Empty);
}
