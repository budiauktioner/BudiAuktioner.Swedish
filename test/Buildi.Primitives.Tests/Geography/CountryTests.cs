using System.Globalization;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

[Collection("CultureSensitive")]
public class CountryTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    [Theory]
    [InlineData("Sweden", true, true, true, true, false, true, "SEK")]
    [InlineData("Finland", true, true, true, true, true, true, "EUR")]
    [InlineData("Denmark", true, true, true, true, false, true, "DKK")]
    [InlineData("Norway", false, true, true, true, false, true, "NOK")]
    [InlineData("Iceland", false, true, true, true, false, true, "ISK")]
    [InlineData("Liechtenstein", false, true, true, false, false, true, "CHF")]
    [InlineData("Switzerland", false, false, true, false, false, true, "CHF")]
    [InlineData("Germany", true, true, true, false, true, true, "EUR")]
    [InlineData("Ireland", true, true, false, false, true, true, "EUR")]
    [InlineData("United Kingdom", false, false, false, false, false, true, "GBP")]
    [InlineData("Åland", false, false, false, false, true, false, "EUR")]
    public void TryParse_ClassificationFlags_ReturnExpectedValues(
        string input,
        bool isInEu,
        bool isInEea,
        bool isInSchengen,
        bool isInNordics,
        bool usesEuro,
        bool isInSepa,
        string currencyCode)
    {
        var ok = Country.TryParse(input, out var country);

        Assert.True(ok);
        Assert.NotNull(country);
        Assert.Equal(isInEu, country!.IsInEuropeanUnion);
        Assert.Equal(isInEea, country.IsInEea);
        Assert.Equal(isInSchengen, country.IsInSchengen);
        Assert.Equal(isInNordics, country.IsInNordics);
        Assert.Equal(usesEuro, country.UsesEuro);
        Assert.Equal(isInSepa, country.IsInSepa);
        Assert.Equal(currencyCode, country.CurrencyCode);
        Assert.Contains(currencyCode, country.CurrencyCodes);
    }

    [Fact]
    public void TryParse_ByLocalizedName_ReturnsCountryWithCurrency()
    {
        var ok = Country.TryParse("Sverige", out var country);

        Assert.True(ok);
        Assert.NotNull(country);
        Assert.Equal("SE", country!.Alpha2Code);
        Assert.Equal("SEK", country.CurrencyCode);
        Assert.True(country.IsInNordics);
    }

    [Fact]
    public void TryParse_Kosovo_ReturnsEuroAndNotSepa()
    {
        var ok = Country.TryParse("Kosovo", out var country);

        Assert.True(ok);
        Assert.NotNull(country);
        Assert.Equal("EUR", country!.CurrencyCode);
        Assert.True(country.UsesEuro);
        Assert.False(country.IsInSepa);
    }

    [Theory]
    [InlineData("THE NETHERLANDS", "NL")]
    [InlineData("The Netherlands", "NL")]
    [InlineData("the netherlands", "NL")]
    [InlineData("The Bahamas", "BS")]
    [InlineData("The Gambia", "GM")]
    [InlineData("the Czech Republic", "CZ")]
    public void TryParse_StripsLeadingTheArticle(string input, string expectedAlpha2)
    {
        var ok = Country.TryParse(input, out var country);

        Assert.True(ok);
        Assert.NotNull(country);
        Assert.Equal(expectedAlpha2, country!.Alpha2Code);
    }

    [Fact]
    public void Format_Normalize_AndToString_RemainStable()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var country = Country.Parse("Tyskland");

        Assert.Equal("Tyskland", Country.Format("DE"));
        Assert.Equal("DE", Country.Normalize("Tyskland"));
        Assert.Equal("DE", country.ToNormalizedString());
        Assert.Equal("Tyskland", country.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Country.Parse("Sweden");
        var b = Country.Parse("Sweden");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Country.Parse("Sweden");
        var b = Country.Parse("Finland");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Country.Parse("Sweden");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Country.Parse("DE");
        var b = Country.Parse("SE");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Country.Parse("SE");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Fact]
    public void PrimaryLanguage_Sweden_IsSwedish()
    {
        var country = Country.Sweden;
        Assert.NotNull(country.PrimaryLanguage);
        Assert.Equal(Language.Swedish, country.PrimaryLanguage);
        Assert.Equal("sv", country.PrimaryLanguage!.Alpha2Code);
    }

    [Fact]
    public void OfficialLanguages_Sweden_ContainsOnlySwedish()
    {
        var country = Country.Sweden;
        Assert.Single(country.OfficialLanguages);
        Assert.Equal(Language.Swedish, country.OfficialLanguages[0]);
    }

    [Theory]
    [InlineData("SE", "sv")]
    [InlineData("NO", "no")]
    [InlineData("DK", "da")]
    [InlineData("FI", "fi")]
    [InlineData("IS", "is")]
    [InlineData("DE", "de")]
    [InlineData("FR", "fr")]
    [InlineData("ES", "es")]
    [InlineData("IT", "it")]
    [InlineData("PT", "pt")]
    [InlineData("GB", "en")]
    [InlineData("US", "en")]
    [InlineData("JP", "ja")]
    [InlineData("CN", "zh")]
    [InlineData("RU", "ru")]
    [InlineData("SA", "ar")]
    [InlineData("TR", "tr")]
    public void PrimaryLanguage_ReturnsExpected(string alpha2, string expectedLangCode)
    {
        var country = Country.Parse(alpha2);
        Assert.NotNull(country.PrimaryLanguage);
        Assert.Equal(expectedLangCode, country.PrimaryLanguage!.Alpha2Code);
    }

    [Fact]
    public void OfficialLanguages_Belgium_HasThreeLanguages()
    {
        var country = Country.Belgium;
        Assert.Equal(3, country.OfficialLanguages.Count);
        Assert.Equal(Language.Dutch, country.OfficialLanguages[0]);
        Assert.Equal(Language.French, country.OfficialLanguages[1]);
        Assert.Equal(Language.German, country.OfficialLanguages[2]);
    }

    [Fact]
    public void OfficialLanguages_Switzerland_HasFourLanguages()
    {
        var country = Country.Switzerland;
        Assert.Equal(4, country.OfficialLanguages.Count);
        Assert.Equal(Language.German, country.OfficialLanguages[0]);
        Assert.Equal(Language.French, country.OfficialLanguages[1]);
        Assert.Equal(Language.Italian, country.OfficialLanguages[2]);
        Assert.Equal(Language.Romansh, country.OfficialLanguages[3]);
    }

    [Fact]
    public void OfficialLanguages_Canada_HasTwoLanguages()
    {
        var country = Country.Parse("CA");
        Assert.Equal(2, country.OfficialLanguages.Count);
        Assert.Equal(Language.English, country.OfficialLanguages[0]);
        Assert.Equal(Language.French, country.OfficialLanguages[1]);
    }

    [Fact]
    public void OfficialLanguages_Finland_HasTwoLanguages()
    {
        var country = Country.Finland;
        Assert.Equal(2, country.OfficialLanguages.Count);
        Assert.Equal(Language.Finnish, country.OfficialLanguages[0]);
        Assert.Equal(Language.Swedish, country.OfficialLanguages[1]);
    }

    [Fact]
    public void PrimaryLanguage_Belgium_IsDutch()
    {
        Assert.Equal(Language.Dutch, Country.Belgium.PrimaryLanguage);
    }

    [Theory]
    [InlineData("SE", 62.0, 15.0)]
    [InlineData("NO", 62.0, 10.0)]
    [InlineData("FI", 64.0, 26.0)]
    [InlineData("DE", 51.0, 9.0)]
    [InlineData("US", 38.0, -97.0)]
    [InlineData("AU", -27.0, 133.0)]
    [InlineData("JP", 36.0, 138.0)]
    public void Coordinate_ReturnsExpectedValues(string alpha2, double expectedLat, double expectedLon)
    {
        var country = Country.Parse(alpha2);
        Assert.Equal(expectedLat, country.Latitude);
        Assert.Equal(expectedLon, country.Longitude);
        Assert.Equal(expectedLat, country.Coordinate.Latitude);
        Assert.Equal(expectedLon, country.Coordinate.Longitude);
    }

    [Fact]
    public void AllCountries_HaveCoordinates()
    {
        foreach (var country in Country.All)
            Assert.NotNull(country.Coordinate);
    }

    [Theory]
    [InlineData("SE", "Stockholm", "Stockholm", "Stockholm")]
    [InlineData("NO", "Oslo", "Oslo", "Oslo")]
    [InlineData("FI", "Helsinki", "Helsingfors", "Helsinki")]
    [InlineData("DK", "Copenhagen", "Köpenhamn", "København")]
    [InlineData("DE", "Berlin", "Berlin", "Berlin")]
    [InlineData("FR", "Paris", "Paris", "Paris")]
    [InlineData("JP", "Tokyo", "Tokyo", "東京")]
    [InlineData("GR", "Athens", "Aten", "Αθήνα")]
    [InlineData("CN", "Beijing", "Peking", "北京")]
    [InlineData("RU", "Moscow", "Moskva", "Москва")]
    [InlineData("EG", "Cairo", "Kairo", "القاهرة")]
    [InlineData("PL", "Warsaw", "Warszawa", "Warszawa")]
    public void Capital_ReturnsExpectedNames(string alpha2, string expectedEn, string expectedSv, string expectedNative)
    {
        var country = Country.Parse(alpha2);

        Assert.NotNull(country.Capital);
        Assert.Equal(expectedEn, country.Capital!.EnglishName);
        Assert.Equal(expectedSv, country.Capital.LocalizedName);
        Assert.Equal(expectedNative, country.Capital.NativeName);
    }

    [Fact]
    public void Capital_HasCoordinate()
    {
        var se = Country.Sweden;
        Assert.NotNull(se.Capital);
        Assert.InRange(se.Capital!.Coordinate.Latitude, 59.0, 60.0);
        Assert.InRange(se.Capital.Coordinate.Longitude, 17.0, 19.0);
    }

    [Fact]
    public void Capital_DisplayName_RespectsUICulture()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        Assert.Equal("Köpenhamn", Country.Denmark.Capital!.DisplayName);
        Assert.Equal("Köpenhamn", Country.Denmark.Capital.ToString());

        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("Copenhagen", Country.Denmark.Capital!.DisplayName);
    }

    [Fact]
    public void FindNeighbors_Sweden_FindsNearbyCountries()
    {
        var sweden = Country.Sweden;
        var neighbors = sweden.FindNeighbors(1000);

        Assert.NotEmpty(neighbors);
        Assert.All(neighbors, n => Assert.True(n.Distance.Kilometers > 0));

        var codes = neighbors.Select(n => n.Country.Alpha2Code).ToList();
        Assert.Contains("NO", codes);
        Assert.Contains("FI", codes);
        Assert.Contains("DK", codes);
    }

    [Fact]
    public void FindNeighbors_ReturnsResultsSortedByDistance()
    {
        var neighbors = Country.Sweden.FindNeighbors(2000);

        for (int i = 1; i < neighbors.Count; i++)
            Assert.True(neighbors[i].Distance.Kilometers >= neighbors[i - 1].Distance.Kilometers);
    }

    [Fact]
    public void FindNeighbors_DoesNotIncludeSelf()
    {
        var neighbors = Country.Sweden.FindNeighbors(10000);
        Assert.DoesNotContain(neighbors, n => n.Country == Country.Sweden);
    }

    [Fact]
    public void FindNeighbors_SmallRadius_ReturnsEmpty()
    {
        var neighbors = Country.Sweden.FindNeighbors(1);
        Assert.Empty(neighbors);
    }

    [Fact]
    public void FindNeighbors_Static_FromCoordinate()
    {
        var stockholm = GeoCoordinate.Create(59.33, 18.07);
        var neighbors = Country.FindNeighbors(stockholm, 500);

        Assert.NotEmpty(neighbors);
        var codes = neighbors.Select(n => n.Country.Alpha2Code).ToList();
        Assert.Contains("SE", codes);
    }
}
