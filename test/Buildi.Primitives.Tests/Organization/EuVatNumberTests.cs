using Buildi.Primitives.Organization;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Organization;

public class EuVatNumberTests
{
    [Theory]
    [InlineData("SE559246042101")] // Budi AB
    [InlineData("SE559323264701")] // Bidpal AB
    [InlineData("SE556984770901")] // Orneholm AB
    public void TryParse_WithValidSwedishNumbers_ShouldSucceed(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.True(result);
    }

    [Theory]
    [InlineData("ATU12345678")] // Austria
    [InlineData("BE0123456749")] // Belgium (valid: 97 - 1234567 mod 97 = 49)
    [InlineData("BG123456789")] // Bulgaria 9 chars
    [InlineData("BG1234567890")] // Bulgaria 10 chars
    [InlineData("HR33392005961")] // Croatia (valid MOD 11-10)
    [InlineData("CY12345678X")] // Cyprus
    [InlineData("CZ12345678")] // Czech Republic 8 chars
    [InlineData("CZ123456789")] // Czech Republic 9 chars
    [InlineData("CZ1234567890")] // Czech Republic 10 chars
    [InlineData("DK12345678")] // Denmark
    [InlineData("EE123456789")] // Estonia
    [InlineData("FI20774740")] // Finland (valid MOD 11-2 check digit)
    [InlineData("FR12345678901")] // France numeric
    [InlineData("FRX2345678901")] // France with letter key
    [InlineData("FRXX345678901")] // France with two-letter key
    [InlineData("DE123456789")] // Germany
    [InlineData("EL123456789")] // Greece
    [InlineData("HU12345678")] // Hungary
    [InlineData("IE1234567WA")] // Ireland company
    [InlineData("IE1234567FA")] // Ireland individual
    [InlineData("IT02118311006")] // Italy (valid Luhn)
    [InlineData("LV12345678901")] // Latvia
    [InlineData("LT123456789")] // Lithuania 9 chars
    [InlineData("LT123456789012")] // Lithuania 12 chars
    [InlineData("LU12345678")] // Luxembourg
    [InlineData("MT12345678")] // Malta
    [InlineData("NL123456789B01")] // Netherlands
    [InlineData("PL1000000006")] // Poland
    [InlineData("PT123456789")] // Portugal
    [InlineData("RO1234567890")] // Romania
    [InlineData("SK2021853504")] // Slovakia (divisible by 11)
    [InlineData("SI12345678")] // Slovenia
    [InlineData("ES12345678X")] // Spain with end letter
    [InlineData("ESX12345678")] // Spain with start letter
    [InlineData("ESX1234567X")] // Spain with both letters
    public void TryParse_WithValidEUFormats_ShouldSucceed(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.True(result);
    }

    [Theory]
    [InlineData("GB123456789")] // UK (non-EU)
    [InlineData("CHE123456789")] // Switzerland (non-EU)
    [InlineData("NO100000008MVA")] // Norway (non-EU)
    public void TryParse_WithValidNonEUFormats_ShouldSucceed(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TryParse_WithEmptyOrNull_ShouldReturnFalse(string? vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber!, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("SE556036123402")] // Invalid checksum
    [InlineData("SE556036123400")] // Invalid ending
    [InlineData("SE556036123411")] // Invalid ending
    public void TryParse_WithInvalidSwedishChecksum_ShouldReturnFalse(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("XX556036123401")] // Invalid country code
    [InlineData("ZZ556036123401")] // Non-existent country code
    [InlineData("QQ556036123401")] // Non-existent country code
    public void TryParse_WithInvalidCountryCodes_ShouldReturnFalse(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("SE55603612340")]   // Too short
    [InlineData("SE5560361234011")]  // Too long
    [InlineData("SEABCDEF123401")]   // Non-numeric
    public void TryParse_WithInvalidFormats_ShouldReturnFalse(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("NL123456789A01")] // Netherlands with wrong letter (not B)
    [InlineData("CY123456789")] // Cyprus without letter
    [InlineData("FR1234567890")] // France wrong length
    [InlineData("IE12345678")] // Ireland without letter
    [InlineData("IE123456789")] // Ireland nine digits only
    [InlineData("PL1000000000")] // Poland invalid checksum
    [InlineData("NO100000009MVA")] // Norway invalid checksum
    [InlineData("ES12345678")] // Spain without letter
    public void TryParse_WithInvalidCountrySpecificFormats_ShouldReturnFalse(string vatNumber)
    {
        var result = EuVatNumber.TryParse(vatNumber, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("EL123456789", "EL")]
    [InlineData("EL123456789", "GR")]
    public void TryParseForCountry_WithGreekVat_AcceptsBothElAndGr(string vatNumber, string countryCode)
    {
        var result = EuVatNumber.TryParseForCountry(vatNumber, countryCode, out var vat);

        Assert.True(result);
        Assert.NotNull(vat);
        Assert.Equal("GR", vat!.CountryCode);
        Assert.Equal("EL", vat.VatPrefix);
    }

    [Fact]
    public void TryParse_WithFinnishRemainderOneChecksumCandidate_ShouldReturnFalse()
    {
        var result = EuVatNumber.TryParse("FI10000080", out _);

        Assert.False(result);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = EuVatNumber.Parse("SE559246042101");
        var b = EuVatNumber.Parse("SE559246042101");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = EuVatNumber.Parse("SE559246042101");
        var b = EuVatNumber.Parse("SE556984770901");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = EuVatNumber.Parse("SE559246042101");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EuVatNumber.Parse("SE556984770901");
        var b = EuVatNumber.Parse("SE559246042101");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EuVatNumber.Parse("SE559246042101");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("AB", false, ValidationErrorReason.InputTooShort)]
    [InlineData("12345", false, ValidationErrorReason.InvalidCountryPrefix)]
    [InlineData("XX123456789", false, ValidationErrorReason.UnknownCountryCode)]
    [InlineData("SE1234", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("SE559246042101", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = EuVatNumber.Validate(input);

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
        var result = EuVatNumber.Validate("AB");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("SE559246042101")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("XX123456789")]
    [InlineData("SE1234")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(EuVatNumber.IsValid(input), EuVatNumber.Validate(input).IsValid);
    }
}
