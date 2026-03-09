using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class InfoCommandFactory
{
    public static Command Create()
    {
        var infoCommand = new Command("info", "Information and diagnostic commands");

        infoCommand.Add(CreateGraphCommand());
        infoCommand.Add(CreateCheckCommand());
        infoCommand.Add(CreateTargetsCommand());
        infoCommand.Add(CreateToolchainCommand());

        return infoCommand;
    }

    private static Command CreateGraphCommand()
    {
        var command = new Command("graph", "Print project DAG and dependency graph");

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori graph: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateCheckCommand()
    {
        var command = new Command("check", "Validate .dotori file");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        command.Add(projectOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori check: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateTargetsCommand()
    {
        var command = new Command("targets", "List available build targets");

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori targets: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateToolchainCommand()
    {
        var command = new Command("toolchain", "Show detected toolchain information");

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori toolchain: not yet implemented");
            return 1;
        });

        return command;
    }
}
