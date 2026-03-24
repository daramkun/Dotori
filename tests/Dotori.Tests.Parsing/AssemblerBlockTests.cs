using Dotori.Core.Model;
using Dotori.Core.Parsing;

namespace Dotori.Tests.Parsing;

[TestClass]
public sealed class AssemblerBlockTests
{
    private static DotoriFile Parse(string src) =>
        DotoriParser.ParseSource(src, "<test>");

    private static FlatProjectModel Flatten(
        string src, string platform = "linux", string compiler = "clang")
    {
        var file = Parse(src);
        var ctx = new TargetContext
        {
            Platform = platform, Config = "debug",
            Compiler = compiler, Runtime = "static",
        };
        return ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
    }

    // ─── Parser tests ─────────────────────────────────────────────────────────

    [TestMethod]
    public void AssemblerBlock_ParsesToolAndIncludes()
    {
        var file = Parse("""
            project Foo {
                assembler {
                    tool = nasm
                    format = "elf64"
                    include "src/**/*.asm"
                    exclude "src/test/**/*.asm"
                }
            }
            """);

        var block = file.Project!.Items.OfType<AssemblerBlock>().Single();
        Assert.AreEqual(AssemblerTool.Nasm,  block.Tool);
        Assert.AreEqual("elf64",             block.Format);
        Assert.AreEqual(2,                   block.Items.Count);
        Assert.IsTrue(block.Items[0].IsInclude);
        Assert.AreEqual("src/**/*.asm",      block.Items[0].Glob);
        Assert.IsFalse(block.Items[1].IsInclude);
        Assert.AreEqual("src/test/**/*.asm", block.Items[1].Glob);
    }

    [TestMethod]
    public void AssemblerBlock_DefaultToolIsAuto()
    {
        var file = Parse("""
            project Foo {
                assembler {
                    include "src/**/*.asm"
                }
            }
            """);

        var block = file.Project!.Items.OfType<AssemblerBlock>().Single();
        Assert.AreEqual(AssemblerTool.Auto, block.Tool);
        Assert.IsNull(block.Format);
    }

    [TestMethod]
    public void AssemblerBlock_ParsesAllToolValues()
    {
        foreach (var (toolStr, expected) in new[]
        {
            ("nasm", AssemblerTool.Nasm),
            ("yasm", AssemblerTool.Yasm),
            ("gas",  AssemblerTool.Gas),
            ("as",   AssemblerTool.Gas),
            ("masm", AssemblerTool.Masm),
            ("auto", AssemblerTool.Auto),
        })
        {
            var src =
                "project Foo {\n" +
                "    assembler {\n" +
                "        tool = " + toolStr + "\n" +
                "        include \"src/**/*.asm\"\n" +
                "    }\n" +
                "}\n";
            var file = Parse(src);
            var block = file.Project!.Items.OfType<AssemblerBlock>().Single();
            Assert.AreEqual(expected, block.Tool, $"tool={toolStr}");
        }
    }

    [TestMethod]
    public void AssemblerBlock_ParsesFlagsAndDefines()
    {
        var file = Parse("""
            project Foo {
                assembler {
                    include "src/**/*.asm"
                    flags { "-g" "-Wall" }
                    defines { "DEBUG" "VERSION=2" }
                }
            }
            """);

        var block = file.Project!.Items.OfType<AssemblerBlock>().Single();
        CollectionAssert.AreEqual(new[] { "-g", "-Wall" },         block.Flags.ToArray());
        CollectionAssert.AreEqual(new[] { "DEBUG", "VERSION=2" },  block.Defines.ToArray());
    }

    [TestMethod]
    public void AssemblerBlock_UnknownTool_ThrowsParseException()
    {
        Assert.ThrowsExactly<ParseException>(() => Parse("""
            project Foo {
                assembler {
                    tool = unknown-tool
                }
            }
            """));
    }

    [TestMethod]
    public void AssemblerBlock_UnknownOption_ThrowsParseException()
    {
        Assert.ThrowsExactly<ParseException>(() => Parse("""
            project Foo {
                assembler {
                    bogus = "value"
                }
            }
            """));
    }

    // ─── Formatter round-trip ─────────────────────────────────────────────────

    [TestMethod]
    public void AssemblerBlock_FormatRoundTrip()
    {
        const string src = """
            project Foo {

                assembler {
                    tool = nasm
                    format = "elf64"
                    include "src/**/*.asm"
                    exclude "src/test/**/*.asm"
                    flags { "-g" }
                    defines { "DEBUG" }
                }
            }
            """;

        var file      = Parse(src);
        var formatted = DotoriFormatter.Format(file);
        var reparsed  = Parse(formatted);

        var orig      = file.Project!.Items.OfType<AssemblerBlock>().Single();
        var roundtrip = reparsed.Project!.Items.OfType<AssemblerBlock>().Single();

        Assert.AreEqual(orig.Tool,   roundtrip.Tool);
        Assert.AreEqual(orig.Format, roundtrip.Format);
        Assert.AreEqual(orig.Items.Count, roundtrip.Items.Count);
        CollectionAssert.AreEqual(orig.Flags.ToArray(),   roundtrip.Flags.ToArray());
        CollectionAssert.AreEqual(orig.Defines.ToArray(), roundtrip.Defines.ToArray());
    }

    // ─── Flattener tests ──────────────────────────────────────────────────────

    [TestMethod]
    public void AssemblerBlock_Flatten_NullWhenNoBlock()
    {
        var model = Flatten("""
            project Foo {
                sources { include "src/**/*.cpp" }
            }
            """);

        Assert.IsNull(model.Assembler);
    }

    [TestMethod]
    public void AssemblerBlock_Flatten_BasicConfig()
    {
        var model = Flatten("""
            project Foo {
                assembler {
                    tool = nasm
                    format = "elf64"
                    include "src/**/*.asm"
                }
            }
            """);

        Assert.IsNotNull(model.Assembler);
        Assert.AreEqual(AssemblerTool.Nasm, model.Assembler.Tool);
        Assert.AreEqual("elf64",            model.Assembler.Format);
        Assert.AreEqual(1,                  model.Assembler.Items.Count);
        Assert.AreEqual("src/**/*.asm",     model.Assembler.Items[0].Glob);
    }

    [TestMethod]
    public void AssemblerBlock_Flatten_PlatformCondition_OnlyMatchingApplied()
    {
        var model = Flatten("""
            project Foo {
                [linux] {
                    assembler {
                        tool = nasm
                        include "src/linux/**/*.asm"
                    }
                }
                [windows] {
                    assembler {
                        tool = masm
                        include "src/win/**/*.asm"
                    }
                }
            }
            """, platform: "linux");

        Assert.IsNotNull(model.Assembler);
        Assert.AreEqual(AssemblerTool.Nasm,          model.Assembler.Tool);
        Assert.AreEqual(1,                            model.Assembler.Items.Count);
        Assert.AreEqual("src/linux/**/*.asm",         model.Assembler.Items[0].Glob);
    }

    [TestMethod]
    public void AssemblerBlock_Flatten_ToolOverrideAndGlobAccumulate()
    {
        // Common sources + platform tool override
        var model = Flatten("""
            project Foo {
                assembler {
                    include "src/common/**/*.asm"
                }
                [linux] {
                    assembler {
                        tool = nasm
                        include "src/linux/**/*.asm"
                    }
                }
            }
            """, platform: "linux");

        Assert.IsNotNull(model.Assembler);
        Assert.AreEqual(AssemblerTool.Nasm, model.Assembler.Tool);   // overridden
        Assert.AreEqual(2, model.Assembler.Items.Count);              // accumulated
        Assert.AreEqual("src/common/**/*.asm", model.Assembler.Items[0].Glob);
        Assert.AreEqual("src/linux/**/*.asm",  model.Assembler.Items[1].Glob);
    }

    [TestMethod]
    public void AssemblerBlock_Flatten_NoAssemblerForWasm()
    {
        // No assembler block → Assembler stays null (platform has no asm)
        var model = Flatten("""
            project Foo {
                [linux] {
                    assembler {
                        tool = nasm
                        include "src/**/*.asm"
                    }
                }
            }
            """, platform: "wasm");

        Assert.IsNull(model.Assembler);
    }
}
