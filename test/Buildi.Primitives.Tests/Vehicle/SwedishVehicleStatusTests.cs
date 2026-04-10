using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class SwedishVehicleStatusTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(7, SwedishVehicleStatus.All.Count);
    }

    [Theory]
    [InlineData("ITRAFIK", "ITRAFIK", "In service", "I trafik")]
    [InlineData("AVST", "AVST", "Deregistered", "Avställd")]
    [InlineData("AVREG", "AVREG", "Unregistered", "Avregistrerad")]
    [InlineData("STULEN", "STULEN", "Stolen", "Stulen")]
    [InlineData("EXPORT", "EXPORT", "Exported", "Exporterad")]
    [InlineData("SKROT", "SKROT", "Scrapped", "Skrotad")]
    [InlineData("ANMSTULEN", "ANMSTULEN", "Reported stolen", "Anmäld stulen")]
    public void StaticInstances_HaveExpectedProperties(
        string code, string expectedValue, string expectedEnglish, string expectedSwedish)
    {
        var e = SwedishVehicleStatus.All.Single(x => x.Code == code);
        Assert.Equal(expectedValue, e.Value);
        Assert.Equal(code, e.Code);
        Assert.Equal(expectedEnglish, e.EnglishName);
        Assert.Equal(expectedSwedish, e.LocalizedName);
    }

    [Theory]
    [InlineData("ITRAFIK")]
    [InlineData("itrafik")]
    [InlineData("I trafik")]
    [InlineData("I TRAFIK")]
    [InlineData("I_TRAFIK")]
    [InlineData("Påställd")]
    [InlineData("In service")]
    [InlineData("Active")]
    [InlineData("Registered")]
    [InlineData("In traffic")]
    [InlineData("AVST")]
    [InlineData("Avställd")]
    [InlineData("AVSTÄLLD")]
    [InlineData("AVSTALLD")]
    [InlineData("Deregistered")]
    [InlineData("Off road")]
    [InlineData("Off-road")]
    [InlineData("Avställning")]
    [InlineData("AVREG")]
    [InlineData("Avregistrerad")]
    [InlineData("AVREGISTRERAD")]
    [InlineData("Unregistered")]
    [InlineData("Permanently deregistered")]
    [InlineData("STULEN")]
    [InlineData("Stulen")]
    [InlineData("Stolen")]
    [InlineData("EXPORT")]
    [InlineData("Exporterad")]
    [InlineData("EXPORTERAD")]
    [InlineData("Exported")]
    [InlineData("SKROT")]
    [InlineData("Skrotad")]
    [InlineData("SKROTAD")]
    [InlineData("Scrapped")]
    [InlineData("Skrotning")]
    [InlineData("ANMSTULEN")]
    [InlineData("Anmäld stulen")]
    [InlineData("ANMÄLD STULEN")]
    [InlineData("Reported stolen")]
    [InlineData("  ITRAFIK  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishVehicleStatus.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    [InlineData("Parkerad")]
    [InlineData("Unknown status")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishVehicleStatus.IsValid(input));
    }

    [Theory]
    [InlineData("ITRAFIK", "ITRAFIK")]
    [InlineData("itrafik", "ITRAFIK")]
    [InlineData("I trafik", "ITRAFIK")]
    [InlineData("I_TRAFIK", "ITRAFIK")]
    [InlineData("Påställd", "ITRAFIK")]
    [InlineData("Active", "ITRAFIK")]
    [InlineData("Registered", "ITRAFIK")]
    [InlineData("In traffic", "ITRAFIK")]
    [InlineData("AVST", "AVST")]
    [InlineData("Avställd", "AVST")]
    [InlineData("Off road", "AVST")]
    [InlineData("Off-road", "AVST")]
    [InlineData("Avställning", "AVST")]
    [InlineData("AVREG", "AVREG")]
    [InlineData("Avregistrerad", "AVREG")]
    [InlineData("Permanently deregistered", "AVREG")]
    [InlineData("STULEN", "STULEN")]
    [InlineData("Stolen", "STULEN")]
    [InlineData("EXPORT", "EXPORT")]
    [InlineData("Exporterad", "EXPORT")]
    [InlineData("SKROT", "SKROT")]
    [InlineData("Skrotning", "SKROT")]
    [InlineData("ANMSTULEN", "ANMSTULEN")]
    [InlineData("Anmäld stulen", "ANMSTULEN")]
    [InlineData("Reported stolen", "ANMSTULEN")]
    [InlineData("  ITRAFIK  ", "ITRAFIK")]
    public void TryParse_ReturnsExpectedCode(string input, string expectedCode)
    {
        var ok = SwedishVehicleStatus.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedCode, result.Code);
        Assert.Same(SwedishVehicleStatus.All.First(x => x.Code == expectedCode), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishVehicleStatus.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishVehicleStatus.Parse(input));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("bogus", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleStatus.Format(input));
    }

    [Theory]
    [InlineData("ITRAFIK")]
    [InlineData("I trafik")]
    [InlineData("AVST")]
    [InlineData("Avställd")]
    public void Format_ReturnsDisplayName_ForValidInput(string input)
    {
        var result = SwedishVehicleStatus.Format(input);
        Assert.NotNull(result);
        var parsed = SwedishVehicleStatus.Parse(input);
        Assert.True(result == parsed.LocalizedName || result == parsed.EnglishName);
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", SwedishVehicleStatus.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("ITRAFIK", "ITRAFIK")]
    [InlineData("I trafik", "ITRAFIK")]
    [InlineData("  itrafik  ", "ITRAFIK")]
    [InlineData("AVST", "AVST")]
    [InlineData("Avställd", "AVST")]
    [InlineData("SKROT", "SKROT")]
    [InlineData("Skrotning", "SKROT")]
    [InlineData("bogus", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Normalize_ReturnsCodeOrNull(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleStatus.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("bogus", SwedishVehicleStatus.Normalize(" bogus ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(SwedishVehicleStatus.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SwedishVehicleStatus.Normalize("  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("ITRAFIK", true)]
    [InlineData("AVST", true)]
    [InlineData("ANMSTULEN", true)]
    [InlineData("itrafik", false)]
    [InlineData("I trafik", false)]
    [InlineData("bogus", false)]
    [InlineData(null, false)]
    public void IsNormalized_RequiresCanonicalCode(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishVehicleStatus.IsNormalized(input));
    }

    [Theory]
    [InlineData("ITRAFIK")]
    [InlineData("AVST")]
    [InlineData("ANMSTULEN")]
    public void ToNormalizedString_ReturnsCode(string code)
    {
        var e = SwedishVehicleStatus.Parse(code);
        Assert.Equal(code, e.ToNormalizedString());
    }

    [Theory]
    [InlineData("ITRAFIK")]
    [InlineData("AVST")]
    [InlineData("STULEN")]
    public void ToString_ReturnsDisplayName(string code)
    {
        var e = SwedishVehicleStatus.Parse(code);
        var s = e.ToString();
        Assert.True(s == e.LocalizedName || s == e.EnglishName);
    }

    [Fact]
    public void Equality_SameStatus()
    {
        var a = SwedishVehicleStatus.Parse("ITRAFIK");
        var b = SwedishVehicleStatus.Parse("I trafik");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentStatuses()
    {
        var a = SwedishVehicleStatus.Parse("ITRAFIK");
        var b = SwedishVehicleStatus.Parse("AVST");
        Assert.True(a != b);
    }
}
