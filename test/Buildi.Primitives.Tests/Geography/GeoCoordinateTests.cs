using Buildi.Primitives.Geography;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Geography;

public class GeoCoordinateTests
{
    [Theory]
    [InlineData("59.3293, 18.0686")]
    [InlineData("59.3293,18.0686")]
    [InlineData("59.3293; 18.0686")]
    [InlineData("59.3293 18.0686")]
    [InlineData("(59.3293, 18.0686)")]
    [InlineData("-33.8688, 151.2093")]
    [InlineData("0, 0")]
    [InlineData("90, 180")]
    [InlineData("-90, -180")]
    [InlineData("59.3293°N, 18.0686°E")]
    [InlineData("33.8688°S, 151.2093°E")]
    [InlineData("59.3293N, 18.0686E")]
    [InlineData("N 59.3293, E 18.0686")]
    [InlineData("N59.3293, E18.0686")]
    [InlineData("59°19'45.5\"N, 18°4'7\"E")]
    [InlineData("59°19.758'N, 18°4.116'E")]
    [InlineData("  59.3293 , 18.0686  ")]
    [InlineData("59.3293°N, 18.0686°Ö")]
    [InlineData("59.3293°N, 18.0686°V")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(GeoCoordinate.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("59.3293")]
    [InlineData("91, 18")]
    [InlineData("59, 181")]
    [InlineData("-91, 0")]
    [InlineData("0, -181")]
    [InlineData("abc, def")]
    [InlineData("59.3293, abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(GeoCoordinate.IsValid(input));
    }

    [Theory]
    [InlineData("59.3293, 18.0686", 59.3293, 18.0686)]
    [InlineData("-33.8688, 151.2093", -33.8688, 151.2093)]
    [InlineData("0, 0", 0, 0)]
    [InlineData("90, 180", 90, 180)]
    [InlineData("-90, -180", -90, -180)]
    [InlineData("59.3293 18.0686", 59.3293, 18.0686)]
    [InlineData("(59.3293, 18.0686)", 59.3293, 18.0686)]
    public void TryParse_DecimalDegrees_ReturnsExpectedProperties(string input, double expectedLat, double expectedLon)
    {
        Assert.True(GeoCoordinate.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedLat, result!.Latitude, 6);
        Assert.Equal(expectedLon, result.Longitude, 6);
    }

    [Theory]
    [InlineData("59.3293°N, 18.0686°E", 59.3293, 18.0686)]
    [InlineData("33.8688°S, 151.2093°E", -33.8688, 151.2093)]
    [InlineData("40.7128°N, 74.006°W", 40.7128, -74.006)]
    [InlineData("59.3293N, 18.0686E", 59.3293, 18.0686)]
    [InlineData("59.3293°N, 18.0686°Ö", 59.3293, 18.0686)]
    [InlineData("33.8688°S, 18.0686°V", -33.8688, -18.0686)]
    public void TryParse_DecimalWithCardinal_ReturnsExpectedProperties(string input, double expectedLat, double expectedLon)
    {
        Assert.True(GeoCoordinate.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedLat, result!.Latitude, 6);
        Assert.Equal(expectedLon, result.Longitude, 6);
    }

    [Theory]
    [InlineData("N 59.3293, E 18.0686", 59.3293, 18.0686)]
    [InlineData("N59.3293, E18.0686", 59.3293, 18.0686)]
    [InlineData("S 33.8688, E 151.2093", -33.8688, 151.2093)]
    public void TryParse_CardinalPrefix_ReturnsExpectedProperties(string input, double expectedLat, double expectedLon)
    {
        Assert.True(GeoCoordinate.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedLat, result!.Latitude, 6);
        Assert.Equal(expectedLon, result.Longitude, 6);
    }

    [Fact]
    public void TryParse_Dms_ReturnsExpectedProperties()
    {
        Assert.True(GeoCoordinate.TryParse("59°19'45.5\"N, 18°4'7\"E", out var result));
        Assert.NotNull(result);
        Assert.Equal(59.329306, result!.Latitude, 4);
        Assert.Equal(18.068611, result.Longitude, 4);
    }

    [Fact]
    public void TryParse_Ddm_ReturnsExpectedProperties()
    {
        Assert.True(GeoCoordinate.TryParse("59°19.758'N, 18°4.116'E", out var result));
        Assert.NotNull(result);
        Assert.Equal(59.3293, result!.Latitude, 4);
        Assert.Equal(18.0686, result.Longitude, 4);
    }

    [Fact]
    public void TryParse_Dms_InvalidMinutes_ReturnsFalse()
    {
        Assert.False(GeoCoordinate.TryParse("59°60'0\"N, 18°0'0\"E", out _));
    }

    [Fact]
    public void TryParse_Dms_InvalidSeconds_ReturnsFalse()
    {
        Assert.False(GeoCoordinate.TryParse("59°0'60\"N, 18°0'0\"E", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("just a number")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(GeoCoordinate.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("91, 0")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => GeoCoordinate.Parse(input));
    }

    [Theory]
    [InlineData("59.3293, 18.0686", "59.3293°N, 18.0686°E")]
    [InlineData("-33.8688, 151.2093", "33.8688°S, 151.2093°E")]
    [InlineData("0, 0", "0°N, 0°E")]
    [InlineData("40.7128°N, 74.006°W", "40.7128°N, 74.006°W")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GeoCoordinate.Format(input));
    }

    [Fact]
    public void Format_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", GeoCoordinate.Format("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(GeoCoordinate.Format("abc"));
    }

    [Theory]
    [InlineData("59.3293, 18.0686", "59.3293, 18.0686")]
    [InlineData("59.3293°N, 18.0686°E", "59.3293, 18.0686")]
    [InlineData("-33.8688, 151.2093", "-33.8688, 151.2093")]
    [InlineData("40.7128°N, 74.006°W", "40.7128, -74.006")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GeoCoordinate.Normalize(input));
    }

    [Theory]
    [InlineData("59.3293, 18.0686", true)]
    [InlineData("59.3293°N, 18.0686°E", false)]
    [InlineData("-33.8688, 151.2093", true)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, GeoCoordinate.IsNormalized(input));
    }

    [Theory]
    [InlineData(59.3293, 18.0686, "59.3293°N, 18.0686°E")]
    [InlineData(-33.8688, 151.2093, "33.8688°S, 151.2093°E")]
    [InlineData(0, 0, "0°N, 0°E")]
    public void ToString_ReturnsFormattedValue(double lat, double lon, string expected)
    {
        var coord = GeoCoordinate.Create(lat, lon);
        Assert.Equal(expected, coord.ToString());
    }

    [Theory]
    [InlineData(59.3293, 18.0686, "59.3293, 18.0686")]
    [InlineData(-33.8688, 151.2093, "-33.8688, 151.2093")]
    [InlineData(0, 0, "0, 0")]
    public void ToNormalizedString_ReturnsDecimalDegrees(double lat, double lon, string expected)
    {
        var coord = GeoCoordinate.Create(lat, lon);
        Assert.Equal(expected, coord.ToNormalizedString());
    }

    [Fact]
    public void ToDmsString_ReturnsExpectedFormat()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        var dms = coord.ToDmsString();
        Assert.StartsWith("59°19'", dms);
        Assert.Contains("N,", dms);
        Assert.Contains("E", dms);
    }

    [Fact]
    public void LatitudeDirection_ReturnsCorrectCardinal()
    {
        Assert.Equal("N", GeoCoordinate.Create(59.3293, 18.0686).LatitudeDirection);
        Assert.Equal("S", GeoCoordinate.Create(-33.8688, 151.2093).LatitudeDirection);
        Assert.Equal("N", GeoCoordinate.Create(0, 0).LatitudeDirection);
    }

    [Fact]
    public void LongitudeDirection_ReturnsCorrectCardinal()
    {
        Assert.Equal("E", GeoCoordinate.Create(59.3293, 18.0686).LongitudeDirection);
        Assert.Equal("W", GeoCoordinate.Create(40.7128, -74.006).LongitudeDirection);
        Assert.Equal("E", GeoCoordinate.Create(0, 0).LongitudeDirection);
    }

    [Fact]
    public void Create_ValidRange_Succeeds()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        Assert.Equal(59.3293, coord.Latitude);
        Assert.Equal(18.0686, coord.Longitude);
    }

    [Fact]
    public void Create_LatitudeOutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoCoordinate.Create(91, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoCoordinate.Create(-91, 0));
    }

    [Fact]
    public void Create_LongitudeOutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoCoordinate.Create(0, 181));
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoCoordinate.Create(0, -181));
    }

    [Fact]
    public void Distance_StockholmToGothenburg_ReturnsReasonableDistance()
    {
        var stockholm = GeoCoordinate.Create(59.3293, 18.0686);
        var gothenburg = GeoCoordinate.Create(57.7089, 11.9746);
        var distance = GeoCoordinate.Distance(stockholm, gothenburg);
        Assert.InRange(distance.Kilometers, 390, 410);
    }

    [Fact]
    public void DistanceTo_SameAsStaticMethod()
    {
        var a = GeoCoordinate.Create(59.3293, 18.0686);
        var b = GeoCoordinate.Create(57.7089, 11.9746);
        Assert.Equal(GeoCoordinate.Distance(a, b).Kilometers, a.DistanceTo(b).Kilometers);
    }

    [Fact]
    public void Distance_SamePoint_ReturnsZero()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        Assert.Equal(0, GeoCoordinate.Distance(coord, coord).Kilometers);
    }

    [Fact]
    public void Distance_CanConvertToSwedishMiles()
    {
        var stockholm = GeoCoordinate.Create(59.3293, 18.0686);
        var gothenburg = GeoCoordinate.Create(57.7089, 11.9746);
        var distance = stockholm.DistanceTo(gothenburg);
        Assert.InRange(distance.SwedishMiles, 39, 41);
    }

    [Fact]
    public void IsWithin_ReturnsTrue_WhenWithinRadius()
    {
        var stockholm = GeoCoordinate.Create(59.3293, 18.0686);
        var solna = GeoCoordinate.Create(59.3600, 18.0000);
        Assert.True(stockholm.IsWithin(solna, Length.FromKilometers(10)));
    }

    [Fact]
    public void IsWithin_ReturnsFalse_WhenOutsideRadius()
    {
        var stockholm = GeoCoordinate.Create(59.3293, 18.0686);
        var gothenburg = GeoCoordinate.Create(57.7089, 11.9746);
        Assert.False(stockholm.IsWithin(gothenburg, Length.FromKilometers(100)));
    }

    [Fact]
    public void IsWithin_ReturnsTrue_WhenExactlyOnBoundary()
    {
        var a = GeoCoordinate.Create(59.3293, 18.0686);
        var b = GeoCoordinate.Create(57.7089, 11.9746);
        var exactDistance = a.DistanceTo(b);
        Assert.True(a.IsWithin(b, exactDistance));
    }

    [Fact]
    public void IsWithin_SamePoint_AlwaysTrue()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        Assert.True(coord.IsWithin(coord, Length.FromMeters(0)));
    }

    [Fact]
    public void IsWithin_Static_MatchesInstance()
    {
        var a = GeoCoordinate.Create(59.3293, 18.0686);
        var b = GeoCoordinate.Create(57.7089, 11.9746);
        var radius = Length.FromKilometers(500);
        Assert.Equal(a.IsWithin(b, radius), GeoCoordinate.IsWithin(a, b, radius));
    }

    [Fact]
    public void Equality_SameCoordinates()
    {
        var a = GeoCoordinate.Create(59.3293, 18.0686);
        var b = GeoCoordinate.Create(59.3293, 18.0686);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentCoordinates()
    {
        var a = GeoCoordinate.Create(59.3293, 18.0686);
        var b = GeoCoordinate.Create(57.7089, 11.9746);
        Assert.False(a == b);
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_NullHandling()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        Assert.False(coord.Equals(null));
        Assert.False(coord == null);
        Assert.True(coord != null);
        Assert.True((GeoCoordinate?)null == null);
    }

    [Theory]
    [InlineData("59.3293°n, 18.0686°e", 59.3293, 18.0686)]
    [InlineData("59.3293°N, 18.0686°E", 59.3293, 18.0686)]
    [InlineData("n 59.3293, e 18.0686", 59.3293, 18.0686)]
    public void TryParse_IsCaseInsensitive(string input, double expectedLat, double expectedLon)
    {
        Assert.True(GeoCoordinate.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedLat, result!.Latitude, 6);
        Assert.Equal(expectedLon, result.Longitude, 6);
    }

    [Fact]
    public void TryParse_SwedishCardinals_East()
    {
        Assert.True(GeoCoordinate.TryParse("59.3293°N, 18.0686°Ö", out var result));
        Assert.Equal(18.0686, result!.Longitude, 6);
    }

    [Fact]
    public void TryParse_SwedishCardinals_West()
    {
        Assert.True(GeoCoordinate.TryParse("40.7128°N, 74.006°V", out var result));
        Assert.Equal(-74.006, result!.Longitude, 6);
    }

    [Fact]
    public void Normalize_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", GeoCoordinate.Normalize("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(GeoCoordinate.Normalize(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(GeoCoordinate.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void TryParse_BoundaryValues()
    {
        Assert.True(GeoCoordinate.TryParse("90, 180", out var ne));
        Assert.Equal(90, ne!.Latitude);
        Assert.Equal(180, ne.Longitude);

        Assert.True(GeoCoordinate.TryParse("-90, -180", out var sw));
        Assert.Equal(-90, sw!.Latitude);
        Assert.Equal(-180, sw.Longitude);
    }

    [Fact]
    public void TryParse_SemicolonSeparator()
    {
        Assert.True(GeoCoordinate.TryParse("59.3293; 18.0686", out var result));
        Assert.Equal(59.3293, result!.Latitude);
        Assert.Equal(18.0686, result.Longitude);
    }
}
