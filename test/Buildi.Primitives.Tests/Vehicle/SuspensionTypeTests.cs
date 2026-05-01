using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class SuspensionTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(19, SuspensionType.All.Count);

    [Theory]
    [InlineData("Coil spring")]
    [InlineData("coil")]
    [InlineData("Spiralfjäder")]
    [InlineData("Skruvfjäder")]
    [InlineData("Fjäder")]
    [InlineData("Leaf spring")]
    [InlineData("Bladfjäder")]
    [InlineData("Blad")]
    [InlineData("Air")]
    [InlineData("Air suspension")]
    [InlineData("Luftfjädring")]
    [InlineData("Luft")]
    [InlineData("Air/Air")]
    [InlineData("Luft/Luft")]
    [InlineData("Air/Leaf")]
    [InlineData("Luft/Blad")]
    [InlineData("Leaf/Leaf")]
    [InlineData("Blad/Blad")]
    [InlineData("Parabolic")]
    [InlineData("Paraboll")]
    [InlineData("AirFront")]
    [InlineData("Luftfjädring fram")]
    [InlineData("AirRear")]
    [InlineData("Luftfjädring bak")]
    [InlineData("Pneumatic")]
    [InlineData("Hydropneumatic")]
    [InlineData("Hydropneumatisk")]
    [InlineData("Torsion bar")]
    [InlineData("Torsion")]
    [InlineData("Torsionsstav")]
    [InlineData("Adaptive")]
    [InlineData("Active suspension")]
    [InlineData("Magnetic ride")]
    [InlineData("MagneRide")]
    [InlineData("Independent")]
    [InlineData("MacPherson")]
    [InlineData("MacPherson strut")]
    [InlineData("Double wishbone")]
    [InlineData("A-arms")]
    [InlineData("Multi-link")]
    [InlineData("Multilink")]
    [InlineData("Solid axle")]
    [InlineData("Live axle")]
    [InlineData("Stel bakaxel")]
    [InlineData("Rigid")]
    [InlineData("Ofjädrad")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(SuspensionType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(SuspensionType.IsValid(input));

    [Theory]
    [InlineData("Spiralfjäder", "Coil spring")]
    [InlineData("Fjäder", "Coil spring")]
    [InlineData("Bladfjäder", "Leaf spring")]
    [InlineData("Blad", "Leaf spring")]
    [InlineData("Luftfjädring", "Air")]
    [InlineData("Luft", "Air")]
    [InlineData("Luft/Luft", "Air/Air")]
    [InlineData("Luft/Blad", "Air/Leaf")]
    [InlineData("Blad/Blad", "Leaf/Leaf")]
    [InlineData("Paraboll", "Parabolic")]
    [InlineData("Luftfjädring fram", "AirFront")]
    [InlineData("Luftfjädring bak", "AirRear")]
    [InlineData("Pneumatic", "Air")]
    [InlineData("MagneRide", "Magnetic ride")]
    [InlineData("Multilink", "Multi-link")]
    [InlineData("Live axle", "Solid axle")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, SuspensionType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(SuspensionType.Air, SuspensionType.Parse("Luftfjädring"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => SuspensionType.Parse("nope"));

    [Fact]
    public void ToMaskedString_PreservesSpaces()
    {
        Assert.Equal("**** ******", SuspensionType.CoilSpring.ToMaskedString());
        Assert.Equal("***", SuspensionType.Air.ToMaskedString());
        Assert.Equal("***/****", SuspensionType.AirLeaf.ToMaskedString());
    }

    [Fact]
    public void Equality()
    {
        var a = SuspensionType.Parse("Air");
        var b = SuspensionType.Parse("Pneumatic");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Suspension Type", SuspensionType.TypeInfo.EnglishName);
        Assert.Equal("Fjädring", SuspensionType.TypeInfo.LocalizedName);
    }
}
