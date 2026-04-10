using Buildi.Primitives.Validation;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class SwedishVehicleRegistrationNumberTests
{
    [Theory]
    [InlineData("ABC123")]
    [InlineData("ABC 123")]
    [InlineData("abc 123")]
    [InlineData("ABC12A")]
    [InlineData("ABC 12A")]
    [InlineData("abc 12a")]
    [InlineData("  ABC 123  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishVehicleRegistrationNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("AB1234")]
    [InlineData("ABCD12")]
    [InlineData("ABC1234")]
    [InlineData("IBC123")]
    [InlineData("QBC123")]
    [InlineData("VBC123")]
    [InlineData("ÅBC123")]
    [InlineData("ÄBC123")]
    [InlineData("ÖBC123")]
    [InlineData("ABC12I")]
    [InlineData("ABC12O")]
    [InlineData("ABC12Q")]
    [InlineData("ABC12V")]
    [InlineData("ABC12Å")]
    [InlineData("ABC12Ä")]
    [InlineData("ABC12Ö")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishVehicleRegistrationNumber.IsValid(input));
    }

    [Theory]
    [InlineData("OBC123")]
    [InlineData("AOC123")]
    [InlineData("ABO123")]
    public void IsValid_ReturnsTrue_O_AllowedInFirstThreePositions(string input)
    {
        Assert.True(SwedishVehicleRegistrationNumber.IsValid(input));
    }

    [Theory]
    [InlineData("ABC123", "ABC123", "ABC 123", "ABC", "123", false)]
    [InlineData("abc 123", "ABC123", "ABC 123", "ABC", "123", false)]
    [InlineData("ABC12A", "ABC12A", "ABC 12A", "ABC", "12A", true)]
    [InlineData("abc 12a", "ABC12A", "ABC 12A", "ABC", "12A", true)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted,
        string expectedLetters, string expectedSuffix, bool expectedNewFormat)
    {
        var ok = SwedishVehicleRegistrationNumber.TryParse(input, out var reg);

        Assert.True(ok);
        Assert.NotNull(reg);
        Assert.Equal(expectedValue, reg!.Value);
        Assert.Equal(expectedFormatted, reg.Formatted);
        Assert.Equal(expectedLetters, reg.Letters);
        Assert.Equal(expectedSuffix, reg.Suffix);
        Assert.Equal(expectedNewFormat, reg.IsNewFormat);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("INVALID")]
    [InlineData("ABC12Ö")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishVehicleRegistrationNumber.TryParse(input, out var reg);

        Assert.False(ok);
        Assert.Null(reg);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => SwedishVehicleRegistrationNumber.Parse("INVALID"));
    }

    [Theory]
    [InlineData("ABC120", 0, 3)]
    [InlineData("ABC121", 1, 4)]
    [InlineData("ABC122", 2, 5)]
    [InlineData("ABC123", 3, 6)]
    [InlineData("ABC124", 4, 8)]
    [InlineData("ABC125", 5, 10)]
    [InlineData("ABC126", 6, 11)]
    [InlineData("ABC127", 7, 12)]
    [InlineData("ABC128", 8, 1)]
    [InlineData("ABC129", 9, 2)]
    public void TaxPayment_ClassicFormat_UsesLastDigit(string input, int expectedDigit, int expectedMonth)
    {
        var reg = SwedishVehicleRegistrationNumber.Parse(input);

        Assert.Equal(expectedDigit, reg.TaxPaymentDigit);
        Assert.Equal(expectedMonth, reg.TaxPaymentMonth);
    }

    [Theory]
    [InlineData("ABC10A", 0, 3)]
    [InlineData("ABC11B", 1, 4)]
    [InlineData("ABC12C", 2, 5)]
    [InlineData("ABC13D", 3, 6)]
    [InlineData("ABC14E", 4, 8)]
    [InlineData("ABC15F", 5, 10)]
    [InlineData("ABC16G", 6, 11)]
    [InlineData("ABC17H", 7, 12)]
    [InlineData("ABC18J", 8, 1)]
    [InlineData("ABC19K", 9, 2)]
    public void TaxPayment_NewFormat_UsesSecondToLastDigit(string input, int expectedDigit, int expectedMonth)
    {
        var reg = SwedishVehicleRegistrationNumber.Parse(input);

        Assert.Equal(expectedDigit, reg.TaxPaymentDigit);
        Assert.Equal(expectedMonth, reg.TaxPaymentMonth);
    }

    [Theory]
    [InlineData("abc123", "ABC 123")]
    [InlineData("ABC 12A", "ABC 12A")]
    [InlineData("invalid", null)]
    [InlineData("  invalid  ", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleRegistrationNumber.Format(input));
    }

    [Theory]
    [InlineData("invalid", "invalid")]
    [InlineData("  invalid  ", "invalid")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleRegistrationNumber.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("abc 123", "ABC123")]
    [InlineData("ABC 12A", "ABC12A")]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishVehicleRegistrationNumber.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        var reg = SwedishVehicleRegistrationNumber.Parse("ABC123");

        Assert.Equal("ABC 123", reg.ToString());
        Assert.Equal("ABC123", reg.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishVehicleRegistrationNumber.Parse("ABC123");
        var b = SwedishVehicleRegistrationNumber.Parse("ABC123");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishVehicleRegistrationNumber.Parse("ABC123");
        var b = SwedishVehicleRegistrationNumber.Parse("ABC12A");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishVehicleRegistrationNumber.Parse("ABC123");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishVehicleRegistrationNumber.Parse("ABC123");
        var b = SwedishVehicleRegistrationNumber.Parse("ABC12A");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishVehicleRegistrationNumber.Parse("ABC 123");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("AB", false, ValidationErrorReason.InvalidLength)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU", false, ValidationErrorReason.InputTooLong)]
    [InlineData("IBC123", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("ABC123", true, null)]
    [InlineData("ABC12A", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishVehicleRegistrationNumber.Validate(input);

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
}
