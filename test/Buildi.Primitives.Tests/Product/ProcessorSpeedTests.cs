using Buildi.Primitives.Measurement;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ProcessorSpeedTests
{
    [Theory]
    [InlineData("3.5")]
    [InlineData("3.5 GHz")]
    [InlineData("2400 MHz")]
    [InlineData("1.8 GHz")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ProcessorSpeed.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-3.5 GHz")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ProcessorSpeed.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToGHz()
    {
        Assert.True(ProcessorSpeed.TryParse("3.5", out var result));
        Assert.Equal(3.5m, result!.Gigahertz);
        Assert.Equal("3.5 GHz", result.Value);
    }

    [Fact]
    public void TryParse_WithUnit_UsesSpecifiedUnit()
    {
        Assert.True(ProcessorSpeed.TryParse("2400 MHz", out var result));
        Assert.Equal(2400m, result!.Megahertz);
    }

    [Theory]
    [InlineData("3.5 GHz", "3.5 GHz")]
    [InlineData("2400 MHz", "2400 MHz")]
    [InlineData("3.5", "3.5 GHz")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ProcessorSpeed.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("3500 MHz", ProcessorSpeed.Format("3.5 GHz", unit: FrequencyUnit.Megahertz));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = ProcessorSpeed.Parse("2.4 GHz");
        var b = ProcessorSpeed.Parse("3.5 GHz");
        Assert.True(a < b);
    }

    [Fact]
    public void Equality_SameSpeed()
    {
        var a = ProcessorSpeed.Parse("3500 MHz");
        var b = ProcessorSpeed.Parse("3.5 GHz");
        Assert.True(a == b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ProcessorSpeed.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var ps = ProcessorSpeed.Create(3.5m, FrequencyUnit.Gigahertz);
        Assert.Equal(3.5m, ps.Gigahertz);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ProcessorSpeed.Create(0m, FrequencyUnit.Gigahertz));
        Assert.Throws<ArgumentOutOfRangeException>(() => ProcessorSpeed.Create(-1m, FrequencyUnit.Gigahertz));
    }

    [Fact]
    public void FromGigahertz_Works()
    {
        var ps = ProcessorSpeed.FromGigahertz(3.5m);
        Assert.Equal(3.5m, ps.Gigahertz);
    }

    [Fact]
    public void FromMegahertz_Works()
    {
        var ps = ProcessorSpeed.FromMegahertz(3500);
        Assert.Equal(3500m, ps.Megahertz);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = ProcessorSpeed.FromGigahertz(3);
        var fromString = ProcessorSpeed.Parse("3 GHz");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ProcessorSpeed.Parse("1.8 GHz");
        var b = ProcessorSpeed.Parse("3.2 GHz");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ProcessorSpeed.Parse("3.5 GHz");
        Assert.Equal(1, a.CompareTo(null));
    }
}
