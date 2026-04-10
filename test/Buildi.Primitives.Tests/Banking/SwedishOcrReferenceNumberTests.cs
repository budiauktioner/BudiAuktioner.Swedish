using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishOcrReferenceNumberTests
{
    [Fact]
    public void IsValid_ReturnsTrue_ForBasicValidReference()
    {
        var input = CreateBasicReference("12345");

        Assert.True(SwedishOcrReferenceNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("12345678901234567890123456")]
    [InlineData("1234567890")]
    public void IsValid_ReturnsFalse_ForInvalidReference(string? input)
    {
        Assert.False(SwedishOcrReferenceNumber.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsComponents_ForBasicReference()
    {
        var input = CreateBasicReference("12345");

        var ok = SwedishOcrReferenceNumber.TryParse($"  {input}  ", out var ocr);

        Assert.True(ok);
        Assert.NotNull(ocr);
        Assert.Equal(input, ocr!.Value);
        Assert.Equal(input.Length, ocr.Length);
        Assert.Null(ocr.LengthDigit);
        Assert.Equal(input[^1] - '0', ocr.CheckDigit);
    }

    [Fact]
    public void TryParseVariableLengthDigit_ReturnsLengthDigit()
    {
        var input = CreateVariableLengthReference("123456");

        var ok = SwedishOcrReferenceNumber.TryParseVariableLengthDigit(input, out var ocr);

        Assert.True(ok);
        Assert.NotNull(ocr);
        Assert.Equal(input[^2] - '0', ocr!.LengthDigit);
    }

    [Fact]
    public void TryParseVariableLengthDigit_ReturnsFalse_WhenLengthDigitIsWrong()
    {
        var input = CreateVariableLengthReference("123456");
        var invalid = input[..^2] + "0" + input[^1];

        var ok = SwedishOcrReferenceNumber.TryParseVariableLengthDigit(invalid, out var ocr);

        Assert.False(ok);
        Assert.Null(ocr);
    }

    [Fact]
    public void TryParseFixedLength_ReturnsTrue_ForMatchingLength()
    {
        var input = CreateBasicReference(new string('7', 9));

        var ok = SwedishOcrReferenceNumber.TryParseFixedLength(input, 10, out var ocr);

        Assert.True(ok);
        Assert.NotNull(ocr);
    }

    [Fact]
    public void TryParseFixedLength_ReturnsFalse_ForWrongLength()
    {
        var input = CreateBasicReference(new string('7', 9));

        var ok = SwedishOcrReferenceNumber.TryParseFixedLength(input, 12, out var ocr);

        Assert.False(ok);
        Assert.Null(ocr);
    }

    [Fact]
    public void Format_Normalize_And_DisplayStrings_ReturnExpectedValues()
    {
        var input = CreateBasicReference("12345");
        var ocr = SwedishOcrReferenceNumber.Parse(input);

        Assert.Equal(input, SwedishOcrReferenceNumber.Format(input));
        Assert.Equal(input, SwedishOcrReferenceNumber.Normalize(input));
        Assert.Equal($"OCR {input}", ocr.ToDisplayString());
        Assert.Equal($"OCR {input}", ocr.ToShortDisplayString());
        Assert.Equal(input, ocr.ToNormalizedString());
        Assert.Equal(input, ocr.ToString());
    }

    [Fact]
    public void Format_ReturnsNull_WhenInvalid_AndTrimmedOriginal_WhenFallbackRequested()
    {
        Assert.Null(SwedishOcrReferenceNumber.Format(" 1234567890 "));
        Assert.Equal("1234567890", SwedishOcrReferenceNumber.Format(" 1234567890 ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SwedishOcrReferenceNumber.Format(" "));
    }

    private static string CreateBasicReference(string bodyWithoutCheckDigit)
        => bodyWithoutCheckDigit + CalculateCheckDigit(bodyWithoutCheckDigit);

    private static string CreateVariableLengthReference(string bodyWithoutLengthAndCheckDigits)
    {
        var lengthDigit = ((bodyWithoutLengthAndCheckDigits.Length + 2) % 10).ToString();
        var prefix = bodyWithoutLengthAndCheckDigits + lengthDigit;
        return prefix + CalculateCheckDigit(prefix);
    }

    private static char CalculateCheckDigit(string digitsWithoutCheckDigit)
    {
        for (var digit = '0'; digit <= '9'; digit++)
        {
            if (IsLuhnValid(digitsWithoutCheckDigit + digit))
                return digit;
        }

        throw new InvalidOperationException("Could not calculate a valid Luhn check digit.");
    }

    private static bool IsLuhnValid(string digits)
    {
        var sum = 0;
        var len = digits.Length;

        for (var i = 0; i < len; i++)
        {
            var d = digits[i] - '0';
            if ((len - i) % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }

            sum += d;
        }

        return sum % 10 == 0;
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var input = CreateBasicReference("12345");
        var a = SwedishOcrReferenceNumber.Parse(input);
        var b = SwedishOcrReferenceNumber.Parse(input);
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishOcrReferenceNumber.Parse(CreateBasicReference("12345"));
        var b = SwedishOcrReferenceNumber.Parse(CreateBasicReference("98765"));
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishOcrReferenceNumber.Parse(CreateBasicReference("12345"));
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishOcrReferenceNumber.Parse(CreateBasicReference("12345"));
        var b = SwedishOcrReferenceNumber.Parse(CreateBasicReference("98765"));
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishOcrReferenceNumber.Parse(CreateBasicReference("12345"));
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("1", false, ValidationErrorReason.InvalidLength)]
    [InlineData("12345678901234567890123456", false, ValidationErrorReason.InvalidLength)]
    [InlineData("123456", false, ValidationErrorReason.InvalidCheckDigit)]
    [InlineData("123455", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishOcrReferenceNumber.Validate(input);

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
        var result = SwedishOcrReferenceNumber.Validate("1");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("123455")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("123456")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishOcrReferenceNumber.IsValid(input), SwedishOcrReferenceNumber.Validate(input).IsValid);
    }
}
