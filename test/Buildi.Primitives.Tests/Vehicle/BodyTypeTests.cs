using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class BodyTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(24, BodyType.All.Count);

    [Theory]
    [InlineData("Sedan")]
    [InlineData("sedan")]
    [InlineData("Saloon")]
    [InlineData("Hatchback")]
    [InlineData("Halvkombi")]
    [InlineData("Kombi")]
    [InlineData("Stationwagon")]
    [InlineData("Station wagon")]
    [InlineData("Estate")]
    [InlineData("SUV")]
    [InlineData("Suvkombi")]
    [InlineData("Stadsjeep")]
    [InlineData("Crossover")]
    [InlineData("CUV")]
    [InlineData("Coupe")]
    [InlineData("Coupé")]
    [InlineData("Convertible")]
    [InlineData("Cabriolet")]
    [InlineData("Cabrio")]
    [InlineData("Roadster")]
    [InlineData("Spider")]
    [InlineData("MPV")]
    [InlineData("Familjebuss")]
    [InlineData("Minivan")]
    [InlineData("Van")]
    [InlineData("Skåpbil")]
    [InlineData("Pickup")]
    [InlineData("Pick-up")]
    [InlineData("Truck")]
    [InlineData("Lastbil")]
    [InlineData("Tung lastbil")]
    [InlineData("HGV")]
    [InlineData("Light truck")]
    [InlineData("Lätt lastbil")]
    [InlineData("Light commercial vehicle")]
    [InlineData("LCV")]
    [InlineData("N1")]
    [InlineData("Off-road")]
    [InlineData("Off-road vehicle")]
    [InlineData("Off-roader")]
    [InlineData("Terrängbil")]
    [InlineData("Terrängfordon")]
    [InlineData("ORV")]
    [InlineData("Bus")]
    [InlineData("Buss")]
    [InlineData("Minibus")]
    [InlineData("Minibuss")]
    [InlineData("Microcar")]
    [InlineData("Mopedbil")]
    [InlineData("Limousine")]
    [InlineData("Limo")]
    [InlineData("Targa")]
    [InlineData("Trailer")]
    [InlineData("Släp")]
    [InlineData("Släpvagn")]
    [InlineData("Semitrailer")]
    [InlineData("Motorhome")]
    [InlineData("Husbil")]
    [InlineData("Camper")]
    [InlineData("RV")]
    [InlineData("Tractor")]
    [InlineData("Traktor")]
    [InlineData("Lantbrukstraktor")]
    [InlineData("Tipper")]
    [InlineData("Tippbil")]
    [InlineData("Dump truck")]
    [InlineData("Dumper")]
    [InlineData("Bergsdumper")]
    [InlineData("Articulated dumper")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(BodyType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("squirrel")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BodyType.IsValid(input));

    [Theory]
    [InlineData("Kombi", "Stationwagon")]
    [InlineData("Halvkombi", "Hatchback")]
    [InlineData("Suvkombi", "SUV")]
    [InlineData("Cabriolet", "Convertible")]
    [InlineData("Saloon", "Sedan")]
    [InlineData("Pick-up", "Pickup")]
    [InlineData("Skåpbil", "Van")]
    [InlineData("Släp", "Trailer")]
    [InlineData("Husbil", "Motorhome")]
    [InlineData("Traktor", "Tractor")]
    [InlineData("Tippbil", "Tipper")]
    [InlineData("Bergsdumper", "Dumper")]
    [InlineData("Lätt lastbil", "Light truck")]
    [InlineData("LCV", "Light truck")]
    [InlineData("Tung lastbil", "Truck")]
    [InlineData("Terrängbil", "Off-road")]
    [InlineData("Off-roader", "Off-road")]
    [InlineData("Minibuss", "Bus")]
    [InlineData("Articulated dump truck", "Dumper")]
    [InlineData("Dump truck", "Tipper")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    [InlineData("Familjebil", null)]
    [InlineData("Öppen", null)]
    [InlineData("4-door", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, BodyType.Normalize(input));

    [Fact]
    public void Normalize_DistinguishesTipperFromDumper()
    {
        Assert.Equal("Tipper", BodyType.Normalize("Tippbil"));
        Assert.Equal("Dumper", BodyType.Normalize("Bergsdumper"));
        Assert.Equal("Dumper", BodyType.Normalize("Articulated dumper"));
    }

    [Fact]
    public void Normalize_DistinguishesTruckFromLightTruck()
    {
        Assert.Equal("Truck", BodyType.Normalize("Lastbil"));
        Assert.Equal("Light truck", BodyType.Normalize("Lätt lastbil"));
    }

    [Fact]
    public void Normalize_DistinguishesSuvFromOffRoad()
    {
        Assert.Equal("SUV", BodyType.Normalize("Suvkombi"));
        Assert.Equal("Off-road", BodyType.Normalize("Terrängbil"));
    }

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(BodyType.Stationwagon, BodyType.Parse("Kombi"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => BodyType.Parse("nope"));

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal(new string('*', "Sedan".Length), BodyType.Sedan.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = BodyType.Parse("Kombi");
        var b = BodyType.Parse("Stationwagon");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Body Type", BodyType.TypeInfo.EnglishName);
        Assert.Equal("Karosstyp", BodyType.TypeInfo.LocalizedName);
    }
}
