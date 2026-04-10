using Buildi.Primitives.Banking;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishBankingNumberParserTests
{
    [Fact]
    public void TryParse_ReturnsBankgiro_ForFormattedBankgiro()
    {
        var ok = SwedishBankingNumberParser.TryParse("5464-9652", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishBankgiroNumber, result.Type);
        Assert.Equal("5464-9652", result.NormalizedValue);
        Assert.NotNull(result.SwedishBankgiroNumber);
        Assert.Null(result.SwedishPostgiroNumber);
    }

    [Fact]
    public void TryParse_ReturnsPlusgiro_ForFormattedPlusgiro()
    {
        var ok = SwedishBankingNumberParser.TryParse("4779202-3", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishPostgiroNumber, result.Type);
        Assert.Equal("4779202-3", result.NormalizedValue);
        Assert.NotNull(result.SwedishPostgiroNumber);
        Assert.Null(result.SwedishBankgiroNumber);
    }

    [Fact]
    public void TryParse_ReturnsIban_ForIban()
    {
        var ok = SwedishBankingNumberParser.TryParse("SE45 5000 0000 0583 9825 7466", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.Iban, result.Type);
        Assert.Equal("SE4550000000058398257466", result.NormalizedValue);
        Assert.NotNull(result.Iban);
    }

    [Fact]
    public void TryParse_ReturnsBic_ForBic()
    {
        var ok = SwedishBankingNumberParser.TryParse("NDEASESS", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.Bic, result.Type);
        Assert.Equal("NDEASESS", result.NormalizedValue);
        Assert.NotNull(result.Bic);
    }

    [Fact]
    public void TryParse_ReturnsSwedishBankAccount_ForDomesticAccount()
    {
        var ok = SwedishBankingNumberParser.TryParse("51000123456", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishBankAccount, result.Type);
        Assert.Equal("51000123456", result.NormalizedValue);
        Assert.NotNull(result.SwedishBankAccount);
    }

    [Fact]
    public void TryParse_ReturnsOcrReference_AsFallback()
    {
        var input = CreateValidOcrReference("123456789");

        var ok = SwedishBankingNumberParser.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishOcrReferenceNumber, result.Type);
        Assert.Equal(input, result.NormalizedValue);
        Assert.NotNull(result.SwedishOcrReferenceNumber);
    }

    [Fact]
    public void TryParse_PrefersBankgiro_ForAmbiguousDigitOnlyInput()
    {
        var ok = SwedishBankingNumberParser.TryParse("54649652", out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishBankgiroNumber, result.Type);
        Assert.NotNull(result.SwedishBankgiroNumber);
    }

    [Theory]
    [InlineData("BG 5464-9652")]
    [InlineData("bg 5464-9652")]
    [InlineData("Bg 5464-9652")]
    [InlineData("Bankgiro 5464-9652")]
    [InlineData("bankgiro 5464-9652")]
    [InlineData("BG 54649652")]
    public void TryParse_ReturnsBankgiro_ForBgPrefix(string input)
    {
        var ok = SwedishBankingNumberParser.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishBankgiroNumber, result.Type);
        Assert.Equal("5464-9652", result.NormalizedValue);
        Assert.NotNull(result.SwedishBankgiroNumber);
    }

    [Theory]
    [InlineData("PG 4779202-3")]
    [InlineData("pg 4779202-3")]
    [InlineData("Pg 4779202-3")]
    [InlineData("Plusgiro 4779202-3")]
    [InlineData("plusgiro 4779202-3")]
    [InlineData("Postgiro 4779202-3")]
    [InlineData("postgiro 4779202-3")]
    public void TryParse_ReturnsPlusgiro_ForPgPrefix(string input)
    {
        var ok = SwedishBankingNumberParser.TryParse(input, out var result);

        Assert.True(ok);
        Assert.Equal(SwedishBankingNumberType.SwedishPostgiroNumber, result.Type);
        Assert.Equal("4779202-3", result.NormalizedValue);
        Assert.NotNull(result.SwedishPostgiroNumber);
    }

    [Theory]
    [InlineData("BG invalid")]
    [InlineData("PG invalid")]
    [InlineData("Bankgiro invalid")]
    [InlineData("Plusgiro invalid")]
    public void TryParse_ReturnsFalse_ForPrefixWithInvalidNumber(string input)
    {
        var ok = SwedishBankingNumberParser.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Equal(SwedishBankingNumberType.Unknown, result.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not a banking number")]
    [InlineData("123")]
    public void TryParse_ReturnsFalse_ForInvalidInput(string? input)
    {
        var ok = SwedishBankingNumberParser.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Equal(SwedishBankingNumberType.Unknown, result.Type);
        Assert.Equal(string.Empty, result.NormalizedValue);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => SwedishBankingNumberParser.Parse("not a banking number"));
    }

    private static string CreateValidOcrReference(string bodyWithoutCheckDigit)
    {
        for (var digit = '0'; digit <= '9'; digit++)
        {
            var candidate = bodyWithoutCheckDigit + digit;
            if (SwedishOcrReferenceNumber.TryParse(candidate, out _))
                return candidate;
        }

        throw new InvalidOperationException("Could not create valid OCR reference.");
    }
}
