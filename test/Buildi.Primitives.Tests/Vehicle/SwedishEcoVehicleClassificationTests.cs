using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class SwedishEcoVehicleClassificationTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(6, SwedishEcoVehicleClassification.All.Count);

    [Theory]
    [InlineData("Miljöbil 2007")]
    [InlineData("miljöbil 2007")]
    [InlineData("Miljöbil  2007")]
    [InlineData("Miljöbil 2013")]
    [InlineData("Miljobil 2013")]
    [InlineData("MB2013")]
    [InlineData("Supermiljöbil")]
    [InlineData("Klimatbonusbil")]
    [InlineData("Bonusbil")]
    [InlineData("Elbil")]
    [InlineData("Electric vehicle")]
    [InlineData("BEV")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(SwedishEcoVehicleClassification.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Euro 6")]
    [InlineData("nope")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(SwedishEcoVehicleClassification.IsValid(input));

    [Theory]
    [InlineData("Miljöbil 2013", "Miljöbil 2013")]
    [InlineData("Miljobil 2013", "Miljöbil 2013")]
    [InlineData("MB2013", "Miljöbil 2013")]
    [InlineData("Supermiljöbil", "Supermiljöbil")]
    [InlineData("Klimatbonusbil", "Klimatbonusbil")]
    [InlineData("BEV", "Elbil")]
    [InlineData(null, null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, SwedishEcoVehicleClassification.Normalize(input));

    [Fact]
    public void Parse_Miljobil2013_HasExpectedYearRange()
    {
        var c = SwedishEcoVehicleClassification.Parse("Miljöbil 2013");
        Assert.Equal(2013, c.IntroductionYear);
        Assert.Equal(2017, c.EndYear);
    }

    [Fact]
    public void Parse_Elbil_IsOpenEnded()
    {
        var c = SwedishEcoVehicleClassification.Parse("Elbil");
        Assert.NotNull(c.IntroductionYear);
        Assert.Null(c.EndYear);
    }

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(SwedishEcoVehicleClassification.Klimatbonusbil,
            SwedishEcoVehicleClassification.Parse("klimatbonusbil"));

    [Fact]
    public void ToMaskedString_PreservesPrefix()
    {
        var masked = SwedishEcoVehicleClassification.Miljobil2013.ToMaskedString();
        Assert.StartsWith("Miljöbil ", masked);
        Assert.EndsWith("****", masked);
    }

    [Fact]
    public void FindCandidatesInText_FindsMiljobil2013()
    {
        var text = "Bilen är en Miljöbil 2013 som ägs av en Klimatbonusbil-användare.";
        var candidates = SwedishEcoVehicleClassification.FindCandidatesInText(text);
        Assert.Equal(2, candidates.Count);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata() =>
        Assert.Equal("Miljöbilsklassning", SwedishEcoVehicleClassification.TypeInfo.LocalizedName);
}
