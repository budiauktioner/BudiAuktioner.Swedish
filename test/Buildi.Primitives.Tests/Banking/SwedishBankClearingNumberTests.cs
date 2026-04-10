using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishBankClearingNumberTests
{
    [Theory]
    [InlineData("5001")]
    [InlineData("6789")]
    [InlineData("7000")]
    [InlineData("1100")]
    [InlineData("3300")]
    [InlineData("3782")]
    [InlineData("9270")]
    [InlineData("9550")]
    [InlineData("81234")]
    [InlineData("  5001  ")]
    [InlineData("5001-")]
    public void IsValid_ReturnsTrue_ForValidClearingNumbers(string input)
    {
        Assert.True(SwedishBankClearingNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("999")]
    [InlineData("9999")]
    [InlineData("00001")]
    [InlineData("41234")]
    [InlineData("123456")]
    public void IsValid_ReturnsFalse_ForInvalidClearingNumbers(string? input)
    {
        Assert.False(SwedishBankClearingNumber.IsValid(input));
    }

    [Theory]
    [InlineData("5001", "5001", SwedishBank.SEB, "SEB")]
    [InlineData("6789", "6789", SwedishBank.Handelsbanken, "Handelsbanken")]
    [InlineData("7000", "7000", SwedishBank.Swedbank, "Swedbank")]
    [InlineData("1100", "1100", SwedishBank.Nordea, "Nordea")]
    [InlineData("3300", "3300", SwedishBank.NordeaPersonkonto, "Nordea Personkonto")]
    [InlineData("9270", "9270", SwedishBank.IcaBanken, "ICA Banken")]
    [InlineData("81234", "81234", SwedishBank.Swedbank, "Swedbank")]
    public void TryParse_ReturnsCorrectBankInfo(string input, string expectedDigits, SwedishBank expectedBank, string expectedBankName)
    {
        var ok = SwedishBankClearingNumber.TryParse(input, out var clearing);

        Assert.True(ok);
        Assert.NotNull(clearing);
        Assert.Equal(expectedDigits, clearing!.Digits);
        Assert.Equal(expectedBank, clearing.Bank);
        Assert.Equal(expectedBankName, clearing.BankName);
    }

    [Theory]
    [InlineData("5001", "5001")]
    [InlineData("81234", "8123-4")]
    [InlineData("  5001  ", "5001")]
    public void ToString_ReturnsDisplayFormat(string input, string expected)
    {
        var clearing = SwedishBankClearingNumber.Parse(input);
        Assert.Equal(expected, clearing.ToString());
    }

    [Theory]
    [InlineData("5001", "5001")]
    [InlineData("81234", "81234")]
    public void ToNormalizedString_ReturnsDigitsOnly(string input, string expected)
    {
        var clearing = SwedishBankClearingNumber.Parse(input);
        Assert.Equal(expected, clearing.ToNormalizedString());
    }

    [Theory]
    [InlineData("5001", "5001")]
    [InlineData("81234", "8123-4")]
    public void Format_ReturnsDisplayFormat_ForValidInput(string input, string expected)
    {
        Assert.Equal(expected, SwedishBankClearingNumber.Format(input));
    }

    [Theory]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishBankClearingNumber.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalidInput()
    {
        Assert.Equal("invalid", SwedishBankClearingNumber.Format("  invalid  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("5001", "5001")]
    [InlineData("81234", "81234")]
    public void Normalize_ReturnsDigits_ForValidInput(string input, string expected)
    {
        Assert.Equal(expected, SwedishBankClearingNumber.Normalize(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    public void Normalize_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.Null(SwedishBankClearingNumber.Normalize(input));
    }

    [Fact]
    public void Parse_ThrowsArgumentException_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => SwedishBankClearingNumber.Parse("invalid"));
    }

    [Fact]
    public void SwedishBankAccount_ExposesClearing_AsTypedProperty()
    {
        var account = SwedishBankAccount.Parse("50011234567");

        Assert.NotNull(account.Clearing);
        Assert.Equal("5001", account.Clearing.Digits);
        Assert.Equal(SwedishBank.SEB, account.Clearing.Bank);
        Assert.Equal("SEB", account.Clearing.BankName);
        Assert.Equal("5001", account.ClearingNumber);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishBankClearingNumber.Parse("5001");
        var b = SwedishBankClearingNumber.Parse("  5001  ");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishBankClearingNumber.Parse("5001");
        var b = SwedishBankClearingNumber.Parse("6789");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishBankClearingNumber.Parse("5001");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishBankClearingNumber.Parse("5001");
        var b = SwedishBankClearingNumber.Parse("6789");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishBankClearingNumber.Parse("1100");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("999", false, ValidationErrorReason.InvalidLength)]
    [InlineData("123456", false, ValidationErrorReason.InvalidLength)]
    [InlineData("41234", false, ValidationErrorReason.InvalidSwedbankFormat)]
    [InlineData("9999", false, ValidationErrorReason.UnknownClearingRange)]
    [InlineData("5001", true, null)]
    [InlineData("81234", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishBankClearingNumber.Validate(input);

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
        var result = SwedishBankClearingNumber.Validate("999");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("5001")]
    [InlineData("81234")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("999")]
    [InlineData("9999")]
    [InlineData("41234")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishBankClearingNumber.IsValid(input), SwedishBankClearingNumber.Validate(input).IsValid);
    }
}
