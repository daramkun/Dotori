namespace Dotori.Core.Parsing;

/// <summary>Convenience entry-point for parsing .dotori files.</summary>
public static class DotoriParser
{
    public static DotoriFile ParseFile(string filePath)
    {
        var source = File.ReadAllText(filePath);
        return ParseSource(source, filePath);
    }

    public static DotoriFile ParseSource(string source, string filePath = "<input>")
    {
        var lexer = new Lexer(source, filePath);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, filePath);
        return parser.ParseFile();
    }
}
