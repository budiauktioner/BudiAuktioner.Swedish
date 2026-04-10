using System.Globalization;
using Buildi.Primitives.Finance;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Finance;

[Collection("CultureSensitive")]
public class CurrencyTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    [Theory]
    [InlineData("SEK")]
    [InlineData("sek")]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("NOK")]
    [InlineData("DKK")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public void IsValid_ReturnsTrue_ForKnownCodes(string input)
    {
        Assert.True(Currency.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XYZ")]
    [InlineData("ABCD")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(Currency.IsValid(input));
    }

    [Theory]
    [InlineData("SEK", "SEK", "Swedish krona", "Svensk krona", "kr", 2)]
    [InlineData("EUR", "EUR", "Euro", "Euro", "€", 2)]
    [InlineData("JPY", "JPY", "Japanese yen", "Japansk yen", "¥", 0)]
    [InlineData("ISK", "ISK", "Icelandic króna", "Isländsk krona", "kr", 0)]
    public void TryParse_ByCode_ReturnsExpectedProperties(string input, string expectedCode, string expectedEn, string expectedSv, string expectedSymbol, int expectedDecimals)
    {
        var ok = Currency.TryParse(input, out var currency);

        Assert.True(ok);
        Assert.NotNull(currency);
        Assert.Equal(expectedCode, currency!.Code);
        Assert.Equal(expectedEn, currency.EnglishName);
        Assert.Equal(expectedSv, currency.LocalizedName);
        Assert.Equal(expectedSymbol, currency.Symbol);
        Assert.Equal(expectedDecimals, currency.DecimalPlaces);
    }

    [Theory]
    [InlineData("Swedish krona", "SEK")]
    [InlineData("Svensk krona", "SEK")]
    [InlineData("Euro", "EUR")]
    [InlineData("Pound sterling", "GBP")]
    [InlineData("Brittiskt pund", "GBP")]
    public void TryParse_ByName_ReturnsExpectedCurrency(string input, string expectedCode)
    {
        var ok = Currency.TryParse(input, out var currency);

        Assert.True(ok);
        Assert.Equal(expectedCode, currency!.Code);
    }

    [Fact]
    public void TryParse_IsCaseInsensitive_ForCodes()
    {
        Assert.True(Currency.TryParse("sek", out var lower));
        Assert.True(Currency.TryParse("SEK", out var upper));
        Assert.Same(lower, upper);
    }

    [Fact]
    public void TryParse_IsCaseInsensitive_ForNames()
    {
        Assert.True(Currency.TryParse("swedish krona", out var lower));
        Assert.True(Currency.TryParse("Swedish krona", out var title));
        Assert.Same(lower, title);
    }

    [Theory]
    [InlineData("SEK", "Svensk krona")]
    [InlineData("EUR", "Euro")]
    [InlineData("GBP", "Brittiskt pund")]
    public void Format_ReturnLocalizedName_ForValidInput(string input, string expected)
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        Assert.Equal(expected, Currency.Format(input));
    }

    [Theory]
    [InlineData("XYZ", null)]
    [InlineData(null, null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, Currency.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalidInput()
    {
        Assert.Equal("XYZ", Currency.Format("  XYZ  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("Swedish krona", "SEK")]
    [InlineData("EUR", "EUR")]
    public void Normalize_ReturnsCode_ForValidInput(string input, string expected)
    {
        Assert.Equal(expected, Currency.Normalize(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("XYZ")]
    public void Normalize_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.Null(Currency.Normalize(input));
    }

    [Fact]
    public void ToNormalizedString_ReturnsCode()
    {
        var currency = Currency.Parse("SEK");
        Assert.Equal("SEK", currency.ToNormalizedString());
    }

    [Fact]
    public void ToString_ReturnsLocalizedName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var currency = Currency.Parse("SEK");
        Assert.Equal("Svensk krona", currency.ToString());
    }

    [Fact]
    public void Parse_ThrowsArgumentException_ForUnknownInput()
    {
        Assert.Throws<ArgumentException>(() => Currency.Parse("XYZ"));
    }

    [Fact]
    public void All_ContainsExpectedCurrencies()
    {
        var all = Currency.All;
        Assert.Contains(all, c => c.Code == "SEK");
        Assert.Contains(all, c => c.Code == "EUR");
        Assert.Contains(all, c => c.Code == "USD");
        Assert.True(all.Count >= 40);
    }

    [Fact]
    public void Country_Sweden_ExposesCurrencyAsTypedModel()
    {
        var ok = Country.TryParse("SE", out var country);
        Assert.True(ok);
        Assert.NotNull(country!.Currency);
        Assert.Equal("SEK", country.Currency!.Code);
        Assert.Equal("Svensk krona", country.Currency.LocalizedName);
        Assert.Single(country.Currencies);
    }

    [Fact]
    public void Country_Germany_ExposesCurrencyAsTypedModel()
    {
        var country = Country.Parse("DE");
        Assert.NotNull(country.Currency);
        Assert.Equal("EUR", country.Currency!.Code);
    }

    [Fact]
    public void Country_CurrencyCode_StillExistsAsString()
    {
        var country = Country.Parse("SE");
        Assert.Equal("SEK", country.CurrencyCode);
        Assert.Contains("SEK", country.CurrencyCodes);
    }

    [Fact]
    public void Constants_ReturnExpectedCurrencies()
    {
        Assert.Equal("SEK", Currency.SEK.Code);
        Assert.Equal("EUR", Currency.EUR.Code);
        Assert.Equal("USD", Currency.USD.Code);
        Assert.Equal("NOK", Currency.NOK.Code);
        Assert.Equal("DKK", Currency.DKK.Code);
        Assert.Equal("GBP", Currency.GBP.Code);
    }

    [Fact]
    public void Constants_AreSameInstancesAsParsed()
    {
        Assert.Same(Currency.SEK, Currency.Parse("SEK"));
        Assert.Same(Currency.EUR, Currency.Parse("EUR"));
        Assert.Same(Currency.USD, Currency.Parse("USD"));
    }

    [Fact]
    public void Country_Constants_ReturnExpectedCountries()
    {
        Assert.Equal("SE", Country.Sweden.Alpha2Code);
        Assert.Equal("NO", Country.Norway.Alpha2Code);
        Assert.Equal("FI", Country.Finland.Alpha2Code);
        Assert.Equal("DK", Country.Denmark.Alpha2Code);
    }

    [Fact]
    public void Country_Sweden_CurrencyConstant_MatchesCurrencyConstant()
    {
        Assert.Same(Currency.SEK, Country.Sweden.Currency);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Currency.Parse("SEK");
        var b = Currency.Parse("SEK");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Currency.Parse("SEK");
        var b = Currency.Parse("EUR");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Currency.Parse("SEK");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Currency.Parse("EUR");
        var b = Currency.Parse("SEK");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Currency.Parse("USD");
        Assert.Equal(1, a.CompareTo(null));
    }
}
