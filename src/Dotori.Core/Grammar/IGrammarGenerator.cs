namespace Dotori.Core.Grammar;

/// <summary>에디터별 문법 정의 파일 생성기 인터페이스</summary>
public interface IGrammarGenerator
{
    /// <summary>생성기가 지원하는 형식 ID (--format 옵션값)</summary>
    string FormatId { get; }

    /// <summary>생성된 파일의 기본 확장자 (점 포함, 예: ".tmLanguage.json")</summary>
    string DefaultExtension { get; }

    /// <summary>문법 정의 파일 내용을 문자열로 생성합니다.</summary>
    string Generate();
}
