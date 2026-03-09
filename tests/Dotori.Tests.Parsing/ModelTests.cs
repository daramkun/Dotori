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
        Assert.IsTrue(model.Defines.Contains("PLATFORM_LINUX"));
        Assert.IsTrue(model.Links.Contains("pthread"));
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
        Assert.IsTrue(model.Defines.Contains("PLATFORM_WINDOWS"));
        Assert.IsTrue(model.Links.Contains("kernel32"));
        Assert.IsFalse(model.Defines.Contains("PLATFORM_LINUX"));
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
        Assert.IsTrue(model.Defines.Contains("PLATFORM_WINDOWS"));
        // [windows.release] (specificity 2) overrides optimize
        Assert.AreEqual(OptimizeLevel.Full, model.Optimize);
        // [windows.release] adds WIN_RELEASE_ONLY define
        Assert.IsTrue(model.Defines.Contains("WIN_RELEASE_ONLY"));
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
        Assert.IsTrue(model.Defines.Contains("DEBUG"));
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
        Assert.IsTrue(model.Defines.Contains("NDEBUG"));
        Assert.IsFalse(model.Defines.Contains("DEBUG"));
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
        Assert.IsTrue(model.EmscriptenFlags.Contains("-sUSE_SDL=2"));
        Assert.IsTrue(model.EmscriptenFlags.Contains("-sALLOW_MEMORY_GROWTH"));
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
        Assert.IsFalse(model.Defines.Contains("PLATFORM_LINUX"));
        Assert.IsFalse(model.Defines.Contains("PLATFORM_WINDOWS"));
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
}
