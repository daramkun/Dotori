using System.CommandLine;
using Dotori.LanguageServer;

namespace Dotori.Cli.Commands;

internal static class LspCommandFactory
{
    public static Command Create()
    {
        var command = new Command("lsp", "Start the Dotori Language Server (LSP, stdio transport)");

        var logFileOption = new Option<string?>("--log-file")
        {
            Description = "Path to write LSP server log output",
        };

        command.Add(logFileOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var logFile = parseResult.GetValue(logFileOption);
            await DotoriLanguageServer.RunAsync(logFile, ct);
            return 0;
        });

        return command;
    }
}
