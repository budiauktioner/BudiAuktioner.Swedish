using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class BoatHullMaterialTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(9, BoatHullMaterial.All.Count);

    [Theory]
    [InlineData("Glasfiber")]
    [InlineData("glasfiber")]
    [InlineData("Fiberglass")]
    [InlineData("Fibreglass")]
    [InlineData("GRP")]
    [InlineData("GFK")]
    [InlineData("Aluminium")]
    [InlineData("Aluminum")]
    [InlineData("Stål")]
    [InlineData("stal")]
    [InlineData("Steel")]
    [InlineData("Trä")]
    [InlineData("tra")]
    [InlineData("Wood")]
    [InlineData("Mahogny")]
    [InlineData("Plast")]
    [InlineData("Polyeten")]
    [InlineData("Roplene")]
    [InlineData("Kolfiber")]
    [InlineData("Carbon fiber")]
    [InlineData("PVC")]
    [InlineData("Hypalon")]
    [InlineData("Gummibåt")]
    [InlineData("RIB")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(BoatHullMaterial.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("granit")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BoatHullMaterial.IsValid(input));

    [Theory]
    [InlineData("Glasfiber", "Fiberglass")]
    [InlineData("GRP", "Fiberglass")]
    [InlineData("Aluminium", "Aluminum")]
    [InlineData("Stål", "Steel")]
    [InlineData("Trä", "Wood")]
    [InlineData("Hypalon", "Inflatable")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, BoatHullMaterial.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(BoatHullMaterial.Fiberglass, BoatHullMaterial.Parse("Glasfiber"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => BoatHullMaterial.Parse("nope"));

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal(new string('*', "Fiberglass".Length), BoatHullMaterial.Fiberglass.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = BoatHullMaterial.Parse("Glasfiber");
        var b = BoatHullMaterial.Parse("Fiberglass");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
