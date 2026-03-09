using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class TestCommandFactory
{
    public static Command Create()
    {
        var command = new Command("test", "Build and run tests");

        var filterOption = new Option<string?>("--filter") { Description = "Test name filter pattern" };

        command.Add(filterOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori test: not yet implemented");
            return 1;
        });

        return command;
    }
}
