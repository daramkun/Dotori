using Dotori.LanguageServer.Providers;

namespace Dotori.Tests.LanguageServer;

[TestClass]
public class HoverProviderTests
{
    [TestMethod]
    public void GetHover_KnownKeyword_ReturnsDocumentation()
    {
        const string text = "project MyApp { type = executable }";
        // Hover over "type" (col 16)
        var hover = HoverProvider.GetHover(text, 0, 17);

        Assert.IsNotNull(hover);
        Assert.AreEqual("markdown", hover.Contents.Kind);
        StringAssert.Contains(hover.Contents.Value, "type");
        StringAssert.Contains(hover.Contents.Value, "executable");
    }

    [TestMethod]
    public void GetHover_UnknownWord_ReturnsNull()
    {
        const string text = "project MyApp { unknownkeyword = \"x\" }";
        var hover = HoverProvider.GetHover(text, 0, 17);

        Assert.IsNull(hover, "Unknown keyword should return null hover");
    }

    [TestMethod]
    public void GetHover_RuntimeLink_HasDescription()
    {
        const string text = "    runtime-link = static";
        // Hover over "runtime-link" starting at col 4
        var hover = HoverProvider.GetHover(text, 0, 8);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Contents.Value, "runtime-link");
        StringAssert.Contains(hover.Contents.Value, "static");
    }

    [TestMethod]
    public void GetHover_Std_HasCxxVersions()
    {
        const string text = "    std = c++23";
        var hover = HoverProvider.GetHover(text, 0, 6);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Contents.Value, "std");
        StringAssert.Contains(hover.Contents.Value, "c++23");
    }

    [TestMethod]
    public void GetHover_ConditionAtom_HasDescription()
    {
        const string text = "    [windows] {";
        // Hover over "windows" at col 5
        var hover = HoverProvider.GetHover(text, 0, 6);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Contents.Value, "windows");
    }

    [TestMethod]
    public void GetHover_OutOfBounds_ReturnsNull()
    {
        const string text = "project MyApp {}";
        // Way beyond end of line
        var hover = HoverProvider.GetHover(text, 0, 500);

        // Should not throw and should return null (nothing meaningful there)
        Assert.IsNull(hover);
    }

    [TestMethod]
    public void GetHover_Range_CoverWord()
    {
        const string text = "    type = executable";
        // Hover at character 6 (inside "type")
        var hover = HoverProvider.GetHover(text, 0, 6);

        Assert.IsNotNull(hover);
        Assert.IsNotNull(hover.Range);
        Assert.AreEqual(0, hover.Range!.Start.Line);
        Assert.AreEqual(0, hover.Range.End.Line);
        Assert.IsGreaterThan(hover.Range.Start.Character, hover.Range.End.Character);
    }

    [TestMethod]
    public void GetHover_Option_HasDescription()
    {
        const string text = "    option simd {";
        // Hover over "option" at col 4
        var hover = HoverProvider.GetHover(text, 0, 5);
        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Contents.Value, "option");
        StringAssert.Contains(hover.Contents.Value, "default");
    }
}
