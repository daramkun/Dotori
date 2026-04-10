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
        new("cxx11",
            "C++11",
            "-std=c++11", "/std:c++11",
            """
            #include <memory>
            #include <functional>
            #include <vector>
            int main() {
                auto p = std::make_shared<int>(42);
                std::vector<int> v = {1, 2, 3};
                auto fn = [](int x) { return x * 2; };
                (void)p; (void)v; (void)fn;
            }
            """),

        new("cxx14",
            "C++14",
            "-std=c++14", "/std:c++14",
            """
            #include <memory>
            template<typename T>
            constexpr T square(T x) { return x * x; }
            int main() {
                auto p = std::make_unique<int>(42);
                constexpr auto s = square(7);
                (void)p; (void)s;
            }
            """),

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

        // ── C++17 language features ───────────────────────────────────────────
        new("structured_bindings",
            "Structured bindings (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <utility>
            int main() {
                auto p = std::pair{1, 2.0};
                auto [a, b] = p;
                return (int)(a + b) - 3;
            }
            """),

        new("if_constexpr",
            "if constexpr (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <type_traits>
            template<typename T>
            auto negate_if_signed(T t) {
                if constexpr (std::is_signed_v<T>) return -t;
                else return t;
            }
            int main() { return negate_if_signed(-1) - 1; }
            """),

        new("fold_expressions",
            "Fold expressions (C++17)",
            "-std=c++17", "/std:c++17",
            """
            template<typename... Ts>
            auto sum(Ts... ts) { return (ts + ...); }
            int main() { return sum(1, 2, 3) - 6; }
            """),

        new("filesystem",
            "std::filesystem (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <filesystem>
            int main() {
                auto p = std::filesystem::current_path();
                (void)p;
            }
            """),

        new("any",
            "std::any (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <any>
            int main() {
                std::any a = 42;
                return std::any_cast<int>(a) - 42;
            }
            """),

        new("string_view",
            "std::string_view (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <string_view>
            constexpr std::string_view hello = "hello";
            static_assert(hello.size() == 5);
            int main() {}
            """),

        new("parallel_algorithms",
            "Parallel algorithms / execution policies (C++17)",
            "-std=c++17", "/std:c++17",
            """
            #include <algorithm>
            #include <execution>
            #include <vector>
            int main() {
                std::vector<int> v = {3,1,4,1,5};
                std::sort(std::execution::par, v.begin(), v.end());
                (void)v;
            }
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

        new("using_enum",
            "using enum (C++20)",
            "-std=c++20", "/std:c++20",
            """
            enum class Color { Red, Green, Blue };
            int main() {
                using enum Color;
                auto c = Red;
                return c == Color::Red ? 0 : 1;
            }
            """),

        new("char8_t",
            "char8_t (C++20)",
            "-std=c++20", "/std:c++20",
            """
            int main() {
                char8_t c = u8'a';
                return c == u8'a' ? 0 : 1;
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

        new("bit_cast",
            "std::bit_cast (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <bit>
            #include <cstdint>
            int main() {
                float f = 0.0f;
                auto i = std::bit_cast<std::uint32_t>(f);
                return i == 0 ? 0 : 1;
            }
            """),

        new("source_location",
            "std::source_location (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <source_location>
            int main() {
                auto loc = std::source_location::current();
                (void)loc;
            }
            """),

        new("is_constant_evaluated",
            "std::is_constant_evaluated (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <type_traits>
            constexpr int f() {
                if (std::is_constant_evaluated()) return 1;
                return 0;
            }
            static_assert(f() == 1);
            int main() {}
            """),

        new("semaphore",
            "std::semaphore (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <semaphore>
            int main() {
                std::counting_semaphore<10> sem(1);
                sem.acquire();
                sem.release();
            }
            """),

        new("latch",
            "std::latch (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <latch>
            int main() {
                std::latch l(1);
                l.count_down();
            }
            """),

        new("barrier",
            "std::barrier (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <barrier>
            int main() {
                std::barrier b(1);
                b.arrive_and_wait();
            }
            """),

        new("atomic_wait",
            "std::atomic::wait/notify (C++20)",
            "-std=c++20", "/std:c++20",
            """
            #include <atomic>
            int main() {
                std::atomic<int> a(0);
                a.store(1);
                a.notify_one();
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

        new("flat_set",
            "std::flat_set (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <flat_set>
            int main() {
                std::flat_set<int> s = {3,1,2};
                return *s.begin() - 1;
            }
            """),

        new("mdspan",
            "std::mdspan (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <mdspan>
            int main() {
                int data[6] = {};
                auto m = std::mdspan(data, 2, 3);
                (void)m;
            }
            """),

        new("stacktrace",
            "std::stacktrace (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <stacktrace>
            int main() {
                auto st = std::stacktrace::current();
                (void)st;
            }
            """),

        new("generator",
            "std::generator (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <generator>
            std::generator<int> iota(int n) {
                for (int i = 0; i < n; ++i) co_yield i;
            }
            int main() {
                for (auto v : iota(3)) (void)v;
            }
            """),

        new("views_zip",
            "std::views::zip (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <ranges>
            #include <vector>
            int main() {
                std::vector<int> a = {1,2,3};
                std::vector<int> b = {4,5,6};
                auto z = std::views::zip(a, b);
                (void)z;
            }
            """),

        new("ranges_fold",
            "std::ranges::fold_left (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <algorithm>
            #include <vector>
            #include <functional>
            int main() {
                std::vector<int> v = {1,2,3,4,5};
                auto s = std::ranges::fold_left(v, 0, std::plus<>{});
                return s - 15;
            }
            """),

        new("if_consteval",
            "if consteval (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            constexpr int f(int x) {
                if consteval { return x * 2; }
                return x;
            }
            static_assert(f(3) == 6);
            int main() {}
            """),

        new("deducing_this",
            "Deducing this / explicit object parameter (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            struct Counter {
                int value = 0;
                Counter& increment(this Counter& self) { ++self.value; return self; }
            };
            int main() {
                Counter c;
                c.increment().increment();
                return c.value - 2;
            }
            """),

        new("multidim_subscript",
            "Multidimensional subscript operator (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            struct Grid {
                int data[3][3] = {};
                int& operator[](int i, int j) { return data[i][j]; }
            };
            int main() {
                Grid g;
                g[1,2] = 5;
                return g[1,2] - 5;
            }
            """),

        new("static_call_op",
            "static operator() (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            struct Mul {
                static int operator()(int a, int b) { return a * b; }
            };
            int main() { return Mul{}(3, 4) - 12; }
            """),

        new("size_t_literal",
            "Literal suffix for size_t (C++23)",
            "-std=c++23", "/std:c++latest",
            """
            #include <cstddef>
            #include <type_traits>
            int main() {
                auto sz = 42uz;
                static_assert(std::is_same_v<decltype(sz), std::size_t>);
            }
            """),

        // ── C++26 language features ───────────────────────────────────────────
        new("pack_indexing",
            "Pack indexing (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            template<typename... Ts>
            using First = Ts...[0];
            int main() { First<int, double> x = 42; (void)x; }
            """),

        new("embed",
            "#embed (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #if !__has_embed(__FILE__)
            #error embed not supported
            #endif
            int main() {}
            """),

        // ── C++26 library features ────────────────────────────────────────────
        new("function_ref",
            "std::function_ref (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #include <functional>
            int add(int a, int b) { return a + b; }
            int main() {
                std::function_ref<int(int,int)> f = add;
                return f(1, 2) - 3;
            }
            """),

        new("saturation_arithmetic",
            "Saturation arithmetic (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #include <numeric>
            #include <climits>
            int main() {
                return std::add_sat(INT_MAX, 1) == INT_MAX ? 0 : 1;
            }
            """),

        new("inplace_vector",
            "std::inplace_vector (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #include <inplace_vector>
            int main() {
                std::inplace_vector<int, 4> v = {1, 2, 3};
                return v[0] - 1;
            }
            """),

        new("simd",
            "<simd> data-parallel types (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #include <simd>
            int main() {
                std::simd<int> v(0);
                (void)v;
            }
            """),

        new("linalg",
            "<linalg> linear algebra (C++26)",
            "-std=c++26", "/std:c++latest",
            """
            #include <linalg>
            int main() {}
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
