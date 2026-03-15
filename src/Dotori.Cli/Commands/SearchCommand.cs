using System.CommandLine;
using Dotori.PackageManager;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class SearchCommandFactory
{
    public static Command Create()
    {
        var command = new Command("search", "Search for packages in a registry");

        var queryArg      = new Argument<string>("query")      { Description = "Search query" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        var pageOption    = new Option<int?>("--page")         { Description = "Page number (default: 1)" };

        command.Add(queryArg);
        command.Add(registryOption);
        command.Add(pageOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var query    = parseResult.GetValue(queryArg) ?? "";
            var regUrl   = parseResult.GetValue(registryOption);
            var page     = parseResult.GetValue(pageOption) ?? 1;

            using var client = RegistryClient.FromConfig(regUrl);
            var result = await client.SearchAsync(query, page, 20, ct);

            if (result.Total == 0)
            {
                Console.WriteLine($"No packages found for '{query}'");
                return;
            }

            Console.WriteLine($"Found {result.Total} package(s) matching '{query}':\n");
            Console.WriteLine($"{"NAME",-35} {"VERSION",-12} DOWNLOADS");
            Console.WriteLine(new string('-', 60));
            foreach (var pkg in result.Items)
            {
                Console.WriteLine($"{pkg.Owner + "/" + pkg.Name,-35} {pkg.LatestVersion ?? "?",-12} {pkg.TotalDownloads}");
                if (pkg.Description is not null)
                    Console.WriteLine($"  {pkg.Description}");
            }
        });

        return command;
    }
}
