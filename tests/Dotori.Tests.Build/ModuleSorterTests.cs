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

        Assert.HasCount(3, sorted);
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

        Assert.HasCount(3, sorted);
        int idxA = IndexOf(sorted, "A");
        int idxB = IndexOf(sorted, "B");
        int idxC = IndexOf(sorted, "C");

        Assert.IsLessThan(idxB, idxC, "C must be before B");
        Assert.IsLessThan(idxA, idxB, "B must be before A");
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

        Assert.HasCount(4, sorted);
        int idxA = IndexOf(sorted, "A");
        int idxB = IndexOf(sorted, "B");
        int idxC = IndexOf(sorted, "C");
        int idxD = IndexOf(sorted, "D");

        Assert.IsLessThan(idxB, idxD, "D must be before B");
        Assert.IsLessThan(idxC, idxD, "D must be before C");
        Assert.IsLessThan(idxA, idxB, "B must be before A");
        Assert.IsLessThan(idxA, idxC, "C must be before A");
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

        Assert.HasCount(1, sorted);
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
        Assert.IsEmpty(sorted);
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

            Assert.HasCount(1, jobs);

            // Check exact args to avoid false positives from file paths containing "-c"
            Assert.IsTrue(jobs[0].Args.Contains("--precompile"), "Should contain --precompile");
            Assert.IsTrue(jobs[0].Args.Contains("-x c++-module"), "Should contain -x c++-module");
            Assert.IsFalse(jobs[0].Args.Contains("-c"), "Should not contain -c (removed)");
            Assert.EndsWith(".pcm", jobs[0].OutputFile, "Output should be .pcm");
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

            Assert.HasCount(1, jobs);

            // Check exact args (not joined string to avoid false positives from file paths)
            Assert.IsTrue(jobs[0].Args.Contains("/interface"), "Should contain /interface");
            Assert.IsTrue(jobs[0].Args.Contains("/TP"), "Should contain /TP");
            Assert.IsFalse(jobs[0].Args.Contains("/c"), "Should not contain /c (removed)");
            Assert.EndsWith(".ifc", jobs[0].OutputFile, "Output should be .ifc");
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

            Assert.HasCount(2, jobs);

            // B's job should reference A's PCM
            var bArgs = string.Join(" ", jobs[1].Args);
            Assert.Contains("-fmodule-file=A=", bArgs, "B's job should reference A's PCM");
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
            ["MyLib", "std.core"], bmiPaths, CompilerKind.Clang);

        Assert.HasCount(1, flags); // "std.core" not in bmiPaths
        Assert.StartsWith("-fmodule-file=MyLib=", flags[0]);
    }

    [TestMethod]
    public void BuildImportFlags_Msvc_ProducesReferenceFlags()
    {
        var bmiPaths = new Dictionary<string, string>
        {
            ["MyLib"] = "/cache/bmi/MyLib.ifc",
        };

        var flags = ModuleSorter.BuildImportFlags(
            ["MyLib"], bmiPaths, CompilerKind.Msvc);

        Assert.HasCount(1, flags);
        Assert.StartsWith("/reference MyLib=", flags[0]);
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
        for (var i = 0; i < sorted.Count; i++)
            if (sorted[i].Provides == provides) return i;
        return -1;
    }
}
