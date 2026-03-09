using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class PackageCommandFactory
{
    public static Command Create()
    {
        var packageCommand = new Command("package", "Package management commands");

        packageCommand.Add(CreateAddCommand());
        packageCommand.Add(CreateRemoveCommand());
        packageCommand.Add(CreateUpdateCommand());
        packageCommand.Add(CreateListCommand());

        return packageCommand;
    }

    private static Command CreateAddCommand()
    {
        var command = new Command("add", "Add a dependency");

        var nameArg = new Argument<string>("name") { Description = "Package name (optionally with @version)" };
        var gitOption = new Option<bool>("--git") { Description = "Add a git dependency" };
        var pathOption = new Option<bool>("--path") { Description = "Add a local path dependency" };

        command.Add(nameArg);
        command.Add(gitOption);
        command.Add(pathOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori add: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateRemoveCommand()
    {
        var command = new Command("remove", "Remove a dependency");

        var nameArg = new Argument<string>("name") { Description = "Package name to remove" };
        command.Add(nameArg);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori remove: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateUpdateCommand()
    {
        var command = new Command("update", "Update dependencies");

        var nameArg = new Argument<string?>("name")
        {
            Description = "Package name to update (omit for all)",
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Add(nameArg);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori update: not yet implemented");
            return 1;
        });

        return command;
    }

    private static Command CreateListCommand()
    {
        var command = new Command("list", "List dependencies");

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori list: not yet implemented");
            return 1;
        });

        return command;
    }
}
