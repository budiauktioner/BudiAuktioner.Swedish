using ActiveLogin.Identity.Swedish;
using Buildi.Primitives.Person;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Person;

public class SwedishPersonalIdentityNumberTests
{
    [Theory]
    [InlineData("199908072391")]
    [InlineData("990807-2391")]
    [InlineData("9908072391")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishPersonalIdentityNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("000000-0000")]
    [InlineData("notanumber")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishPersonalIdentityNumber.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsExpectedProperties()
    {
        Assert.True(SwedishPersonalIdentityNumber.TryParse("990807-2391", out var pin));
        Assert.Equal("199908072391", pin!.Value);
        Assert.Equal("990807-2391", pin.Formatted);
        Assert.Equal(new DateTime(1999, 8, 7), pin.DateOfBirthHint);
        Assert.Equal(ActiveLogin.Identity.Swedish.Gender.Male, pin.GenderHint);
    }

    [Theory]
    [InlineData("990807-2391", "990807-2391")]
    [InlineData("199908072391", "990807-2391")]
    public void Format_ReturnsDisplayForm(string input, string expected)
    {
        Assert.Equal(expected, SwedishPersonalIdentityNumber.Format(input));
    }

    [Theory]
    [InlineData("990807-2391", "199908072391")]
    [InlineData("199908072391", "199908072391")]
    public void Normalize_Returns12DigitForm(string input, string expected)
    {
        Assert.Equal(expected, SwedishPersonalIdentityNumber.Normalize(input));
    }

    [Fact]
    public void Normalize_InvalidInput_ReturnsNull()
    {
        Assert.Null(SwedishPersonalIdentityNumber.Normalize("invalid"));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => SwedishPersonalIdentityNumber.Parse("invalid"));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        var pin = SwedishPersonalIdentityNumber.Parse("990807-2391");
        Assert.Equal("990807-2391", pin.ToString());
        Assert.Equal("199908072391", pin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishPersonalIdentityNumber.Parse("990807-2391");
        var b = SwedishPersonalIdentityNumber.Parse("990807-2391");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishPersonalIdentityNumber.Parse("990807-2391");
        var b = SwedishPersonalIdentityNumber.Parse("199102152387");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishPersonalIdentityNumber.Parse("990807-2391");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishPersonalIdentityNumber.Parse("199102152387");
        var b = SwedishPersonalIdentityNumber.Parse("990807-2391");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishPersonalIdentityNumber.Parse("990807-2391");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("123", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("000000-0000", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("199908072391", true, null)]
    [InlineData("990807-2391", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishPersonalIdentityNumber.Validate(input);

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
        var result = SwedishPersonalIdentityNumber.Validate("123");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("199908072391")]
    [InlineData("990807-2391")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("000000-0000")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishPersonalIdentityNumber.IsValid(input), SwedishPersonalIdentityNumber.Validate(input).IsValid);
    }
}
