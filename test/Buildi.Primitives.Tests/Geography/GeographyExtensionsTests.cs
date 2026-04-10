using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

public class GeographyExtensionsTests
{
    [Fact]
    public void GetBorderingCountries_Sweden_ReturnsFinlandAndNorway()
    {
        var borders = Country.Sweden.GetBorderingCountries();

        var codes = borders.Select(c => c.Alpha2Code).ToList();
        Assert.Contains("FI", codes);
        Assert.Contains("NO", codes);
        Assert.Equal(2, borders.Count);
    }

    [Fact]
    public void GetBorderingCountries_Germany_ReturnsNineNeighbors()
    {
        var borders = Country.Germany.GetBorderingCountries();

        var codes = borders.Select(c => c.Alpha2Code).ToHashSet();
        Assert.Contains("AT", codes);
        Assert.Contains("BE", codes);
        Assert.Contains("CZ", codes);
        Assert.Contains("DK", codes);
        Assert.Contains("FR", codes);
        Assert.Contains("LU", codes);
        Assert.Contains("NL", codes);
        Assert.Contains("PL", codes);
        Assert.Contains("CH", codes);
        Assert.Equal(9, borders.Count);
    }

    [Fact]
    public void GetBorderingCountries_Iceland_ReturnsEmpty()
    {
        var borders = Country.Iceland.GetBorderingCountries();
        Assert.Empty(borders);
    }

    [Fact]
    public void GetBorderingCountries_Australia_ReturnsEmpty()
    {
        var borders = Country.Parse("AU").GetBorderingCountries();
        Assert.Empty(borders);
    }

    [Theory]
    [InlineData("FR", "ES")]
    [InlineData("ES", "FR")]
    [InlineData("US", "CA")]
    [InlineData("CA", "US")]
    [InlineData("RU", "NO")]
    [InlineData("NO", "RU")]
    public void GetBorderingCountries_IsSymmetric(string alpha2A, string alpha2B)
    {
        var bordersA = Country.Parse(alpha2A).GetBorderingCountries();
        Assert.Contains(bordersA, c => c.Alpha2Code == alpha2B);
    }

    [Fact]
    public void GetCountries_Europe_ContainsSweden()
    {
        var european = Continent.Europe.GetCountries();

        Assert.Contains(european, c => c.Alpha2Code == "SE");
        Assert.Contains(european, c => c.Alpha2Code == "DE");
        Assert.Contains(european, c => c.Alpha2Code == "FR");
        Assert.DoesNotContain(european, c => c.Alpha2Code == "US");
        Assert.DoesNotContain(european, c => c.Alpha2Code == "JP");
    }

    [Fact]
    public void GetCountries_ReturnsOrderedByEnglishName()
    {
        var european = Continent.Europe.GetCountries();

        var names = european.Select(c => c.EnglishName).ToList();
        var sorted = names.OrderBy(n => n).ToList();
        Assert.Equal(sorted, names);
    }

    [Fact]
    public void GetCountries_AllContinents_CoverAllCountries()
    {
        var allFromContinents = Continent.All
            .SelectMany(cont => cont.GetCountries())
            .Select(c => c.Alpha2Code)
            .ToHashSet();

        foreach (var country in Country.All)
            Assert.Contains(country.Alpha2Code, allFromContinents);
    }
}
