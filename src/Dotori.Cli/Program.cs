using System.CommandLine;
using Dotori.Cli.Commands;

var rootCommand = new RootCommand("dotori — C++ build system and package manager");

rootCommand.Add(BuildCommandFactory.Create());
rootCommand.Add(RunCommandFactory.Create());
rootCommand.Add(TestCommandFactory.Create());
rootCommand.Add(CleanCommandFactory.Create());
rootCommand.Add(PackageCommandFactory.Create());
rootCommand.Add(InfoCommandFactory.Create());

return await rootCommand.Parse(args).InvokeAsync();
