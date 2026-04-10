using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishBankgiroNumberTests
{
    [Theory]
    [InlineData("2359321")]
    [InlineData("235-9321")]
    [InlineData("54649652")]
    [InlineData("5464-9652")]
    [InlineData("54899109")]
    [InlineData("5489-9109")]
    [InlineData("  5489  -  9109  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishBankgiroNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("2359322")]
    [InlineData("54649653")]
    [InlineData("54899108")]
    [InlineData("235-932A")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishBankgiroNumber.IsValid(input));
    }

    [Theory]
    [InlineData(" 235-9321 ", "2359321")]
    [InlineData(" 5464-9652 ", "54649652")]
    [InlineData("54 64-9652", "54649652")]
    [InlineData("235-9321", "2359321")]
    [InlineData("5489  - 9109", "54899109")]
    public void TryParse_ReturnsDigits_ForValidInput(string input, string expectedDigits)
    {
        var ok = SwedishBankgiroNumber.TryParse(input, out var bg);

        Assert.True(ok);
        Assert.NotNull(bg);
        Assert.Equal(expectedDigits, bg!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123456")]
    [InlineData("123456789")]
    [InlineData("ABCDE")]
    [InlineData("2359322")]
    [InlineData("54649653")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishBankgiroNumber.TryParse(input, out var bg);

        Assert.False(ok);
        Assert.Null(bg);
    }

    [Theory]
    [InlineData("2359321", "235-9321")]
    [InlineData("235-9321", "235-9321")]
    [InlineData("54649652", "5464-9652")]
    [InlineData("5464-9652", "5464-9652")]
    [InlineData("  5489-9109  ", "5489-9109")]
    public void TryParse_Formatted_ReturnsExpectedValue(string input, string expectedFormatted)
    {
        var ok = SwedishBankgiroNumber.TryParse(input, out var bg);

        Assert.True(ok);
        Assert.Equal(expectedFormatted, bg!.Formatted);
    }

    [Theory]
    [InlineData("2359321", "235-9321")]
    [InlineData("54649652", "5464-9652")]
    [InlineData("5464-9652", "5464-9652")]
    [InlineData("  5489-9109  ", "5489-9109")]
    public void Parse_Formatted_ReturnsExpectedValue(string input, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);

        Assert.Equal(expected, bg.Formatted);
    }

    [Theory]
    [InlineData("2359322")]
    [InlineData("235932")]
    [InlineData("23593211")]
    [InlineData("235932111")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishBankgiroNumber.Parse(input));
    }

    [Theory]
    [InlineData("2359321", "Bankgiro 235-9321")]
    [InlineData(" 5464-9652 ", "Bankgiro 5464-9652")]
    public void ToDisplayString_ReturnsFullDisplay(string input, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);

        Assert.Equal(expected, bg.ToDisplayString());
    }

    [Theory]
    [InlineData("2359321", "BG 235-9321")]
    [InlineData(" 5464-9652 ", "BG 5464-9652")]
    public void ToShortDisplayString_ReturnsShortDisplay(string input, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);

        Assert.Equal(expected, bg.ToShortDisplayString());
    }

    [Theory]
    [InlineData("2359321", "235-9321")]
    [InlineData("5464-9652", "5464-9652")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);

        Assert.Equal(expected, bg.ToString());
    }

    [Theory]
    [InlineData("2359321", "235-9321")]
    [InlineData("235-9321", "235-9321")]
    [InlineData("54649652", "5464-9652")]
    [InlineData("5464-9652", "5464-9652")]
    [InlineData("  5489-9109  ", "5489-9109")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishBankgiroNumber.Normalize(input));
    }

    [Theory]
    [InlineData("235-9321", true)]
    [InlineData("2359321", false)]
    [InlineData("5464-9652", true)]
    [InlineData("54649652", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishBankgiroNumber.IsNormalized(input));
    }

    [Theory]
    [InlineData("2359321", "235-9321")]
    [InlineData("54649652", "5464-9652")]
    public void ToNormalizedString_ReturnsHyphenatedForm(string input, string expected)
    {
        var bg = SwedishBankgiroNumber.Parse(input);

        Assert.Equal(expected, bg.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishBankgiroNumber.Parse("2359321");
        var b = SwedishBankgiroNumber.Parse("235-9321");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishBankgiroNumber.Parse("2359321");
        var b = SwedishBankgiroNumber.Parse("54649652");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishBankgiroNumber.Parse("2359321");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishBankgiroNumber.Parse("2359321");
        var b = SwedishBankgiroNumber.Parse("54649652");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishBankgiroNumber.Parse("54899109");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("123456789012345678901", false, ValidationErrorReason.InputTooLong)]
    [InlineData("123456", false, ValidationErrorReason.InvalidLength)]
    [InlineData("2359322", false, ValidationErrorReason.InvalidCheckDigit)]
    [InlineData("58056201", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishBankgiroNumber.Validate(input);

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
        var result = SwedishBankgiroNumber.Validate("123456");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("58056201")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456")]
    [InlineData("2359322")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishBankgiroNumber.IsValid(input), SwedishBankgiroNumber.Validate(input).IsValid);
    }
}
