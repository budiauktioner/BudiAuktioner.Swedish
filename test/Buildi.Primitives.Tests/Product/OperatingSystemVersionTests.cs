using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class OperatingSystemVersionTests
{
    [Theory]
    [InlineData("10")]
    [InlineData("11")]
    [InlineData("14.5")]
    [InlineData("10.15.7")]
    [InlineData("22.04")]
    [InlineData("v10")]
    [InlineData("V14.5")]
    [InlineData("17.4.1")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(OperatingSystemVersion.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("..")]
    [InlineData("1.2.3.4.5")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(OperatingSystemVersion.IsValid(input));
    }

    [Theory]
    [InlineData("10", 10, null, null, null)]
    [InlineData("14.5", 14, 5, null, null)]
    [InlineData("10.15.7", 10, 15, 7, null)]
    [InlineData("v11", 11, null, null, null)]
    [InlineData("22.04", 22, 4, null, null)]
    public void TryParse_ReturnsExpectedParts(string input, int major, int? minor, int? patch, int? build)
    {
        Assert.True(OperatingSystemVersion.TryParse(input, out var result));
        Assert.Equal(major, result!.Major);
        Assert.Equal(minor, result.Minor);
        Assert.Equal(patch, result.Patch);
        Assert.Equal(build, result.Build);
    }

    [Theory]
    [InlineData("v10", "10")]
    [InlineData("V14.5", "14.5")]
    [InlineData(" 11 ", "11")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OperatingSystemVersion.Format(input));
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OperatingSystemVersion.Parse(input));
    }

    [Fact]
    public void Comparison_Works()
    {
        var v10 = OperatingSystemVersion.Parse("10");
        var v11 = OperatingSystemVersion.Parse("11");
        var v10_5 = OperatingSystemVersion.Parse("10.5");
        Assert.True(v10 < v11);
        Assert.True(v10 < v10_5);
        Assert.True(v11 > v10_5);
    }

    [Fact]
    public void Equality_SameVersion()
    {
        var a = OperatingSystemVersion.Parse("14.5");
        var b = OperatingSystemVersion.Parse("v14.5");
        Assert.True(a == b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = OperatingSystemVersion.Parse("10.0");
        var b = OperatingSystemVersion.Parse("11.0");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = OperatingSystemVersion.Parse("22.04");
        Assert.Equal(1, a.CompareTo(null));
    }
}
