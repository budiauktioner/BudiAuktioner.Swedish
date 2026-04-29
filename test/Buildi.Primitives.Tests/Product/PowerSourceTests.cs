using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class PowerSourceTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(10, PowerSource.All.Count);

    [Theory]
    [InlineData("Electric")]
    [InlineData("electric")]
    [InlineData("El")]
    [InlineData("Elektrisk")]
    [InlineData("Eldriven")]
    [InlineData("Mains")]
    [InlineData("Corded")]
    [InlineData("Plug-in")]
    [InlineData("Nätansluten")]
    [InlineData("230V")]
    [InlineData("Battery")]
    [InlineData("Batteri")]
    [InlineData("Batteridriven")]
    [InlineData("Cordless")]
    [InlineData("Sladdlös")]
    [InlineData("Solar")]
    [InlineData("Solenergi")]
    [InlineData("Solcell")]
    [InlineData("Soldriven")]
    [InlineData("Hybrid")]
    [InlineData("Hybriddrift")]
    [InlineData("PHEV")]
    [InlineData("Laddhybrid")]
    [InlineData("Petrol")]
    [InlineData("Bensin")]
    [InlineData("Bensinmotor")]
    [InlineData("Gasoline")]
    [InlineData("Diesel")]
    [InlineData("Dieseldriven")]
    [InlineData("Dieselmotor")]
    [InlineData("Hydrogen")]
    [InlineData("Vätgas")]
    [InlineData("Vätgasdriven")]
    [InlineData("Bränslecell")]
    [InlineData("Pneumatic")]
    [InlineData("Pneumatisk")]
    [InlineData("Tryckluft")]
    [InlineData("Compressed air")]
    [InlineData("Hydraulic")]
    [InlineData("Hydraulisk")]
    [InlineData("Hydrauldriven")]
    [InlineData("Manual")]
    [InlineData("Manuell")]
    [InlineData("Muskelkraft")]
    [InlineData("Pedal")]
    [InlineData("  Electric  ")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(PowerSource.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("nope")]
    [InlineData("anti-matter")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(PowerSource.IsValid(input));

    [Theory]
    [InlineData("El", "Electric")]
    [InlineData("Eldriven", "Electric")]
    [InlineData("Mains", "Electric")]
    [InlineData("Plug-in", "Electric")]
    [InlineData("Cordless", "Battery")]
    [InlineData("Batteri", "Battery")]
    [InlineData("Solenergi", "Solar")]
    [InlineData("Laddhybrid", "Hybrid")]
    [InlineData("Bensin", "Petrol")]
    [InlineData("Gasoline", "Petrol")]
    [InlineData("Vätgas", "Hydrogen")]
    [InlineData("Bränslecell", "Hydrogen")]
    [InlineData("Tryckluft", "Pneumatic")]
    [InlineData("Hydraulisk", "Hydraulic")]
    [InlineData("Muskelkraft", "Manual")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, PowerSource.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(PowerSource.Battery, PowerSource.Parse("cordless"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => PowerSource.Parse("nope"));

    [Fact]
    public void IsElectric_TrueForElectricalSources()
    {
        Assert.True(PowerSource.Electric.IsElectric);
        Assert.True(PowerSource.Battery.IsElectric);
        Assert.True(PowerSource.Solar.IsElectric);
        Assert.True(PowerSource.Hybrid.IsElectric);
        Assert.True(PowerSource.Hydrogen.IsElectric);
        Assert.False(PowerSource.Petrol.IsElectric);
        Assert.False(PowerSource.Diesel.IsElectric);
        Assert.False(PowerSource.Pneumatic.IsElectric);
        Assert.False(PowerSource.Hydraulic.IsElectric);
        Assert.False(PowerSource.Manual.IsElectric);
    }

    [Fact]
    public void IsCombustion_TrueForFossilFuels()
    {
        Assert.True(PowerSource.Petrol.IsCombustion);
        Assert.True(PowerSource.Diesel.IsCombustion);
        Assert.True(PowerSource.Hybrid.IsCombustion);
        Assert.False(PowerSource.Electric.IsCombustion);
        Assert.False(PowerSource.Battery.IsCombustion);
        Assert.False(PowerSource.Manual.IsCombustion);
    }

    [Fact]
    public void RequiresFuel_TrueForFuelConsumingSources()
    {
        Assert.True(PowerSource.Petrol.RequiresFuel);
        Assert.True(PowerSource.Diesel.RequiresFuel);
        Assert.True(PowerSource.Hybrid.RequiresFuel);
        Assert.True(PowerSource.Hydrogen.RequiresFuel);
        Assert.False(PowerSource.Electric.RequiresFuel);
        Assert.False(PowerSource.Battery.RequiresFuel);
        Assert.False(PowerSource.Manual.RequiresFuel);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("********", PowerSource.Electric.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = PowerSource.Parse("Electric");
        var b = PowerSource.Parse("El");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Power Source", PowerSource.TypeInfo.EnglishName);
        Assert.Equal("Strömkälla", PowerSource.TypeInfo.LocalizedName);
    }
}
