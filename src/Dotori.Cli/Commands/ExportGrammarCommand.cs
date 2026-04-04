using System.CommandLine;
using Dotori.Core.Grammar;

namespace Dotori.Cli.Commands;

internal static class ExportGrammarCommandFactory
{
    private static readonly Dictionary<string, IGrammarGenerator> Generators = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["textmate"]    = new TextMateGrammarGenerator(),
        ["vim"]         = new VimSyntaxGenerator(),
        ["emacs"]       = new EmacsGenerator(),
        ["sublime"]     = new SublimeSyntaxGenerator(),
        ["zed"]         = new ZedHighlightGenerator(),
        ["tree-sitter"] = new TreeSitterGrammarGenerator(),
    };

    public static Command Create()
    {
        var command = new Command("grammar", "Export editor syntax grammar definition files");

        var formatOption = new Option<string>("--format")
        {
            Description  = "Output format: textmate (default), vim, emacs, sublime, zed, tree-sitter",
            DefaultValueFactory = _ => "textmate",
        };
        var outputOption = new Option<string?>("--output")
        {
            Description = "Output file path. Omit to write to stdout.",
        };

        command.Add(formatOption);
        command.Add(outputOption);

        command.SetAction((parseResult, ct) =>
        {
            var format = parseResult.GetValue(formatOption)!;
            var output = parseResult.GetValue(outputOption);

            if (!Generators.TryGetValue(format, out var generator))
            {
                Console.Error.WriteLine(
                    $"error: unknown format '{format}'. "
                    + $"Available: {string.Join(", ", Generators.Keys)}"
                );
                return Task.FromResult(1);
            }

            var content = generator.Generate();

            if (output is null)
            {
                Console.Write(content);
            }
            else
            {
                var outputPath = Path.IsPathRooted(output)
                    ? output
                    : Path.GetFullPath(output);

                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(outputPath, content);
                Console.WriteLine($"Grammar written to '{outputPath}'");
            }

            return Task.FromResult(0);
        });

        return command;
    }
}
