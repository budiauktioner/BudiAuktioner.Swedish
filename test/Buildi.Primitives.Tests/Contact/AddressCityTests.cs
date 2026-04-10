using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class AddressCityTests
{
    [Theory]
    [InlineData("Stockholm", "Stockholm")]
    [InlineData("STOCKHOLM", "Stockholm")]
    [InlineData("stockholm", "Stockholm")]
    [InlineData("ÅKERSBERGA", "Åkersberga")]
    [InlineData("saltsjö-boo", "Saltsjö-Boo")]
    [InlineData("  Stockholm  ", "Stockholm")]
    [InlineData("münchen", "München")]
    [InlineData("MÜNCHEN", "München")]
    [InlineData("København", "København")]
    [InlineData("zürich", "Zürich")]
    [InlineData("KRAKÓW", "Kraków")]
    [InlineData("düsseldorf", "Düsseldorf")]
    [InlineData("Bruxelles", "Bruxelles")]
    [InlineData("HELSINKI", "Helsinki")]
    [InlineData("Saint-Étienne", "Saint-Étienne")]
    [InlineData("Málaga", "Málaga")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(AddressCity.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(AddressCity.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Stockholm", true)]
    [InlineData("München", true)]
    [InlineData("København", true)]
    [InlineData("Kraków", true)]
    [InlineData("Saint-Étienne", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, AddressCity.IsValid(input));
    }

    [Theory]
    [InlineData("ÅKERSBERGA", "Åkersberga")]
    [InlineData("A", "A")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, AddressCity.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !AddressCity.IsValid(input)));
    }

    [Theory]
    [InlineData("STOCKHOLM", "Stockholm")]
    [InlineData("saltsjö-boo", "Saltsjö-Boo")]
    [InlineData("münchen", "München")]
    [InlineData("KRAKÓW", "Kraków")]
    [InlineData("A", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, AddressCity.Normalize(input));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => AddressCity.Parse(""));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var city = AddressCity.Parse("Stockholm");
        Assert.Equal("Stockholm", city.ToString());
        Assert.Equal("Stockholm", city.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = AddressCity.Parse("STOCKHOLM");
        var b = AddressCity.Parse("Stockholm");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = AddressCity.Parse("Stockholm");
        var b = AddressCity.Parse("München");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = AddressCity.Parse("Stockholm");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = AddressCity.Parse("HELSINKI");
        var b = AddressCity.Parse("Stockholm");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = AddressCity.Parse("Bruxelles");
        Assert.Equal(1, a.CompareTo(null));
    }
}
