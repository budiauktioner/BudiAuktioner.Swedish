using Buildi.Primitives.Contact;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use generic Stockholm addresses (Storgatan 12, 114 53 Stockholm).
/// </summary>
public class SwedishAddressTests
{
    // --- IsValid ---

    [Theory]
    [InlineData("Storgatan 1, 114 53 Stockholm")]
    [InlineData("Storgatan 1, 114 53, Stockholm")]
    [InlineData("Storgatan 1, 11453, Stockholm")]
    [InlineData("Storgatan 1, 114 53 Stockholm, SE")]
    [InlineData("Storgatan 1, 114 53 Stockholm, Sverige")]
    [InlineData("Box 123, 114 53 Stockholm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishAddress.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Storgatan 1")]
    [InlineData("Storgatan 1, Stockholm")]
    [InlineData("Storgatan 1, 114 53 Stockholm, DE")]
    [InlineData("Storgatan 1, 114 53 Stockholm, Norway")]
    [InlineData("Storgatan 1, DK-9000 Aalborg")]
    [InlineData("Hauptstraße 1, 10115 Berlin, DE")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishAddress.IsValid(input));
    }

    // --- TryParse (single string) ---

    [Fact]
    public void TryParse_SingleString_ExtractsComponents()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 12, 114 53 Stockholm", out var result));
        Assert.Equal("Storgatan", result!.Street.StreetName);
        Assert.Equal("12", result.Street.StreetNumber);
        Assert.Equal("11453", result.ZipCode.Value);
        Assert.Equal("114 53", result.ZipCode.Formatted);
        Assert.Equal("Stockholm", result.City.Value);
    }

    [Fact]
    public void TryParse_WithExplicitSweden_Succeeds()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 1, 114 53 Stockholm, SE", out var result));
        Assert.NotNull(result);
        Assert.Equal("SE", result!.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_WithNonSwedishCountry_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Storgatan 1, 114 53 Stockholm, DE", out _));
    }

    [Fact]
    public void TryParse_WithInternationalZipCode_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Baker Street 1, W1A 1AB London, GB", out _));
    }

    [Fact]
    public void TryParse_NoZipCode_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Storgatan 1, Stockholm", out _));
    }

    [Fact]
    public void TryParse_NoCity_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Storgatan 1, 11453", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_NullOrEmpty_ReturnsNull(string? input)
    {
        Assert.False(SwedishAddress.TryParse(input, out var result));
        Assert.Null(result);
    }

    // --- TryParse (separate components) ---

    [Fact]
    public void TryParse_Components_Succeeds()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 12", "114 53", "Stockholm", out var result));
        Assert.Equal("Storgatan", result!.Street.StreetName);
        Assert.Equal("11453", result.ZipCode.Value);
        Assert.Equal("Stockholm", result.City.Value);
    }

    [Fact]
    public void TryParse_Components_WithCountry_SE_Succeeds()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 1", "11453", "Stockholm", "SE", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_WithCountry_NonSE_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Storgatan 1", "11453", "Stockholm", "DE", out _));
    }

    [Fact]
    public void TryParse_Components_InternationalZip_Fails()
    {
        Assert.False(SwedishAddress.TryParse("Baker Street 1", "W1A 1AB", "London", out _));
    }

    // --- Parse ---

    [Fact]
    public void Parse_SingleString_ReturnsInstance()
    {
        var addr = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm");
        Assert.Equal("Storgatan 12", addr.Street.Street);
        Assert.Equal("114 53", addr.ZipCode.Formatted);
    }

    [Fact]
    public void Parse_Components_ReturnsInstance()
    {
        var addr = SwedishAddress.Parse("Storgatan 12", "114 53", "Stockholm");
        Assert.Equal("Storgatan 12", addr.Street.Street);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishAddress.Parse(input));
    }

    // --- Convenience properties ---

    [Fact]
    public void CareOf_Exposed()
    {
        Assert.True(SwedishAddress.TryParse("c/o Anna, Storgatan 1, 114 53 Stockholm", out var result));
        Assert.Equal("Anna", result!.CareOf);
    }

    [Fact]
    public void ApartmentNumber_Exposed()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 1 lgh 1201, 114 53 Stockholm", out var result));
        Assert.Equal("1201", result!.ApartmentNumber);
    }

    [Fact]
    public void PostBox_Exposed()
    {
        Assert.True(SwedishAddress.TryParse("Box 123, 114 53 Stockholm", out var result));
        Assert.True(result!.IsPostBox);
        Assert.Equal("123", result.PostBox);
    }

    // --- Format / Normalize ---

    [Theory]
    [InlineData("Storgatan 12, 114 53 Stockholm", "Storgatan 12, 114 53 Stockholm")]
    [InlineData(null, null)]
    [InlineData("invalid", null)]
    [InlineData("Storgatan 1, DK-9000 Aalborg", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishAddress.Format(input));
    }

    [Fact]
    public void Format_FallbackReturnsInput()
    {
        Assert.Equal("invalid", SwedishAddress.Format("invalid", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_ReturnsNormalized()
    {
        var normalized = SwedishAddress.Normalize("Storgatan 12, 114 53 Stockholm");
        Assert.NotNull(normalized);
        Assert.Contains("11453", normalized);
        Assert.Contains("SE", normalized);
    }

    [Fact]
    public void IsNormalized_ReturnsFalse_ForNonNormalized()
    {
        Assert.False(SwedishAddress.IsNormalized("Storgatan 12, 114 53 Stockholm"));
    }

    // --- ToString ---

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm");
        var str = addr.ToString();
        Assert.Equal("Storgatan 12, 114 53 Stockholm", str);
        Assert.DoesNotContain("Sverige", str);
        Assert.DoesNotContain("SE", str);
    }

    [Fact]
    public void ToString_WithCareOf()
    {
        var addr = SwedishAddress.Parse("c/o Anna, Storgatan 12, 114 53 Stockholm");
        Assert.Equal("c/o Anna, Storgatan 12, 114 53 Stockholm", addr.ToString());
    }

    [Fact]
    public void ToString_WithApartment()
    {
        var addr = SwedishAddress.Parse("Storgatan 12 lgh 1201, 114 53 Stockholm");
        Assert.Equal("Storgatan 12 lgh 1201, 114 53 Stockholm", addr.ToString());
    }

    // --- ToMultilineString ---

    [Fact]
    public void ToMultilineString_FormatsCorrectly()
    {
        var addr = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm");
        var lines = addr.ToMultilineString().Split(Environment.NewLine);
        Assert.Equal(2, lines.Length);
        Assert.Equal("Storgatan 12", lines[0]);
        Assert.Equal("114 53 Stockholm", lines[1]);
    }

    [Fact]
    public void ToMultilineString_WithCareOf()
    {
        var addr = SwedishAddress.Parse("c/o Anna, Storgatan 12, 114 53 Stockholm");
        var lines = addr.ToMultilineString().Split(Environment.NewLine);
        Assert.Equal(3, lines.Length);
        Assert.Equal("c/o Anna", lines[0]);
        Assert.Equal("Storgatan 12", lines[1]);
        Assert.Equal("114 53 Stockholm", lines[2]);
    }

    // --- Underlying Address ---

    [Fact]
    public void Address_ExposesUnderlyingType()
    {
        var swedish = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm");
        Assert.NotNull(swedish.Address);
        Assert.NotNull(swedish.Address.Country);
        Assert.Equal("SE", swedish.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void Address_AlwaysHasSwedenCountry()
    {
        Assert.True(SwedishAddress.TryParse("Storgatan 12, 114 53 Stockholm", out var result));
        Assert.NotNull(result!.Address.Country);
        Assert.Equal("SE", result.Address.Country!.Alpha2Code);
    }

    // --- Masking ---

    [Fact]
    public void ToMaskedString_DelegatesToUnderlying()
    {
        var addr = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm");
        var masked = addr.ToMaskedString();
        Assert.Contains("Storgatan", masked);
        Assert.Contains("**", masked);
        Assert.Contains("Stockholm", masked);
    }

    // --- Scanning ---

    [Fact]
    public void FindCandidatesInText_FindsSwedishAddress()
    {
        var results = SwedishAddress.FindCandidatesInText(
            "Kontoret ligger på Storgatan 12, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Equal("114 53", results[0].Value.ZipCode.Formatted);
        Assert.Equal("Stockholm", results[0].Value.City.Value);
    }

    [Fact]
    public void FindCandidatesInText_IgnoresNonSwedishAddress()
    {
        var results = SwedishAddress.FindCandidatesInText(
            "Visit us at Baker Street 221B, W1A 1AB London.");
        Assert.Empty(results);
    }

    [Fact]
    public void FindCandidatesInText_EmptyInput()
    {
        Assert.Empty(SwedishAddress.FindCandidatesInText(""));
    }

    [Fact]
    public void FindCandidatesInText_NullInput()
    {
        Assert.Empty(SwedishAddress.FindCandidatesInText(null!));
    }

    [Fact]
    public void FindCandidatesInText_HasMaskedForm()
    {
        var results = SwedishAddress.FindCandidatesInText(
            "Besök Storgatan 12, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Contains("Storgatan", results[0].MaskedForm);
        Assert.Contains("**", results[0].MaskedForm);
    }

    [Fact]
    public void FindCandidatesInText_TypeNameIsSwedishAddress()
    {
        var results = SwedishAddress.FindCandidatesInText(
            "Storgatan 12, 114 53 Stockholm");
        Assert.Single(results);
        Assert.Equal("SwedishAddress", results[0].TypeName);
    }

    // --- TextScanResult.SwedishAddresses ---

    [Fact]
    public void TextScanResult_SwedishAddresses_FilteredFromAddresses()
    {
        var scanner = new TextScanner();
        var result = scanner.Scan("Kontoret på Storgatan 12, 114 53 Stockholm");
        Assert.True(result.Addresses.Count >= 1);
        Assert.True(result.SwedishAddresses.Count >= 1);
        Assert.Equal("114 53", result.SwedishAddresses[0].Value.ZipCode.Formatted);
    }
}
