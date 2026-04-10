using Buildi.Primitives.Finance;

namespace Buildi.Primitives.Tests.Finance;

public class MoneyAmountTests
{
    [Theory]
    [InlineData("1000 SEK", 1000, "SEK")]
    [InlineData("1 000 SEK", 1000, "SEK")]
    [InlineData("1000SEK", 1000, "SEK")]
    [InlineData("SEK1000", 1000, "SEK")]
    [InlineData("SEK 1000", 1000, "SEK")]
    [InlineData("  1000 SEK  ", 1000, "SEK")]
    public void TryParse_BasicCodeFormats(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("1000 kr", 1000, "SEK")]
    [InlineData("kr1000", 1000, "SEK")]
    [InlineData("kr 1000", 1000, "SEK")]
    [InlineData("$500", 500, "USD")]
    [InlineData("€1000", 1000, "EUR")]
    [InlineData("£250", 250, "GBP")]
    public void TryParse_SymbolFormats(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("1 000,50 SEK", 1000.50, "SEK")]
    [InlineData("1000,50 SEK", 1000.50, "SEK")]
    [InlineData("1 000,00 SEK", 1000.00, "SEK")]
    public void TryParse_SwedishDecimalFormat(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("1.000,50 EUR", 1000.50, "EUR")]
    [InlineData("10.000,99 EUR", 10000.99, "EUR")]
    public void TryParse_EuropeanThousandsDecimalFormat(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("1,000.50 USD", 1000.50, "USD")]
    [InlineData("10,000.99 USD", 10000.99, "USD")]
    public void TryParse_UsThousandsDecimalFormat(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("USD500", 500, "USD")]
    [InlineData("EUR 1 000", 1000, "EUR")]
    [InlineData("GBP100", 100, "GBP")]
    public void TryParse_PrefixCodeWithoutSpace(string input, decimal expectedAmount, string expectedCode)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal(expectedCode, result.Currency.Code);
    }

    [Theory]
    [InlineData("-1000 SEK", -1000)]
    [InlineData("+500 SEK", 500)]
    [InlineData("- 1 000 SEK", -1000)]
    public void TryParse_HandlesSignedAmounts(string input, decimal expectedAmount)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAmount, result!.Amount);
        Assert.Equal("SEK", result.Currency.Code);
    }

    [Fact]
    public void TryParse_WithFallbackCurrency_WhenNoCurrencyInString()
    {
        var ok = MoneyAmount.TryParse("1000", Currency.SEK, out var result);

        Assert.True(ok);
        Assert.Equal(1000m, result!.Amount);
        Assert.Equal("SEK", result.Currency.Code);
    }

    [Fact]
    public void TryParse_WithFallbackCurrency_WithDecimal()
    {
        var ok = MoneyAmount.TryParse("1 000,50", Currency.SEK, out var result);

        Assert.True(ok);
        Assert.Equal(1000.50m, result!.Amount);
        Assert.Equal("SEK", result.Currency.Code);
    }

    [Fact]
    public void TryParse_AmbiguousSymbol_UsesFallbackCurrencyOverride()
    {
        var ok = MoneyAmount.TryParse("1000 kr", Currency.NOK, out var result);

        Assert.True(ok);
        Assert.Equal(1000m, result!.Amount);
        Assert.Equal("NOK", result.Currency.Code);
    }

    [Fact]
    public void TryParse_StringCurrencyOverridesFallback()
    {
        var ok = MoneyAmount.TryParse("500 EUR", Currency.SEK, out var result);

        Assert.True(ok);
        Assert.Equal(500m, result!.Amount);
        Assert.Equal("EUR", result.Currency.Code);
    }

    [Fact]
    public void TryParse_FailsWithoutCurrencyOrFallback()
    {
        var ok = MoneyAmount.TryParse("1000", out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("SEK")]
    [InlineData("kr")]
    public void TryParse_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(MoneyAmount.TryParse(input, out _));
    }

    [Fact]
    public void Create_ReturnsExpectedValues()
    {
        var money = MoneyAmount.Create(1500.75m, Currency.SEK);

        Assert.Equal(1500.75m, money.Amount);
        Assert.Same(Currency.SEK, money.Currency);
    }

    [Fact]
    public void ToString_ReturnsSwedishFormattedString()
    {
        var money = MoneyAmount.Create(1000.50m, Currency.SEK);
        var s = money.ToString();

        Assert.Contains("SEK", s);
        Assert.Contains(",50", s);
    }

    [Fact]
    public void ToNormalizedString_ReturnsInvariantFormat()
    {
        var money = MoneyAmount.Create(1000.50m, Currency.SEK);

        Assert.Equal("1000.50 SEK", money.ToNormalizedString());
    }

    [Fact]
    public void ToNormalizedString_ZeroDecimalCurrency()
    {
        var money = MoneyAmount.Create(5000m, Currency.Parse("JPY"));

        Assert.Equal("5000 JPY", money.ToNormalizedString());
    }

    [Theory]
    [InlineData("1000 SEK", "1000 SEK")]
    [InlineData("1 000,50 EUR", "1000.50 EUR")]
    public void Normalize_ReturnsInvariantString(string input, string expected)
    {
        Assert.Equal(expected, MoneyAmount.Normalize(input));
    }

    [Theory]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, MoneyAmount.Normalize(input));
    }

    [Fact]
    public void Format_ReturnsNull_ForInvalidInput()
    {
        Assert.Null(MoneyAmount.Format("invalid"));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalidInput()
    {
        Assert.Equal("invalid", MoneyAmount.Format("  invalid  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void IsValid_WithFallback_ReturnsTrueForPlainNumber()
    {
        Assert.True(MoneyAmount.IsValid("1000", Currency.SEK));
        Assert.False(MoneyAmount.IsValid("1000"));
    }

    [Fact]
    public void Parse_ThrowsForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => MoneyAmount.Parse("invalid"));
    }

    [Fact]
    public void Parse_WithFallback_Succeeds()
    {
        var money = MoneyAmount.Parse("1000", Currency.EUR);

        Assert.Equal(1000m, money.Amount);
        Assert.Equal("EUR", money.Currency.Code);
    }

    [Theory]
    [InlineData("1.5 JPY")]
    [InlineData("1,5 JPY")]
    [InlineData("1.50 CLP")]
    public void TryParse_RejectsUnsupportedDecimalPrecision(string input)
    {
        var ok = MoneyAmount.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_RejectsUnsupportedDecimalPrecision_ForAmbiguousSymbolOverride()
    {
        var ok = MoneyAmount.TryParse("1,25 kr", Currency.Parse("ISK"), out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Fact]
    public void Create_ThrowsForUnsupportedDecimalPrecision()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyAmount.Create(1.5m, Currency.Parse("JPY")));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = MoneyAmount.Create(100m, Currency.SEK);
        var b = MoneyAmount.Create(100m, Currency.SEK);
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = MoneyAmount.Create(100m, Currency.SEK);
        var b = MoneyAmount.Create(200m, Currency.SEK);
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);

        var c = MoneyAmount.Create(100m, Currency.EUR);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
        Assert.False(a == c);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = MoneyAmount.Create(100m, Currency.SEK);
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void Operator_Add_SameCurrency_Works()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var b = MoneyAmount.Create(50m, Currency.Parse("SEK"));
        var result = a + b;
        Assert.Equal(150m, result.Amount);
        Assert.Equal("SEK", result.Currency.Code);
    }

    [Fact]
    public void Operator_Add_DifferentCurrency_Throws()
    {
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var usd = MoneyAmount.Create(50m, Currency.Parse("USD"));
        Assert.Throws<InvalidOperationException>(() => sek + usd);
    }

    [Fact]
    public void Operator_Subtract_SameCurrency_Works()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var b = MoneyAmount.Create(30m, Currency.Parse("SEK"));
        Assert.Equal(70m, (a - b).Amount);
    }

    [Fact]
    public void Operator_Subtract_DifferentCurrency_Throws()
    {
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var usd = MoneyAmount.Create(50m, Currency.Parse("USD"));
        Assert.Throws<InvalidOperationException>(() => sek - usd);
    }

    [Fact]
    public void Operator_Multiply_Works()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Equal(200m, (a * 2m).Amount);
        Assert.Equal(200m, (2m * a).Amount);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Equal(50m, (a / 2m).Amount);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Equal(-100m, (-a).Amount);
    }

    [Fact]
    public void ConvertTo_WithRate_ConvertsAmount()
    {
        var sek = MoneyAmount.Create(1000m, Currency.Parse("SEK"));
        var usd = sek.ConvertTo(Currency.Parse("USD"), 0.095m);
        Assert.Equal(95.00m, usd.Amount);
        Assert.Equal("USD", usd.Currency.Code);
    }

    [Fact]
    public void ConvertTo_WithRate_RoundsToTargetCurrencyDecimals()
    {
        var usd = MoneyAmount.Create(100m, Currency.Parse("USD"));
        var jpy = usd.ConvertTo(Currency.Parse("JPY"), 157.35m);
        Assert.Equal(15735m, jpy.Amount);
        Assert.Equal("JPY", jpy.Currency.Code);
    }

    [Fact]
    public void ConvertTo_WithRate_NegativeRate_Throws()
    {
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Throws<ArgumentOutOfRangeException>(() => sek.ConvertTo(Currency.Parse("USD"), -1m));
    }

    [Fact]
    public void ConvertTo_WithRate_ZeroRate_Throws()
    {
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Throws<ArgumentOutOfRangeException>(() => sek.ConvertTo(Currency.Parse("USD"), 0m));
    }

    [Fact]
    public void ConvertTo_WithExchangeRates_Works()
    {
        var rates = new ExchangeRates()
            .AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0.095m);

        var sek = MoneyAmount.Create(1000m, Currency.Parse("SEK"));
        var usd = sek.ConvertTo(Currency.Parse("USD"), rates);
        Assert.Equal(95.00m, usd.Amount);
        Assert.Equal("USD", usd.Currency.Code);
    }

    [Fact]
    public void ConvertTo_WithExchangeRates_UsesInverseRate()
    {
        var rates = new ExchangeRates()
            .AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0.095m);

        var usd = MoneyAmount.Create(9.50m, Currency.Parse("USD"));
        var sek = usd.ConvertTo(Currency.Parse("SEK"), rates);
        Assert.Equal(100.00m, sek.Amount);
    }

    [Fact]
    public void ConvertTo_WithExchangeRates_SameCurrency_ReturnsSameAmount()
    {
        var rates = new ExchangeRates();
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var result = sek.ConvertTo(Currency.Parse("SEK"), rates);
        Assert.Equal(100m, result.Amount);
    }

    [Fact]
    public void ConvertTo_WithExchangeRates_MissingRate_Throws()
    {
        var rates = new ExchangeRates();
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Throws<InvalidOperationException>(() => sek.ConvertTo(Currency.Parse("USD"), rates));
    }

    [Fact]
    public void ExchangeRates_AddRate_ZeroRate_Throws()
    {
        var rates = new ExchangeRates();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rates.AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0m));
    }

    [Fact]
    public void ExchangeRates_TryGetRate_ReturnsTrue_WhenAvailable()
    {
        var rates = new ExchangeRates()
            .AddRate(Currency.Parse("SEK"), Currency.Parse("EUR"), 0.087m);

        Assert.True(rates.TryGetRate(Currency.Parse("SEK"), Currency.Parse("EUR"), out var rate));
        Assert.Equal(0.087m, rate);
    }

    [Fact]
    public void ExchangeRates_TryGetRate_ReturnsFalse_WhenMissing()
    {
        var rates = new ExchangeRates();
        Assert.False(rates.TryGetRate(Currency.Parse("SEK"), Currency.Parse("EUR"), out _));
    }

    [Fact]
    public void ExchangeRates_ExplicitBothDirections_OverridesInverse()
    {
        var rates = new ExchangeRates()
            .AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0.095m)
            .AddRate(Currency.Parse("USD"), Currency.Parse("SEK"), 10.60m);

        Assert.Equal(10.60m, rates.GetRate(Currency.Parse("USD"), Currency.Parse("SEK")));
        Assert.Equal(0.095m, rates.GetRate(Currency.Parse("SEK"), Currency.Parse("USD")));
    }

    [Fact]
    public void ExchangeRates_FluentChaining_Works()
    {
        var rates = new ExchangeRates()
            .AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0.095m)
            .AddRate(Currency.Parse("SEK"), Currency.Parse("EUR"), 0.087m)
            .AddRate(Currency.Parse("SEK"), Currency.Parse("GBP"), 0.074m);

        Assert.Equal(0.095m, rates.GetRate(Currency.Parse("SEK"), Currency.Parse("USD")));
        Assert.Equal(0.087m, rates.GetRate(Currency.Parse("SEK"), Currency.Parse("EUR")));
        Assert.Equal(0.074m, rates.GetRate(Currency.Parse("SEK"), Currency.Parse("GBP")));
    }

    [Fact]
    public void CompareTo_SameCurrency_OrdersByAmount()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        var b = MoneyAmount.Create(200m, Currency.Parse("SEK"));
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void CompareTo_DifferentCurrency_OrdersByCurrencyCodeFirst()
    {
        var eur = MoneyAmount.Create(100m, Currency.Parse("EUR"));
        var sek = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.True(eur.CompareTo(sek) < 0);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = MoneyAmount.Create(100m, Currency.Parse("SEK"));
        Assert.Equal(1, a.CompareTo(null));
    }
}
