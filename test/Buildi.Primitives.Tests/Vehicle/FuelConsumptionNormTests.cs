using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class FuelConsumptionNormTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(7, FuelConsumptionNorm.All.Count);

    [Theory]
    [InlineData("WLTP")]
    [InlineData("wltp")]
    [InlineData("NEDC")]
    [InlineData("nedc")]
    [InlineData("EPA")]
    [InlineData("JC08")]
    [InlineData("CLTC")]
    [InlineData("Unknown")]
    [InlineData("Okänd")]
    [InlineData("okand")]
    [InlineData("WLTP-cykel")]
    [InlineData("EPA estimate")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(FuelConsumptionNorm.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("Euro 6")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(FuelConsumptionNorm.IsValid(input));

    [Theory]
    [InlineData("WLTP", "WLTP")]
    [InlineData("wltp", "WLTP")]
    [InlineData("WLTP-cykel", "WLTP")]
    [InlineData("Okänd", "Unknown")]
    [InlineData("okand", "Unknown")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, FuelConsumptionNorm.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(FuelConsumptionNorm.Wltp, FuelConsumptionNorm.Parse("WLTP"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => FuelConsumptionNorm.Parse("nope"));

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Fuel Consumption Norm", FuelConsumptionNorm.TypeInfo.EnglishName);
        Assert.Equal("Förbrukningsnorm", FuelConsumptionNorm.TypeInfo.LocalizedName);
    }

    [Fact]
    public void Region_IsExposed()
    {
        Assert.Contains("EU", FuelConsumptionNorm.Wltp.Region);
        Assert.Equal("US", FuelConsumptionNorm.Epa.Region);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("****", FuelConsumptionNorm.Wltp.ToMaskedString());

    [Fact]
    public void Equality_AndComparison()
    {
        var a = FuelConsumptionNorm.Parse("WLTP");
        var b = FuelConsumptionNorm.Parse("wltp");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }
}
