using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

public class SwedishMunicipalityTests
{
    [Theory]
    [InlineData("0180", "Stockholm", "01")]
    [InlineData("Stockholm", "Stockholm", "01")]
    [InlineData("Malmö", "Malmö", "12")]
    [InlineData("Stockholms", "Stockholm", "01")]
    [InlineData("Göteborgs", "Göteborg", "14")]
    [InlineData("Malmös", "Malmö", "12")]
    [InlineData("Uppsalas", "Uppsala", "03")]
    [InlineData("Gothenburg", "Göteborg", "14")]
    public void TryParse_ReturnsExpectedMunicipality(string input, string expectedName, string expectedCountyCode)
    {
        var ok = SwedishMunicipality.TryParse(input, out var municipality);

        Assert.True(ok);
        Assert.NotNull(municipality);
        Assert.Equal(expectedName, municipality!.LocalizedName);
        Assert.Equal(expectedCountyCode, municipality.CountyCode);
        Assert.Equal(expectedCountyCode, municipality.County.Code);
    }

    [Theory]
    [InlineData("1480", "Gothenburg")]
    [InlineData("0180", "Stockholm")]
    public void EnglishName_ReturnsExpectedValue(string code, string expectedEnglish)
    {
        var municipality = SwedishMunicipality.Parse(code);
        Assert.Equal(expectedEnglish, municipality.EnglishName);
        Assert.Equal(expectedEnglish, municipality.ToEnglishString());
    }

    [Fact]
    public void EnglishName_FallsBackToLocalizedName_WhenNoOverride()
    {
        var malmö = SwedishMunicipality.Parse("Malmö");
        Assert.Equal("Malmö", malmö.EnglishName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("018")]
    [InlineData("01800")]
    [InlineData("9999")]
    [InlineData("1999")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(SwedishMunicipality.IsValid(input));
    }

    [Fact]
    public void Format_Normalize_And_ToString_ReturnExpectedValues()
    {
        var municipality = SwedishMunicipality.Parse("0180");

        Assert.Equal("Stockholm", SwedishMunicipality.Format("0180"));
        Assert.Equal("0180", SwedishMunicipality.Normalize("Stockholm"));
        Assert.Equal("0180", municipality.ToNormalizedString());
        Assert.Equal("Stockholm", municipality.ToString());
    }

    [Theory]
    [InlineData("0180", 59.3275, 18.054719)]
    [InlineData("1480", 57.7, 11.933333)]
    [InlineData("1280", 55.565, 13.018611)]
    [InlineData("2584", 67.854722, 20.222778)]
    public void Coordinates_ReturnExpectedValues(string code, double expectedLat, double expectedLon)
    {
        var municipality = SwedishMunicipality.Parse(code);
        Assert.NotNull(municipality.Coordinate);
        Assert.Equal(expectedLat, municipality.Latitude, 4);
        Assert.Equal(expectedLon, municipality.Longitude, 4);
        Assert.Equal(municipality.Latitude, municipality.Coordinate.Latitude);
        Assert.Equal(municipality.Longitude, municipality.Coordinate.Longitude);
    }

    [Fact]
    public void All_Municipalities_HaveCoordinates()
    {
        foreach (var m in SwedishMunicipality.All)
        {
            Assert.True(m.Latitude > 54 && m.Latitude < 70, $"{m.LocalizedName} ({m.Code}) lat={m.Latitude} outside Sweden range");
            Assert.True(m.Longitude > 10 && m.Longitude < 25, $"{m.LocalizedName} ({m.Code}) lon={m.Longitude} outside Sweden range");
        }
    }

    [Fact]
    public void FindNeighbors_Stockholm_Includes_NearbyMunicipalities()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var neighbors = stockholm.FindNeighbors(20);

        Assert.True(neighbors.Count > 0);
        var names = neighbors.Select(n => n.Municipality.LocalizedName).ToList();
        Assert.Contains("Solna", names);
        Assert.Contains("Sundbyberg", names);
        Assert.Contains("Nacka", names);
        Assert.DoesNotContain("Stockholm", names);
    }

    [Fact]
    public void FindNeighbors_ReturnsOrderedByDistance()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var neighbors = stockholm.FindNeighbors(50);

        for (int i = 1; i < neighbors.Count; i++)
            Assert.True(neighbors[i].Distance.Kilometers >= neighbors[i - 1].Distance.Kilometers);
    }

    [Fact]
    public void FindNeighbors_SmallRadius_ExcludesDistantMunicipalities()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var neighbors = stockholm.FindNeighbors(10);

        var names = neighbors.Select(n => n.Municipality.LocalizedName).ToList();
        Assert.DoesNotContain("Malmö", names);
        Assert.DoesNotContain("Göteborg", names);
        Assert.DoesNotContain("Kiruna", names);
    }

    [Fact]
    public void FindNeighbors_LargeRadius_IncludesAll()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var neighbors = stockholm.FindNeighbors(2000);

        Assert.Equal(SwedishMunicipality.All.Count - 1, neighbors.Count);
    }

    [Fact]
    public void FindNeighbors_ThrowsForInvalidRadius()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        Assert.Throws<ArgumentOutOfRangeException>(() => stockholm.FindNeighbors(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => stockholm.FindNeighbors(-10));
    }

    [Fact]
    public void Distance_Stockholm_Goteborg_IsApproximatelyCorrect()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var goteborg = SwedishMunicipality.Parse("Göteborg");
        var distance = SwedishMunicipality.Distance(stockholm, goteborg);

        Assert.InRange(distance.Kilometers, 380, 480);
    }

    [Fact]
    public void Distance_SamePoint_IsZero()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        Assert.Equal(0, SwedishMunicipality.Distance(stockholm, stockholm).Kilometers);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var a = SwedishMunicipality.Parse("Stockholm");
        var b = SwedishMunicipality.Parse("Malmö");
        Assert.Equal(
            SwedishMunicipality.Distance(a, b).Kilometers,
            SwedishMunicipality.Distance(b, a).Kilometers);
    }

    [Fact]
    public void Distance_ToGeoCoordinate_ReturnsReasonableDistance()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var arlandaApprox = GeoCoordinate.Create(59.6498, 17.9238);
        var distance = SwedishMunicipality.Distance(stockholm, arlandaApprox);
        Assert.InRange(distance.Kilometers, 30, 45);
    }

    [Fact]
    public void DistanceTo_GeoCoordinate_MatchesStaticMethod()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var coord = GeoCoordinate.Create(57.7089, 11.9746);
        Assert.Equal(
            SwedishMunicipality.Distance(stockholm, coord).Kilometers,
            stockholm.DistanceTo(coord).Kilometers);
    }

    [Fact]
    public void Distance_ReturnsLength_ConvertibleToUnits()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var goteborg = SwedishMunicipality.Parse("Göteborg");
        var distance = SwedishMunicipality.Distance(stockholm, goteborg);
        Assert.InRange(distance.Kilometers, 380, 480);
        Assert.InRange(distance.SwedishMiles, 38, 48);
    }

    [Fact]
    public void DistanceTo_Municipality_ReturnsLength()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var goteborg = SwedishMunicipality.Parse("Göteborg");
        var distance = stockholm.DistanceTo(goteborg);
        Assert.Equal(SwedishMunicipality.Distance(stockholm, goteborg).Kilometers, distance.Kilometers);
    }

    [Fact]
    public void DistanceTo_GeoCoordinate_ReturnsLength()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var arlanda = GeoCoordinate.Create(59.6498, 17.9238);
        var distance = stockholm.DistanceTo(arlanda);
        Assert.InRange(distance.Kilometers, 30, 45);
    }

    [Fact]
    public void MunicipalityDistance_HasDistanceAsLength()
    {
        var stockholm = SwedishMunicipality.Parse("Stockholm");
        var neighbors = stockholm.FindNeighbors(20);
        Assert.True(neighbors.Count > 0);
        var first = neighbors[0];
        Assert.True(first.Distance.Kilometers > 0);
    }

    [Fact]
    public void FindNeighbors_FromGeoCoordinate_ReturnsNearbyMunicipalities()
    {
        var arlanda = GeoCoordinate.Create(59.6498, 17.9238);
        var neighbors = SwedishMunicipality.FindNeighbors(arlanda, 20);

        Assert.True(neighbors.Count > 0);
        var names = neighbors.Select(n => n.Municipality.LocalizedName).ToList();
        Assert.Contains("Sigtuna", names);
    }

    [Fact]
    public void FindNeighbors_FromGeoCoordinate_ReturnsOrderedByDistance()
    {
        var coord = GeoCoordinate.Create(59.3293, 18.0686);
        var neighbors = SwedishMunicipality.FindNeighbors(coord, 50);

        for (int i = 1; i < neighbors.Count; i++)
            Assert.True(neighbors[i].Distance.Kilometers >= neighbors[i - 1].Distance.Kilometers);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishMunicipality.Parse("0180");
        var b = SwedishMunicipality.Parse("0180");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishMunicipality.Parse("0180");
        var b = SwedishMunicipality.Parse("Malmö");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishMunicipality.Parse("0180");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishMunicipality.Parse("0180");
        var b = SwedishMunicipality.Parse("1480");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishMunicipality.Parse("0180");
        Assert.Equal(1, a.CompareTo(null));
    }
}
