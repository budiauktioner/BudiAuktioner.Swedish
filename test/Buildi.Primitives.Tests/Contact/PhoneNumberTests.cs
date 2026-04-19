using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Contact;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("0701740633", true)]
    [InlineData("070-174 06 33", true)]
    [InlineData("+46701740633", true)]
    [InlineData("0046701740633", true)]
    [InlineData("46701740633", true)]
    [InlineData("08-4650 04 10", true)]
    [InlineData("+44 20 7946 0958", true)]
    [InlineData("+1-555-123-4567", true)]
    [InlineData("0044207946 0958", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PhoneNumber.IsValid(input));
    }

    [Theory]
    [InlineData("0701740633", "0046701740633")]
    [InlineData("070-174 06 33", "0046701740633")]
    [InlineData("+46701740633", "0046701740633")]
    [InlineData("0046701740633", "0046701740633")]
    public void TryParse_SwedishInput_ReturnsNormalizedDigits(string input, string expectedDigits)
    {
        Assert.True(PhoneNumber.TryParse(input, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
        Assert.True(result.IsSwedish);
    }

    [Theory]
    [InlineData("+44 20 7946 0958", "00442079460958")]
    [InlineData("+1-555-123-4567", "0015551234567")]
    [InlineData("0044207946 0958", "00442079460958")]
    public void TryParse_InternationalInput_ReturnsNormalizedDigits(string input, string expectedDigits)
    {
        Assert.True(PhoneNumber.TryParse(input, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
        Assert.False(result.IsSwedish);
    }

    [Theory]
    [InlineData("0701740633", true)]
    [InlineData("0846500410", false)]
    [InlineData("0313900610", false)]
    [InlineData("+44 20 7946 0958", false)]
    public void IsMobile_ReturnsExpected(string input, bool expectedMobile)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expectedMobile, phone.IsMobile);
    }

    [Theory]
    [InlineData("0701740633", "+46701740633")]
    [InlineData("0846500410", "+46846500410")]
    [InlineData("+44 20 7946 0958", "+442079460958")]
    public void ToInternationalString_ReturnsExpected(string input, string expected)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expected, phone.ToInternationalString());
    }

    [Fact]
    public void ToLocalString_Swedish_ReturnsLocalFormat()
    {
        var phone = PhoneNumber.Parse("0701740633");
        Assert.StartsWith("0701-", phone.ToLocalString());
    }

    [Fact]
    public void ToLocalString_NonSwedish_ReturnsGenericLocalFormat()
    {
        var phone = PhoneNumber.Parse("+44 20 7946 0958");
        Assert.Equal("02079460958", phone.ToLocalString());
    }

    [Fact]
    public void Formatted_Swedish_UsesLocalFormat()
    {
        var phone = PhoneNumber.Parse("0701740633");
        Assert.Equal(phone.ToLocalString(), phone.Formatted);
    }

    [Fact]
    public void Formatted_NonSwedish_UsesInternationalFormat()
    {
        var phone = PhoneNumber.Parse("+44 20 7946 0958");
        Assert.Equal("+44 20 7946 0958", phone.Formatted);
    }

    [Theory]
    [InlineData("+442079460958", "+44 20 7946 0958")]
    [InlineData("004722123456", "+47 22 12 34 56")]
    [InlineData("+15551234567", "+1 555 123 4567")]
    public void Formatted_NonSwedish_AddsRelevantWhitespace_WhenInputHasNone(string input, string expected)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expected, phone.Formatted);
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Parse("invalid"));
    }

    [Fact]
    public void ToString_ReturnsSameAsFormatted()
    {
        var phone = PhoneNumber.Parse("0701740633");
        Assert.Equal(phone.Formatted, phone.ToString());
    }

    [Theory]
    [InlineData("0701740633", "46")]
    [InlineData("+44 20 7946 0958", "44")]
    [InlineData("+1-555-123-4567", "1")]
    [InlineData("+47 22 12 34 56", "47")]
    [InlineData("+49 30 12345678", "49")]
    public void CountryCallingCode_ReturnsExpected(string input, string expectedCode)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expectedCode, phone.CountryCallingCode.Value);
    }

    [Theory]
    [InlineData("0701740633", "SE")]
    [InlineData("+44 20 7946 0958", "GB")]
    [InlineData("+1-555-123-4567", "US")]
    [InlineData("+47 22 12 34 56", "NO")]
    [InlineData("+49 30 12345678", "DE")]
    [InlineData("+45 32 12 34 56", "DK")]
    [InlineData("+358 9 1234567", "FI")]
    public void Country_ReturnsExpectedCountry(string input, string expectedAlpha2)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.NotNull(phone.Country);
        Assert.Equal(expectedAlpha2, phone.Country!.Alpha2Code);
    }

    [Fact]
    public void Country_Swedish_ReturnsSwedenInstance()
    {
        var phone = PhoneNumber.Parse("0701740633");
        Assert.Same(Country.Sweden, phone.Country);
    }

    [Theory]
    [InlineData("021-123 45 67", "47", "0047211234567")]
    [InlineData("0701740633", "47", "0047701740633")]
    [InlineData("040 123 456", "45", "004540123456")]
    public void TryParse_WithDefaultCallingCode_UsesSpecifiedCountry(string input, string defaultCode, string expectedDigits)
    {
        Assert.True(PhoneNumber.TryParse(input, defaultCode, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
    }

    [Theory]
    [InlineData("021-123 45 67", "47", "NO")]
    [InlineData("0701740633", "47", "NO")]
    [InlineData("040 123 456", "45", "DK")]
    public void TryParse_WithDefaultCallingCode_SetsCountry(string input, string defaultCode, string expectedAlpha2)
    {
        Assert.True(PhoneNumber.TryParse(input, defaultCode, out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal(expectedAlpha2, result.Country!.Alpha2Code);
    }

    [Theory]
    [InlineData("+46701740633", "47", "SE")]
    [InlineData("0046701740633", "47", "SE")]
    public void TryParse_WithExplicitPrefix_IgnoresDefaultCallingCode(string input, string defaultCode, string expectedAlpha2)
    {
        Assert.True(PhoneNumber.TryParse(input, defaultCode, out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal(expectedAlpha2, result.Country!.Alpha2Code);
    }

    [Theory]
    [InlineData("0701740633", "46", "0046701740633")]
    [InlineData("0701740633", "47", "0047701740633")]
    public void Normalize_WithDefaultCallingCode_ReturnsExpected(string input, string defaultCode, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Normalize(input, defaultCode));
    }

    [Theory]
    [InlineData("0701740633", "0046701740633")]
    public void Normalize_WithoutDefaultCallingCode_DefaultsToSwedish(string input, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Normalize(input));
    }

    [Fact]
    public void IsValid_WithDefaultCallingCode_Works()
    {
        Assert.True(PhoneNumber.IsValid("021-123 45 67", "47"));
    }

    [Fact]
    public void Parse_WithDefaultCallingCode_Works()
    {
        var phone = PhoneNumber.Parse("021-123 45 67", "47");
        Assert.Equal("NO", phone.Country!.Alpha2Code);
    }

    [Fact]
    public void Parse_WithDefaultCallingCode_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Parse("invalid", "47"));
    }

    [Fact]
    public void Format_WithDefaultCallingCode_ReturnsFormatted()
    {
        var formatted = PhoneNumber.Format("021-123 45 67", "47");
        Assert.NotNull(formatted);
    }

    [Theory]
    [InlineData("+442079460958", "46", "+44 20 7946 0958")]
    [InlineData("004722123456", "46", "+47 22 12 34 56")]
    [InlineData("+15551234567", "46", "+1 555 123 4567")]
    public void Format_WithDefaultCallingCode_AddsRelevantWhitespace_ForDenseInternationalInput(string input, string defaultCode, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Format(input, defaultCode));
    }

    [Fact]
    public void Format_WithDefaultCallingCode_InvalidInput_ReturnsNull()
    {
        Assert.Null(PhoneNumber.Format("invalid", "47"));
    }

    [Fact]
    public void Format_WithDefaultCallingCode_FallbackToInput()
    {
        Assert.Equal("invalid", PhoneNumber.Format("invalid", "47", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("tel:+46701740633", "0046701740633")]
    [InlineData("tel:0701740633", "0046701740633")]
    [InlineData("TEL:+46701740633", "0046701740633")]
    [InlineData("sms:+46701740633", "0046701740633")]
    [InlineData("sms:0701740633", "0046701740633")]
    [InlineData("call:+46701740633", "0046701740633")]
    [InlineData("callto:+46701740633", "0046701740633")]
    [InlineData("tel:+44 20 7946 0958", "00442079460958")]
    [InlineData("tel: +46701740633", "0046701740633")]
    public void TryParse_StripsUriSchemePrefix(string input, string expectedDigits)
    {
        Assert.True(PhoneNumber.TryParse(input, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
    }

    [Theory]
    [InlineData("021-123 45 67", "NO", "0047211234567")]
    [InlineData("0701740633", "NO", "0047701740633")]
    [InlineData("040 123 456", "DK", "004540123456")]
    public void TryParse_WithDefaultCountry_UsesCountryCallingCode(string input, string alpha2, string expectedDigits)
    {
        var country = Country.Parse(alpha2);
        Assert.True(PhoneNumber.TryParse(input, country, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
    }

    [Theory]
    [InlineData("+46701740633", "NO", "SE")]
    [InlineData("0046701740633", "NO", "SE")]
    public void TryParse_WithDefaultCountry_ExplicitPrefixIgnoresDefault(string input, string defaultAlpha2, string expectedAlpha2)
    {
        var defaultCountry = Country.Parse(defaultAlpha2);
        Assert.True(PhoneNumber.TryParse(input, defaultCountry, out var result));
        Assert.Equal(expectedAlpha2, result!.Country!.Alpha2Code);
    }

    [Fact]
    public void Parse_WithDefaultCountry_Works()
    {
        var phone = PhoneNumber.Parse("021-123 45 67", Country.Norway);
        Assert.Equal("NO", phone.Country!.Alpha2Code);
    }

    [Fact]
    public void Parse_WithDefaultCountry_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Parse("invalid", Country.Norway));
    }

    [Fact]
    public void IsValid_WithDefaultCountry_Works()
    {
        Assert.True(PhoneNumber.IsValid("021-123 45 67", Country.Norway));
        Assert.False(PhoneNumber.IsValid("invalid", Country.Norway));
    }

    [Fact]
    public void Format_WithDefaultCountry_ReturnsLocalFormatForMatchingCountry()
    {
        var formatted = PhoneNumber.Format("021-123 45 67", Country.Norway);
        Assert.NotNull(formatted);
        Assert.DoesNotContain("+47", formatted);
    }

    [Fact]
    public void Format_WithDefaultCountry_ReturnsInternationalForNonMatchingCountry()
    {
        var formatted = PhoneNumber.Format("+46701740633", Country.Norway);
        Assert.NotNull(formatted);
        Assert.StartsWith("+46", formatted);
    }

    [Fact]
    public void Format_WithDefaultCallingCode_SkipsCodeWhenMatching()
    {
        var formatted = PhoneNumber.Format("0701740633", "46");
        Assert.NotNull(formatted);
        Assert.StartsWith("0701-", formatted);
    }

    [Fact]
    public void Format_WithDefaultCallingCode_IncludesCodeWhenNotMatching()
    {
        var formatted = PhoneNumber.Format("+46701740633", "47");
        Assert.NotNull(formatted);
        Assert.Equal("+46 70 174 06 33", formatted);
    }

    [Theory]
    [InlineData("0701740633", "NO", "0047701740633")]
    [InlineData("0701740633", "SE", "0046701740633")]
    public void Normalize_WithDefaultCountry_ReturnsExpected(string input, string alpha2, string expected)
    {
        var country = Country.Parse(alpha2);
        Assert.Equal(expected, PhoneNumber.Normalize(input, country));
    }

    [Fact]
    public void ToLocalString_WithMatchingCountry_ReturnsLocalFormat()
    {
        var phone = PhoneNumber.Parse("0701740633");
        var local = phone.ToLocalString(Country.Sweden);
        Assert.StartsWith("0701-", local);
    }

    [Fact]
    public void ToLocalString_WithNonMatchingCountry_ReturnsInternationalFormat()
    {
        var phone = PhoneNumber.Parse("0701740633");
        var result = phone.ToLocalString(Country.Norway);
        Assert.Equal("+46 70 174 06 33", result);
    }

    [Fact]
    public void ToLocalString_NorwegianNumber_WithNorwayDefault_ReturnsLocal()
    {
        var phone = PhoneNumber.Parse("021-123 45 67", Country.Norway);
        var local = phone.ToLocalString(Country.Norway);
        Assert.StartsWith("0", local);
        Assert.DoesNotContain("+", local);
    }

    [Fact]
    public void ToLocalString_NorwegianNumber_WithSwedenDefault_ReturnsInternational()
    {
        var phone = PhoneNumber.Parse("021-123 45 67", Country.Norway);
        var result = phone.ToLocalString(Country.Sweden);
        Assert.Equal("+47 21 123 45 67", result);
    }

    [Theory]
    [InlineData("0701740633", "46", "0046701740633")]
    [InlineData("021-123 45 67", "47", "0047211234567")]
    public void TryParse_WithPhoneCallingCode_UsesCallingCode(string input, string code, string expectedDigits)
    {
        var callingCode = PhoneCallingCode.Parse(code);
        Assert.True(PhoneNumber.TryParse(input, callingCode, out var result));
        Assert.Equal(expectedDigits, result!.Digits);
    }

    [Fact]
    public void Parse_WithPhoneCallingCode_Works()
    {
        var phone = PhoneNumber.Parse("0701740633", PhoneCallingCode.Sweden);
        Assert.Equal("0046701740633", phone.Digits);
    }

    [Fact]
    public void Parse_WithPhoneCallingCode_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Parse("invalid", PhoneCallingCode.Sweden));
    }

    [Fact]
    public void IsValid_WithPhoneCallingCode_Works()
    {
        Assert.True(PhoneNumber.IsValid("0701740633", PhoneCallingCode.Sweden));
        Assert.False(PhoneNumber.IsValid("invalid", PhoneCallingCode.Sweden));
    }

    [Fact]
    public void Format_WithPhoneCallingCode_MatchingCode_ReturnsLocalFormat()
    {
        var result = PhoneNumber.Format("0701740633", PhoneCallingCode.Sweden);
        Assert.StartsWith("0", result);
        Assert.DoesNotContain("+", result!);
    }

    [Fact]
    public void Format_WithPhoneCallingCode_NonMatchingCode_ReturnsInternationalFormat()
    {
        var result = PhoneNumber.Format("+46701740633", PhoneCallingCode.Norway);
        Assert.Equal("+46 70 174 06 33", result);
    }

    [Theory]
    [InlineData("0701740633", "46", "0046701740633")]
    [InlineData("021-123 45 67", "47", "0047211234567")]
    public void Normalize_WithPhoneCallingCode_ReturnsExpected(string input, string code, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Normalize(input, PhoneCallingCode.Parse(code)));
    }

    [Fact]
    public void ToLocalString_WithPhoneCallingCode_Matching_ReturnsLocal()
    {
        var phone = PhoneNumber.Parse("0701740633");
        var local = phone.ToLocalString(PhoneCallingCode.Sweden);
        Assert.StartsWith("0", local);
        Assert.DoesNotContain("+", local);
    }

    [Fact]
    public void ToLocalString_WithPhoneCallingCode_NonMatching_ReturnsInternational()
    {
        var phone = PhoneNumber.Parse("0701740633");
        var result = phone.ToLocalString(PhoneCallingCode.Norway);
        Assert.Equal("+46 70 174 06 33", result);
    }

    [Theory]
    [InlineData("+44 20 7946 0958", "+44 20 7946 0958")]
    [InlineData("+47 22 12 34 56", "+47 22 12 34 56")]
    [InlineData("+1-555-123-4567", "+1 555 123 4567")]
    public void ToString_NonSwedish_ReturnsSpacedInternationalDisplay(string input, string expected)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expected, phone.ToString());
    }

    [Theory]
    [InlineData("+442079460958", "+44 20 7946 0958")]
    [InlineData("004722123456", "+47 22 12 34 56")]
    [InlineData("+15551234567", "+1 555 123 4567")]
    public void ToString_NonSwedish_AddsRelevantWhitespace_WhenParsedWithoutIt(string input, string expected)
    {
        var phone = PhoneNumber.Parse(input);
        Assert.Equal(expected, phone.ToString());
    }

    [Fact]
    public void CountryCallingCode_IsPhoneCallingCode()
    {
        var phone = PhoneNumber.Parse("0701740633");
        Assert.IsType<PhoneCallingCode>(phone.CountryCallingCode);
        Assert.Equal("46", phone.CountryCallingCode.Value);
        Assert.Equal("+46", phone.CountryCallingCode.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = PhoneNumber.Parse("0701740633");
        var b = PhoneNumber.Parse("0701740633");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = PhoneNumber.Parse("0701740633");
        var b = PhoneNumber.Parse("+44 20 7946 0958");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = PhoneNumber.Parse("0701740633");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = PhoneNumber.Parse("+44 20 7946 0958");
        var b = PhoneNumber.Parse("0701740633");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = PhoneNumber.Parse("+1-555-123-4567");
        Assert.Equal(1, a.CompareTo(null));
    }
}
