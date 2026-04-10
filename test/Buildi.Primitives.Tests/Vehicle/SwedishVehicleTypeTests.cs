using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class SwedishVehicleTypeTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(17, SwedishVehicleType.All.Count);
    }

    [Theory]
    [InlineData("PB", "PB", "Passenger car", "Personbil")]
    [InlineData("LB", "LB", "Truck", "Lastbil")]
    [InlineData("BU", "BU", "Bus", "Buss")]
    [InlineData("MC", "MC", "Motorcycle", "Motorcykel")]
    [InlineData("MR", "MR", "Moped class I", "Moped klass I")]
    [InlineData("EU-M", "EU-M", "Moped class II", "EU-moped")]
    [InlineData("SL", "SL", "Trailer", "Släpvagn")]
    [InlineData("SA", "SA", "Semi-trailer", "Påhängsvagn")]
    [InlineData("HV", "HV", "Caravan", "Husvagn")]
    [InlineData("HB", "HB", "Motorhome", "Husbil")]
    [InlineData("TK", "TK", "All-terrain vehicle", "Terrängvagn")]
    [InlineData("TM", "TM", "Snowmobile", "Terrängskoter")]
    [InlineData("TH", "TH", "ATV", "Terränghjuling")]
    [InlineData("AT", "AT", "A-tractor", "A-traktor")]
    [InlineData("EP", "EP", "Power vehicle cl. I", "Motorredskap klass I")]
    [InlineData("MRK2", "MRK2", "Power vehicle cl. II", "Motorredskap klass II")]
    [InlineData("LL", "LL", "Light truck", "Lätt lastbil")]
    public void StaticInstances_HaveExpectedProperties(
        string code, string expectedValue, string expectedEnglish, string expectedSwedish)
    {
        var e = SwedishVehicleType.All.Single(x => x.Code == code);
        Assert.Equal(expectedValue, e.Value);
        Assert.Equal(code, e.Code);
        Assert.Equal(expectedEnglish, e.EnglishName);
        Assert.Equal(expectedSwedish, e.LocalizedName);
    }

    [Theory]
    [InlineData("PB")]
    [InlineData("pb")]
    [InlineData("Personbil")]
    [InlineData("PERSONBIL")]
    [InlineData("Passenger car")]
    [InlineData("Car")]
    [InlineData("Bil")]
    [InlineData("LB")]
    [InlineData("Lastbil")]
    [InlineData("Lorry")]
    [InlineData("Tung lastbil")]
    [InlineData("BU")]
    [InlineData("Buss")]
    [InlineData("Coach")]
    [InlineData("Linjebuss")]
    [InlineData("Turistbuss")]
    [InlineData("MC")]
    [InlineData("Motorcykel")]
    [InlineData("Motorbike")]
    [InlineData("MR")]
    [InlineData("Moped klass I")]
    [InlineData("Moped klass 1")]
    [InlineData("Moped I")]
    [InlineData("Moped 1")]
    [InlineData("EU-M")]
    [InlineData("EUM")]
    [InlineData("EU moped")]
    [InlineData("Moped klass II")]
    [InlineData("Moped klass 2")]
    [InlineData("SL")]
    [InlineData("Släpvagn")]
    [InlineData("Släp")]
    [InlineData("SA")]
    [InlineData("Påhängsvagn")]
    [InlineData("Semitrailer")]
    [InlineData("HV")]
    [InlineData("Husvagn")]
    [InlineData("Campingvagn")]
    [InlineData("HB")]
    [InlineData("Husbil")]
    [InlineData("Camper")]
    [InlineData("Campingbil")]
    [InlineData("TK")]
    [InlineData("Terrängvagn")]
    [InlineData("TM")]
    [InlineData("Terrängskoter")]
    [InlineData("Snöskoter")]
    [InlineData("Skoter")]
    [InlineData("TH")]
    [InlineData("Terränghjuling")]
    [InlineData("ATV")]
    [InlineData("Quad")]
    [InlineData("Fyrhjuling")]
    [InlineData("AT")]
    [InlineData("A-traktor")]
    [InlineData("Atraktor")]
    [InlineData("A tractor")]
    [InlineData("EPA-traktor")]
    [InlineData("EPA")]
    [InlineData("EP")]
    [InlineData("Motorredskap klass I")]
    [InlineData("Motorredskap klass 1")]
    [InlineData("MRK2")]
    [InlineData("Motorredskap klass II")]
    [InlineData("Motorredskap klass 2")]
    [InlineData("LL")]
    [InlineData("Lätt lastbil")]
    [InlineData("Skåpbil")]
    [InlineData("  PB  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishVehicleType.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    [InlineData("Flygplan")]
    [InlineData("Bicycle")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishVehicleType.IsValid(input));
    }

    [Theory]
    [InlineData("PB", "PB")]
    [InlineData("pb", "PB")]
    [InlineData("Personbil", "PB")]
    [InlineData("Car", "PB")]
    [InlineData("Bil", "PB")]
    [InlineData("LB", "LB")]
    [InlineData("Lastbil", "LB")]
    [InlineData("Lorry", "LB")]
    [InlineData("MC", "MC")]
    [InlineData("Motorcykel", "MC")]
    [InlineData("EU-M", "EU-M")]
    [InlineData("EUM", "EU-M")]
    [InlineData("EU moped", "EU-M")]
    [InlineData("Moped klass 2", "EU-M")]
    [InlineData("AT", "AT")]
    [InlineData("EPA", "AT")]
    [InlineData("A-traktor", "AT")]
    [InlineData("ATV", "TH")]
    [InlineData("Fyrhjuling", "TH")]
    [InlineData("Snöskoter", "TM")]
    [InlineData("Skåpbil", "LL")]
    [InlineData("  PB  ", "PB")]
    public void TryParse_ReturnsExpectedCode(string input, string expectedCode)
    {
        var ok = SwedishVehicleType.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedCode, result.Code);
        Assert.Same(SwedishVehicleType.All.First(x => x.Code == expectedCode), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishVehicleType.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishVehicleType.Parse(input));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("bogus", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleType.Format(input));
    }

    [Theory]
    [InlineData("PB")]
    [InlineData("Personbil")]
    [InlineData("Car")]
    public void Format_ReturnsDisplayName_ForValidInput(string input)
    {
        var result = SwedishVehicleType.Format(input);
        Assert.NotNull(result);
        var parsed = SwedishVehicleType.Parse(input);
        Assert.True(result == parsed.LocalizedName || result == parsed.EnglishName);
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", SwedishVehicleType.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("PB", "PB")]
    [InlineData("Personbil", "PB")]
    [InlineData("  pb  ", "PB")]
    [InlineData("Lorry", "LB")]
    [InlineData("bogus", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Normalize_ReturnsCodeOrNull(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleType.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("bogus", SwedishVehicleType.Normalize(" bogus ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(SwedishVehicleType.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SwedishVehicleType.Normalize("  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("PB", true)]
    [InlineData("LB", true)]
    [InlineData("EU-M", true)]
    [InlineData("MRK2", true)]
    [InlineData("pb", false)]
    [InlineData("Personbil", false)]
    [InlineData("bogus", false)]
    [InlineData(null, false)]
    public void IsNormalized_RequiresCanonicalCode(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishVehicleType.IsNormalized(input));
    }

    [Theory]
    [InlineData("PB")]
    [InlineData("LB")]
    [InlineData("AT")]
    [InlineData("EU-M")]
    public void ToNormalizedString_ReturnsCode(string code)
    {
        var e = SwedishVehicleType.Parse(code);
        Assert.Equal(code, e.ToNormalizedString());
    }

    [Theory]
    [InlineData("PB")]
    [InlineData("LB")]
    [InlineData("TH")]
    public void ToString_ReturnsDisplayName(string code)
    {
        var e = SwedishVehicleType.Parse(code);
        var s = e.ToString();
        Assert.True(s == e.LocalizedName || s == e.EnglishName);
    }

    [Fact]
    public void Equality_SameType()
    {
        var a = SwedishVehicleType.Parse("PB");
        var b = SwedishVehicleType.Parse("Personbil");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentTypes()
    {
        var a = SwedishVehicleType.Parse("PB");
        var b = SwedishVehicleType.Parse("LB");
        Assert.True(a != b);
    }
}
