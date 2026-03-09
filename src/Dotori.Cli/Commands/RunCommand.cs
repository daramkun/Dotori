using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class RunCommandFactory
{
    public static Command Create()
    {
        var command = new Command("run", "Build and run the project");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var releaseOption = new Option<bool>("--release") { Description = "Run in Release configuration" };

        command.Add(projectOption);
        command.Add(releaseOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori run: not yet implemented");
            return 1;
        });

        return command;
    }
}
