using Dotori.Core.Model;
using Dotori.Core.Parsing;

namespace Dotori.Tests.Parsing;

[TestClass]
public sealed class CopyBlockParserTests
{
    private static DotoriFile Parse(string src) =>
        DotoriParser.ParseSource(src, "<test>");

    // ─── Parser tests ─────────────────────────────────────────────────────────

    [TestMethod]
    public void CopyBlock_ParsesFromTo()
    {
        var file = Parse("""
            project Foo {
                copy {
                    from "assets/**/*" to "bin/assets/"
                    from "config.json" to "bin/"
                }
            }
            """);

        var copy = file.Project!.Items.OfType<CopyBlock>().Single();
        Assert.AreEqual(2, copy.Items.Count);
        Assert.AreEqual("assets/**/*", copy.Items[0].From);
        Assert.AreEqual("bin/assets/", copy.Items[0].To);
        Assert.AreEqual("config.json", copy.Items[1].From);
        Assert.AreEqual("bin/",        copy.Items[1].To);
    }

    [TestMethod]
    public void CopyBlock_EmptyBlock_ParsesOk()
    {
        var file = Parse("""
            project Foo {
                copy {}
            }
            """);

        var copy = file.Project!.Items.OfType<CopyBlock>().Single();
        Assert.AreEqual(0, copy.Items.Count);
    }

    [TestMethod]
    public void CopyBlock_InsideCondition_ParsesOk()
    {
        var file = Parse("""
            project Foo {
                [release] {
                    copy {
                        from "data/**/*"  to "dist/data/"
                    }
                }
            }
            """);

        var cond = file.Project!.Items.OfType<ConditionBlock>().Single();
        var copy = cond.Items.OfType<CopyBlock>().Single();
        Assert.AreEqual("data/**/*", copy.Items[0].From);
        Assert.AreEqual("dist/data/", copy.Items[0].To);
    }

    [TestMethod]
    [ExpectedException(typeof(ParseException))]
    public void CopyBlock_MissingTo_ThrowsParseException()
    {
        Parse("""
            project Foo {
                copy {
                    from "assets/**/*"
                }
            }
            """);
    }

    // ─── Formatter round-trip ─────────────────────────────────────────────────

    [TestMethod]
    public void CopyBlock_FormatRoundTrip()
    {
        const string src = """
            project Foo {

                copy {
                    from "assets/**/*"  to "bin/assets/"
                    from "config.json"  to "bin/"
                }
            }
            """;

        var file      = Parse(src);
        var formatted = DotoriFormatter.Format(file);
        var reparsed  = Parse(formatted);

        var orig     = file.Project!.Items.OfType<CopyBlock>().Single();
        var roundtrip = reparsed.Project!.Items.OfType<CopyBlock>().Single();

        Assert.AreEqual(orig.Items.Count, roundtrip.Items.Count);
        for (int i = 0; i < orig.Items.Count; i++)
        {
            Assert.AreEqual(orig.Items[i].From, roundtrip.Items[i].From);
            Assert.AreEqual(orig.Items[i].To,   roundtrip.Items[i].To);
        }
    }

    // ─── Flattener tests ──────────────────────────────────────────────────────

    private static FlatProjectModel Flatten(string src, string? platform = "macos", string? config = "debug")
    {
        var file = Parse(src);
        var ctx = new TargetContext
        {
            Platform = platform!, Config = config!,
            Compiler = "clang", Runtime = "static",
        };
        return ProjectFlattener.Flatten(file.Project!, "<test>", ctx);
    }

    [TestMethod]
    public void CopyBlock_Flatten_AccumulatesCopyItems()
    {
        var model = Flatten("""
            project Foo {
                copy {
                    from "a/**/*"  to "bin/a/"
                }
                copy {
                    from "b/*.json"  to "bin/"
                }
            }
            """);

        Assert.AreEqual(2, model.CopyItems.Count);
        Assert.AreEqual("a/**/*",  model.CopyItems[0].From);
        Assert.AreEqual("bin/a/",  model.CopyItems[0].To);
        Assert.AreEqual("b/*.json", model.CopyItems[1].From);
        Assert.AreEqual("bin/",    model.CopyItems[1].To);
    }

    [TestMethod]
    public void CopyBlock_Flatten_ConditionMerge()
    {
        var model = Flatten("""
            project Foo {
                copy {
                    from "common/**/*"  to "bin/"
                }
                [macos] {
                    copy {
                        from "macos/**/*"  to "bin/"
                    }
                }
                [windows] {
                    copy {
                        from "win/**/*"  to "bin/"
                    }
                }
            }
            """, platform: "macos");

        // common + macos blocks should be merged; windows block should be skipped
        Assert.AreEqual(2, model.CopyItems.Count);
        Assert.AreEqual("common/**/*", model.CopyItems[0].From);
        Assert.AreEqual("macos/**/*",  model.CopyItems[1].From);
    }

    [TestMethod]
    public void CopyBlock_Flatten_EnvExpansion()
    {
        Environment.SetEnvironmentVariable("TEST_COPY_SRC", "assets");
        Environment.SetEnvironmentVariable("TEST_COPY_DST", "bin/assets/");
        try
        {
            var model = Flatten("""
                project Foo {
                    copy {
                        from "${TEST_COPY_SRC}/**/*"  to "${TEST_COPY_DST}"
                    }
                }
                """);

            Assert.AreEqual("assets/**/*",  model.CopyItems[0].From);
            Assert.AreEqual("bin/assets/",  model.CopyItems[0].To);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_COPY_SRC", null);
            Environment.SetEnvironmentVariable("TEST_COPY_DST", null);
        }
    }
}
