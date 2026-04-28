using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class DisplayPanelTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(16, DisplayPanelType.All.Count);

    [Theory]
    [InlineData("LCD")]
    [InlineData("lcd")]
    [InlineData("OLED")]
    [InlineData("oled")]
    [InlineData("AMOLED")]
    [InlineData("Super AMOLED")]
    [InlineData("QLED")]
    [InlineData("QD-OLED")]
    [InlineData("MiniLED")]
    [InlineData("Mini-LED")]
    [InlineData("MicroLED")]
    [InlineData("Micro LED")]
    [InlineData("IPS")]
    [InlineData("In-Plane Switching")]
    [InlineData("VA")]
    [InlineData("MVA")]
    [InlineData("PVA")]
    [InlineData("TN")]
    [InlineData("TFT")]
    [InlineData("Plasma")]
    [InlineData("CRT")]
    [InlineData("Katodstrålerör")]
    [InlineData("E-Ink")]
    [InlineData("ePaper")]
    [InlineData("Elektroniskt bläck")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(DisplayPanelType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("paper")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(DisplayPanelType.IsValid(input));

    [Theory]
    [InlineData("oled", "OLED")]
    [InlineData("Super AMOLED", "AMOLED")]
    [InlineData("Mini LED", "MiniLED")]
    [InlineData("Quantum Dot LED", "QLED")]
    [InlineData("Quantum Dot OLED", "QD-OLED")]
    [InlineData("Cathode ray tube", "CRT")]
    [InlineData("Electronic paper", "E-Ink")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, DisplayPanelType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(DisplayPanelType.Oled, DisplayPanelType.Parse("oled"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => DisplayPanelType.Parse("nope"));

    [Fact]
    public void Family_GroupsRelatedPanels()
    {
        Assert.Equal("OLED", DisplayPanelType.Amoled.Family);
        Assert.Equal("OLED", DisplayPanelType.QdOled.Family);
        Assert.Equal("LCD", DisplayPanelType.Ips.Family);
        Assert.Equal("LCD", DisplayPanelType.Va.Family);
        Assert.Equal("LCD", DisplayPanelType.MiniLed.Family);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("****", DisplayPanelType.Oled.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = DisplayPanelType.Parse("OLED");
        var b = DisplayPanelType.Parse("oled");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Display Panel Type", DisplayPanelType.TypeInfo.EnglishName);
        Assert.Equal("Skärmpaneltyp", DisplayPanelType.TypeInfo.LocalizedName);
    }
}
