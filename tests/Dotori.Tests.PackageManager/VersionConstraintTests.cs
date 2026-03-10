using Dotori.PackageManager;

namespace Dotori.Tests.PackageManager;

[TestClass]
public sealed class SemanticVersionTests
{
    // ─── Parsing ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Parse_SimpleVersion_ParsesCorrectly()
    {
        var v = SemanticVersion.Parse("1.2.3");
        Assert.AreEqual(1, v.Major);
        Assert.AreEqual(2, v.Minor);
        Assert.AreEqual(3, v.Patch);
        Assert.IsNull(v.PreRelease);
    }

    [TestMethod]
    public void Parse_VersionWithLeadingV_Strips()
    {
        var v = SemanticVersion.Parse("v2.0.0");
        Assert.AreEqual(2, v.Major);
        Assert.AreEqual(0, v.Minor);
        Assert.AreEqual(0, v.Patch);
    }

    [TestMethod]
    public void Parse_VersionWithPreRelease_ParsesCorrectly()
    {
        var v = SemanticVersion.Parse("1.0.0-alpha");
        Assert.AreEqual(1, v.Major);
        Assert.AreEqual("alpha", v.PreRelease);
    }

    [TestMethod]
    public void Parse_MajorOnly_DefaultsMinorPatch()
    {
        var v = SemanticVersion.Parse("3");
        Assert.AreEqual(3, v.Major);
        Assert.AreEqual(0, v.Minor);
        Assert.AreEqual(0, v.Patch);
    }

    [TestMethod]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.IsFalse(SemanticVersion.TryParse("not-a-version", out _));
    }

    // ─── Comparison ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Compare_OlderVersionIsLess()
    {
        var v1 = SemanticVersion.Parse("1.0.0");
        var v2 = SemanticVersion.Parse("2.0.0");
        Assert.IsTrue(v1 < v2);
        Assert.IsTrue(v2 > v1);
    }

    [TestMethod]
    public void Compare_SameVersionIsEqual()
    {
        var v1 = SemanticVersion.Parse("1.2.3");
        var v2 = SemanticVersion.Parse("1.2.3");
        Assert.AreEqual(v1, v2);
        Assert.IsTrue(v1 == v2);
    }

    [TestMethod]
    public void Compare_ReleaseIsGreaterThanPreRelease()
    {
        var release    = SemanticVersion.Parse("1.0.0");
        var preRelease = SemanticVersion.Parse("1.0.0-alpha");
        Assert.IsTrue(release > preRelease);
    }

    [TestMethod]
    public void ToString_RoundTrips()
    {
        var v = SemanticVersion.Parse("1.2.3");
        Assert.AreEqual("1.2.3", v.ToString());
    }
}

[TestClass]
public sealed class VersionConstraintTests
{
    // ─── Parsing ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Parse_Exact_AllowsOnlyThatVersion()
    {
        var c = VersionConstraint.Parse("1.2.3");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.2.3")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.2.4")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.2.2")));
    }

    [TestMethod]
    public void Parse_Wildcard_AllowsAny()
    {
        var c = VersionConstraint.Parse("*");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.0.0")));
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("99.99.99")));
    }

    [TestMethod]
    public void Parse_CaretRange_AllowsCompatibleVersions()
    {
        var c = VersionConstraint.Parse("^1.2.3");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.2.3")));
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.9.9")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("2.0.0")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.2.2")));
    }

    [TestMethod]
    public void Parse_TildeRange_AllowsPatchUpdates()
    {
        var c = VersionConstraint.Parse("~1.2.3");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.2.3")));
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.2.9")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.3.0")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.2.2")));
    }

    [TestMethod]
    public void Parse_GreaterThanOrEqual_Works()
    {
        var c = VersionConstraint.Parse(">=2.0.0");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("2.0.0")));
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("3.0.0")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("1.9.9")));
    }

    [TestMethod]
    public void Parse_LessThan_Works()
    {
        var c = VersionConstraint.Parse("<2.0.0");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.9.9")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("2.0.0")));
    }

    [TestMethod]
    public void Parse_TwoPartRange_Works()
    {
        var c = VersionConstraint.Parse(">=1.0.0 <2.0.0");
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.0.0")));
        Assert.IsTrue(c.Allows(SemanticVersion.Parse("1.9.9")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("2.0.0")));
        Assert.IsFalse(c.Allows(SemanticVersion.Parse("0.9.9")));
    }

    // ─── Intersection ────────────────────────────────────────────────────────

    [TestMethod]
    public void Intersect_CompatibleExact_ReturnsExact()
    {
        var a = VersionConstraint.Parse("1.2.3");
        var b = VersionConstraint.Parse("^1.0.0");
        var result = VersionConstraint.Intersect(a, b);
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.Allows(SemanticVersion.Parse("1.2.3")));
        Assert.IsFalse(result.Allows(SemanticVersion.Parse("1.3.0")));
    }

    [TestMethod]
    public void Intersect_IncompatibleExact_ReturnsNull()
    {
        var a = VersionConstraint.Parse("1.2.3");
        var b = VersionConstraint.Parse("1.2.4");
        var result = VersionConstraint.Intersect(a, b);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Intersect_OverlappingRanges_ReturnsNarrower()
    {
        var a = VersionConstraint.Parse("^1.0.0");    // >=1.0.0 <2.0.0
        var b = VersionConstraint.Parse(">=1.5.0");
        var result = VersionConstraint.Intersect(a, b);
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.Allows(SemanticVersion.Parse("1.5.0")));
        Assert.IsFalse(result.Allows(SemanticVersion.Parse("1.4.9")));
        Assert.IsFalse(result.Allows(SemanticVersion.Parse("2.0.0")));
    }

    [TestMethod]
    public void Intersect_NonOverlappingRanges_ReturnsNull()
    {
        var a = VersionConstraint.Parse("<1.0.0");
        var b = VersionConstraint.Parse(">=2.0.0");
        var result = VersionConstraint.Intersect(a, b);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Intersect_WithWildcard_ReturnsOther()
    {
        var a = VersionConstraint.Any;
        var b = VersionConstraint.Parse("^2.0.0");
        var result = VersionConstraint.Intersect(a, b);
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.Allows(SemanticVersion.Parse("2.5.0")));
        Assert.IsFalse(result.Allows(SemanticVersion.Parse("3.0.0")));
    }
}
