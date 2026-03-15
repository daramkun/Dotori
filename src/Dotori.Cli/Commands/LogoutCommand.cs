using System.CommandLine;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class LogoutCommandFactory
{
    public static Command Create()
    {
        var command = new Command("logout", "Remove stored credentials for a registry");
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        command.Add(registryOption);

        command.SetAction((parseResult, _) =>
        {
            var registryUrl = parseResult.GetValue(registryOption)
                ?? DotoriConfigManager.Load().DefaultRegistry;

            DotoriConfigManager.RemoveToken(registryUrl);
            Console.WriteLine($"Logged out from {registryUrl}");
            return Task.CompletedTask;
        });

        return command;
    }
}
