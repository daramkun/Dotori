using System.Diagnostics;

namespace Dotori.Core.Toolchain;

/// <summary>
/// Describes a single C++ language or library feature to probe.
/// </summary>
public sealed record CxxFeature(
    /// <summary>Machine-readable identifier, e.g. "cxx20".</summary>
    string Id,
    /// <summary>Human-readable label shown in the output table.</summary>
    string Description,
    /// <summary>Minimum C++ standard flag for Clang/GCC, e.g. "-std=c++20". Null = no flag.</summary>
    string? ClangStdFlag,
    /// <summary>Minimum C++ standard flag for MSVC, e.g. "/std:c++20". Null = no flag.</summary>
    string? MsvcStdFlag,
    /// <summary>Small C++ source snippet to compile. Must compile to an object file cleanly.</summary>
    string Snippet
);

/// <summary>
/// Probes whether a toolchain supports individual C++ features by attempting to compile
/// a small test snippet and checking the exit code.
/// </summary>
public static class CxxFeatureProber
{
    // ─── Well-known features ──────────────────────────────────────────────────

    public static readonly IReadOnlyList<CxxFeature> KnownFeatures =
    [
        // ── Standard version probes ──────────────────────────────────────────
        new("cxx17",
            "C++17",
            "-std=c++17", "/std:c++17",
            """
            #include <optional>
            #include <string_view>
            #include <variant>
            int main() {
                std::optional<int> o = 42;
                std::string_view sv = "hello";
                std::variant<int, double> v = 3.14;
                (void)o; (void)sv; (void)v;
            }
            """),

        new("cxx20",
            "C++20",
            "-std=c++20", "/std:c++20",
            """
            #include <concepts>
            #include <span>
            template<typename T>
            concept Numeric = std::integral<T> || std::floating_point<T>;
            int main() {
                int arr[] = {1, 2, 3};
                std::span<int> s(arr);
                (void)s;
            }
            """),

        new("cxx23",
            "C++23",
            "-std=c++23", "/std:c++latest",
            """
            #include <expected>
            int main() {
                std::expected<int, int> e = 42;
                (void)e;
            }
            """),

        new("cxx26",
            "C++26 (experimental)",
            "-std=c++26", "/std:c++latest",
            """
            #include <type_traits>
            static_assert(__cplusplus >= 202400L);
            int main() {}
            """),

        // ── C++20 language features ───────────────────────────────────────────
        new("concepts",
            "Concepts (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <concepts>
            template<std::integral T>
            T add(T a, T b) { return a + b; }
            int main() { return add(1, 2) - 3; }
            """),

        new("coroutines",
            "Coroutines (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <coroutine>
            struct Task {
                struct promise_type {
                    Task get_return_object() { return {}; }
                    std::suspend_never initial_suspend() { return {}; }
                    std::suspend_never final_suspend() noexcept { return {}; }
                    void return_void() {}
                    void unhandled_exception() {}
                };
            };
            Task coro() { co_return; }
            int main() { coro(); }
            """),

        new("ranges",
            "Ranges (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <ranges>
            #include <vector>
            int main() {
                std::vector<int> v = {3,1,4,1,5};
                auto even = v | std::views::filter([](int x){ return x % 2 == 0; });
                (void)even;
            }
            """),

        new("modules",
            "C++ Modules (C++20)",
            "-std=c++20", "/std:c++20",
            """
            export module probe;
            export int answer() { return 42; }
            """),

        new("consteval",
            "consteval (C++20)",
            "-std=c++20", "/std:c++20",
            """
            consteval int square(int x) { return x * x; }
            static_assert(square(4) == 16);
            int main() {}
            """),

        new("constinit",
            "constinit (C++20)",
            "-std=c++20", "/std:c++20",
            """
            constinit int g = 42;
            int main() { return g - 42; }
            """),

        new("three_way_compare",
            "Three-way comparison / <=> (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <compare>
            int main() {
                auto r = (1 <=> 2);
                return r < 0 ? 0 : 1;
            }
            """),

        new("designated_init",
            "Designated initializers (C++20)",
            "-std=c++20", "/std:c++20",
            """
            struct Point { int x; int y; };
            int main() {
                Point p = { .x = 1, .y = 2 };
                return p.x + p.y - 3;
            }
            """),

        // ── C++20 library features ────────────────────────────────────────────
        new("format",
            "std::format (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <format>
            #include <string>
            int main() {
                std::string s = std::format("Hello, {}!", 42);
                (void)s;
            }
            """),

        new("span",
            "std::span (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <span>
            int main() {
                int arr[] = {1,2,3};
                std::span<int> s(arr);
                return (int)s.size() - 3;
            }
            """),

        new("jthread",
            "std::jthread (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <thread>
            int main() {
                std::jthread t([]{ });
            }
            """),

        // ── C++23 library features ────────────────────────────────────────────
        new("expected",
            "std::expected (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <expected>
            std::expected<int,int> safe_div(int a, int b) {
                if (b == 0) return std::unexpected(1);
                return a / b;
            }
            int main() { return safe_div(4,2).value() - 2; }
            """),

        new("print",
            "std::print (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <print>
            int main() { std::print("probe\n"); }
            """),

        new("flat_map",
            "std::flat_map (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <flat_map>
            int main() {
                std::flat_map<int,int> m;
                m[1] = 2;
                return m[1] - 2;
            }
            """),
    ];

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Probe a single feature against the given toolchain.
    /// Returns true if compilation succeeds.
    /// </summary>
    public static bool Probe(ToolchainInfo toolchain, CxxFeature feature)
    {
        if (feature.Id == "modules")
            return ProbeModules(toolchain);

        var (src, obj) = WriteTempSource(feature.Snippet);
        try
        {
            var (exe, args) = BuildArgs(toolchain, feature, src, obj);
            return RunCompile(exe, args);
        }
        finally
        {
            TryDelete(src);
            TryDelete(obj);
        }
    }

    // ─── Internals ────────────────────────────────────────────────────────────

    private static bool ProbeModules(ToolchainInfo toolchain)
    {
        var feature = KnownFeatures.First(f => f.Id == "modules");
        var (src, _) = WriteTempSource(feature.Snippet, ".cppm");
        var outExt  = toolchain.Kind == CompilerKind.Msvc ? ".ifc" : ".pcm";
        var outFile = Path.ChangeExtension(src, outExt);
        try
        {
            string exe  = toolchain.CompilerPath;
            string args = toolchain.Kind == CompilerKind.Msvc
                ? $"/nologo /std:c++20 /experimental:module /interface /c \"{src}\" \"/Fo{outFile}\""
                : $"-std=c++20 --precompile -x c++-module \"{src}\" -o \"{outFile}\"";

            return RunCompile(exe, args);
        }
        finally
        {
            TryDelete(src);
            TryDelete(outFile);
        }
    }

    private static (string exe, string args) BuildArgs(
        ToolchainInfo toolchain, CxxFeature feature, string src, string obj)
    {
        string exe = toolchain.CompilerPath;
        string args;

        if (toolchain.Kind == CompilerKind.Msvc)
        {
            var stdFlag = feature.MsvcStdFlag ?? string.Empty;
            args = $"/nologo {stdFlag} /c \"{src}\" \"/Fo{obj}\"";
        }
        else
        {
            var stdFlag = feature.ClangStdFlag ?? string.Empty;
            args = $"{stdFlag} -c \"{src}\" -o \"{obj}\"";
        }

        return (exe, args);
    }

    private static (string src, string obj) WriteTempSource(string snippet, string ext = ".cpp")
    {
        var tmp  = Path.GetTempPath();
        var name = Path.GetRandomFileName();
        var src  = Path.Combine(tmp, Path.ChangeExtension(name, ext));
        var obj  = Path.Combine(tmp, Path.ChangeExtension(name, ".o"));
        File.WriteAllText(src, snippet);
        return (src, obj);
    }

    private static bool RunCompile(string exe, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };
            using var proc = Process.Start(psi)!;
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void TryDelete(string? path)
    {
        try { if (path is not null && File.Exists(path)) File.Delete(path); } catch { }
    }
}
