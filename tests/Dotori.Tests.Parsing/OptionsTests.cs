using Dotori.Core.Model;
using Dotori.Core.Parsing;

namespace Dotori.Tests.Parsing;

// ─── Parser tests for option blocks ─────────────────────────────────────────

[TestClass]
public sealed class OptionParserTests
{
    [TestMethod]
    public void Parser_OptionBlock_BasicParsed()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                option simd {
                    default = true
                    defines { "SIMD_ENABLED" }
                }
            }
            """, "<test>");
        var opts = file.Project!.Items.OfType<OptionBlock>().ToList();
        Assert.HasCount(1, opts);
        Assert.AreEqual("simd", opts[0].Name);
        Assert.IsTrue(opts[0].Default);
        Assert.HasCount(1, opts[0].Defines);
        Assert.AreEqual("SIMD_ENABLED", opts[0].Defines[0]);
    }

    [TestMethod]
    public void Parser_OptionBlock_DefaultFalse()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                option experimental {
                    default = false
                }
            }
            """, "<test>");
        var opt = file.Project!.Items.OfType<OptionBlock>().Single();
        Assert.AreEqual("experimental", opt.Name);
        Assert.IsFalse(opt.Default);
        Assert.HasCount(0, opt.Defines);
        Assert.HasCount(0, opt.Dependencies);
    }

    [TestMethod]
    public void Parser_OptionBlock_WithDependencies()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                option extra {
                    default = false
                    dependencies {
                        extra-lib = "1.0.0"
                    }
                }
            }
            """, "<test>");
        var opt = file.Project!.Items.OfType<OptionBlock>().Single();
        Assert.HasCount(1, opt.Dependencies);
        Assert.AreEqual("extra-lib", opt.Dependencies[0].Name);
        Assert.IsInstanceOfType<VersionDependency>(opt.Dependencies[0].Value);
        Assert.AreEqual("1.0.0", ((VersionDependency)opt.Dependencies[0].Value).Version);
    }

    [TestMethod]
    public void Parser_OptionBlock_MultipleDefines()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                option simd {
                    default = true
                    defines { "SIMD_ENABLED" "SIMD_VER=2" }
                }
            }
            """, "<test>");
        var opt = file.Project!.Items.OfType<OptionBlock>().Single();
        Assert.HasCount(2, opt.Defines);
        Assert.AreEqual("SIMD_ENABLED", opt.Defines[0]);
        Assert.AreEqual("SIMD_VER=2",   opt.Defines[1]);
    }

    [TestMethod]
    public void Parser_OptionBlock_MissingDefault_Throws()
    {
        Assert.ThrowsExactly<ParseException>(() =>
            DotoriParser.ParseSource("""
                project MyApp {
                    type = executable
                    option badopt {
                    }
                }
                """, "<test>"));
    }

    [TestMethod]
    public void Parser_OptionBlock_FixtureFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", "options.dotori");
        var file = DotoriParser.ParseFile(path);
        var opts = file.Project!.Items.OfType<OptionBlock>().ToList();
        Assert.HasCount(2, opts);
        Assert.AreEqual("simd",         opts[0].Name);
        Assert.IsTrue(opts[0].Default);
        Assert.AreEqual("experimental", opts[1].Name);
        Assert.IsFalse(opts[1].Default);
    }
}

// ─── Flattener tests for options ─────────────────────────────────────────────

[TestClass]
public sealed class OptionFlattenerTests
{
    private static string FixturePath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", name);

    private static FlatProjectModel Flatten(string fixture, TargetContext ctx)
    {
        var file = DotoriParser.ParseFile(FixturePath(fixture));
        return ProjectFlattener.Flatten(file.Project!, FixturePath(fixture), ctx);
    }

    [TestMethod]
    public void Flatten_Option_DefaultTrue_DefinesIncluded()
    {
        // simd has default=true, so SIMD_ENABLED should be in defines even without EnabledOptions
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("options.dotori", ctx);
        Assert.Contains("SIMD_ENABLED",  model.Defines);
        Assert.Contains("SIMD_VER=2",    model.Defines);
    }

    [TestMethod]
    public void Flatten_Option_DefaultFalse_DefinesExcluded()
    {
        // experimental has default=false, so EXPERIMENTAL should NOT be in defines
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var model = Flatten("options.dotori", ctx);
        Assert.DoesNotContain("EXPERIMENTAL", model.Defines);
    }

    [TestMethod]
    public void Flatten_Option_EnabledOptions_ActivatesOption()
    {
        // Explicitly enable "experimental"
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "experimental", "simd" },
        };
        var model = Flatten("options.dotori", ctx);
        Assert.Contains("EXPERIMENTAL",  model.Defines);
        Assert.Contains("SIMD_ENABLED",  model.Defines);
    }

    [TestMethod]
    public void Flatten_Option_EnabledOptions_DisablesDefaultTrue()
    {
        // EnabledOptions set but "simd" is NOT in it → simd is disabled
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "experimental" },
        };
        var model = Flatten("options.dotori", ctx);
        Assert.DoesNotContain("SIMD_ENABLED", model.Defines);
        Assert.Contains("EXPERIMENTAL",       model.Defines);
    }

    [TestMethod]
    public void Flatten_Option_ActiveAtoms_ConditionBlockApplied()
    {
        // With simd active, [simd] condition block (compile-flags "-mavx2") should apply
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "simd" },
        };
        var model = Flatten("options.dotori", ctx);
        Assert.Contains("-mavx2", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_Option_InactiveAtoms_ConditionBlockNotApplied()
    {
        // With simd NOT active, [simd] condition block should NOT apply
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        };
        var model = Flatten("options.dotori", ctx);
        Assert.DoesNotContain("-mavx2", model.CompileFlags);
    }

    [TestMethod]
    public void Flatten_Option_DefaultTrue_ConditionBlockApplied_NoExplicitOptions()
    {
        // simd default=true + no EnabledOptions → simd atom is NOT in active atoms
        // (TargetContext.EnabledOptions is null → fallback to defaults for option application,
        // but atoms are NOT added unless EnabledOptions is set)
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            // EnabledOptions is null → OptionBlock.Default governs defines/deps,
            // but condition blocks [simd] won't fire unless simd is in ActiveAtoms
        };
        // NOTE: When EnabledOptions is null, condition blocks like [simd] do NOT fire
        // (simd is not in ActiveAtoms). Only the defines/dependencies from the option block
        // itself are applied based on Default.
        var model = Flatten("options.dotori", ctx);
        Assert.Contains("SIMD_ENABLED",   model.Defines);      // from OptionBlock.Default=true
        Assert.DoesNotContain("-mavx2",   model.CompileFlags); // [simd] condition NOT active
    }

    [TestMethod]
    public void Flatten_Option_Dependencies_AddedWhenActive()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "experimental" },
        };
        var model = Flatten("options.dotori", ctx);
        Assert.Contains("extra-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_Option_Dependencies_NotAddedWhenInactive()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        };
        var model = Flatten("options.dotori", ctx);
        Assert.DoesNotContain("extra-lib", model.Dependencies.Select(d => d.Name));
    }
}

// ─── Parser tests for option field in dependency items ───────────────────────

[TestClass]
public sealed class DependencyOptionFieldTests
{
    [TestMethod]
    public void Parser_ComplexDependency_SingleOptionParsed()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    my-lib = { path = "../lib", option = "simd" }
                }
            }
            """, "<test>");
        var depsBlock = file.Project!.Items.OfType<DependenciesBlock>().Single();
        var dep = depsBlock.Items.Single();
        Assert.AreEqual("my-lib", dep.Name);
        var cd = Assert.IsInstanceOfType<ComplexDependency>(dep.Value);
        Assert.AreEqual("../lib", cd.Path);
        Assert.IsNotNull(cd.Options);
        Assert.HasCount(1, cd.Options);
        Assert.AreEqual("simd", cd.Options[0]);
    }

    [TestMethod]
    public void Parser_ComplexDependency_MultipleOptionsParsed()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    my-lib = { version = "1.0.0", option = { "simd" "experimental" } }
                }
            }
            """, "<test>");
        var depsBlock = file.Project!.Items.OfType<DependenciesBlock>().Single();
        var dep = depsBlock.Items.Single();
        var cd = Assert.IsInstanceOfType<ComplexDependency>(dep.Value);
        Assert.IsNotNull(cd.Options);
        Assert.HasCount(2, cd.Options);
        Assert.AreEqual("simd",         cd.Options[0]);
        Assert.AreEqual("experimental", cd.Options[1]);
    }

    [TestMethod]
    public void Parser_ComplexDependency_NoOption_IsNull()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    my-lib = { path = "../lib" }
                }
            }
            """, "<test>");
        var depsBlock = file.Project!.Items.OfType<DependenciesBlock>().Single();
        var dep = depsBlock.Items.Single();
        var cd = Assert.IsInstanceOfType<ComplexDependency>(dep.Value);
        Assert.IsNull(cd.Options);
    }

    [TestMethod]
    public void Flatten_DepOptionField_ExcludedWhenOptionInactive()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    always-lib   = "1.0.0"
                    optional-lib = { version = "2.0.0", option = "myopt" }
                }
            }
            """, "<test>");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        };
        var model = ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
        Assert.Contains("always-lib",   model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("optional-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_DepOptionField_IncludedWhenOptionActive()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    always-lib   = "1.0.0"
                    optional-lib = { version = "2.0.0", option = "myopt" }
                }
            }
            """, "<test>");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "myopt" },
        };
        var model = ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
        Assert.Contains("always-lib",   model.Dependencies.Select(d => d.Name));
        Assert.Contains("optional-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_DepOptionField_ExcludedWhenNoEnabledOptions()
    {
        // When EnabledOptions is null, option-gated deps are excluded (no explicit activation)
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    always-lib   = "1.0.0"
                    optional-lib = { version = "2.0.0", option = "myopt" }
                }
            }
            """, "<test>");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            // EnabledOptions is null
        };
        var model = ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
        Assert.Contains("always-lib",   model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("optional-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_MultipleOptions_IncludedWhenAllActive()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    multi-lib = { version = "1.0.0", option = { "opt-a" "opt-b" } }
                }
            }
            """, "<test>");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "opt-a", "opt-b" },
        };
        var model = ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
        Assert.Contains("multi-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_MultipleOptions_ExcludedWhenOnlyOneActive()
    {
        var file = DotoriParser.ParseSource("""
            project MyApp {
                type = executable
                dependencies {
                    multi-lib = { version = "1.0.0", option = { "opt-a" "opt-b" } }
                }
            }
            """, "<test>");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "opt-a" },
        };
        var model = ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
        Assert.DoesNotContain("multi-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_DepOptionField_FixtureFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", "options.dotori");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "experimental", "simd" },
        };
        var file = DotoriParser.ParseFile(path);
        var model = ProjectFlattener.Flatten(file.Project!, path, ctx);
        Assert.Contains("always-lib",   model.Dependencies.Select(d => d.Name));
        Assert.Contains("optional-lib", model.Dependencies.Select(d => d.Name));
        Assert.Contains("extra-lib",    model.Dependencies.Select(d => d.Name));
        Assert.Contains("multi-opt-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_DepOptionField_FixtureFile_PartialOptions()
    {
        // optional-lib needs only "experimental", multi-opt-lib needs both "simd" AND "experimental"
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", "options.dotori");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "experimental" },
        };
        var file = DotoriParser.ParseFile(path);
        var model = ProjectFlattener.Flatten(file.Project!, path, ctx);
        Assert.Contains("always-lib",          model.Dependencies.Select(d => d.Name));
        Assert.Contains("optional-lib",        model.Dependencies.Select(d => d.Name));
        Assert.Contains("extra-lib",           model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("multi-opt-lib", model.Dependencies.Select(d => d.Name));
    }

    [TestMethod]
    public void Flatten_DepOptionField_FixtureFile_OptionInactive()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", "options.dotori");
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        };
        var file = DotoriParser.ParseFile(path);
        var model = ProjectFlattener.Flatten(file.Project!, path, ctx);
        Assert.Contains("always-lib",          model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("optional-lib",  model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("extra-lib",     model.Dependencies.Select(d => d.Name));
        Assert.DoesNotContain("multi-opt-lib", model.Dependencies.Select(d => d.Name));
    }
}

// ─── TargetContext.ActiveAtoms with EnabledOptions ───────────────────────────

[TestClass]
public sealed class TargetContextOptionsTests
{
    [TestMethod]
    public void ActiveAtoms_EnabledOptions_IncludedInAtoms()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
            EnabledOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "simd", "experimental" },
        };
        var atoms = ctx.ActiveAtoms();
        Assert.Contains("simd",         atoms);
        Assert.Contains("experimental", atoms);
    }

    [TestMethod]
    public void ActiveAtoms_NoEnabledOptions_StandardAtomsOnly()
    {
        var ctx = new TargetContext
        {
            Platform = "linux", Config = "debug",
            Compiler = "clang", Runtime = "static",
        };
        var atoms = ctx.ActiveAtoms();
        Assert.DoesNotContain("simd", atoms);
        Assert.Contains("linux",      atoms);
        Assert.Contains("debug",      atoms);
    }
}
