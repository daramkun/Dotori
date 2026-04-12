using System.Diagnostics;
using System.Text;

namespace Dotori.Core.Debugger;

/// <summary>
/// Launches executables under debuggers with platform-specific configuration.
/// </summary>
public static class DebuggerLauncher
{
    /// <summary>
    /// Launch an executable under a debugger.
    /// </summary>
    /// <param name="debugger">Debugger to use</param>
    /// <param name="executablePath">Path to executable to debug</param>
    /// <param name="executableArgs">Arguments to pass to executable</param>
    /// <param name="debuggerArgs">Additional debugger-specific arguments</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Exit code from debugger session</returns>
    public static async Task<int> LaunchAsync(
        DebuggerInfo          debugger,
        string                executablePath,
        IReadOnlyList<string> executableArgs,
        IReadOnlyList<string> debuggerArgs,
        CancellationToken     ct = default)
    {
        var (debuggerExe, arguments) = BuildDebuggerCommandLine(
            debugger, executablePath, executableArgs, debuggerArgs);

        var psi = new ProcessStartInfo(debuggerExe, arguments)
        {
            UseShellExecute = false,
            // Debugger needs terminal control - don't redirect streams
            RedirectStandardOutput = false,
            RedirectStandardError  = false,
            CreateNoWindow         = false,
        };

        try
        {
            using var proc = Process.Start(psi);
            if (proc is null)
            {
                throw new InvalidOperationException($"Failed to start debugger: {debuggerExe}");
            }

            await proc.WaitForExitAsync(ct);
            return proc.ExitCode;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Error launching debugger: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Build the complete command line for launching a debugger.
    /// </summary>
    private static (string executable, string arguments) BuildDebuggerCommandLine(
        DebuggerInfo          debugger,
        string                executablePath,
        IReadOnlyList<string> executableArgs,
        IReadOnlyList<string> debuggerArgs)
    {
        var sb = new StringBuilder();

        switch (debugger.Kind)
        {
            case DebuggerKind.Lldb:
                // lldb [debugger-args] -- <executable> <args...>
                AppendArgs(sb, debuggerArgs);
                sb.Append(" -- ");
                AppendQuoted(sb, executablePath);
                AppendArgs(sb, executableArgs);
                break;

            case DebuggerKind.Gdb:
                // gdb [debugger-args] --args <executable> <args...>
                AppendArgs(sb, debuggerArgs);
                sb.Append(" --args ");
                AppendQuoted(sb, executablePath);
                AppendArgs(sb, executableArgs);
                break;

            case DebuggerKind.VsDbg:
                // devenv.exe /DebugExe <executable> <args...>
                sb.Append("/DebugExe ");
                AppendQuoted(sb, executablePath);
                AppendArgs(sb, executableArgs);
                break;

            case DebuggerKind.WinDbg:
            case DebuggerKind.Cdb:
                // windbg.exe [debugger-args] <executable> <args...>
                // cdb.exe [debugger-args] <executable> <args...>
                AppendArgs(sb, debuggerArgs);
                if (debuggerArgs.Count > 0) sb.Append(' ');
                AppendQuoted(sb, executablePath);
                AppendArgs(sb, executableArgs);
                break;

            default:
                throw new NotSupportedException($"Debugger kind {debugger.Kind} is not supported");
        }

        return (debugger.ExecutablePath, sb.ToString().Trim());
    }

    /// <summary>Append a list of arguments with proper quoting</summary>
    private static void AppendArgs(StringBuilder sb, IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            sb.Append(' ');
            AppendQuoted(sb, arg);
        }
    }

    /// <summary>Append a single argument with quotes if needed</summary>
    private static void AppendQuoted(StringBuilder sb, string arg)
    {
        // Quote if contains spaces or special characters
        if (arg.Contains(' ') || arg.Contains('"'))
        {
            sb.Append('"');
            sb.Append(arg.Replace("\"", "\\\""));
            sb.Append('"');
        }
        else
        {
            sb.Append(arg);
        }
    }
}
