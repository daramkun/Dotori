using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class CleanCommandFactory
{
    public static Command Create()
    {
        var command = new Command("clean", "Remove build artifacts");

        var allOption = new Option<bool>("--all") { Description = "Remove all cached artifacts including packages" };

        command.Add(allOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori clean: not yet implemented");
            return 1;
        });

        return command;
    }
}
