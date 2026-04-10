using Buildi.Primitives.Property;

namespace Buildi.Primitives.Tests.Property;

public class SwedishPropertyDesignationTests
{
    [Theory]
    [InlineData("Stockholm Soder 75:2", "Stockholm Soder", "75:2", 75, 2)]
    [InlineData("Gavle Olsbacka 11:1", "Gavle Olsbacka", "11:1", 11, 1)]
    [InlineData("  stockholm  soder   75 : 2  ", "Stockholm Soder", "75:2", 75, 2)]
    [InlineData("Vasteras Staden 1:75", "Vasteras Staden", "1:75", 1, 75)]
    public void TryParse_ReturnsExpectedComponents(
        string input,
        string expectedName,
        string expectedRegisterNumber,
        int expectedBlockNumber,
        int expectedUnitNumber)
    {
        var ok = SwedishPropertyDesignation.TryParse(input, out var designation);

        Assert.True(ok);
        Assert.NotNull(designation);
        Assert.Equal(expectedName, designation!.DesignationName);
        Assert.Equal(expectedRegisterNumber, designation.RegisterNumber);
        Assert.Equal(expectedBlockNumber, designation.BlockNumber);
        Assert.Equal(expectedUnitNumber, designation.UnitNumber);
        Assert.Equal($"{expectedName} {expectedRegisterNumber}", designation.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("75:2")]
    [InlineData("Stockholm Soder")]
    [InlineData("Stockholm Soder 75")]
    [InlineData("Stockholm Soder 75-2")]
    [InlineData("Stockholm Soder 0:2")]
    [InlineData("Stockholm Soder 75:0")]
    [InlineData("Stockholm / Soder 75:2")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishPropertyDesignation.IsValid(input));
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => SwedishPropertyDesignation.Parse("Stockholm Soder"));
    }

    [Fact]
    public void Format_And_Normalize_ReturnExpectedValues()
    {
        Assert.Equal("Stockholm Soder 75:2", SwedishPropertyDesignation.Format(" stockholm soder 75 : 2 "));
        Assert.Equal("Stockholm Soder 75:2", SwedishPropertyDesignation.Normalize(" stockholm soder 75 : 2 "));
        Assert.Equal("not a designation", SwedishPropertyDesignation.Format("  not a designation  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SwedishPropertyDesignation.Format(" "));
    }

    [Fact]
    public void ToString_And_ToNormalizedString_ReturnValue()
    {
        var designation = SwedishPropertyDesignation.Parse("Gavle Olsbacka 11:1");

        Assert.Equal("Gavle Olsbacka 11:1", designation.ToString());
        Assert.Equal("Gavle Olsbacka 11:1", designation.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        var b = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        var b = SwedishPropertyDesignation.Parse("Gavle Olsbacka 11:1");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishPropertyDesignation.Parse("Gavle Olsbacka 11:1");
        var b = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishPropertyDesignation.Parse("Stockholm Soder 75:2");
        Assert.Equal(1, a.CompareTo(null));
    }
}
