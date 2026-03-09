using System.CommandLine;

namespace Dotori.Cli.Commands;

internal static class BuildCommandFactory
{
    public static Command Create()
    {
        var command = new Command("build", "Build the project");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var allOption = new Option<bool>("--all") { Description = "Build all projects without prompt" };
        var releaseOption = new Option<bool>("--release") { Description = "Build in Release configuration" };
        var targetOption = new Option<string?>("--target") { Description = "Build target (e.g. windows-x64, linux-x64)" };
        var compilerOption = new Option<string?>("--compiler") { Description = "Compiler to use (msvc, clang)" };
        var runtimeLinkOption = new Option<string?>("--runtime-link") { Description = "Runtime link mode (static, dynamic)" };
        var libcOption = new Option<string?>("--libc") { Description = "C runtime library (glibc, musl)" };
        var stdlibOption = new Option<string?>("--stdlib") { Description = "C++ standard library (libc++, libstdc++)" };
        var jobsOption = new Option<int?>("--jobs") { Description = "Number of parallel jobs" };
        var noModulesOption = new Option<bool>("--no-modules") { Description = "Disable C++ Modules support" };
        var fileOption = new Option<string?>("--file") { Description = "Build a single source file" };
        var noLinkOption = new Option<bool>("--no-link") { Description = "Compile only, do not link" };
        var noUnityOption = new Option<bool>("--no-unity") { Description = "Ignore unity build for --file" };

        command.Add(projectOption);
        command.Add(allOption);
        command.Add(releaseOption);
        command.Add(targetOption);
        command.Add(compilerOption);
        command.Add(runtimeLinkOption);
        command.Add(libcOption);
        command.Add(stdlibOption);
        command.Add(jobsOption);
        command.Add(noModulesOption);
        command.Add(fileOption);
        command.Add(noLinkOption);
        command.Add(noUnityOption);

        command.SetAction((parseResult) =>
        {
            Console.Error.WriteLine("dotori build: not yet implemented");
            return 1;
        });

        return command;
    }
}
