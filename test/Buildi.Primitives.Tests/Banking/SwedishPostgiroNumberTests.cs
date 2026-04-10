using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishPostgiroNumberTests
{
    [Theory]
    [InlineData("47792023")]
    [InlineData("4779202-3")]
    [InlineData("  4779 202 - 3  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishPostgiroNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("4779202")]
    [InlineData("047792023")]
    [InlineData("4779202-4")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishPostgiroNumber.IsValid(input));
    }

    [Theory]
    [InlineData("47792023", "47792023")]
    [InlineData("4779202-3", "47792023")]
    [InlineData("  4779 202 - 3  ", "47792023")]
    public void TryParse_ReturnsDigits_ForValidInput(string input, string expectedDigits)
    {
        var ok = SwedishPostgiroNumber.TryParse(input, out var pg);

        Assert.True(ok);
        Assert.NotNull(pg);
        Assert.Equal(expectedDigits, pg!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("4")]
    [InlineData("477920233")]
    [InlineData("ABCDE")]
    [InlineData("4779202-4")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishPostgiroNumber.TryParse(input, out var pg);

        Assert.False(ok);
        Assert.Null(pg);
    }

    [Theory]
    [InlineData("47792023", "4779202-3")]
    [InlineData("4779202-3", "4779202-3")]
    [InlineData("  4779 202 - 3  ", "4779202-3")]
    public void TryParse_Formatted_ReturnsExpectedValue(string input, string expectedFormatted)
    {
        var ok = SwedishPostgiroNumber.TryParse(input, out var pg);

        Assert.True(ok);
        Assert.Equal(expectedFormatted, pg!.Formatted);
    }

    [Theory]
    [InlineData("47792023", "4779202-3")]
    [InlineData("  4779 202 - 3  ", "4779202-3")]
    [InlineData("4779202-3", "4779202-3")]
    public void Parse_Formatted_ReturnsExpectedValue(string input, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);

        Assert.Equal(expected, pg.Formatted);
    }

    [Theory]
    [InlineData("4779202-4")]
    [InlineData("477920")]
    [InlineData("047792023")]
    [InlineData("4")]
    [InlineData("477920233")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishPostgiroNumber.Parse(input));
    }

    [Theory]
    [InlineData("47792023", "Plusgiro 4779202-3")]
    [InlineData("  4779 202 - 3  ", "Plusgiro 4779202-3")]
    public void ToDisplayString_ReturnsFullDisplay(string input, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);

        Assert.Equal(expected, pg.ToDisplayString());
    }

    [Theory]
    [InlineData("47792023", "PG 4779202-3")]
    [InlineData("  4779 202 - 3  ", "PG 4779202-3")]
    public void ToShortDisplayString_ReturnsShortDisplay(string input, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);

        Assert.Equal(expected, pg.ToShortDisplayString());
    }

    [Theory]
    [InlineData("47792023", "4779202-3")]
    [InlineData("4779202-3", "4779202-3")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);

        Assert.Equal(expected, pg.ToString());
    }

    [Theory]
    [InlineData("47792023", "4779202-3")]
    [InlineData("4779202-3", "4779202-3")]
    [InlineData("  4779 202 - 3  ", "4779202-3")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishPostgiroNumber.Normalize(input));
    }

    [Theory]
    [InlineData("4779202-3", true)]
    [InlineData("47792023", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishPostgiroNumber.IsNormalized(input));
    }

    [Theory]
    [InlineData("47792023", "4779202-3")]
    [InlineData("4779202-3", "4779202-3")]
    public void ToNormalizedString_ReturnsHyphenatedForm(string input, string expected)
    {
        var pg = SwedishPostgiroNumber.Parse(input);

        Assert.Equal(expected, pg.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishPostgiroNumber.Parse("47792023");
        var b = SwedishPostgiroNumber.Parse("4779202-3");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishPostgiroNumber.Parse("47792023");
        var b = SwedishPostgiroNumber.Parse("4131300-8");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishPostgiroNumber.Parse("47792023");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishPostgiroNumber.Parse("4131300-8");
        var b = SwedishPostgiroNumber.Parse("47792023");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishPostgiroNumber.Parse("4779202-3");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("123456789012345678901", false, ValidationErrorReason.InputTooLong)]
    [InlineData("4", false, ValidationErrorReason.InvalidLength)]
    [InlineData("4779202", false, ValidationErrorReason.InvalidCheckDigit)]
    [InlineData("47792023", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishPostgiroNumber.Validate(input);

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
        var result = SwedishPostgiroNumber.Validate("4");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("47792023")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("4")]
    [InlineData("4779202")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishPostgiroNumber.IsValid(input), SwedishPostgiroNumber.Validate(input).IsValid);
    }
}
