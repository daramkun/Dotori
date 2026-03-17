using Dotori.Core.Parsing;

namespace Dotori.Tests.Parsing;

/// <summary>owner/name 형식 dep_item 파싱 테스트</summary>
[TestClass]
public sealed class OwnerNameDepTests
{
    private static ProjectDecl Parse(string src)
    {
        var tokens = new Lexer(src, "<test>").Tokenize();
        var parser = new Parser(tokens, "<test>");
        var file = parser.ParseFile();
        return file.Project!;
    }

    [TestMethod]
    public void Lexer_SlashToken()
    {
        var tokens = new Lexer("fmtlib/fmt", "<test>").Tokenize();
        Assert.AreEqual(TokenKind.Ident, tokens[0].Kind);
        Assert.AreEqual("fmtlib", tokens[0].Text);
        Assert.AreEqual(TokenKind.Slash, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Ident, tokens[2].Kind);
        Assert.AreEqual("fmt", tokens[2].Text);
    }

    [TestMethod]
    public void Parser_OwnerNameDep_VersionString()
    {
        var proj = Parse("""
            project MyApp {
                type = executable
                dependencies {
                    fmtlib/fmt = "10.2.0"
                }
            }
            """);

        var dep = proj.Items.OfType<DependenciesBlock>().Single().Items.Single();
        Assert.AreEqual("fmtlib/fmt", dep.Name);
        Assert.IsInstanceOfType<VersionDependency>(dep.Value);
        Assert.AreEqual("10.2.0", ((VersionDependency)dep.Value).Version);
    }

    [TestMethod]
    public void Parser_OwnerNameDep_ComplexValue()
    {
        var proj = Parse("""
            project MyApp {
                type = executable
                dependencies {
                    myorg/mylib = { git = "https://github.com/myorg/mylib", tag = "v1.0.0" }
                }
            }
            """);

        var dep = proj.Items.OfType<DependenciesBlock>().Single().Items.Single();
        Assert.AreEqual("myorg/mylib", dep.Name);
        var complex = (ComplexDependency)dep.Value;
        Assert.AreEqual("https://github.com/myorg/mylib", complex.Git);
        Assert.AreEqual("v1.0.0", complex.Tag);
    }

    [TestMethod]
    public void Parser_MixedDeps_OwnerNameAndSimple()
    {
        var proj = Parse("""
            project MyApp {
                type = executable
                dependencies {
                    fmtlib/fmt    = "10.2.0"
                    locallib      = { path = "../lib" }
                    myorg/spdlog  = { git = "https://github.com/myorg/spdlog", tag = "v1.0.0" }
                }
            }
            """);

        var deps = proj.Items.OfType<DependenciesBlock>().Single().Items;
        Assert.HasCount(3, deps);
        Assert.AreEqual("fmtlib/fmt", deps[0].Name);
        Assert.AreEqual("locallib", deps[1].Name);
        Assert.AreEqual("myorg/spdlog", deps[2].Name);
    }

    [TestMethod]
    public void Parser_OwnerNameDep_PreservesSlashInName()
    {
        var proj = Parse("""
            project X {
                type = executable
                dependencies {
                    alice/my-lib = "2.0.0"
                    bob/util_kit = "1.0.0"
                }
            }
            """);

        var deps = proj.Items.OfType<DependenciesBlock>().Single().Items;
        Assert.AreEqual("alice/my-lib", deps[0].Name);
        Assert.AreEqual("bob/util_kit", deps[1].Name);
    }
}
