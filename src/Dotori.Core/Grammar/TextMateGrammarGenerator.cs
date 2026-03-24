using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dotori.Core.Grammar;

/// <summary>TextMate 형식 (.tmLanguage.json) 문법 파일 생성기.</summary>
/// <remarks>VSCode, JetBrains/IntelliJ, Sublime Text, TextMate, Atom 등에서 사용합니다.</remarks>
public sealed class TextMateGrammarGenerator : IGrammarGenerator
{
    public string FormatId => "textmate";
    public string DefaultExtension => ".tmLanguage.json";

    public string Generate()
    {
        var root = new JsonObject
        {
            ["$schema"] = "https://raw.githubusercontent.com/martinring/tmlanguage/master/tmlanguage.json",
            ["name"] = "Dotori",
            ["scopeName"] = "source.dotori",
            ["fileTypes"] = new JsonArray("dotori"),
            ["patterns"] = new JsonArray(
                Include("comments-line"),
                Include("comments-block"),
                Include("top-keywords"),
                Include("block-keywords"),
                Include("property-keywords"),
                Include("value-keywords"),
                Include("condition-blocks"),
                Include("strings"),
                Include("numbers"),
                Include("operators"),
                Include("identifiers")
            ),
            ["repository"] = BuildRepository(),
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject Include(string name) =>
        new() { ["include"] = $"#{name}" };

    private static JsonObject BuildRepository()
    {
        return new JsonObject
        {
            ["comments-line"] = new JsonObject
            {
                ["patterns"] = new JsonArray(
                    new JsonObject
                    {
                        ["name"] = "comment.line.number-sign.dotori",
                        ["match"] = "#.*$",
                    },
                    new JsonObject
                    {
                        ["name"] = "comment.line.double-slash.dotori",
                        ["match"] = "//.*$",
                    }
                ),
            },

            ["comments-block"] = new JsonObject
            {
                ["name"] = "comment.block.dotori",
                ["begin"] = "\\(\\*",
                ["end"] = "\\*\\)",
                ["captures"] = new JsonObject
                {
                    ["0"] = new JsonObject { ["name"] = "punctuation.definition.comment.dotori" },
                },
            },

            ["top-keywords"] = new JsonObject
            {
                ["name"] = "keyword.declaration.dotori",
                ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.TopLevelKeywords)})\\b",
            },

            ["block-keywords"] = new JsonObject
            {
                ["name"] = "keyword.other.block.dotori",
                ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.BlockKeywords)})\\b",
            },

            ["property-keywords"] = new JsonObject
            {
                ["name"] = "support.type.property-name.dotori",
                ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.PropertyKeywords)})\\b",
            },

            ["value-keywords"] = new JsonObject
            {
                ["patterns"] = new JsonArray(
                    new JsonObject
                    {
                        ["name"] = "support.constant.type.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.TypeValues)})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "constant.language.std.dotori",
                        // c++ 에 포함된 '+' 이스케이프
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.StdValues.Select(EscapeRegex))})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "constant.language.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.EnumValues.Select(EscapeRegex))})\\b",
                    }
                ),
            },

            ["condition-blocks"] = new JsonObject
            {
                ["name"] = "meta.condition.dotori",
                ["begin"] = "\\[",
                ["end"] = "\\]",
                ["beginCaptures"] = new JsonObject
                {
                    ["0"] = new JsonObject { ["name"] = "punctuation.section.condition.begin.dotori" },
                },
                ["endCaptures"] = new JsonObject
                {
                    ["0"] = new JsonObject { ["name"] = "punctuation.section.condition.end.dotori" },
                },
                ["patterns"] = new JsonArray(
                    new JsonObject
                    {
                        ["name"] = "keyword.control.platform.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.PlatformConditions)})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "keyword.control.config.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.ConfigConditions)})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "keyword.control.compiler.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.CompilerConditions)})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "keyword.control.runtime.dotori",
                        ["match"] = $"\\b({Alternation(DotoriGrammarDefinition.RuntimeConditions)})\\b",
                    },
                    new JsonObject
                    {
                        ["name"] = "punctuation.separator.condition.dotori",
                        ["match"] = "\\.",
                    }
                ),
            },

            ["strings"] = new JsonObject
            {
                ["name"] = "string.quoted.double.dotori",
                ["begin"] = "\"",
                ["end"] = "\"",
                ["patterns"] = new JsonArray(
                    // ${VAR} 환경변수 보간
                    new JsonObject
                    {
                        ["name"] = "variable.other.env.dotori",
                        ["match"] = "\\$\\{[^}]*\\}",
                    },
                    // 이스케이프 문자
                    new JsonObject
                    {
                        ["name"] = "constant.character.escape.dotori",
                        ["match"] = "\\\\.",
                    }
                ),
            },

            ["numbers"] = new JsonObject
            {
                ["name"] = "constant.numeric.integer.dotori",
                ["match"] = "\\b\\d+\\b",
            },

            ["operators"] = new JsonObject
            {
                ["name"] = "keyword.operator.assignment.dotori",
                ["match"] = "=",
            },

            ["identifiers"] = new JsonObject
            {
                ["name"] = "variable.other.dotori",
                ["match"] = "[a-zA-Z_][a-zA-Z0-9_\\-]*",
            },
        };
    }

    /// <summary>키워드 배열을 정규식 교대(alternation) 패턴으로 결합합니다.</summary>
    private static string Alternation(IEnumerable<string> keywords) =>
        string.Join("|", keywords);

    /// <summary>정규식 내 특수문자를 이스케이프합니다.</summary>
    private static string EscapeRegex(string s) =>
        s.Replace("+", "\\+").Replace(".", "\\.");
}
