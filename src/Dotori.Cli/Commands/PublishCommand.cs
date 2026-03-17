using System.CommandLine;
using System.Security.Cryptography;
using Dotori.Core.Parsing;
using Dotori.PackageManager;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class PublishCommandFactory
{
    public static Command Create()
    {
        var command = new Command("publish", "Publish a package to the registry");

        var projectOption  = new Option<string?>("--project")  { Description = "Path to .dotori or project directory" };
        var registryOption = new Option<string?>("--registry") { Description = "Registry URL" };
        var prefixOption   = new Option<string?>("--prefix")   { Description = "Local directory to deploy the package into (instead of registry)" };
        var dryRunOption   = new Option<bool>("--dry-run")     { Description = "Validate without uploading" };

        command.Add(projectOption);
        command.Add(registryOption);
        command.Add(prefixOption);
        command.Add(dryRunOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var projectArg = parseResult.GetValue(projectOption);
            var regUrl     = parseResult.GetValue(registryOption);
            var prefix     = parseResult.GetValue(prefixOption);
            var dryRun     = parseResult.GetValue(dryRunOption);

            if (regUrl is not null && prefix is not null)
            {
                Console.Error.WriteLine("error: --registry and --prefix are mutually exclusive");
                return;
            }

            // .dotori 파일 찾기
            var dotoriPath = FindDotoriFile(projectArg);
            if (dotoriPath is null)
            {
                Console.Error.WriteLine("error: no .dotori file found");
                return;
            }
            var projectDir = Path.GetDirectoryName(dotoriPath)!;

            // 파싱 및 package {} 블록 확인
            DotoriFile file;
            try
            {
                var src = await File.ReadAllTextAsync(dotoriPath, ct);
                var tokens = new Lexer(src, dotoriPath).Tokenize();
                file = new Parser(tokens, dotoriPath).ParseFile();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"error: parse failed: {ex.Message}");
                return;
            }

            if (file.Package is null)
            {
                Console.Error.WriteLine("error: .dotori has no 'package { }' block — cannot publish");
                return;
            }

            var pkgName    = file.Package.Name ?? "";
            var pkgVersion = file.Package.Version ?? "";

            if (string.IsNullOrEmpty(pkgName) || string.IsNullOrEmpty(pkgVersion))
            {
                Console.Error.WriteLine("error: package block must have 'name' and 'version'");
                return;
            }

            Console.WriteLine($"Publishing {pkgName} v{pkgVersion}...");

            // 수집할 파일들
            var filesToPack = CollectFiles(projectDir, file);

            Console.WriteLine($"  Files: {filesToPack.Count}");

            // tar.gz 아카이브 생성
            var tmpPath = Path.GetTempFileName() + ".dotori-pkg";
            try
            {
                await PackageInstaller.PackAsync(projectDir, tmpPath, filesToPack, ct);
                var hash = await PackageInstaller.ComputeHashAsync(tmpPath, ct);
                var size = new FileInfo(tmpPath).Length;

                Console.WriteLine($"  Archive: {size / 1024.0:F1} KB, {hash}");

                if (dryRun)
                {
                    Console.WriteLine("[dry-run] Skipping upload");
                    return;
                }

                if (prefix is not null)
                {
                    // 로컬 prefix 디렉토리에 배포
                    var destDir = Path.Combine(Path.GetFullPath(prefix), pkgName, pkgVersion);
                    Directory.CreateDirectory(destDir);
                    var destFile = Path.Combine(destDir, $"{pkgName}-{pkgVersion}.dotori-pkg");
                    File.Copy(tmpPath, destFile, overwrite: true);
                    Console.WriteLine($"Successfully published {pkgName} v{pkgVersion} → {destFile}");
                }
                else
                {
                    // 레지스트리에 업로드
                    var regConfig = DotoriConfigManager.Load().GetRegistry(regUrl);
                    if (regConfig.Token is null)
                    {
                        Console.Error.WriteLine($"error: not logged in. Run 'dotori login' first");
                        return;
                    }

                    using var client = new RegistryClient(regConfig.Url, regConfig.Token);
                    await using var archiveStream = new FileStream(tmpPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize: 81920, useAsync: true);
                    await client.PublishAsync(archiveStream, $"{pkgName}-{pkgVersion}.dotori-pkg", ct);

                    Console.WriteLine($"Successfully published {pkgName} v{pkgVersion}!");
                }
            }
            finally
            {
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
            }
        });

        return command;
    }

    private static string? FindDotoriFile(string? projectArg)
    {
        if (projectArg is not null)
        {
            if (File.Exists(projectArg) && projectArg.EndsWith(".dotori")) return projectArg;
            var candidate = Path.Combine(projectArg, ".dotori");
            if (File.Exists(candidate)) return candidate;
            return null;
        }

        var current = Directory.GetCurrentDirectory();
        while (true)
        {
            var path = Path.Combine(current, ".dotori");
            if (File.Exists(path)) return path;
            var parent = Directory.GetParent(current)?.FullName;
            if (parent is null || parent == current) return null;
            current = parent;
        }
    }

    private static List<string> CollectFiles(string projectDir, DotoriFile file)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 필수 파일
        var dotori = Path.Combine(projectDir, ".dotori");
        if (File.Exists(dotori)) files.Add(dotori);

        var license = Path.Combine(projectDir, "LICENSE");
        if (File.Exists(license)) files.Add(license);

        // sources glob에서 수집 (헤더만 포함하는 경우도 있으므로)
        if (file.Project is not null)
        {
            foreach (var item in file.Project.Items.OfType<SourcesBlock>())
            {
                foreach (var src in item.Items.Where(i => i.IsInclude))
                {
                    foreach (var match in Dotori.Core.Build.GlobExpander.Expand(projectDir, [src.Glob], []))
                        files.Add(match);
                }
            }

            foreach (var item in file.Project.Items.OfType<HeadersBlock>())
            {
                foreach (var hdr in item.Items)
                {
                    var hdrDir = Path.Combine(projectDir, hdr.Path);
                    if (Directory.Exists(hdrDir))
                    {
                        foreach (var f in Directory.EnumerateFiles(hdrDir, "*", SearchOption.AllDirectories))
                            files.Add(f);
                    }
                }
            }
        }

        // 제외: .dotori-cache, .git, *.obj, *.a, *.lib
        return files.Where(f =>
        {
            var rel = Path.GetRelativePath(projectDir, f);
            return !rel.StartsWith(".dotori-cache") &&
                   !rel.StartsWith(".git") &&
                   !f.EndsWith(".obj", StringComparison.OrdinalIgnoreCase) &&
                   !f.EndsWith(".lib", StringComparison.OrdinalIgnoreCase) &&
                   !f.EndsWith(".a",   StringComparison.OrdinalIgnoreCase);
        }).ToList();
    }
}
