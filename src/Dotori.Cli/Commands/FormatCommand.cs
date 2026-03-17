using System.CommandLine;
using Dotori.Core.Parsing;

namespace Dotori.Cli.Commands;

internal static class FormatCommandFactory
{
    public static Command Create()
    {
        var command = new Command("format", "Format .dotori file(s)");

        var fileArg = new Argument<string?>("file")
        {
            Description = "Path to .dotori file or directory (optional)",
            Arity       = ArgumentArity.ZeroOrOne,
        };
        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var checkOption   = new Option<bool>("--check")      { Description = "Check formatting without modifying files (exit 1 if any file needs reformatting)" };
        var stdoutOption  = new Option<bool>("--stdout")     { Description = "Write formatted output to stdout instead of modifying the file" };

        command.Add(fileArg);
        command.Add(projectOption);
        command.Add(checkOption);
        command.Add(stdoutOption);

        command.SetAction((parseResult) =>
        {
            var file    = parseResult.GetValue(fileArg);
            var project = parseResult.GetValue(projectOption);
            var check   = parseResult.GetValue(checkOption);
            var stdout  = parseResult.GetValue(stdoutOption);

            // Positional file argument takes precedence over --project
            var targetArg = file ?? project;

            var paths = BuildContext.ResolveProjectPaths(targetArg, buildAll: true);
            if (paths.Count == 0) return 1;

            int needsReformat = 0;

            foreach (var path in paths)
            {
                try
                {
                    var parsed    = DotoriParser.ParseFile(path);
                    var formatted = DotoriFormatter.Format(parsed);

                    if (stdout)
                    {
                        Console.Write(formatted);
                        continue;
                    }

                    var original           = File.ReadAllText(path);
                    var normalizedOriginal = NormalizeLineEndings(original);
                    var normalizedFormatted = NormalizeLineEndings(formatted);

                    if (normalizedOriginal == normalizedFormatted)
                    {
                        if (!check)
                            Console.WriteLine($"  OK (unchanged): {path}");
                        else
                            Console.WriteLine($"  OK: {path}");
                    }
                    else if (check)
                    {
                        Console.Error.WriteLine($"  Would reformat: {path}");
                        needsReformat++;
                    }
                    else
                    {
                        File.WriteAllText(path, formatted);
                        Console.WriteLine($"  Reformatted: {path}");
                    }
                }
                catch (ParseException ex)
                {
                    Console.Error.WriteLine($"  Error in '{path}': {ex.Message}");
                    needsReformat++;
                }
            }

            if (check && needsReformat > 0)
            {
                Console.Error.WriteLine($"\n{needsReformat} file(s) need reformatting.");
                return 1;
            }

            return needsReformat == 0 ? 0 : 1;
        });

        return command;
    }

    private static string NormalizeLineEndings(string s) =>
        s.Replace("\r\n", "\n").Replace("\r", "\n");
}
