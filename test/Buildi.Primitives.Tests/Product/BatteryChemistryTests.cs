using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class BatteryChemistryTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(11, BatteryChemistry.All.Count);

    [Theory]
    [InlineData("Li-ion")]
    [InlineData("li-ion")]
    [InlineData("Li ion")]
    [InlineData("Lithium-ion")]
    [InlineData("Lithium ion")]
    [InlineData("Litium-jon")]
    [InlineData("Litiumjon")]
    [InlineData("LiFePO4")]
    [InlineData("LFP")]
    [InlineData("Litiumjärnfosfat")]
    [InlineData("LiPo")]
    [InlineData("LTO")]
    [InlineData("NiMH")]
    [InlineData("NiCd")]
    [InlineData("AGM")]
    [InlineData("Gel")]
    [InlineData("Pb-Acid")]
    [InlineData("Bly")]
    [InlineData("Blybatteri")]
    [InlineData("Lead-acid")]
    [InlineData("Alkaline")]
    [InlineData("Alkalisk")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(BatteryChemistry.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("uranium")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BatteryChemistry.IsValid(input));

    [Theory]
    [InlineData("Li-ion", "Li-ion")]
    [InlineData("Litium-jon", "Li-ion")]
    [InlineData("LFP", "LiFePO4")]
    [InlineData("Bly", "Pb-Acid")]
    [InlineData("Lead-acid", "Pb-Acid")]
    [InlineData("AGM", "AGM")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, BatteryChemistry.Normalize(input));

    [Fact]
    public void Parse_LiFePO4_HasExpectedProperties()
    {
        var c = BatteryChemistry.Parse("LiFePO4");
        Assert.True(c.IsRechargeable);
        Assert.Equal(3.2m, c.NominalCellVoltageV);
    }

    [Fact]
    public void Parse_Alkaline_IsNotRechargeable()
    {
        var c = BatteryChemistry.Parse("Alkaline");
        Assert.False(c.IsRechargeable);
    }

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(BatteryChemistry.LithiumIon, BatteryChemistry.Parse("Li-ion"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => BatteryChemistry.Parse("nope"));

    [Fact]
    public void ToMaskedString_ReturnsStars()
    {
        var c = BatteryChemistry.LithiumIon;
        Assert.Equal(new string('*', c.Value.Length), c.ToMaskedString());
    }

    [Fact]
    public void Equality()
    {
        var a = BatteryChemistry.Parse("Li-ion");
        var b = BatteryChemistry.Parse("Lithium-ion");
        Assert.True(a == b);
    }
}
