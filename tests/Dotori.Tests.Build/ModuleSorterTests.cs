using Dotori.Core.Build;
using Dotori.Core.Toolchain;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class ModuleSorterTests
{
    // ─── Sort ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Sort_NoDependencies_ReturnsSameCount()
    {
        var deps = new[]
        {
            Dep("a.cppm", provides: "A"),
            Dep("b.cppm", provides: "B"),
            Dep("c.cppm", provides: "C"),
        };

        var sorted = ModuleSorter.Sort(deps);

        Assert.AreEqual(3, sorted.Count);
    }

    [TestMethod]
    public void Sort_LinearChain_CorrectOrder()
    {
        // A → B → C  (C must come first, then B, then A)
        var deps = new[]
        {
            Dep("a.cppm", provides: "A", requires: "B"),
            Dep("b.cppm", provides: "B", requires: "C"),
            Dep("c.cppm", provides: "C"),
        };

        var sorted = ModuleSorter.Sort(deps);

        Assert.AreEqual(3, sorted.Count);
        int idxA = IndexOf(sorted, "A");
        int idxB = IndexOf(sorted, "B");
        int idxC = IndexOf(sorted, "C");

        Assert.IsTrue(idxC < idxB, "C must be before B");
        Assert.IsTrue(idxB < idxA, "B must be before A");
    }

    [TestMethod]
    public void Sort_DiamondDependency_CorrectOrder()
    {
        // A depends on B and C; B and C both depend on D
        var deps = new[]
        {
            Dep("a.cppm", provides: "A", requires: "B", requires2: "C"),
            Dep("b.cppm", provides: "B", requires: "D"),
            Dep("c.cppm", provides: "C", requires: "D"),
            Dep("d.cppm", provides: "D"),
        };

        var sorted = ModuleSorter.Sort(deps);

        Assert.AreEqual(4, sorted.Count);
        int idxA = IndexOf(sorted, "A");
        int idxB = IndexOf(sorted, "B");
        int idxC = IndexOf(sorted, "C");
        int idxD = IndexOf(sorted, "D");

        Assert.IsTrue(idxD < idxB, "D must be before B");
        Assert.IsTrue(idxD < idxC, "D must be before C");
        Assert.IsTrue(idxB < idxA, "B must be before A");
        Assert.IsTrue(idxC < idxA, "C must be before A");
    }

    [TestMethod]
    public void Sort_ExternalRequires_Ignored()
    {
        // "std.core" is not in the project, so it should be ignored in sort
        var deps = new[]
        {
            Dep("a.cppm", provides: "A", requires: "std.core"),
        };

        var sorted = ModuleSorter.Sort(deps);

        Assert.AreEqual(1, sorted.Count);
        Assert.AreEqual("A", sorted[0].Provides);
    }

    [TestMethod]
    public void Sort_CycleDetected_ThrowsInvalidOperationException()
    {
        // A → B → A (cycle)
        var deps = new[]
        {
            Dep("a.cppm", provides: "A", requires: "B"),
            Dep("b.cppm", provides: "B", requires: "A"),
        };

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            ModuleSorter.Sort(deps));
    }

    [TestMethod]
    public void Sort_EmptyList_ReturnsEmpty()
    {
        var sorted = ModuleSorter.Sort(Array.Empty<ModuleScanner.ModuleDep>());
        Assert.AreEqual(0, sorted.Count);
    }

    // ─── BuildModuleJobs ──────────────────────────────────────────────────────

    [TestMethod]
    public void BuildModuleJobs_Clang_GeneratesPrecompileArgs()
    {
        var deps = new[] { Dep("mylib.cppm", provides: "MyLib") };
        var objDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(objDir);

        try
        {
            var jobs = ModuleSorter.BuildModuleJobs(
                deps, objDir, CompilerKind.Clang,
                new[] { "-std=c++23", "-c" });

            Assert.AreEqual(1, jobs.Count);
            var args = string.Join(" ", jobs[0].Args);

            Assert.IsTrue(args.Contains("--precompile"), "Should contain --precompile");
            Assert.IsTrue(args.Contains("-x c++-module"), "Should contain -x c++-module");
            Assert.IsFalse(args.Contains("-c"), "Should not contain -c (removed)");
            Assert.IsTrue(jobs[0].OutputFile.EndsWith(".pcm"), "Output should be .pcm");
        }
        finally
        {
            Directory.Delete(objDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildModuleJobs_Msvc_GeneratesInterfaceArgs()
    {
        var deps = new[] { Dep("mylib.cppm", provides: "MyLib") };
        var objDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(objDir);

        try
        {
            var jobs = ModuleSorter.BuildModuleJobs(
                deps, objDir, CompilerKind.Msvc,
                new[] { "/std:c++latest", "/c" });

            Assert.AreEqual(1, jobs.Count);
            var args = string.Join(" ", jobs[0].Args);

            Assert.IsTrue(args.Contains("/interface"), "Should contain /interface");
            Assert.IsTrue(args.Contains("/TP"), "Should contain /TP");
            Assert.IsFalse(args.Contains("/c"), "Should not contain /c (removed)");
            Assert.IsTrue(jobs[0].OutputFile.EndsWith(".ifc"), "Output should be .ifc");
        }
        finally
        {
            Directory.Delete(objDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildModuleJobs_WithDependencies_AddsModuleFileFlags()
    {
        // B depends on A, so B's job should have -fmodule-file=A=<a.pcm>
        var sortedDeps = new[]
        {
            Dep("a.cppm", provides: "A"),
            Dep("b.cppm", provides: "B", requires: "A"),
        };

        var objDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(objDir);

        try
        {
            var jobs = ModuleSorter.BuildModuleJobs(
                sortedDeps, objDir, CompilerKind.Clang,
                new[] { "-std=c++23" });

            Assert.AreEqual(2, jobs.Count);

            // B's job should reference A's PCM
            var bArgs = string.Join(" ", jobs[1].Args);
            Assert.IsTrue(bArgs.Contains("-fmodule-file=A="), "B's job should reference A's PCM");
        }
        finally
        {
            Directory.Delete(objDir, recursive: true);
        }
    }

    // ─── BuildImportFlags ─────────────────────────────────────────────────────

    [TestMethod]
    public void BuildImportFlags_Clang_ProducesModuleFileFlags()
    {
        var bmiPaths = new Dictionary<string, string>
        {
            ["MyLib"] = "/cache/bmi/MyLib.pcm",
        };

        var flags = ModuleSorter.BuildImportFlags(
            new[] { "MyLib", "std.core" }, bmiPaths, CompilerKind.Clang);

        Assert.AreEqual(1, flags.Count); // "std.core" not in bmiPaths
        Assert.IsTrue(flags[0].StartsWith("-fmodule-file=MyLib="));
    }

    [TestMethod]
    public void BuildImportFlags_Msvc_ProducesReferenceFlags()
    {
        var bmiPaths = new Dictionary<string, string>
        {
            ["MyLib"] = "/cache/bmi/MyLib.ifc",
        };

        var flags = ModuleSorter.BuildImportFlags(
            new[] { "MyLib" }, bmiPaths, CompilerKind.Msvc);

        Assert.AreEqual(1, flags.Count);
        Assert.IsTrue(flags[0].StartsWith("/reference MyLib="));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ModuleScanner.ModuleDep Dep(
        string sourceFile,
        string? provides = null,
        string? requires = null,
        string? requires2 = null)
    {
        var req = new List<string>();
        if (requires  is not null) req.Add(requires);
        if (requires2 is not null) req.Add(requires2);

        return new ModuleScanner.ModuleDep
        {
            SourceFile = sourceFile,
            Provides   = provides,
            Requires   = req,
        };
    }

    private static int IndexOf(IReadOnlyList<ModuleScanner.ModuleDep> sorted, string provides)
    {
        for (int i = 0; i < sorted.Count; i++)
            if (sorted[i].Provides == provides) return i;
        return -1;
    }
}
