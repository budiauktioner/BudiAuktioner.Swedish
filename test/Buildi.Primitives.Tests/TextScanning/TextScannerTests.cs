using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.TextScanning;

public class TextScannerTests
{
    private readonly TextScanner _scanner = new();

    [Fact]
    public void Scan_EmptyString_ReturnsEmptyResult()
    {
        var result = _scanner.Scan("");
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void Scan_NullString_ReturnsEmptyResult()
    {
        var result = _scanner.Scan(null!);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void Scan_PlainText_ReturnsEmptyResult()
    {
        var result = _scanner.Scan("Det här är en vanlig text utan strukturerad data.");
        Assert.Empty(result.Emails);
        Assert.Empty(result.PersonalIdentityNumbers);
    }

    [Fact]
    public void Scan_FindsEmailInProse()
    {
        var result = _scanner.Scan("Kontakta oss på info@example.com för mer information.");
        Assert.Single(result.Emails);
        Assert.Equal("info@example.com", result.Emails[0].NormalizedForm);
        Assert.Equal(TextMatchConfidence.High, result.Emails[0].Confidence);
    }

    [Fact]
    public void Scan_FindsMultipleTypesInMixedText()
    {
        var text = "Maila info@example.com och org.nr 559246-0421 tack";
        var result = _scanner.Scan(text);

        Assert.True(result.Emails.Count >= 1);
        Assert.True(result.OrganizationNumbers.Count >= 1);
    }

    [Fact]
    public void Scan_FindsPersonalIdentityNumber()
    {
        var result = _scanner.Scan("Personnummer: 990807-2391");
        Assert.Single(result.PersonalIdentityNumbers);
        Assert.Equal(TextMatchConfidence.High, result.PersonalIdentityNumbers[0].Confidence);
    }

    [Fact]
    public void Scan_FindsIban()
    {
        var result = _scanner.Scan("IBAN: SE4550000000058398257466");
        Assert.Single(result.Ibans);
        Assert.Equal(TextMatchConfidence.High, result.Ibans[0].Confidence);
    }

    [Fact]
    public void Scan_FindsBankgiro()
    {
        var result = _scanner.Scan("Betala till bankgiro 5805-6201");
        Assert.Single(result.BankgiroNumbers);
    }

    [Fact]
    public void Scan_FindsVehicleRegistrationNumber()
    {
        var result = _scanner.Scan("Bilen har reg.nr ABC 123");
        Assert.Single(result.VehicleRegistrationNumbers);
    }

    [Fact]
    public void Scan_FindsAddress()
    {
        var result = _scanner.Scan("Besök oss på Storgatan 12, 114 53 Stockholm.");
        Assert.Single(result.Addresses);
        Assert.Equal(TextMatchConfidence.Medium, result.Addresses[0].Confidence);
    }

    [Fact]
    public void Scan_AddressContainsZipCode_ResolvedPreferAddress()
    {
        var text = "Storgatan 12, 114 53 Stockholm";
        var result = _scanner.Scan(text);

        var addressInResolved = result.ResolvedCandidates.Any(c => c.TypeName == nameof(Address));
        Assert.True(addressInResolved, "Full address should appear in resolved candidates");

        var zipInsideAddress = result.ResolvedCandidates
            .Where(c => c.TypeName == nameof(AddressZipCode))
            .Any(z =>
            {
                var addr = result.ResolvedCandidates.FirstOrDefault(a => a.TypeName == nameof(Address));
                return addr != null && addr.Contains(z);
            });
        Assert.False(zipInsideAddress, "Zip code contained within address should be removed from resolved");
    }

    [Fact]
    public void Scan_OptionsIncludeCategories_FiltersCorrectly()
    {
        var text = "Email: test@example.com PIN: 990807-2391";
        var options = new TextScannerOptions
        {
            IncludeCategories = new HashSet<TextCandidateCategory> { TextCandidateCategory.Contact }
        };
        var result = _scanner.Scan(text, options);

        Assert.True(result.Emails.Count >= 1);
        Assert.Empty(result.PersonalIdentityNumbers);
    }

    [Fact]
    public void Scan_OptionsExcludeCategories_FiltersCorrectly()
    {
        var text = "Email: test@example.com PIN: 990807-2391";
        var options = new TextScannerOptions
        {
            ExcludeCategories = new HashSet<TextCandidateCategory> { TextCandidateCategory.PersonalIdentifier }
        };
        var result = _scanner.Scan(text, options);

        Assert.True(result.Emails.Count >= 1);
        Assert.Empty(result.PersonalIdentityNumbers);
    }

    [Fact]
    public void Scan_OptionsMinimumConfidence_FiltersLowConfidence()
    {
        var text = "Postnummer: 114 53";
        var optionsLow = new TextScannerOptions { MinimumConfidence = TextMatchConfidence.Low };
        var optionsHigh = new TextScannerOptions { MinimumConfidence = TextMatchConfidence.High };

        var lowResult = _scanner.Scan(text, optionsLow);
        var highResult = _scanner.Scan(text, optionsHigh);

        Assert.True(lowResult.ZipCodes.Count >= 1);
        Assert.Empty(highResult.ZipCodes);
    }

    [Fact]
    public void Scan_FindsCountry()
    {
        var result = _scanner.Scan("Vi bor i Sverige.");
        Assert.Single(result.Countries);
        Assert.Equal("SE", result.Countries[0].NormalizedForm);
    }

    [Fact]
    public void Scan_CountryIncludedInAll()
    {
        var result = _scanner.Scan("Flytta till Finland.");
        Assert.Contains(result.All, c => c.TypeName == nameof(Country));
    }

    [Fact]
    public void Scan_FindsUrl()
    {
        var result = _scanner.Scan("Besök https://www.example.com för mer info.");
        Assert.Single(result.Urls);
        Assert.Equal("www.example.com", result.Urls[0].Value.Host);
    }
}
