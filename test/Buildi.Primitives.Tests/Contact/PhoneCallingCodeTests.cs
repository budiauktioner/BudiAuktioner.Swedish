using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class PhoneCallingCodeTests
{
    [Theory]
    [InlineData("46")]
    [InlineData("+46")]
    [InlineData("0046")]
    [InlineData("1")]
    [InlineData("+1")]
    [InlineData("001")]
    [InlineData("358")]
    [InlineData("+358")]
    [InlineData("00358")]
    [InlineData("44")]
    [InlineData("7")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(PhoneCallingCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("999")]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("abc")]
    [InlineData("+abc")]
    [InlineData("1234")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(PhoneCallingCode.IsValid(input));
    }

    [Theory]
    [InlineData("46", "46", "SE")]
    [InlineData("+46", "46", "SE")]
    [InlineData("0046", "46", "SE")]
    [InlineData("1", "1", "US")]
    [InlineData("+1", "1", "US")]
    [InlineData("001", "1", "US")]
    [InlineData("44", "44", "GB")]
    [InlineData("7", "7", "RU")]
    [InlineData("358", "358", "FI")]
    [InlineData("+358", "358", "FI")]
    [InlineData("47", "47", "NO")]
    [InlineData("49", "49", "DE")]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(string input, string expectedValue, string expectedCountryCode)
    {
        Assert.True(PhoneCallingCode.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedCountryCode, result.CountryCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("999")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(PhoneCallingCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("999")]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => PhoneCallingCode.Parse(input));
    }

    [Theory]
    [InlineData("46", "+46")]
    [InlineData("+46", "+46")]
    [InlineData("0046", "+46")]
    [InlineData("1", "+1")]
    [InlineData("358", "+358")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PhoneCallingCode.Format(input));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("999", null)]
    public void Format_ReturnsNull_ForInvalidInputs(string? input, string? expected)
    {
        Assert.Equal(expected, PhoneCallingCode.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput()
    {
        Assert.Equal("999", PhoneCallingCode.Format("999", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Equal("abc", PhoneCallingCode.Format(" abc ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Format(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Format("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Format(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("46", "46")]
    [InlineData("+46", "46")]
    [InlineData("0046", "46")]
    [InlineData("1", "1")]
    [InlineData("358", "358")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PhoneCallingCode.Normalize(input));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("999", null)]
    public void Normalize_ReturnsNull_ForInvalidInputs(string? input, string? expected)
    {
        Assert.Equal(expected, PhoneCallingCode.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput()
    {
        Assert.Equal("999", PhoneCallingCode.Normalize("999", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Normalize(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PhoneCallingCode.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("46", true)]
    [InlineData("1", true)]
    [InlineData("+46", false)]
    [InlineData("0046", false)]
    [InlineData("999", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PhoneCallingCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        var code = PhoneCallingCode.Parse("46");
        Assert.Equal("+46", code.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsDigitsOnly()
    {
        var code = PhoneCallingCode.Parse("46");
        Assert.Equal("46", code.ToNormalizedString());
    }

    [Fact]
    public void Constants_Sweden_HasCorrectValues()
    {
        Assert.Equal("46", PhoneCallingCode.Sweden.Value);
        Assert.Equal("SE", PhoneCallingCode.Sweden.CountryCode);
        Assert.Equal("+46", PhoneCallingCode.Sweden.ToString());
    }

    [Fact]
    public void Constants_UnitedStates_HasCorrectValues()
    {
        Assert.Equal("1", PhoneCallingCode.UnitedStates.Value);
        Assert.Equal("US", PhoneCallingCode.UnitedStates.CountryCode);
    }

    [Fact]
    public void Constants_UnitedKingdom_HasCorrectValues()
    {
        Assert.Equal("44", PhoneCallingCode.UnitedKingdom.Value);
        Assert.Equal("GB", PhoneCallingCode.UnitedKingdom.CountryCode);
    }

    [Fact]
    public void Constants_Russia_HasCorrectValues()
    {
        Assert.Equal("7", PhoneCallingCode.Russia.Value);
        Assert.Equal("RU", PhoneCallingCode.Russia.CountryCode);
    }

    [Theory]
    [InlineData("46", "SE")]
    [InlineData("47", "NO")]
    [InlineData("358", "FI")]
    [InlineData("45", "DK")]
    [InlineData("49", "DE")]
    [InlineData("48", "PL")]
    [InlineData("372", "EE")]
    [InlineData("370", "LT")]
    [InlineData("40", "RO")]
    [InlineData("359", "BG")]
    [InlineData("371", "LV")]
    [InlineData("420", "CZ")]
    [InlineData("34", "ES")]
    [InlineData("31", "NL")]
    [InlineData("30", "GR")]
    [InlineData("39", "IT")]
    [InlineData("386", "SI")]
    [InlineData("385", "HR")]
    [InlineData("351", "PT")]
    [InlineData("36", "HU")]
    [InlineData("33", "FR")]
    [InlineData("421", "SK")]
    [InlineData("32", "BE")]
    [InlineData("44", "GB")]
    [InlineData("43", "AT")]
    [InlineData("357", "CY")]
    [InlineData("354", "IS")]
    [InlineData("41", "CH")]
    [InlineData("353", "IE")]
    [InlineData("352", "LU")]
    [InlineData("356", "MT")]
    [InlineData("423", "LI")]
    [InlineData("1", "US")]
    [InlineData("7", "RU")]
    public void TryParse_MapsCallingCodeToCountryCode(string code, string expectedCountryCode)
    {
        Assert.True(PhoneCallingCode.TryParse(code, out var result));
        Assert.Equal(expectedCountryCode, result!.CountryCode);
    }

    [Fact]
    public void All_ContainsAllKnownCodes()
    {
        Assert.True(PhoneCallingCode.All.Count >= 190);
        Assert.Contains(PhoneCallingCode.All, c => c.Value == "46");
        Assert.Contains(PhoneCallingCode.All, c => c.Value == "1");
        Assert.Contains(PhoneCallingCode.All, c => c.Value == "86");
    }

    [Fact]
    public void All_AllCodesAreUnique()
    {
        var values = PhoneCallingCode.All.Select(c => c.Value).ToList();
        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void TryParse_TrimsWhitespace()
    {
        Assert.True(PhoneCallingCode.TryParse(" 46 ", out var result));
        Assert.Equal("46", result!.Value);
    }

    [Theory]
    [InlineData("91", "IN")]
    [InlineData("81", "JP")]
    [InlineData("86", "CN")]
    [InlineData("61", "AU")]
    [InlineData("55", "BR")]
    public void TryParse_NonEuropeanCodes_AreRecognized(string code, string expectedCountryCode)
    {
        Assert.True(PhoneCallingCode.TryParse(code, out var result));
        Assert.Equal(expectedCountryCode, result!.CountryCode);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = PhoneCallingCode.Parse("46");
        var b = PhoneCallingCode.Parse("46");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = PhoneCallingCode.Parse("46");
        var b = PhoneCallingCode.Parse("44");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = PhoneCallingCode.Parse("46");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = PhoneCallingCode.Parse("1");
        var b = PhoneCallingCode.Parse("46");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = PhoneCallingCode.Parse("358");
        Assert.Equal(1, a.CompareTo(null));
    }
}
