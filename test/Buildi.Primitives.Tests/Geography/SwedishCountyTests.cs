using System.Globalization;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

[Collection("CultureSensitive")]
public class SwedishCountyTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    [Theory]
    [InlineData("01", "Stockholms län")]
    [InlineData("Stockholms län", "Stockholms län")]
    [InlineData("Stockholm County", "Stockholms län")]
    [InlineData("Stockholms", "Stockholms län")]
    [InlineData("Skåne", "Skåne län")]
    [InlineData("Skånes", "Skåne län")]
    [InlineData("Blekinge", "Blekinge län")]
    [InlineData("Blekinges", "Blekinge län")]
    [InlineData("Dalarna", "Dalarnas län")]
    public void TryParse_ReturnsExpectedCounty(string input, string expectedLocalizedName)
    {
        var ok = SwedishCounty.TryParse(input, out var county);

        Assert.True(ok);
        Assert.NotNull(county);
        Assert.Equal(expectedLocalizedName, county!.LocalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("02")]
    [InlineData("11")]
    [InlineData("99")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(SwedishCounty.IsValid(input));
    }

    [Fact]
    public void Format_Normalize_And_ToString_ReturnExpectedValues()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var county = SwedishCounty.Parse("01");

        Assert.Equal("Stockholms län", SwedishCounty.Format("01"));
        Assert.Equal("01", SwedishCounty.Normalize("Stockholms län"));
        Assert.Equal("01", county.ToNormalizedString());
        Assert.Equal("Stockholms län", county.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishCounty.Parse("01");
        var b = SwedishCounty.Parse("01");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishCounty.Parse("01");
        var b = SwedishCounty.Parse("Skåne");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishCounty.Parse("01");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishCounty.Parse("01");
        var b = SwedishCounty.Parse("Skåne");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishCounty.Parse("01");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData("01", 59.3275, 18.054719)]
    [InlineData("14", 57.7, 11.933333)]
    [InlineData("12", 55.565, 13.018611)]
    [InlineData("25", 65.584444, 22.153889)]
    public void Coordinates_ReturnExpectedValues(string code, double expectedLat, double expectedLon)
    {
        var county = SwedishCounty.Parse(code);
        Assert.NotNull(county.Coordinate);
        Assert.Equal(expectedLat, county.Latitude, 4);
        Assert.Equal(expectedLon, county.Longitude, 4);
        Assert.Equal(county.Latitude, county.Coordinate.Latitude);
        Assert.Equal(county.Longitude, county.Coordinate.Longitude);
    }

    [Fact]
    public void All_Counties_HaveCoordinatesInSwedenRange()
    {
        foreach (var code in new[] { "01", "03", "04", "05", "06", "07", "08", "09", "10", "12", "13", "14", "17", "18", "19", "20", "21", "22", "23", "24", "25" })
        {
            var county = SwedishCounty.Parse(code);
            Assert.True(county.Latitude > 54 && county.Latitude < 70, $"{county.LocalizedName} ({code}) lat={county.Latitude} outside Sweden range");
            Assert.True(county.Longitude > 10 && county.Longitude < 25, $"{county.LocalizedName} ({code}) lon={county.Longitude} outside Sweden range");
        }
    }

    [Fact]
    public void Distance_StockholmToSkane_ReturnsReasonableDistance()
    {
        var stockholm = SwedishCounty.Parse("01");
        var skane = SwedishCounty.Parse("12");
        var distance = SwedishCounty.Distance(stockholm, skane);
        Assert.InRange(distance.Kilometers, 500, 560);
    }

    [Fact]
    public void Distance_ToGeoCoordinate_ReturnsReasonableDistance()
    {
        var stockholm = SwedishCounty.Parse("01");
        var arlandaApprox = GeoCoordinate.Create(59.6498, 17.9238);
        var distance = SwedishCounty.Distance(stockholm, arlandaApprox);
        Assert.InRange(distance.Kilometers, 30, 45);
    }

    [Fact]
    public void DistanceTo_GeoCoordinate_MatchesStaticMethod()
    {
        var stockholm = SwedishCounty.Parse("01");
        var coord = GeoCoordinate.Create(55.6050, 13.0038);
        Assert.Equal(
            SwedishCounty.Distance(stockholm, coord).Kilometers,
            stockholm.DistanceTo(coord).Kilometers);
    }

    [Fact]
    public void Distance_ReturnsLength_ConvertibleToUnits()
    {
        var stockholm = SwedishCounty.Parse("01");
        var skane = SwedishCounty.Parse("12");
        var distance = SwedishCounty.Distance(stockholm, skane);
        Assert.InRange(distance.Kilometers, 500, 560);
        Assert.InRange(distance.SwedishMiles, 50, 56);
    }

    [Fact]
    public void DistanceTo_County_ReturnsLength()
    {
        var stockholm = SwedishCounty.Parse("01");
        var skane = SwedishCounty.Parse("12");
        var distance = stockholm.DistanceTo(skane);
        Assert.Equal(SwedishCounty.Distance(stockholm, skane).Kilometers, distance.Kilometers);
    }

    [Fact]
    public void DistanceTo_GeoCoordinate_ReturnsLength()
    {
        var stockholm = SwedishCounty.Parse("01");
        var coord = GeoCoordinate.Create(55.6050, 13.0038);
        var distance = stockholm.DistanceTo(coord);
        Assert.InRange(distance.Kilometers, 500, 560);
    }
}
