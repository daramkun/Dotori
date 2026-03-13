using Dotori.Core.Model;
using Dotori.Core.Parsing;

namespace Dotori.Tests.Parsing;

[TestClass]
public sealed class ProjectFlattenerTests
{
    private static string FixturePath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", name);

    private static FlatProjectModel Flatten(string fixture, TargetContext ctx)
    {
        var file = DotoriParser.ParseFile(FixturePath(fixture));
        return ProjectFlattener.Flatten(file.Project!, FixturePath(fixture), ctx);
    }

    // ─── Condition matching ─────────────────────────────────────────────────

    [TestMethod]
    public void Flatten_Linux_AppliesLinuxBlock()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            Libc = "glibc", Stdlib = "libstdc++",
        };
        var model = Flatten("lib-with-package.dotori", ctx);
        Assert.Contains("PLATFORM_LINUX", model.Defines);
        Assert.Contains("pthread", model.Links);
    }

    [TestMethod]
    public void Flatten_Windows_AppliesWindowsBlock()
    {
        var ctx = new TargetContext
        {
            Platform = "windows", Config = "debug",
            Compiler = "msvc",    Runtime = "static",
        };
        var model = Flatten("lib-with-package.dotori", ctx);
        Assert.Contains("PLATFORM_WINDOWS", model.Defines);
        Assert.Contains("kernel32", model.Links);
        Assert.DoesNotContain("PLATFORM_LINUX", model.Defines);
    }

    [TestMethod]
    public void Flatten_WindowsRelease_MoreSpecificOverrides()
    {
        var ctx = new TargetContext
        {
            Platform = "windows", Config = "release",
            Compiler = "msvc",    Runtime = "static",
        };
        var model = Flatten("complex-conditions.dotori", ctx);
        // [windows] block applies: PLATFORM_WINDOWS
        Assert.Contains("PLATFORM_WINDOWS", model.Defines);
        // [windows.release] (specificity 2) overrides optimize
        Assert.AreEqual(OptimizeLevel.Full, model.Optimize);
        // [windows.release] adds WIN_RELEASE_ONLY define
        Assert.Contains("WIN_RELEASE_ONLY", model.Defines);
    }

    [TestMethod]
    public void Flatten_Debug_AppliesDebugBlock()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("simple-app.dotori", ctx);
        Assert.Contains("DEBUG", model.Defines);
        Assert.AreEqual(OptimizeLevel.None, model.Optimize);
        Assert.AreEqual(DebugInfoLevel.Full, model.DebugInfo);
    }

    [TestMethod]
    public void Flatten_Release_AppliesReleaseBlock()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "release",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("simple-app.dotori", ctx);
        Assert.Contains("NDEBUG", model.Defines);
        Assert.DoesNotContain("DEBUG", model.Defines);
        Assert.AreEqual(OptimizeLevel.Speed, model.Optimize);
        Assert.IsTrue(model.Lto);
    }

    [TestMethod]
    public void Flatten_WasmEmscripten_FlagsCollected()
    {
        var ctx = new TargetContext
        {
            Platform    = "wasm",  Config  = "release",
            Compiler    = "clang", Runtime = "static",
            WasmBackend = "emscripten",
        };
        var model = Flatten("complex-conditions.dotori", ctx);
        Assert.Contains("-sUSE_SDL=2", model.EmscriptenFlags);
        Assert.Contains("-sALLOW_MEMORY_GROWTH", model.EmscriptenFlags);
    }

    [TestMethod]
    public void Flatten_UnmatchedPlatform_NotApplied()
    {
        var ctx = new TargetContext
        {
            Platform = "macos", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("lib-with-package.dotori", ctx);
        Assert.DoesNotContain("PLATFORM_LINUX", model.Defines);
        Assert.DoesNotContain("PLATFORM_WINDOWS", model.Defines);
    }

    // ─── RuntimeEnforcer ────────────────────────────────────────────────────

    [TestMethod]
    public void RuntimeEnforcer_Uwp_ForcedDynamic()
    {
        var model = new FlatProjectModel
        {
            Name = "T", ProjectDir = "/", DotoriPath = "/.dotori",
            RuntimeLink = RuntimeLink.Static,
        };
        RuntimeEnforcer.Enforce(model, "uwp");
        Assert.AreEqual(RuntimeLink.Dynamic, model.RuntimeLink);
    }

    [TestMethod]
    public void RuntimeEnforcer_Ios_ForcedStatic()
    {
        var model = new FlatProjectModel
        {
            Name = "T", ProjectDir = "/", DotoriPath = "/.dotori",
            RuntimeLink = RuntimeLink.Dynamic,
        };
        RuntimeEnforcer.Enforce(model, "ios");
        Assert.AreEqual(RuntimeLink.Static, model.RuntimeLink);
    }

    [TestMethod]
    public void RuntimeEnforcer_Wasm_ForcedStatic()
    {
        var model = new FlatProjectModel
        {
            Name = "T", ProjectDir = "/", DotoriPath = "/.dotori",
            RuntimeLink = RuntimeLink.Dynamic,
        };
        RuntimeEnforcer.Enforce(model, "wasm");
        Assert.AreEqual(RuntimeLink.Static, model.RuntimeLink);
    }

    [TestMethod]
    public void RuntimeEnforcer_Linux_NotChanged()
    {
        var model = new FlatProjectModel
        {
            Name = "T", ProjectDir = "/", DotoriPath = "/.dotori",
            RuntimeLink = RuntimeLink.Dynamic,
        };
        RuntimeEnforcer.Enforce(model, "linux");
        Assert.AreEqual(RuntimeLink.Dynamic, model.RuntimeLink);
    }

    // ─── Dependency merging ─────────────────────────────────────────────────

    [TestMethod]
    public void Flatten_Dependencies_PathDepPresent()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("simple-app.dotori", ctx);
        var myLib = model.Dependencies.Single(d => d.Name == "my-lib");
        Assert.IsInstanceOfType<ComplexDependency>(myLib.Value);
        Assert.AreEqual("../lib", ((ComplexDependency)myLib.Value).Path);
    }

    // ─── Phase 1-J: output / pre-build / post-build / module export-map ─────

    private static FlatProjectModel FlattenSource(string source, TargetContext ctx)
    {
        var file = DotoriParser.ParseSource(source, "<test>");
        return ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
    }

    private static TargetContext LinuxDebug => new()
    {
        Platform = "linux", Config = "debug", Compiler = "clang", Runtime = "static",
    };

    [TestMethod]
    public void Flatten_OutputBlock_MergedIntoModel()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                output {
                    binaries  = "bin/"
                    libraries = "lib/"
                    symbols   = "pdb/"
                }
            }
            """, LinuxDebug);

        Assert.IsNotNull(model.Output);
        Assert.AreEqual("bin/",  model.Output.Binaries);
        Assert.AreEqual("lib/",  model.Output.Libraries);
        Assert.AreEqual("pdb/",  model.Output.Symbols);
    }

    [TestMethod]
    public void Flatten_OutputBlock_ConditionOverrides()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                output {
                    binaries = "out/"
                }
                [release] {
                    output {
                        binaries = "dist/"
                    }
                }
            }
            """, new TargetContext
            {
                Platform = "linux", Config = "release", Compiler = "clang", Runtime = "static",
            });

        Assert.IsNotNull(model.Output);
        Assert.AreEqual("dist/", model.Output.Binaries);
    }

    [TestMethod]
    public void Flatten_OutputBlock_NotAppliedInWrongConfig()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                output {
                    binaries = "out/"
                }
                [release] {
                    output {
                        binaries = "dist/"
                    }
                }
            }
            """, LinuxDebug);

        Assert.IsNotNull(model.Output);
        Assert.AreEqual("out/", model.Output.Binaries);
    }

    [TestMethod]
    public void Flatten_PreBuildCommands_Accumulated()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                pre-build {
                    "step1.sh"
                    "step2.sh"
                }
                [debug] {
                    pre-build {
                        "step3.sh"
                    }
                }
            }
            """, LinuxDebug);

        Assert.HasCount(3, model.PreBuildCommands);
        Assert.AreEqual("step1.sh", model.PreBuildCommands[0]);
        Assert.AreEqual("step2.sh", model.PreBuildCommands[1]);
        Assert.AreEqual("step3.sh", model.PreBuildCommands[2]);
    }

    [TestMethod]
    public void Flatten_PostBuildCommands_Accumulated()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                post-build {
                    "sign.sh"
                }
            }
            """, LinuxDebug);

        Assert.HasCount(1, model.PostBuildCommands);
        Assert.AreEqual("sign.sh", model.PostBuildCommands[0]);
    }

    [TestMethod]
    public void Flatten_PreBuildCommands_ConditionalNotApplied()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                [release] {
                    pre-build {
                        "release-only.sh"
                    }
                }
            }
            """, LinuxDebug);  // debug context

        Assert.HasCount(0, model.PreBuildCommands);
    }

    [TestMethod]
    public void Flatten_ModuleExportMap_DefaultTrue()
    {
        var model = FlattenSource("""
            project MyLib {
                type = static-library
                modules {
                    include "src/**/*.cppm"
                }
            }
            """, LinuxDebug);

        Assert.IsTrue(model.ModuleExportMap);
    }

    [TestMethod]
    public void Flatten_ModuleExportMap_CanBeDisabled()
    {
        var model = FlattenSource("""
            project MyLib {
                type = static-library
                modules {
                    include "src/**/*.cppm"
                    export-map = false
                }
            }
            """, LinuxDebug);

        Assert.IsFalse(model.ModuleExportMap);
    }

    [TestMethod]
    public void Flatten_ModuleExportMap_ConditionOverrides()
    {
        var model = FlattenSource("""
            project MyLib {
                type = static-library
                modules {
                    include "src/**/*.cppm"
                    export-map = true
                }
                [debug] {
                    modules {
                        export-map = false
                    }
                }
            }
            """, LinuxDebug);

        Assert.IsFalse(model.ModuleExportMap);
    }

    // ─── Phase 1-K: compile-flags / link-flags ──────────────────────────────

    [TestMethod]
    public void Flatten_CompileFlags_TopLevel_Accumulated()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                compile-flags { "-DAPP_BUILD_NUMBER=42" }
            }
            """, LinuxDebug);

        Assert.Contains("-DAPP_BUILD_NUMBER=42", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_LinkFlags_TopLevel_Accumulated()
    {
        var model = FlattenSource("""
            project MyApp {
                type = executable
                link-flags { "-Wl,--as-needed" }
            }
            """, LinuxDebug);

        Assert.Contains("-Wl,--as-needed", model.LinkFlags);
    }

    [TestMethod]
    public void Flatten_CompileFlags_ConditionApplied()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug", Compiler = "clang", Runtime = "static",
        };
        var model = FlattenSource("""
            project MyApp {
                type = executable
                [clang] {
                    compile-flags { "-march=native" "-ffast-math" }
                }
                [msvc] {
                    compile-flags { "/arch:AVX2" }
                }
            }
            """, ctx);

        Assert.Contains("-march=native", model.CompileFlags);
        Assert.Contains("-ffast-math", model.CompileFlags);
        Assert.DoesNotContain("/arch:AVX2", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_LinkFlags_ConditionApplied()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "release", Compiler = "clang", Runtime = "static",
        };
        var model = FlattenSource("""
            project MyApp {
                type = executable
                [clang] {
                    link-flags { "-Wl,--as-needed" }
                }
                [msvc] {
                    link-flags { "/OPT:REF" }
                }
            }
            """, ctx);

        Assert.Contains("-Wl,--as-needed", model.LinkFlags);
        Assert.DoesNotContain("/OPT:REF", model.LinkFlags);
    }

    [TestMethod]
    public void Flatten_CompileFlags_Accumulated_AcrossMultipleBlocks()
    {
        // Common block + compiler condition block: both should be appended
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug", Compiler = "clang", Runtime = "static",
        };
        var model = FlattenSource("""
            project MyApp {
                type = executable
                compile-flags { "-DCOMMON=1" }
                [clang] {
                    compile-flags { "-march=native" }
                }
                [debug] {
                    compile-flags { "-DDEBUG_EXTRA" }
                }
            }
            """, ctx);

        Assert.Contains("-DCOMMON=1", model.CompileFlags);
        Assert.Contains("-march=native", model.CompileFlags);
        Assert.Contains("-DDEBUG_EXTRA", model.CompileFlags);
        Assert.HasCount(3, model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_CompileFlags_MoreSpecificConditionAppended()
    {
        // [release.msvc] should be appended (not override) after [msvc]
        var ctx = new TargetContext
        {
            Platform = "windows", Config = "release", Compiler = "msvc", Runtime = "static",
        };
        var model = FlattenSource("""
            project MyApp {
                type = executable
                [msvc] {
                    compile-flags { "/arch:AVX2" }
                }
                [release.msvc] {
                    compile-flags { "/Oi" "/Ot" }
                }
            }
            """, ctx);

        Assert.Contains("/arch:AVX2", model.CompileFlags);
        Assert.Contains("/Oi", model.CompileFlags);
        Assert.Contains("/Ot", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_CompileFlags_FromFixture_ClangContext()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug", Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("custom-flags.dotori", ctx);

        // Top-level
        Assert.Contains("-DAPP_BUILD_NUMBER=42", model.CompileFlags);
        // [clang] block
        Assert.Contains("-march=native", model.CompileFlags);
        Assert.Contains("-ffast-math", model.CompileFlags);
        // [debug] block
        Assert.Contains("-DDEBUG_EXTRA", model.CompileFlags);
        // [msvc] block should NOT be present
        Assert.DoesNotContain("/arch:AVX2", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_LinkFlags_FromFixture_MsvcContext()
    {
        var ctx = new TargetContext
        {
            Platform = "windows", Config = "release", Compiler = "msvc", Runtime = "static",
        };
        var model = Flatten("custom-flags.dotori", ctx);

        // [msvc] block
        Assert.Contains("/SUBSYSTEM:WINDOWS", model.LinkFlags);
        Assert.Contains("/OPT:REF", model.LinkFlags);
        // [release.msvc] block
        Assert.Contains("/LTCG", model.LinkFlags);
        // [clang] / [release.clang] blocks should NOT be present
        Assert.DoesNotContain("-Wl,--as-needed", model.LinkFlags);
        Assert.DoesNotContain("-Wl,--gc-sections", model.LinkFlags);
    }
}
