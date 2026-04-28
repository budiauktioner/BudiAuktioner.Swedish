using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class TireTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(10, TireType.All.Count);

    [Theory]
    [InlineData("Summer")]
    [InlineData("Sommardäck")]
    [InlineData("Sommar")]
    [InlineData("Winter (studded)")]
    [InlineData("Studded")]
    [InlineData("Dubbdäck")]
    [InlineData("Vinterdäck (dubb)")]
    [InlineData("Winter (friction)")]
    [InlineData("Friction")]
    [InlineData("Friktionsdäck")]
    [InlineData("Vinterdäck (friktion)")]
    [InlineData("All-season")]
    [InlineData("All season")]
    [InlineData("Helårsdäck")]
    [InlineData("All-terrain")]
    [InlineData("AT")]
    [InlineData("Mud-terrain")]
    [InlineData("MT")]
    [InlineData("Track")]
    [InlineData("Racing")]
    [InlineData("Slick")]
    [InlineData("Industrial")]
    [InlineData("Massivdäck")]
    [InlineData("Agricultural")]
    [InlineData("Traktordäck")]
    [InlineData("Spare")]
    [InlineData("Reservdäck")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(TireType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(TireType.IsValid(input));

    [Theory]
    [InlineData("Sommardäck", "Summer")]
    [InlineData("Dubbdäck", "Winter (studded)")]
    [InlineData("Friktionsdäck", "Winter (friction)")]
    [InlineData("Helårsdäck", "All-season")]
    [InlineData("Reservdäck", "Spare")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, TireType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(TireType.Summer, TireType.Parse("Sommardäck"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => TireType.Parse("nope"));

    [Fact]
    public void ToMaskedString_PreservesParenthesesAndSpaces()
    {
        Assert.Equal("******", TireType.Summer.ToMaskedString());
        Assert.Equal("****** (*******)", TireType.WinterStudded.ToMaskedString());
        Assert.Equal("***-******", TireType.AllSeason.ToMaskedString());
    }

    [Fact]
    public void Equality()
    {
        var a = TireType.Parse("Sommardäck");
        var b = TireType.Parse("Summer");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Tire Type", TireType.TypeInfo.EnglishName);
        Assert.Equal("Däcktyp", TireType.TypeInfo.LocalizedName);
    }
}
