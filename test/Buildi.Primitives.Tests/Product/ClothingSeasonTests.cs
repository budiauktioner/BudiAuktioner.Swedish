using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ClothingSeasonTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(5, ClothingSeason.All.Count);

    [Theory]
    [InlineData("Spring")]
    [InlineData("spring")]
    [InlineData("SPRING")]
    [InlineData("Vår")]
    [InlineData("vår")]
    [InlineData("Vårsäsong")]
    [InlineData("springtime")]
    [InlineData("SS")]
    [InlineData("Summer")]
    [InlineData("Sommar")]
    [InlineData("Sommarsäsong")]
    [InlineData("Autumn")]
    [InlineData("Höst")]
    [InlineData("Fall")]
    [InlineData("AW")]
    [InlineData("FW")]
    [InlineData("autumn/winter")]
    [InlineData("Winter")]
    [InlineData("Vinter")]
    [InlineData("Vintersäsong")]
    [InlineData("All-Season")]
    [InlineData("All Season")]
    [InlineData("Allseason")]
    [InlineData("All-year")]
    [InlineData("Year-round")]
    [InlineData("Året runt")]
    [InlineData("Året om")]
    [InlineData("Helår")]
    [InlineData("4-season")]
    [InlineData("Four-season")]
    [InlineData("  Spring  ")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(ClothingSeason.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("nope")]
    [InlineData("monsoon")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(ClothingSeason.IsValid(input));

    [Theory]
    [InlineData("Vår", "Spring")]
    [InlineData("vår", "Spring")]
    [InlineData("springtime", "Spring")]
    [InlineData("Sommar", "Summer")]
    [InlineData("Höst", "Autumn")]
    [InlineData("Fall", "Autumn")]
    [InlineData("AW", "Autumn")]
    [InlineData("Vinter", "Winter")]
    [InlineData("Året runt", "All-Season")]
    [InlineData("Helår", "All-Season")]
    [InlineData("year round", "All-Season")]
    [InlineData("All season", "All-Season")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, ClothingSeason.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(ClothingSeason.Spring, ClothingSeason.Parse("vår"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => ClothingSeason.Parse("nope"));

    [Fact]
    public void IsAllSeason_TrueOnlyForAllSeasonEntry()
    {
        Assert.True(ClothingSeason.AllSeason.IsAllSeason);
        Assert.False(ClothingSeason.Spring.IsAllSeason);
        Assert.False(ClothingSeason.Summer.IsAllSeason);
        Assert.False(ClothingSeason.Autumn.IsAllSeason);
        Assert.False(ClothingSeason.Winter.IsAllSeason);
    }

    [Fact]
    public void MonthsCovered_MatchesNorthernHemisphere()
    {
        Assert.Equal([3, 4, 5], ClothingSeason.Spring.MonthsCovered);
        Assert.Equal([6, 7, 8], ClothingSeason.Summer.MonthsCovered);
        Assert.Equal([9, 10, 11], ClothingSeason.Autumn.MonthsCovered);
        Assert.Equal([12, 1, 2], ClothingSeason.Winter.MonthsCovered);
        Assert.Equal(12, ClothingSeason.AllSeason.MonthsCovered.Count);
    }

    [Fact]
    public void CompareTo_OrdersBySortOrder()
    {
        Assert.True(ClothingSeason.Spring < ClothingSeason.Summer);
        Assert.True(ClothingSeason.Summer < ClothingSeason.Autumn);
        Assert.True(ClothingSeason.Autumn < ClothingSeason.Winter);
        Assert.True(ClothingSeason.Winter < ClothingSeason.AllSeason);
    }

    [Fact]
    public void ToMaskedString_PreservesHyphen()
    {
        Assert.Equal("******", ClothingSeason.Spring.ToMaskedString());
        Assert.Equal("***-******", ClothingSeason.AllSeason.ToMaskedString());
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid() =>
        Assert.Equal("x", ClothingSeason.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));

    [Fact]
    public void Equality()
    {
        var a = ClothingSeason.Parse("Vår");
        var b = ClothingSeason.Parse("Spring");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Clothing Season", ClothingSeason.TypeInfo.EnglishName);
        Assert.Equal("Säsong", ClothingSeason.TypeInfo.LocalizedName);
    }
}
