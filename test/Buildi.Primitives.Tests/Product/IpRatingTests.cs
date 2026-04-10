using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class IpRatingTests
{
    [Theory]
    [InlineData("IP65")]
    [InlineData("IP67")]
    [InlineData("IPX4")]
    [InlineData("IP5X")]
    [InlineData("IP00")]
    [InlineData("IP69")]
    [InlineData("ip65")]
    [InlineData("IP 65")]
    [InlineData("IP-65")]
    [InlineData("Ip65")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(IpRating.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("IP")]
    [InlineData("IP6")]
    [InlineData("IP655")]
    [InlineData("IP7X")]
    [InlineData("65")]
    [InlineData("XX65")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(IpRating.IsValid(input));
    }

    [Theory]
    [InlineData("IP65", '6', '5')]
    [InlineData("IP67", '6', '7')]
    [InlineData("IPX4", 'X', '4')]
    [InlineData("IP5X", '5', 'X')]
    [InlineData("IP00", '0', '0')]
    [InlineData("ip65", '6', '5')]
    [InlineData("IP 65", '6', '5')]
    [InlineData("ipx4", 'X', '4')]
    public void TryParse_ReturnsExpectedProperties(string input, char expectedSolids, char expectedLiquids)
    {
        Assert.True(IpRating.TryParse(input, out var result));
        Assert.Equal(expectedSolids, result!.SolidsProtection);
        Assert.Equal(expectedLiquids, result.LiquidsProtection);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(IpRating.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("IP")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => IpRating.Parse(input));
    }

    [Theory]
    [InlineData("IP65", "IP65")]
    [InlineData("ip65", "IP65")]
    [InlineData("IP 65", "IP65")]
    [InlineData("IP-65", "IP65")]
    [InlineData("  IP65  ", "IP65")]
    [InlineData("IPX4", "IPX4")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, IpRating.Format(input));
    }

    [Theory]
    [InlineData("IP65", "IP65")]
    [InlineData("ip65", "IP65")]
    [InlineData("IP 65", "IP65")]
    [InlineData("IPX4", "IPX4")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, IpRating.Normalize(input));
    }

    [Theory]
    [InlineData("IP65", "IP65")]
    [InlineData("IPX4", "IPX4")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var rating = IpRating.Parse(input);
        Assert.Equal(expected, rating.ToString());
    }

    [Theory]
    [InlineData("IP65", "IP65")]
    [InlineData("IPX4", "IPX4")]
    public void ToNormalizedString_ReturnsCanonicalForm(string input, string expected)
    {
        var rating = IpRating.Parse(input);
        Assert.Equal(expected, rating.ToNormalizedString());
    }

    [Fact]
    public void IsNormalized_True_ForNormalizedInput()
    {
        Assert.True(IpRating.IsNormalized("IP65"));
        Assert.True(IpRating.IsNormalized("IPX4"));
    }

    [Fact]
    public void IsNormalized_False_ForUnnormalizedInput()
    {
        Assert.False(IpRating.IsNormalized("ip65"));
        Assert.False(IpRating.IsNormalized("IP 65"));
    }

    [Fact]
    public void SolidsDescription_ReturnsExpected()
    {
        Assert.Equal("Dust tight", IpRating.Parse("IP65").SolidsDescription);
        Assert.Equal("Dust protected", IpRating.Parse("IP54").SolidsDescription);
        Assert.Equal("No protection", IpRating.Parse("IP00").SolidsDescription);
        Assert.Equal("Not tested", IpRating.Parse("IPX4").SolidsDescription);
    }

    [Fact]
    public void LiquidsDescription_ReturnsExpected()
    {
        Assert.Equal("Water jets", IpRating.Parse("IP65").LiquidsDescription);
        Assert.Equal("Temporary immersion", IpRating.Parse("IP67").LiquidsDescription);
        Assert.Equal("Continuous immersion", IpRating.Parse("IP68").LiquidsDescription);
        Assert.Equal("High-pressure/steam cleaning", IpRating.Parse("IP69").LiquidsDescription);
        Assert.Equal("Not tested", IpRating.Parse("IP5X").LiquidsDescription);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = IpRating.Parse("IP44");
        var b = IpRating.Parse("IP65");
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = IpRating.Parse("IP65");
        var b = IpRating.Parse("ip65");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue()
    {
        var a = IpRating.Parse("IP65");
        var b = IpRating.Parse("IP67");
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Format_Fallback_ReturnsNull_ForInvalid()
    {
        Assert.Null(IpRating.Format("invalid"));
    }

    [Fact]
    public void Format_Fallback_ReturnsTrimmedInput_WhenEnabled()
    {
        Assert.Equal("invalid", IpRating.Format("  invalid  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_Fallback_ReturnsNull_ForNull()
    {
        Assert.Null(IpRating.Normalize(null));
    }

    [Fact]
    public void Value_ReturnsNormalizedCode()
    {
        var rating = IpRating.Parse("ip 65");
        Assert.Equal("IP65", rating.Value);
    }

    [Fact]
    public void AllSolidsLevels_ParseCorrectly()
    {
        for (var c = '0'; c <= '6'; c++)
        {
            Assert.True(IpRating.IsValid($"IP{c}0"));
        }
        Assert.True(IpRating.IsValid("IPX0"));
    }

    [Fact]
    public void AllLiquidsLevels_ParseCorrectly()
    {
        for (var c = '0'; c <= '9'; c++)
        {
            Assert.True(IpRating.IsValid($"IP6{c}"));
        }
        Assert.True(IpRating.IsValid("IP6X"));
    }
}
