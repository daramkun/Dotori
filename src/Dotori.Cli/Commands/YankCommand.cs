using System.CommandLine;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class YankCommandFactory
{
    public static Command CreateYank()
    {
        var command = new Command("yank", "Yank a specific version (prevent new use)");
        AddCommon(command, true);
        return command;
    }

    public static Command CreateUnyank()
    {
        var command = new Command("unyank", "Un-yank a previously yanked version");
        AddCommon(command, false);
        return command;
    }

    private static void AddCommon(Command command, bool yank)
    {
        var pkgArg        = new Argument<string>("package")    { Description = "Package version (owner/name@version)" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        command.Add(pkgArg); command.Add(registryOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var pkg = parseResult.GetValue(pkgArg)!;
            var (owner, name, version) = SplitPackageVersion(pkg);

            var config = DotoriConfigManager.Load();
            var reg = config.GetRegistry(parseResult.GetValue(registryOption));
            if (reg.Token is null)
            {
                Console.Error.WriteLine("error: not logged in. Run 'dotori login' first");
                return;
            }

            using var http = new HttpClient { BaseAddress = new Uri(reg.Url.TrimEnd('/') + "/api/v1/") };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("dotori/1.0");
            http.DefaultRequestHeaders.Authorization = new("Bearer", reg.Token);

            HttpResponseMessage resp;
            if (yank)
                resp = await http.DeleteAsync($"packages/{owner}/{name}/{version}/yank", ct);
            else
                resp = await http.PostAsync($"packages/{owner}/{name}/{version}/unyank", null, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                Console.Error.WriteLine($"error ({(int)resp.StatusCode}): {body}");
                return;
            }

            var action = yank ? "yanked" : "un-yanked";
            Console.WriteLine($"Successfully {action} {owner}/{name}@{version}");
        });
    }

    private static (string owner, string name, string version) SplitPackageVersion(string pkg)
    {
        var atIdx = pkg.LastIndexOf('@');
        if (atIdx < 0) throw new ArgumentException($"Package must include version: owner/name@version");
        var version = pkg[(atIdx + 1)..];
        var nameAndOwner = pkg[..atIdx];
        var slashIdx = nameAndOwner.IndexOf('/');
        if (slashIdx < 0) throw new ArgumentException($"Package must be in 'owner/name@version' format");
        return (nameAndOwner[..slashIdx], nameAndOwner[(slashIdx + 1)..], version);
    }
}
