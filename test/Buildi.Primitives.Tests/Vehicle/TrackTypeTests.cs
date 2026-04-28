using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class TrackTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(6, TrackType.All.Count);

    [Theory]
    [InlineData("Steel")]
    [InlineData("Steel tracks")]
    [InlineData("Stålband")]
    [InlineData("Stål")]
    [InlineData("Rubber")]
    [InlineData("Rubber tracks")]
    [InlineData("Gummiband")]
    [InlineData("Polyurethane")]
    [InlineData("PU")]
    [InlineData("Polyuretan")]
    [InlineData("Rubber pad")]
    [InlineData("Steel with rubber pads")]
    [InlineData("Half-track")]
    [InlineData("Halftrack")]
    [InlineData("Halvband")]
    [InlineData("Composite")]
    [InlineData("Kompositband")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(TrackType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(TrackType.IsValid(input));

    [Theory]
    [InlineData("Stålband", "Steel")]
    [InlineData("Gummiband", "Rubber")]
    [InlineData("Polyuretan", "Polyurethane")]
    [InlineData("Halftrack", "Half-track")]
    [InlineData("Kompositband", "Composite")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, TrackType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(TrackType.Steel, TrackType.Parse("Stålband"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => TrackType.Parse("nope"));

    [Fact]
    public void ToMaskedString_PreservesHyphens()
    {
        Assert.Equal("*****", TrackType.Steel.ToMaskedString());
        Assert.Equal("****-*****", TrackType.HalfTrack.ToMaskedString());
    }

    [Fact]
    public void Equality()
    {
        var a = TrackType.Parse("Stålband");
        var b = TrackType.Parse("Steel");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Track Type", TrackType.TypeInfo.EnglishName);
        Assert.Equal("Bandtyp", TrackType.TypeInfo.LocalizedName);
    }
}
