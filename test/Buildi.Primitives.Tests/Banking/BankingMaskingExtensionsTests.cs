using Buildi.Primitives.Banking;

namespace Buildi.Primitives.Tests.Banking;

public class BankingMaskingExtensionsTests
{
    [Fact]
    public void BankAccount_DefaultMasksAccountNumber()
    {
        var account = SwedishBankAccount.Parse("50011234567");
        var masked = account.ToMaskedString();
        Assert.Equal("5001-*******", masked);
    }

    [Fact]
    public void BankAccount_MaskClearingNumber_MasksEverything()
    {
        var account = SwedishBankAccount.Parse("50011234567");
        var masked = account.ToMaskedString(maskClearingNumber: true);
        Assert.Equal("****-*******", masked);
    }

    [Fact]
    public void BankAccount_PreservesClearingNumberLength()
    {
        var account = SwedishBankAccount.Parse("50011234567");
        var masked = account.ToMaskedString();
        Assert.StartsWith(account.ClearingNumber, masked);
    }

    [Fact]
    public void Iban_MasksAccountButShowsCountryAndCheck()
    {
        var iban = Iban.Parse("SE4550000000058398257466");
        var masked = iban.ToMaskedString();
        Assert.Equal("SE45 **** **** **** **** ****", masked);
    }

    [Fact]
    public void Iban_PreservesSpaceGrouping()
    {
        var iban = Iban.Parse("SE4550000000058398257466");
        var masked = iban.ToMaskedString();
        var groups = masked.Split(' ');
        Assert.Equal(6, groups.Length);
        Assert.Equal("SE45", groups[0]);
        Assert.All(groups[1..], g => Assert.Equal("****", g));
    }

    [Theory]
    [InlineData("54649652", true, "****-9652")]
    [InlineData("54649652", false, "5464-****")]
    [InlineData("2359321", true, "***-9321")]
    [InlineData("2359321", false, "235-****")]
    public void Bankgiro_ToMaskedString_ReturnsExpected(string input, bool showLastDigits, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);
        Assert.Equal(expected, bg.ToMaskedString(showLastDigits));
    }

    [Theory]
    [InlineData("47792023", true, "*******-3")]
    [InlineData("47792023", false, "4779202-*")]
    public void Postgiro_ToMaskedString_ReturnsExpected(string input, bool showControlDigit, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);
        Assert.Equal(expected, pg.ToMaskedString(showControlDigit));
    }

    [Fact]
    public void Bic_MasksInstitutionCodeAndBranch()
    {
        var bic = Bic.Parse("NDEASESS");
        var masked = bic.ToMaskedString();
        Assert.Equal("****SE**", masked);
    }

    [Fact]
    public void Bic_11Char_MasksCorrectly()
    {
        var bic = Bic.Parse("NDEASESSXXX");
        var masked = bic.ToMaskedString();
        Assert.Equal("****SE*****", masked);
    }

    [Fact]
    public void Bic_PreservesCountryCode()
    {
        var bic = Bic.Parse("ESSESESS");
        var masked = bic.ToMaskedString();
        Assert.Equal("****SE**", masked);
        Assert.Equal("SE", masked.Substring(4, 2));
    }

    [Fact]
    public void OcrReference_MasksAllDigits()
    {
        var ocr = SwedishOcrReferenceNumber.Parse("12345682");
        var masked = ocr.ToMaskedString();
        Assert.Equal("********", masked);
        Assert.Equal(8, masked.Length);
    }

    [Fact]
    public void OcrReference_MaskedLengthMatchesValueLength()
    {
        var ocr = SwedishOcrReferenceNumber.Parse("12345682");
        var masked = ocr.ToMaskedString();
        Assert.Equal(ocr.Value.Length, masked.Length);
        Assert.All(masked, c => Assert.Equal('*', c));
    }
}
