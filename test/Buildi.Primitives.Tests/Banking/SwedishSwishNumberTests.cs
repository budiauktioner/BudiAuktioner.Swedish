using Buildi.Primitives.Banking;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishSwishNumberTests
{
    // --- IsValid ---

    [Theory]
    [InlineData("1231234567")]
    [InlineData("1236652895")]
    [InlineData("123-665 28 95")]
    [InlineData("123 665 28 95")]
    [InlineData("1239020033")]
    [InlineData("9020033")]
    [InlineData("902 00 33")]
    [InlineData("900 80 95")]
    [InlineData("  1231234567  ")]
    [InlineData("0701234567")]
    [InlineData("070-123 45 67")]
    [InlineData("+46701234567")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishSwishNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("1234567890123")]
    [InlineData("4561234567")]
    [InlineData("0812345678")]
    [InlineData("ABC")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishSwishNumber.IsValid(input));
    }

    // --- TryParse: Swish 123 ---

    [Theory]
    [InlineData("1231234567", "1231234567", "123-123 45 67", true, false, false)]
    [InlineData("1236652895", "1236652895", "123-665 28 95", true, false, false)]
    [InlineData("123-665 28 95", "1236652895", "123-665 28 95", true, false, false)]
    [InlineData("123 665 28 95", "1236652895", "123-665 28 95", true, false, false)]
    [InlineData("  1231234567  ", "1231234567", "123-123 45 67", true, false, false)]
    public void TryParse_Swish123_ReturnsExpectedProperties(
        string input, string expectedValue, string expectedFormatted,
        bool expectedIs123, bool expectedIsMobile, bool expectedIs90)
    {
        var ok = SwedishSwishNumber.TryParse(input, out var swish);
        Assert.True(ok);
        Assert.NotNull(swish);
        Assert.Equal(expectedValue, swish.Value);
        Assert.Equal(expectedFormatted, swish.Formatted);
        Assert.Equal(expectedIs123, swish.IsSwish123);
        Assert.Equal(expectedIsMobile, swish.IsMobileNumber);
        Assert.Equal(expectedIs90, swish.Is90Number);
        Assert.Null(swish.PhoneNumber);
    }

    // --- TryParse: 90-numbers ---

    [Theory]
    [InlineData("9020033", "1239020033", "902 00 33", true)]
    [InlineData("902 00 33", "1239020033", "902 00 33", true)]
    [InlineData("9008095", "1239008095", "900 80 95", true)]
    [InlineData("1239020033", "1239020033", "902 00 33", true)]
    [InlineData("123-902 00 33", "1239020033", "902 00 33", true)]
    public void TryParse_90Number_ReturnsExpectedProperties(
        string input, string expectedValue, string expectedFormatted, bool expectedIs90)
    {
        var ok = SwedishSwishNumber.TryParse(input, out var swish);
        Assert.True(ok);
        Assert.NotNull(swish);
        Assert.Equal(expectedValue, swish.Value);
        Assert.Equal(expectedFormatted, swish.Formatted);
        Assert.True(swish.IsSwish123);
        Assert.False(swish.IsMobileNumber);
        Assert.Equal(expectedIs90, swish.Is90Number);
        Assert.Null(swish.PhoneNumber);
    }

    // --- TryParse: Mobile numbers ---

    [Theory]
    [InlineData("0701234567")]
    [InlineData("070-123 45 67")]
    [InlineData("+46701234567")]
    public void TryParse_MobileNumber_ReturnsExpectedProperties(string input)
    {
        var ok = SwedishSwishNumber.TryParse(input, out var swish);
        Assert.True(ok);
        Assert.NotNull(swish);
        Assert.False(swish.IsSwish123);
        Assert.True(swish.IsMobileNumber);
        Assert.False(swish.Is90Number);
        Assert.NotNull(swish.PhoneNumber);
        Assert.True(swish.PhoneNumber.IsSwedish);
        Assert.True(swish.PhoneNumber.IsMobile);
    }

    // --- TryParse: Invalid ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("4561234567")]
    [InlineData("0812345678")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishSwishNumber.TryParse(input, out var swish);
        Assert.False(ok);
        Assert.Null(swish);
    }

    // --- Parse throws ---

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("invalid")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishSwishNumber.Parse(input));
    }

    // --- Format ---

    [Theory]
    [InlineData("1236652895", "123-665 28 95")]
    [InlineData("9020033", "902 00 33")]
    [InlineData("0701234567", "0701-23 45 67")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishSwishNumber.Format(input));
    }

    [Theory]
    [InlineData("invalid", "invalid")]
    [InlineData("  invalid  ", "invalid")]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Format_WithFallback_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishSwishNumber.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    // --- Normalize ---

    [Theory]
    [InlineData("1236652895", "1236652895")]
    [InlineData("123-665 28 95", "1236652895")]
    [InlineData("9020033", "1239020033")]
    [InlineData("902 00 33", "1239020033")]
    [InlineData("0701234567", "0046701234567")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishSwishNumber.Normalize(input));
    }

    // --- IsNormalized ---

    [Theory]
    [InlineData("1236652895")]
    [InlineData("1231234567")]
    [InlineData("1239020033")]
    [InlineData("0046701234567")]
    public void IsNormalized_ReturnsTrue_ForNormalizedInputs(string input)
    {
        Assert.True(SwedishSwishNumber.IsNormalized(input));
    }

    [Theory]
    [InlineData("123-665 28 95")]
    [InlineData("9020033")]
    [InlineData("0701234567")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void IsNormalized_ReturnsFalse_ForNonNormalizedInputs(string? input)
    {
        Assert.False(SwedishSwishNumber.IsNormalized(input));
    }

    // --- ToString / ToNormalizedString ---

    [Theory]
    [InlineData("1236652895", "123-665 28 95")]
    [InlineData("9020033", "902 00 33")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var swish = SwedishSwishNumber.Parse(input);
        Assert.Equal(expected, swish.ToString());
    }

    [Theory]
    [InlineData("1236652895", "1236652895")]
    [InlineData("9020033", "1239020033")]
    public void ToNormalizedString_ReturnsNormalizedValue(string input, string expected)
    {
        var swish = SwedishSwishNumber.Parse(input);
        Assert.Equal(expected, swish.ToNormalizedString());
    }

    // --- ToMaskedString ---

    [Fact]
    public void ToMaskedString_Swish123_MasksDigits()
    {
        var swish = SwedishSwishNumber.Parse("1236652895");
        Assert.Equal("123-*** ** **", swish.ToMaskedString());
    }

    [Fact]
    public void ToMaskedString_90Number_MasksDigits()
    {
        var swish = SwedishSwishNumber.Parse("9020033");
        Assert.Equal("9** ** **", swish.ToMaskedString());
    }

    // --- Landline phone rejected ---

    [Fact]
    public void TryParse_RejectsSwedishLandline()
    {
        Assert.False(SwedishSwishNumber.IsValid("0812345678"));
    }

    [Fact]
    public void TryParse_RejectsNonSwedishMobile()
    {
        Assert.False(SwedishSwishNumber.IsValid("+447012345678"));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishSwishNumber.Parse("1236652895");
        var b = SwedishSwishNumber.Parse("123-665 28 95");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishSwishNumber.Parse("1231234567");
        var b = SwedishSwishNumber.Parse("1236652895");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishSwishNumber.Parse("1231234567");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishSwishNumber.Parse("1231234567");
        var b = SwedishSwishNumber.Parse("1236652895");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishSwishNumber.Parse("9020033");
        Assert.Equal(1, a.CompareTo(null));
    }
}
