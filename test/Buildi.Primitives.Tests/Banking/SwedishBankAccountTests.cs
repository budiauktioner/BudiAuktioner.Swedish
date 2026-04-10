using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishBankAccountTests
{
    [Theory]
    [InlineData("50011234567")]
    [InlineData("6789123456789")]
    [InlineData("70001234567")]
    [InlineData("81234123456")]
    [InlineData("33001234567890")]
    [InlineData("37821234567890")]
    [InlineData("11001234567")]
    [InlineData("95001234567890")]
    [InlineData("12001234567")]
    [InlineData("34001234567")]
    [InlineData("90201234567")]
    [InlineData("91501234567")]
    [InlineData("92501234567")]
    [InlineData("92701234567")]
    [InlineData("97501234567")]
    [InlineData("  7000  1234567  ")]
    [InlineData("81234-123456")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishBankAccount.IsValid(input));
    }

    [Theory]
    [InlineData("50010123456")]
    [InlineData("6789012345678")]
    [InlineData("70000123456")]
    [InlineData("81234012345")]
    [InlineData("33000123456789")]
    [InlineData("37820123456789")]
    [InlineData("11000123456")]
    [InlineData("95000123456789")]
    [InlineData("12000123456")]
    [InlineData("34000123456")]
    [InlineData("90200123456")]
    [InlineData("91500123456")]
    [InlineData("92500123456")]
    [InlineData("92700123456")]
    [InlineData("97500123456")]
    public void IsValid_ReturnsTrue_ForPaddedAccountAfterClearing(string input)
    {
        Assert.True(SwedishBankAccount.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("700012345678901")]
    [InlineData("11001234567890")]
    [InlineData("330012345678900")]
    [InlineData("378212345678900")]
    [InlineData("24001234567890")]
    [InlineData("90201234567890")]
    [InlineData("95001234567")]
    [InlineData("92701234567890")]
    [InlineData("99991234567")]
    [InlineData("400-012345")]
    [InlineData("915012345678")]
    [InlineData("927012345678")]
    [InlineData("8123412345")]
    [InlineData("95701234567")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishBankAccount.IsValid(input));
    }

    [Theory]
    [InlineData("81234123456", "81234", "123456")]
    [InlineData("70001234567", "7000", "1234567")]
    [InlineData("  7000 1234567  ", "7000", "1234567")]
    [InlineData("81234-123456", "81234", "123456")]
    [InlineData("8105-9694719622-3", "81059", "6947196223")]
    [InlineData("33007109230511", "3300", "7109230511")]
    [InlineData("3300 590413-0035", "3300", "5904130035")]
    public void TryParse_ReturnsClearingAndAccount_ForValidInput(string input, string expectedClearing,
        string expectedAccount)
    {
        var ok = SwedishBankAccount.TryParse(input, out var acct);

        Assert.True(ok);
        Assert.NotNull(acct);
        Assert.Equal(expectedClearing, acct!.ClearingNumber);
        Assert.Equal(expectedAccount, acct.AccountNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("9999-1234567")]
    [InlineData("9270-12345678")]
    [InlineData("400-012345")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishBankAccount.TryParse(input, out var acct);

        Assert.False(ok);
        Assert.Null(acct);
    }

    [Theory]
    [InlineData("81234123456", "81234-123456")]
    [InlineData("70001234567", "7000-1234567")]
    [InlineData("7000-1234567", "7000-1234567")]
    [InlineData("81234-123456", "81234-123456")]
    public void TryParse_Formatted_ReturnsExpectedValue(string input, string expectedFormatted)
    {
        var ok = SwedishBankAccount.TryParse(input, out var acct);

        Assert.True(ok);
        Assert.Equal(expectedFormatted, acct!.Formatted);
    }

    [Theory]
    [InlineData("81234123456", "81234-123456")]
    [InlineData("70001234567", "7000-1234567")]
    public void Parse_Formatted_ReturnsExpectedValue(string input, string expected)
    {
        var acct = SwedishBankAccount.Parse(input);

        Assert.Equal(expected, acct.Formatted);
    }

    [Theory]
    [InlineData("99991234567")]
    [InlineData("927012345678")]
    [InlineData("700012345678901")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishBankAccount.Parse(input));
    }

    [Theory]
    [InlineData(" 7000 1234567 ", "Bankkonto 7000-1234567")]
    public void ToDisplayString_ReturnsFullDisplay(string input, string expected)
    {
        var acct = SwedishBankAccount.Parse(input);

        Assert.Equal(expected, acct.ToDisplayString());
    }

    [Theory]
    [InlineData(" 7000 1234567 ", "7000-1234567")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var acct = SwedishBankAccount.Parse(input);

        Assert.Equal(expected, acct.ToString());
    }

    [Theory]
    [InlineData("11001234567", "Nordea")]
    [InlineData("11501234567", "Nordea")]
    [InlineData("11991234567", "Nordea")]
    [InlineData("12001234567", "Danske Bank")]
    [InlineData("12951234567", "Danske Bank")]
    [InlineData("13991234567", "Danske Bank")]
    [InlineData("14001234567", "Nordea")]
    [InlineData("17491234567", "Nordea")]
    [InlineData("20991234567", "Nordea")]
    [InlineData("23001234567", "Ålandsbanken Abp (Finland) svensk filial")]
    [InlineData("23451234567", "Ålandsbanken Abp (Finland) svensk filial")]
    [InlineData("23991234567", "Ålandsbanken Abp (Finland) svensk filial")]
    [InlineData("24001234567", "Danske Bank")]
    [InlineData("24451234567", "Danske Bank")]
    [InlineData("24991234567", "Danske Bank")]
    [InlineData("30001234567", "Nordea")]
    [InlineData("31451234567", "Nordea")]
    [InlineData("32991234567", "Nordea")]
    [InlineData("33001234567890", "Nordea Personkonto")]
    [InlineData("33011234567", "Nordea")]
    [InlineData("33501234567", "Nordea")]
    [InlineData("33991234567", "Nordea")]
    [InlineData("34001234567", "Länsförsäkringar Bank")]
    [InlineData("34051234567", "Länsförsäkringar Bank")]
    [InlineData("34091234567", "Länsförsäkringar Bank")]
    [InlineData("34101234567", "Nordea")]
    [InlineData("35901234567", "Nordea")]
    [InlineData("37811234567", "Nordea")]
    [InlineData("37821234567890", "Nordea Personkonto")]
    [InlineData("37831234567", "Nordea")]
    [InlineData("43811234567", "Nordea")]
    [InlineData("49991234567", "Nordea")]
    [InlineData("50001234567", "SEB")]
    [InlineData("54951234567", "SEB")]
    [InlineData("59991234567", "SEB")]
    [InlineData("600012345678", "Handelsbanken")]
    [InlineData("649512345678", "Handelsbanken")]
    [InlineData("699912345678", "Handelsbanken")]
    [InlineData("90201234567", "Länsförsäkringar Bank")]
    [InlineData("90251234567", "Länsförsäkringar Bank")]
    [InlineData("90291234567", "Länsförsäkringar Bank")]
    [InlineData("90401234567", "Citibank Europe plc")]
    [InlineData("90451234567", "Citibank Europe plc")]
    [InlineData("90491234567", "Citibank Europe plc")]
    [InlineData("90601234567", "Länsförsäkringar Bank")]
    [InlineData("90651234567", "Länsförsäkringar Bank")]
    [InlineData("90691234567", "Länsförsäkringar Bank")]
    [InlineData("90701234567", "Multitude Bank")]
    [InlineData("90751234567", "Multitude Bank")]
    [InlineData("90791234567", "Multitude Bank")]
    [InlineData("91001234567", "Nordnet Bank")]
    [InlineData("91051234567", "Nordnet Bank")]
    [InlineData("91091234567", "Nordnet Bank")]
    [InlineData("91201234567", "SEB")]
    [InlineData("91221234567", "SEB")]
    [InlineData("91241234567", "SEB")]
    [InlineData("91301234567", "SEB")]
    [InlineData("91391234567", "SEB")]
    [InlineData("91491234567", "SEB")]
    [InlineData("91501234567", "Skandiabanken")]
    [InlineData("91601234567", "Skandiabanken")]
    [InlineData("91691234567", "Skandiabanken")]
    [InlineData("91701234567", "Ikano Bank")]
    [InlineData("91751234567", "Ikano Bank")]
    [InlineData("91791234567", "Ikano Bank")]
    [InlineData("91801234567", "Danske Bank")]
    [InlineData("91851234567", "Danske Bank")]
    [InlineData("91891234567", "Danske Bank")]
    [InlineData("91901234567", "DNB Sverige")]
    [InlineData("91951234567", "DNB Sverige")]
    [InlineData("91991234567", "DNB Sverige")]
    [InlineData("92301234567", "Marginalen Bank")]
    [InlineData("92351234567", "Marginalen Bank")]
    [InlineData("92391234567", "Marginalen Bank")]
    [InlineData("92501234567", "SBAB Bank")]
    [InlineData("92551234567", "SBAB Bank")]
    [InlineData("92591234567", "SBAB Bank")]
    [InlineData("92701234567", "ICA Banken")]
    [InlineData("92751234567", "ICA Banken")]
    [InlineData("92791234567", "ICA Banken")]
    [InlineData("92801234567", "Resurs Bank")]
    [InlineData("92851234567", "Resurs Bank")]
    [InlineData("92891234567", "Resurs Bank")]
    [InlineData("93001234567", "Swedbank")]
    [InlineData("93151234567", "Swedbank")]
    [InlineData("93291234567", "Swedbank")]
    [InlineData("93301234567", "Swedbank")]
    [InlineData("93391234567", "Swedbank")]
    [InlineData("93491234567", "Swedbank")]
    [InlineData("93901234567", "Landshypotek Bank")]
    [InlineData("93951234567", "Landshypotek Bank")]
    [InlineData("93991234567", "Landshypotek Bank")]
    [InlineData("94601234567", "Santander")]
    [InlineData("94651234567", "Santander")]
    [InlineData("94691234567", "Santander")]
    [InlineData("94701234567", "BNP Paribas S.A. Bankfilial Sverige")]
    [InlineData("94751234567", "BNP Paribas S.A. Bankfilial Sverige")]
    [InlineData("94791234567", "BNP Paribas S.A. Bankfilial Sverige")]
    [InlineData("95001234567890", "Nordea (Plusgirot)")]
    [InlineData("95251234567890", "Nordea (Plusgirot)")]
    [InlineData("95491234567890", "Nordea (Plusgirot)")]
    [InlineData("95501234567", "Avanza Bank")]
    [InlineData("95551234567", "Avanza Bank")]
    [InlineData("95691234567", "Avanza Bank")]
    [InlineData("95701234567890", "Sparbanken Syd")]
    [InlineData("95751234567890", "Sparbanken Syd")]
    [InlineData("95791234567890", "Sparbanken Syd")]
    [InlineData("95801234567", "AION Bank")]
    [InlineData("95851234567", "AION Bank")]
    [InlineData("95891234567", "AION Bank")]
    [InlineData("95901234567", "EP Bank")]
    [InlineData("95951234567", "EP Bank")]
    [InlineData("95991234567", "EP Bank")]
    [InlineData("96301234567", "Lån & Spar Bank")]
    [InlineData("96351234567", "Lån & Spar Bank")]
    [InlineData("96391234567", "Lån & Spar Bank")]
    [InlineData("96601234567", "Svea Bank")]
    [InlineData("96651234567", "Svea Bank")]
    [InlineData("96691234567", "Svea Bank")]
    [InlineData("96701234567", "JAK Medlemsbank")]
    [InlineData("96751234567", "JAK Medlemsbank")]
    [InlineData("96791234567", "JAK Medlemsbank")]
    [InlineData("96801234567", "Enity Bank Group")]
    [InlineData("96851234567", "Enity Bank Group")]
    [InlineData("96891234567", "Enity Bank Group")]
    [InlineData("97001234567", "Ekobanken")]
    [InlineData("97051234567", "Ekobanken")]
    [InlineData("97091234567", "Ekobanken")]
    [InlineData("97101234567", "Lunar Bank")]
    [InlineData("97151234567", "Lunar Bank")]
    [InlineData("97191234567", "Lunar Bank")]
    [InlineData("97501234567", "Northmill Bank")]
    [InlineData("97551234567", "Northmill Bank")]
    [InlineData("97591234567", "Northmill Bank")]
    [InlineData("97801234567", "Klarna Bank")]
    [InlineData("97851234567", "Klarna Bank")]
    [InlineData("97891234567", "Klarna Bank")]
    [InlineData("98801234567890", "Riksgälden")]
    [InlineData("98851234567890", "Riksgälden")]
    [InlineData("98891234567890", "Riksgälden")]
    [InlineData("99601234567890", "Nordea (Plusgirot)")]
    [InlineData("99651234567890", "Nordea (Plusgirot)")]
    [InlineData("99691234567890", "Nordea (Plusgirot)")]
    public void TryParse_BankName_MapsCorrectly(string input, string expectedBankName)
    {
        var ok = SwedishBankAccount.TryParse(input, out var acct);

        Assert.True(ok);
        Assert.NotNull(acct);
        Assert.Equal(expectedBankName, acct!.BankName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("12-3")]
    [InlineData("99991234567")]
    public void TryParse_ReturnsNull_ForUnknownOrInvalidClearing(string? input)
    {
        var ok = SwedishBankAccount.TryParse(input, out var acct);

        Assert.False(ok);
        Assert.Null(acct);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishBankAccount.Parse("70001234567");
        var b = SwedishBankAccount.Parse("  7000  1234567  ");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishBankAccount.Parse("50011234567");
        var b = SwedishBankAccount.Parse("70001234567");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishBankAccount.Parse("50011234567");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishBankAccount.Parse("50011234567");
        var b = SwedishBankAccount.Parse("70001234567");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishBankAccount.Parse("81234123456");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("12345678901234567890123456789012345678901", false, ValidationErrorReason.InputTooLong)]
    [InlineData("12345", false, ValidationErrorReason.InvalidLength)]
    [InlineData("11001234567890", false, ValidationErrorReason.InvalidAccountLengthForBank)]
    [InlineData("50011234567", true, null)]
    [InlineData("81234123456", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishBankAccount.Validate(input);

        Assert.Equal(input, result.RawInput);
        Assert.Equal(expectedIsValid, result.IsValid);

        if (expectedReason is not null)
        {
            Assert.Single(result.Issues);
            Assert.Equal(expectedReason.Value, result.Issues[0].Reason);
        }
        else
        {
            Assert.Empty(result.Issues);
        }
    }

    [Fact]
    public void Validate_Issues_ContainBothLanguageDescriptions()
    {
        var result = SwedishBankAccount.Validate("12345");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("50011234567")]
    [InlineData("81234123456")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("99991234567")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishBankAccount.IsValid(input), SwedishBankAccount.Validate(input).IsValid);
    }
}
