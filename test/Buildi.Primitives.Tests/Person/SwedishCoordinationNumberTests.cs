using ActiveLogin.Identity.Swedish;
using Buildi.Primitives.Person;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Person;

public class SwedishCoordinationNumberTests
{
    [Theory]
    [InlineData("196801642395")]
    [InlineData("680164-2395")]
    [InlineData("6801642395")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishCoordinationNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("000000-0000")]
    [InlineData("notanumber")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishCoordinationNumber.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsExpectedProperties()
    {
        Assert.True(SwedishCoordinationNumber.TryParse("680164-2395", out var cn));
        Assert.Equal("196801642395", cn!.Value);
        Assert.Equal("680164-2395", cn.Formatted);
        Assert.Equal(4, cn.RealDay);
        Assert.Equal(ActiveLogin.Identity.Swedish.Gender.Male, cn.GenderHint);
    }

    [Theory]
    [InlineData("680164-2395", "680164-2395")]
    [InlineData("196801642395", "680164-2395")]
    public void Format_ReturnsDisplayForm(string input, string expected)
    {
        Assert.Equal(expected, SwedishCoordinationNumber.Format(input));
    }

    [Theory]
    [InlineData("680164-2395", "196801642395")]
    [InlineData("196801642395", "196801642395")]
    public void Normalize_Returns12DigitForm(string input, string expected)
    {
        Assert.Equal(expected, SwedishCoordinationNumber.Normalize(input));
    }

    [Fact]
    public void Normalize_InvalidInput_ReturnsNull()
    {
        Assert.Null(SwedishCoordinationNumber.Normalize("invalid"));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => SwedishCoordinationNumber.Parse("invalid"));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        var cn = SwedishCoordinationNumber.Parse("680164-2395");
        Assert.Equal("680164-2395", cn.ToString());
        Assert.Equal("196801642395", cn.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishCoordinationNumber.Parse("680164-2395");
        var b = SwedishCoordinationNumber.Parse("680164-2395");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishCoordinationNumber.Parse("680164-2395");
        var b = SwedishCoordinationNumber.Parse("990867-2398");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishCoordinationNumber.Parse("680164-2395");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishCoordinationNumber.Parse("680164-2395");
        var b = SwedishCoordinationNumber.Parse("990867-2398");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishCoordinationNumber.Parse("680164-2395");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("123", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("000000-0000", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("196801642395", true, null)]
    [InlineData("680164-2395", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishCoordinationNumber.Validate(input);

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
        var result = SwedishCoordinationNumber.Validate("123");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("196801642395")]
    [InlineData("680164-2395")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("000000-0000")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishCoordinationNumber.IsValid(input), SwedishCoordinationNumber.Validate(input).IsValid);
    }
}
