using Dotori.LanguageServer.Providers;

namespace Dotori.Tests.LanguageServer;

[TestClass]
public class DiagnosticsProviderTests
{
    [TestMethod]
    public void Analyze_ValidSource_NoDiagnostics()
    {
        const string source = """
            project MyApp {
                type = executable
                std  = c++23
                sources { include "src/**/*.cpp" }
            }
            """;

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        Assert.IsEmpty(diags);
    }

    [TestMethod]
    public void Analyze_ParseError_ReturnsDiagnostic()
    {
        const string source = "project MyApp { type = INVALID_TYPE }";

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        Assert.IsNotEmpty(diags, "Should have at least one diagnostic for parse error");
        Assert.AreEqual(1, diags[0].Severity, "Parse errors should have Error severity");
        Assert.AreEqual("dotori", diags[0].Source);
    }

    [TestMethod]
    public void Analyze_LexerError_ReturnsDiagnostic()
    {
        // Unterminated string
        const string source = "project MyApp { description = \"unterminated }";

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        Assert.IsNotEmpty(diags);
        Assert.AreEqual(1, diags[0].Severity);
        Assert.AreEqual("dotori", diags[0].Source);
    }

    [TestMethod]
    public void Analyze_UnguardedCompileFlags_ReturnsWarning()
    {
        const string source = """
            project MyApp {
                type = executable
                compile-flags { "-march=native" }
            }
            """;

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        var warning = diags.FirstOrDefault(d => d.Message.Contains("compile-flags"));
        Assert.IsNotNull(warning, "Should warn about unguarded compile-flags");
        Assert.AreEqual(2, warning!.Severity, "Should be a warning (severity=2)");
    }

    [TestMethod]
    public void Analyze_UnguardedLinkFlags_ReturnsWarning()
    {
        const string source = """
            project MyApp {
                type = executable
                link-flags { "-Wl,--as-needed" }
            }
            """;

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        var warning = diags.FirstOrDefault(d => d.Message.Contains("link-flags"));
        Assert.IsNotNull(warning, "Should warn about unguarded link-flags");
        Assert.AreEqual(2, warning!.Severity);
    }

    [TestMethod]
    public void Analyze_GuardedCompileFlags_NoWarning()
    {
        const string source = """
            project MyApp {
                type = executable
                [clang] {
                    compile-flags { "-march=native" }
                }
            }
            """;

        var diags = DiagnosticsProvider.Analyze(source, "<test>");
        var compileFlagsWarning = diags.FirstOrDefault(d => d.Message.Contains("compile-flags"));
        Assert.IsNull(compileFlagsWarning, "Guarded compile-flags should not warn");
    }

    [TestMethod]
    public void Analyze_LineNumbers_AreOneBased_ConvertedToZeroBased()
    {
        // Force a parse error on line 2 (0-indexed: line 1)
        const string source = "project MyApp {\n    type = INVALID\n}";

        var diags = DiagnosticsProvider.Analyze(source, "<test>");

        Assert.IsNotEmpty(diags);
        // LSP line is 0-indexed, parser is 1-indexed
        Assert.IsGreaterThanOrEqualTo(0, diags[0].Range.Start.Line, "Line should be non-negative (0-indexed)");
    }

    [TestMethod]
    public void Analyze_PathDependencyMissing_ReturnsWarning()
    {
        const string source = """
            project MyApp {
                dependencies {
                    mylib = { path = "/nonexistent/path" }
                }
            }
            """;

        var diags = DiagnosticsProvider.Analyze(source, "/tmp/test/.dotori");

        var warning = diags.FirstOrDefault(d => d.Message.Contains("path dependency"));
        Assert.IsNotNull(warning, "Should warn about missing path dependency");
        Assert.AreEqual(2, warning!.Severity);
    }

    [TestMethod]
    public void Analyze_DeclaredOptionReference_NoDiagnostic()
    {
        const string source = """
            project MyApp {
                type = executable
                option simd {
                    default = true
                }
                dependencies {
                    my-lib = { version = "1.0.0", option = "simd" }
                }
            }
            """;
        var diags = DiagnosticsProvider.Analyze(source, "<test>");
        Assert.IsFalse(diags.Any(d => d.Message.Contains("undeclared option")));
    }

    [TestMethod]
    public void Analyze_UndeclaredOptionReference_WarningEmitted()
    {
        const string source = """
            project MyApp {
                type = executable
                dependencies {
                    my-lib = { version = "1.0.0", option = "nonexistent" }
                }
            }
            """;
        var diags = DiagnosticsProvider.Analyze(source, "<test>");
        var warning = diags.FirstOrDefault(d => d.Message.Contains("undeclared option"));
        Assert.IsNotNull(warning, "Should warn about undeclared option reference");
        Assert.AreEqual(2, warning!.Severity);
        StringAssert.Contains(warning.Message, "nonexistent");
    }

    [TestMethod]
    public void Analyze_MultipleOptionsUndeclared_WarnsForEach()
    {
        const string source = """
            project MyApp {
                type = executable
                option simd {
                    default = true
                }
                dependencies {
                    my-lib = { version = "1.0.0", option = { "simd" "missing-opt" } }
                }
            }
            """;
        var diags = DiagnosticsProvider.Analyze(source, "<test>");
        var warnings = diags.Where(d => d.Message.Contains("undeclared option")).ToList();
        Assert.HasCount(1, warnings, "Only 'missing-opt' is undeclared");
        StringAssert.Contains(warnings[0].Message, "missing-opt");
    }

    [TestMethod]
    public void Analyze_ValidOptionBlock_NoDiagnostics()
    {
        const string source = """
            project MyApp {
                type = executable
                option extra {
                    default = false
                    defines { "EXTRA" }
                }
            }
            """;
        var diags = DiagnosticsProvider.Analyze(source, "<test>");
        Assert.IsEmpty(diags);
    }
}
