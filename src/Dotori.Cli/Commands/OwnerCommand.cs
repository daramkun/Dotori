using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class OwnerCommandFactory
{
    public static Command Create()
    {
        var command = new Command("owner", "Manage package ownership and collaborators");
        command.Add(CreateListCommand());
        command.Add(CreateAddCommand());
        command.Add(CreateRemoveCommand());
        command.Add(CreateTransferCommand());
        return command;
    }

    private static Command CreateListCommand()
    {
        var cmd = new Command("list", "List owners and collaborators");
        var pkgArg        = new Argument<string>("package")    { Description = "Package name (owner/name)" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        cmd.Add(pkgArg); cmd.Add(registryOption);

        cmd.SetAction(async (parseResult, ct) =>
        {
            var (owner, name) = SplitPackage(parseResult.GetValue(pkgArg)!);
            using var http = BuildClient(parseResult.GetValue(registryOption));

            var resp = await http.GetAsync($"packages/{owner}/{name}/owners", ct);
            if (!resp.IsSuccessStatusCode) { PrintError(resp); return; }

            var list = await resp.Content.ReadFromJsonAsync(OwnerJsonContext.Default.ListCollaboratorDto, ct) ?? [];
            Console.WriteLine($"{"ROLE",-14} USERNAME");
            Console.WriteLine(new string('-', 30));
            foreach (var c in list)
                Console.WriteLine($"{c.Role,-14} {c.Username}");
        });
        return cmd;
    }

    private static Command CreateAddCommand()
    {
        var cmd = new Command("add", "Add a collaborator");
        var pkgArg        = new Argument<string>("package")    { Description = "Package name (owner/name)" };
        var userArg       = new Argument<string>("username")   { Description = "Username to add" };
        var roleOption    = new Option<string?>("--role")      { Description = "Role: collaborator (default) or owner" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        cmd.Add(pkgArg); cmd.Add(userArg); cmd.Add(roleOption); cmd.Add(registryOption);

        cmd.SetAction(async (parseResult, ct) =>
        {
            var (owner, name) = SplitPackage(parseResult.GetValue(pkgArg)!);
            var username = parseResult.GetValue(userArg)!;
            var role     = parseResult.GetValue(roleOption) ?? "collaborator";
            using var http = BuildClient(parseResult.GetValue(registryOption));

            var resp = await http.PostAsJsonAsync($"packages/{owner}/{name}/owners",
                new CollaboratorRequest { Username = username, Role = role },
                OwnerJsonContext.Default.CollaboratorRequest, ct);
            if (!resp.IsSuccessStatusCode) { PrintError(resp); return; }
            Console.WriteLine($"Added {username} as {role} of {owner}/{name}");
        });
        return cmd;
    }

    private static Command CreateRemoveCommand()
    {
        var cmd = new Command("remove", "Remove a collaborator");
        var pkgArg        = new Argument<string>("package")    { Description = "Package name (owner/name)" };
        var userArg       = new Argument<string>("username")   { Description = "Username to remove" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        cmd.Add(pkgArg); cmd.Add(userArg); cmd.Add(registryOption);

        cmd.SetAction(async (parseResult, ct) =>
        {
            var (owner, name) = SplitPackage(parseResult.GetValue(pkgArg)!);
            var username = parseResult.GetValue(userArg)!;
            using var http = BuildClient(parseResult.GetValue(registryOption));

            var resp = await http.DeleteAsync($"packages/{owner}/{name}/owners/{username}", ct);
            if (!resp.IsSuccessStatusCode) { PrintError(resp); return; }
            Console.WriteLine($"Removed {username} from {owner}/{name}");
        });
        return cmd;
    }

    private static Command CreateTransferCommand()
    {
        var cmd = new Command("transfer", "Transfer package ownership");
        var pkgArg        = new Argument<string>("package")    { Description = "Package name (owner/name)" };
        var newOwnerArg   = new Argument<string>("new-owner")  { Description = "New owner username" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        cmd.Add(pkgArg); cmd.Add(newOwnerArg); cmd.Add(registryOption);

        cmd.SetAction(async (parseResult, ct) =>
        {
            var (owner, name) = SplitPackage(parseResult.GetValue(pkgArg)!);
            var newOwner = parseResult.GetValue(newOwnerArg)!;
            using var http = BuildClient(parseResult.GetValue(registryOption));

            Console.Write($"Transfer {owner}/{name} to {newOwner}? [y/N] ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (confirm is not "y" and not "yes") { Console.WriteLine("Cancelled."); return; }

            var resp = await http.PostAsJsonAsync($"packages/{owner}/{name}/owners/transfer",
                new TransferRequest { NewOwner = newOwner },
                OwnerJsonContext.Default.TransferRequest, ct);
            if (!resp.IsSuccessStatusCode) { PrintError(resp); return; }
            Console.WriteLine($"Ownership of {owner}/{name} transferred to {newOwner}");
        });
        return cmd;
    }

    private static HttpClient BuildClient(string? registryUrl)
    {
        var config = DotoriConfigManager.Load();
        var reg = config.GetRegistry(registryUrl);
        var http = new HttpClient { BaseAddress = new Uri(reg.Url.TrimEnd('/') + "/api/v1/") };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("dotori/1.0");
        if (reg.Token is not null)
            http.DefaultRequestHeaders.Authorization = new("Bearer", reg.Token);
        return http;
    }

    private static (string owner, string name) SplitPackage(string pkg)
    {
        var idx = pkg.IndexOf('/');
        if (idx < 0) throw new ArgumentException($"Package must be in 'owner/name' format: {pkg}");
        return (pkg[..idx], pkg[(idx + 1)..]);
    }

    private static void PrintError(HttpResponseMessage resp)
    {
        var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        Console.Error.WriteLine($"error ({(int)resp.StatusCode}): {body}");
    }
}

// DTOs
internal sealed class CollaboratorDto
{
    [JsonPropertyName("username")] public string Username { get; init; } = "";
    [JsonPropertyName("role")]     public string Role { get; init; } = "";
    [JsonPropertyName("addedAt")]  public DateTime AddedAt { get; init; }
}

internal sealed class CollaboratorRequest
{
    [JsonPropertyName("username")] public string Username { get; init; } = "";
    [JsonPropertyName("role")]     public string Role { get; init; } = "collaborator";
}

internal sealed class TransferRequest
{
    [JsonPropertyName("newOwner")] public string NewOwner { get; init; } = "";
}

[JsonSerializable(typeof(List<CollaboratorDto>))]
[JsonSerializable(typeof(CollaboratorRequest))]
[JsonSerializable(typeof(TransferRequest))]
internal partial class OwnerJsonContext : JsonSerializerContext { }
