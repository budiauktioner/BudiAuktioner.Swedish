using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class RefrigerantTests
{
    [Fact]
    public void All_HasExpectedCount()
    {
        Assert.True(Refrigerant.All.Count >= 20);
    }

    [Theory]
    [InlineData("R-134a")]
    [InlineData("R134a")]
    [InlineData("r134a")]
    [InlineData("R-290")]
    [InlineData("R290")]
    [InlineData("R-744")]
    [InlineData("R744")]
    [InlineData("CO2")]
    [InlineData("R-1234yf")]
    [InlineData("R1234yf")]
    [InlineData("Propane")]
    [InlineData("Propan")]
    [InlineData("Ammonia")]
    [InlineData("R32")]
    [InlineData("R-410A")]
    [InlineData("R-452A")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(Refrigerant.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("R0")]
    [InlineData("R99999")]
    [InlineData("nope")]
    [InlineData("hydrogen")]
    public void IsValid_ReturnsFalse_ForUnknownInputs(string? input) =>
        Assert.False(Refrigerant.IsValid(input));

    [Theory]
    [InlineData("R-134a", "R-134a")]
    [InlineData("R134a", "R-134a")]
    [InlineData("r134a", "R-134a")]
    [InlineData("CO2", "R-744")]
    [InlineData("Propane", "R-290")]
    [InlineData("Ammonia", "R-717")]
    public void Normalize_ReturnsCanonicalCode(string input, string expected) =>
        Assert.Equal(expected, Refrigerant.Normalize(input));

    [Fact]
    public void Parse_R134a_ReturnsExpectedMetadata()
    {
        var r = Refrigerant.Parse("R134a");
        Assert.Equal("R-134a", r.Value);
        Assert.Equal("R134a", r.CompactCode);
        Assert.Equal(Refrigerant.RefrigerantFamily.Hfc, r.Family);
        Assert.Equal(1430, r.Gwp100Year);
        Assert.Equal("A1", r.SafetyClass);
    }

    [Fact]
    public void RefrigerantFamily_DefaultIsUnknown()
    {
        Assert.Equal(Refrigerant.RefrigerantFamily.Unknown, default);
    }

    [Fact]
    public void Parse_R290_HasNaturalFamilyAndLowGwp()
    {
        var r = Refrigerant.Parse("R-290");
        Assert.Equal(Refrigerant.RefrigerantFamily.Natural, r.Family);
        Assert.True(r.Gwp100Year < 10);
    }

    [Fact]
    public void Parse_R404A_IsRestrictedInEu()
    {
        var r = Refrigerant.Parse("R-404A");
        Assert.True(r.IsRestrictedInEu);
    }

    [Fact]
    public void Parse_Throws_ForUnknown() =>
        Assert.Throws<ArgumentException>(() => Refrigerant.Parse("R-9999"));

    [Fact]
    public void ToMaskedString_PreservesPrefix()
    {
        var r = Refrigerant.Parse("R-134a");
        Assert.Equal("R-****", r.ToMaskedString());
    }

    [Fact]
    public void Equality_SameRefrigerant()
    {
        var a = Refrigerant.Parse("r134a");
        var b = Refrigerant.Parse("R-134A");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_Different()
    {
        var a = Refrigerant.Parse("R-134a");
        var b = Refrigerant.Parse("R-290");
        Assert.False(a.Equals(b));
        Assert.True(a != b);
    }

    [Theory]
    [InlineData("R134a", true)]
    [InlineData("R-134a", true)]
    [InlineData("nope", false)]
    public void IsValid_Cases(string input, bool expected) =>
        Assert.Equal(expected, Refrigerant.IsValid(input));

    [Fact]
    public void Parse_ReturnsSameInstance()
    {
        Assert.Same(Refrigerant.R134a, Refrigerant.Parse("R-134a"));
    }

    [Fact]
    public void FindCandidatesInText_FindsR134a()
    {
        var text = "Köldmedium: R-134a och R290 i annan utrustning.";
        var candidates = Refrigerant.FindCandidatesInText(text);
        Assert.Equal(2, candidates.Count);
        Assert.Equal("R-134a", candidates[0].NormalizedForm);
        Assert.Equal("R-290", candidates[1].NormalizedForm);
    }

    [Fact]
    public void FindCandidatesInText_IgnoresUnknown()
    {
        var text = "Random R9999 noise";
        Assert.Empty(Refrigerant.FindCandidatesInText(text));
    }
}
