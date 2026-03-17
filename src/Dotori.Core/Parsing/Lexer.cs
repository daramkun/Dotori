namespace Dotori.Core.Parsing;

public enum TokenKind
{
    // Literals
    Ident,          // foo, my-lib, c++23
    String,         // "hello"
    Integer,        // 42
    BoolTrue,       // true
    BoolFalse,      // false

    // Comment (# text  or  (* text *)  or  // text)
    Comment,        // text content after stripping delimiter

    // Punctuation
    LBrace,         // {
    RBrace,         // }
    LBracket,       // [
    RBracket,       // ]
    Equals,         // =
    Comma,          // ,

    // Special
    Dot,            // . (inside condition)
    Slash,          // / (inside dep_name: owner/package)
    Eof,
}

public readonly record struct Token(TokenKind Kind, string Text, SourceLocation Location);

public sealed class LexerException(string message, SourceLocation location)
    : Exception($"{location}: {message}")
{
    public SourceLocation Location { get; } = location;
}

public sealed class Lexer
{
    private readonly string _source;
    private readonly string _file;
    private int _pos;
    private int _line = 1;
    private int _col = 1;

    public Lexer(string source, string file)
    {
        _source = source;
        _file = file;
    }

    private char Current => _pos < _source.Length ? _source[_pos] : '\0';
    private char Peek(int offset = 1) => (_pos + offset) < _source.Length ? _source[_pos + offset] : '\0';

    private SourceLocation Here => new(_file, _line, _col);

    private void Advance()
    {
        if (_pos >= _source.Length) return;
        if (_source[_pos] == '\n') { _line++; _col = 1; }
        else { _col++; }
        _pos++;
    }

    private void SkipWhitespace()
    {
        while (_pos < _source.Length && char.IsWhiteSpace(Current))
            Advance();
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (true)
        {
            SkipWhitespace();
            if (_pos >= _source.Length)
            {
                tokens.Add(new Token(TokenKind.Eof, "", Here));
                break;
            }

            var loc = Here;
            var ch = Current;

            // ── # line comment ────────────────────────────────────────────────
            if (ch == '#')
            {
                Advance(); // consume '#'
                var text = ReadLineRest().Trim();
                tokens.Add(new Token(TokenKind.Comment, text, loc));
                continue;
            }

            // ── // line comment ───────────────────────────────────────────────
            if (ch == '/' && Peek() == '/')
            {
                Advance(); Advance(); // consume '//'
                var text = ReadLineRest().Trim();
                tokens.Add(new Token(TokenKind.Comment, text, loc));
                continue;
            }

            // ── (* ... *) block comment (nestable, backward-compat) ───────────
            if (ch == '(' && Peek() == '*')
            {
                Advance(); Advance(); // consume '(*'
                tokens.AddRange(ReadBlockComment(loc));
                continue;
            }

            switch (ch)
            {
                case '{': Advance(); tokens.Add(new Token(TokenKind.LBrace, "{", loc)); break;
                case '}': Advance(); tokens.Add(new Token(TokenKind.RBrace, "}", loc)); break;
                case '[': Advance(); tokens.Add(new Token(TokenKind.LBracket, "[", loc)); break;
                case ']': Advance(); tokens.Add(new Token(TokenKind.RBracket, "]", loc)); break;
                case '=': Advance(); tokens.Add(new Token(TokenKind.Equals, "=", loc)); break;
                case ',': Advance(); tokens.Add(new Token(TokenKind.Comma, ",", loc)); break;
                case '.': Advance(); tokens.Add(new Token(TokenKind.Dot, ".", loc)); break;
                case '/': Advance(); tokens.Add(new Token(TokenKind.Slash, "/", loc)); break;

                case '"':
                {
                    var text = ReadString(loc);
                    tokens.Add(new Token(TokenKind.String, text, loc));
                    break;
                }

                default:
                    if (char.IsDigit(ch))
                    {
                        var num = ReadInteger(loc);
                        tokens.Add(new Token(TokenKind.Integer, num, loc));
                    }
                    else if (IsIdentStart(ch))
                    {
                        var ident = ReadIdent();
                        var kind = ident switch
                        {
                            "true"  => TokenKind.BoolTrue,
                            "false" => TokenKind.BoolFalse,
                            _       => TokenKind.Ident,
                        };
                        tokens.Add(new Token(kind, ident, loc));
                    }
                    else
                    {
                        throw new LexerException($"Unexpected character '{ch}'", loc);
                    }
                    break;
            }
        }
        return tokens;
    }

    /// <summary>Reads the rest of the current line (not including the newline).</summary>
    private string ReadLineRest()
    {
        var start = _pos;
        while (_pos < _source.Length && Current != '\n')
            Advance();
        return _source.Substring(start, _pos - start);
    }

    /// <summary>
    /// Reads a nestable <c>(*...*)&gt;</c> block comment and returns one
    /// <see cref="TokenKind.Comment"/> token per non-empty content line.
    /// Called after the opening <c>(*</c> has already been consumed.
    /// </summary>
    private IEnumerable<Token> ReadBlockComment(SourceLocation loc)
    {
        var sb = new System.Text.StringBuilder();
        int depth = 1;
        while (_pos < _source.Length && depth > 0)
        {
            if (Current == '(' && Peek() == '*') { Advance(); Advance(); depth++; sb.Append("(*"); }
            else if (Current == '*' && Peek() == ')') { Advance(); Advance(); depth--; if (depth > 0) sb.Append("*)"); }
            else { sb.Append(Current); Advance(); }
        }

        // Split into lines, trim each, drop empty leading/trailing lines
        var lines = sb.ToString()
                      .Split('\n')
                      .Select(l => l.Trim())
                      .ToList();

        while (lines.Count > 0 && string.IsNullOrEmpty(lines[0]))   lines.RemoveAt(0);
        while (lines.Count > 0 && string.IsNullOrEmpty(lines[^1])) lines.RemoveAt(lines.Count - 1);

        return lines.Select(l => new Token(TokenKind.Comment, l, loc));
    }

    private string ReadString(SourceLocation loc)
    {
        Advance(); // consume opening "
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length && Current != '"')
        {
            if (Current == '\\')
            {
                Advance();
                var escaped = Current switch
                {
                    'n'  => '\n',
                    'r'  => '\r',
                    't'  => '\t',
                    '"'  => '"',
                    '\\' => '\\',
                    _    => throw new LexerException($"Unknown escape sequence '\\{Current}'", Here),
                };
                sb.Append(escaped);
                Advance();
            }
            else
            {
                sb.Append(Current);
                Advance();
            }
        }
        if (_pos >= _source.Length)
            throw new LexerException("Unterminated string literal", loc);
        Advance(); // consume closing "
        return sb.ToString();
    }

    private string ReadInteger(SourceLocation loc)
    {
        var start = _pos;
        while (_pos < _source.Length && char.IsDigit(Current))
            Advance();
        return _source.Substring(start, _pos - start);
    }

    // Ident characters: letters, digits, underscore, hyphen, +
    // (needed for c++23, my-lib, etc.)
    private static bool IsIdentStart(char c) =>
        char.IsLetter(c) || c == '_';

    private static bool IsIdentContinue(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '+';

    private string ReadIdent()
    {
        var start = _pos;
        while (_pos < _source.Length && IsIdentContinue(Current))
            Advance();
        return _source.Substring(start, _pos - start);
    }
}
