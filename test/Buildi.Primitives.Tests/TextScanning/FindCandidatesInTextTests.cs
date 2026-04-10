using System.Globalization;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;
using Buildi.Primitives.Product;
using Buildi.Primitives.Property;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.TextScanning;

[Collection("CultureSensitive")]
public class FindCandidatesInTextTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    // --- EmailAddress ---

    [Fact]
    public void EmailAddress_FindsInProse()
    {
        var results = EmailAddress.FindCandidatesInText("Maila oss: info@example.com tack!");
        Assert.Single(results);
        Assert.Equal("info@example.com", results[0].NormalizedForm);
        Assert.Equal(11, results[0].StartIndex);
        Assert.Equal(16, results[0].Length);
    }

    [Fact]
    public void EmailAddress_FindsMultiple()
    {
        var results = EmailAddress.FindCandidatesInText("a@b.com och c@d.com");
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void EmailAddress_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(EmailAddress.FindCandidatesInText(""));
        Assert.Empty(EmailAddress.FindCandidatesInText(null!));
    }

    [Fact]
    public void EmailAddress_NoFalsePositiveOnPlainText()
    {
        Assert.Empty(EmailAddress.FindCandidatesInText("Det här är vanlig text."));
    }

    // --- SwedishPersonalIdentityNumber ---

    [Fact]
    public void PIN_FindsWithDash()
    {
        var results = SwedishPersonalIdentityNumber.FindCandidatesInText("Person: 990807-2391 bor här.");
        Assert.Single(results);
        Assert.Equal("990807-2391", results[0].OriginalText);
        Assert.Equal(TextMatchConfidence.High, results[0].Confidence);
    }

    [Fact]
    public void PIN_FindsWithPlus()
    {
        var results = SwedishPersonalIdentityNumber.FindCandidatesInText("Nummer: 990807+2391");
        Assert.Single(results);
    }

    [Fact]
    public void PIN_DoesNotMatchWithoutSeparator()
    {
        var results = SwedishPersonalIdentityNumber.FindCandidatesInText("Bara siffror: 199908072391.");
        Assert.Empty(results);
    }

    // --- SwedishCoordinationNumber ---

    [Fact]
    public void CoordinationNumber_FindsInText()
    {
        var results = SwedishCoordinationNumber.FindCandidatesInText("Samordningsnummer: 680164-2395");
        Assert.Single(results);
    }

    // --- SwedishOrganizationNumber ---

    [Fact]
    public void OrgNumber_FindsInText()
    {
        var results = SwedishOrganizationNumber.FindCandidatesInText("Orgnr: 559246-0421");
        Assert.Single(results);
        Assert.Equal("559246-0421", results[0].FormattedForm);
    }

    [Fact]
    public void OrgNumber_FindsWithoutDash()
    {
        var results = SwedishOrganizationNumber.FindCandidatesInText("Orgnr: 5592460421");
        Assert.Single(results);
    }

    [Fact]
    public void OrgNumber_DoesNotReturnPersonBasedNumbers()
    {
        var results = SwedishOrganizationNumber.FindCandidatesInText("Person: 990807-2391");
        Assert.Empty(results);
    }

    // --- IBAN ---

    [Fact]
    public void Iban_FindsInText()
    {
        var results = Iban.FindCandidatesInText("IBAN: SE4550000000058398257466");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.High, results[0].Confidence);
    }

    [Fact]
    public void Iban_FindsWithSpaces()
    {
        var results = Iban.FindCandidatesInText("Konto: SE45 5000 0000 0583 9825 7466 tack");
        Assert.Single(results);
    }

    // --- EuVatNumber ---

    [Fact]
    public void EuVatNumber_FindsInText()
    {
        var results = EuVatNumber.FindCandidatesInText("Momsnr: SE559246042101");
        Assert.Single(results);
    }

    // --- LeiCode ---

    [Fact]
    public void LeiCode_FindsInText()
    {
        var results = LeiCode.FindCandidatesInText("LEI: 5493001KJTIIGC8Y1R12 registrerat.");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.High, results[0].Confidence);
    }

    // --- SwedishBankgiroNumber ---

    [Fact]
    public void Bankgiro_FindsHyphenated()
    {
        var results = SwedishBankgiroNumber.FindCandidatesInText("Bankgiro: 5805-6201");
        Assert.Single(results);
    }

    [Fact]
    public void Bankgiro_DoesNotMatchWithoutHyphen()
    {
        var results = SwedishBankgiroNumber.FindCandidatesInText("Nummer: 58056201");
        Assert.Empty(results);
    }

    // --- SwedishPostgiroNumber ---

    [Fact]
    public void Postgiro_FindsHyphenated()
    {
        var results = SwedishPostgiroNumber.FindCandidatesInText("Plusgiro: 4779202-3");
        Assert.Single(results);
    }

    // --- SwedishVehicleRegistrationNumber ---

    [Fact]
    public void VehicleReg_FindsInText()
    {
        var results = SwedishVehicleRegistrationNumber.FindCandidatesInText("Bilen ABC 123 parkerad.");
        Assert.Single(results);
    }

    [Fact]
    public void VehicleReg_FindsLowerCase()
    {
        var results = SwedishVehicleRegistrationNumber.FindCandidatesInText("Bilen abc 123 parkerad.");
        Assert.Single(results);
    }

    // --- Gtin13 ---

    [Fact]
    public void Gtin13_FindsInText()
    {
        var results = Gtin13.FindCandidatesInText("Streckkod: 5901234123457");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.High, results[0].Confidence);
    }

    [Fact]
    public void Gtin13_DoesNotMatchInvalidCheckDigit()
    {
        var results = Gtin13.FindCandidatesInText("Streckkod: 5901234123458");
        Assert.Empty(results);
    }

    // --- Gtin8 ---

    [Fact]
    public void Gtin8_FindsInText()
    {
        var results = Gtin8.FindCandidatesInText("EAN-8: 96385074");
        Assert.Single(results);
    }

    // --- PhoneNumber ---

    [Fact]
    public void PhoneNumber_FindsSwedishMobile()
    {
        var results = PhoneNumber.FindCandidatesInText("Ring: 070-174 06 33");
        Assert.Single(results);
    }

    [Fact]
    public void PhoneNumber_FindsInternational()
    {
        var results = PhoneNumber.FindCandidatesInText("Telefon: +46 70 174 06 33");
        Assert.Single(results);
    }

    // --- AddressZipCode ---

    [Fact]
    public void ZipCode_FindsSwedish5Digit()
    {
        var results = AddressZipCode.FindCandidatesInText("Adress: 114 53 Stockholm");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
    }

    // --- Bic ---

    [Fact]
    public void Bic_FindsInText()
    {
        var results = Bic.FindCandidatesInText("BIC/SWIFT: NDEASESS");
        Assert.Single(results);
    }

    // --- VehicleIdentificationNumber ---

    [Fact]
    public void Vin_FindsInText()
    {
        var results = VehicleIdentificationNumber.FindCandidatesInText("VIN: WBA3A5C55CF256789");
        Assert.Single(results);
    }

    // --- SwedishPropertyDesignation ---

    [Fact]
    public void PropertyDesignation_FindsInText()
    {
        var results = SwedishPropertyDesignation.FindCandidatesInText("Fastighet: Stockholm Söder 75:2");
        Assert.Single(results);
        Assert.Equal("Stockholm Söder 75:2", results[0].OriginalText);
        Assert.Equal(TextMatchConfidence.Medium, results[0].Confidence);
    }

    [Fact]
    public void PropertyDesignation_LimitsNameToTwoWords()
    {
        var results = SwedishPropertyDesignation.FindCandidatesInText(
            "The property Stockholm Söder 75:2 was recently listed.");
        Assert.Single(results);
        Assert.Equal("Stockholm Söder 75:2", results[0].OriginalText);
    }

    [Theory]
    [InlineData("Söder 75:2", "Söder 75:2")]
    [InlineData("Norra Ängby 1:1", "Norra Ängby 1:1")]
    [InlineData("Backa 100:42", "Backa 100:42")]
    public void PropertyDesignation_MatchesOneOrTwoWordNames(string text, string expected)
    {
        var results = SwedishPropertyDesignation.FindCandidatesInText(text);
        Assert.Single(results);
        Assert.Equal(expected, results[0].OriginalText);
    }

    [Fact]
    public void PropertyDesignation_RequiresUppercaseStart()
    {
        Assert.Empty(SwedishPropertyDesignation.FindCandidatesInText("the söder 75:2"));
    }

    // --- DunsNumber ---

    [Fact]
    public void DunsNumber_FindsInText()
    {
        var results = DunsNumber.FindCandidatesInText("DUNS: 123456789");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
    }

    // --- Address ---

    [Fact]
    public void Address_FindsStreetWithZipCity()
    {
        var results = Address.FindCandidatesInText("Besök oss på Storgatan 12, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Medium, results[0].Confidence);
        Assert.Equal(TextCandidateCategory.Contact, results[0].Category);
    }

    [Fact]
    public void Address_FindsWithHouseNumberLetter()
    {
        var results = Address.FindCandidatesInText("Adress: Kungsgatan 44B, 111 35 Stockholm");
        Assert.Single(results);
    }

    [Fact]
    public void Address_FindsBoxAddress()
    {
        var results = Address.FindCandidatesInText("Skicka till Box 123, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.True(results[0].Value.IsPostBox);
    }

    [Fact]
    public void Address_FindsWithApartment()
    {
        var results = Address.FindCandidatesInText("Bor på Storgatan 12 lgh 1201, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Equal("1201", results[0].Value.ApartmentNumber);
    }

    [Fact]
    public void Address_FindsWithCareOf()
    {
        var results = Address.FindCandidatesInText("c/o Svensson, Storgatan 12, 114 53 Stockholm");
        Assert.Single(results);
        Assert.NotNull(results[0].Value.CareOf);
    }

    [Fact]
    public void Address_FindsWithCountry()
    {
        var results = Address.FindCandidatesInText("Adress: Kungsgatan 44, 111 35 Stockholm, Sverige");
        Assert.Single(results);
        Assert.NotNull(results[0].Value.Country);
    }

    [Fact]
    public void Address_FindsMultipleSuffixes()
    {
        Assert.Single(Address.FindCandidatesInText("Birger Jarlsgatan 10, 114 34 Stockholm"));
        Assert.Single(Address.FindCandidatesInText("Ringvägen 100, 118 60 Stockholm"));
        Assert.Single(Address.FindCandidatesInText("Brunnsgränd 4, 111 30 Stockholm"));
    }

    [Fact]
    public void Address_PositionsAreCorrect()
    {
        var text = "Prefix Storgatan 12, 114 53 Stockholm suffix";
        var results = Address.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(text.Substring(c.StartIndex, c.Length), c.OriginalText);
    }

    [Fact]
    public void Address_NoMatchOnPlainText()
    {
        Assert.Empty(Address.FindCandidatesInText("Det här är vanlig text utan adresser."));
    }

    [Fact]
    public void Address_HasMaskedForm()
    {
        var results = Address.FindCandidatesInText("Storgatan 12, 114 53 Stockholm");
        Assert.Single(results);
        var masked = results[0].MaskedForm;
        Assert.Contains("Storgatan", masked);
        Assert.DoesNotContain("12", masked);
        Assert.Contains("Stockholm", masked);
    }

    [Fact]
    public void Address_FindsSpecialCharsInStreetName()
    {
        var results = Address.FindCandidatesInText("Vi bor på Karl XII:s gata 5, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Equal("5", results[0].Value.Street.StreetNumber);
        Assert.Contains("gata", results[0].Value.Street.StreetName!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("114 53", results[0].Value.ZipCode!.Formatted);
    }

    [Fact]
    public void Address_FindsSpecialCharsInStreetName_WhenAfterColon()
    {
        var results = Address.FindCandidatesInText("Adress: Karl XII:s gata 5, 114 53 Stockholm.");
        Assert.Single(results);
        Assert.Equal("Karl XII:s gata", results[0].Value.Street.StreetName);
        Assert.Equal("5", results[0].Value.Street.StreetNumber);
    }

    [Fact]
    public void Address_FindsMultiWordProperName()
    {
        var results = Address.FindCandidatesInText("Kontor: Nils Ericsons gata 14, 411 03 Göteborg");
        Assert.Single(results);
        Assert.Equal("Nils Ericsons gata", results[0].Value.Street.StreetName);
    }

    [Fact]
    public void Address_SuffixFallback_FindsWithoutZipCode()
    {
        var results = Address.FindCandidatesInText("Vi flyttade till Storgatan 12 förra året.");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
        Assert.Equal("12", results[0].Value.Street.StreetNumber);
    }

    [Fact]
    public void Address_SuffixFallback_NotAddedWhenCoveredByZipAnchor()
    {
        var results = Address.FindCandidatesInText("Storgatan 12, 114 53 Stockholm");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Medium, results[0].Confidence);
    }

    [Fact]
    public void Address_FindsTwoAddresses()
    {
        var text = "Kontor: Kungsgatan 44, 111 35 Stockholm. Lager: Ringvägen 100, 118 60 Stockholm.";
        var results = Address.FindCandidatesInText(text);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Address_ZipAnchorPrefersBestParse()
    {
        var results = Address.FindCandidatesInText("Skicka fakturan till Box 456, 411 03 Göteborg tack!");
        Assert.Single(results);
        Assert.True(results[0].Value.IsPostBox);
        Assert.Equal("456", results[0].Value.PostBox);
    }

    // --- SwedishBankAccount ---

    [Fact]
    public void BankAccount_FindsInText()
    {
        var results = SwedishBankAccount.FindCandidatesInText("Konto: 5001-0123456");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
    }

    [Fact]
    public void BankAccount_FindsContinuousDigits()
    {
        var results = SwedishBankAccount.FindCandidatesInText("Kontonummer 50010123456 hos banken");
        Assert.Single(results);
    }

    // --- SwedishOcrReferenceNumber ---

    [Fact]
    public void OcrReference_FindsInText()
    {
        var results = SwedishOcrReferenceNumber.FindCandidatesInText("OCR: 12345682");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
    }

    // --- Country ---

    [Fact]
    public void Country_FindsLocalizedName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = Country.FindCandidatesInText("Vi reste till Tyskland förra sommaren.");
        Assert.Single(results);
        Assert.Equal("DE", results[0].NormalizedForm);
        Assert.Equal("Tyskland", results[0].FormattedForm);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
        Assert.Equal(TextCandidateCategory.Geography, results[0].Category);
    }

    [Fact]
    public void Country_FindsEnglishName()
    {
        var results = Country.FindCandidatesInText("We moved to Sweden last year.");
        Assert.Single(results);
        Assert.Equal("SE", results[0].NormalizedForm);
    }

    [Fact]
    public void Country_FindsNativeName()
    {
        var results = Country.FindCandidatesInText("Willkommen in Deutschland!");
        Assert.Single(results);
        Assert.Equal("DE", results[0].NormalizedForm);
    }

    [Fact]
    public void Country_FindsAlias()
    {
        var results = Country.FindCandidatesInText("Vi åkte till Holland på semester.");
        Assert.Single(results);
        Assert.Equal("NL", results[0].NormalizedForm);
    }

    [Fact]
    public void Country_FindsMultiple()
    {
        var results = Country.FindCandidatesInText("Flög från Sverige till Norge och sedan Danmark.");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Country_CaseInsensitive()
    {
        var results = Country.FindCandidatesInText("Jag bor i SVERIGE.");
        Assert.Single(results);
        Assert.Equal("SE", results[0].NormalizedForm);
    }

    [Fact]
    public void Country_PositionsAreCorrect()
    {
        var text = "Prefix Sverige suffix";
        var results = Country.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(7, c.StartIndex);
        Assert.Equal(7, c.Length);
        Assert.Equal(14, c.EndIndex);
        Assert.Equal("Sverige", c.OriginalText);
    }

    [Fact]
    public void Country_NoMatchOnPlainText()
    {
        Assert.Empty(Country.FindCandidatesInText("Det här är vanlig text."));
    }

    [Fact]
    public void Country_DoesNotMatchShortCodes()
    {
        Assert.Empty(Country.FindCandidatesInText("Skicka SE till 1234."));
    }

    [Fact]
    public void Country_WordBoundaryRequired()
    {
        Assert.Empty(Country.FindCandidatesInText("Frankrikesson är ett efternamn."));
    }

    // --- SwedishMunicipality ---

    [Fact]
    public void Municipality_FindsNameInProse()
    {
        var results = SwedishMunicipality.FindCandidatesInText("Vi besökte Linköping förra veckan.");
        Assert.Single(results);
        Assert.Equal("0580", results[0].NormalizedForm);
        Assert.Equal("Linköping", results[0].FormattedForm);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
        Assert.Equal(TextCandidateCategory.Geography, results[0].Category);
    }

    [Fact]
    public void Municipality_FindsMultiple()
    {
        var results = SwedishMunicipality.FindCandidatesInText("Resan gick via Göteborg till Malmö och sedan Uppsala.");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Municipality_SkipsShortNames()
    {
        Assert.Empty(SwedishMunicipality.FindCandidatesInText("Vi åkte till Ale och Kil."));
    }

    [Fact]
    public void Municipality_PositionsAreCorrect()
    {
        var text = "Prefix Stockholm suffix";
        var results = SwedishMunicipality.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(7, c.StartIndex);
        Assert.Equal(9, c.Length);
        Assert.Equal("Stockholm", c.OriginalText);
    }

    [Fact]
    public void Municipality_FindsGenitiveForm()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = SwedishMunicipality.FindCandidatesInText("Göteborgs universitet är ett stort lärosäte.");
        Assert.Single(results);
        Assert.Equal("Göteborg", results[0].FormattedForm);
        Assert.Equal("1480", results[0].NormalizedForm);
        Assert.Equal("Göteborgs", results[0].OriginalText);
    }

    [Fact]
    public void Municipality_FindsMultipleGenitives()
    {
        var results = SwedishMunicipality.FindCandidatesInText("Stockholms stad och Malmös hamn.");
        Assert.Equal(2, results.Count);
        Assert.Equal("Stockholm", results[0].FormattedForm);
        Assert.Equal("Malmö", results[1].FormattedForm);
    }

    [Fact]
    public void Municipality_FindsEnglishName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = SwedishMunicipality.FindCandidatesInText("She visited Gothenburg last summer.");
        Assert.Single(results);
        Assert.Equal("1480", results[0].NormalizedForm);
        Assert.Equal("Göteborg", results[0].FormattedForm);
    }

    [Fact]
    public void Municipality_FindsLowerCaseName()
    {
        var results = SwedishMunicipality.FindCandidatesInText("vi besökte stockholm förra veckan.");
        Assert.Single(results);
        Assert.Equal("0180", results[0].NormalizedForm);
        Assert.Equal("stockholm", results[0].OriginalText);
    }

    [Fact]
    public void Municipality_NoMatchOnPlainText()
    {
        Assert.Empty(SwedishMunicipality.FindCandidatesInText("Det här är vanlig text."));
    }

    [Fact]
    public void Municipality_WordBoundaryRequired()
    {
        Assert.Empty(SwedishMunicipality.FindCandidatesInText("Stockholmskusten är vacker."));
    }

    [Fact]
    public void Municipality_GenitiveWordBoundaryRequired()
    {
        Assert.Empty(SwedishMunicipality.FindCandidatesInText("Stockholmshem säljer lägenheter."));
    }

    // --- SwedishCounty ---

    [Fact]
    public void County_FindsLocalizedName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = SwedishCounty.FindCandidatesInText("Han bor i Stockholms län sedan länge.");
        Assert.Single(results);
        Assert.Equal("01", results[0].NormalizedForm);
        Assert.Equal("Stockholms län", results[0].FormattedForm);
        Assert.Equal(TextMatchConfidence.Low, results[0].Confidence);
        Assert.Equal(TextCandidateCategory.Geography, results[0].Category);
    }

    [Fact]
    public void County_FindsEnglishName()
    {
        var results = SwedishCounty.FindCandidatesInText("She lives in Stockholm County.");
        Assert.Single(results);
        Assert.Equal("01", results[0].NormalizedForm);
    }

    [Fact]
    public void County_FindsMultiple()
    {
        var results = SwedishCounty.FindCandidatesInText("Stockholms län och Skåne län har flest invånare.");
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void County_PositionsAreCorrect()
    {
        var text = "Prefix Skåne län suffix";
        var results = SwedishCounty.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(7, c.StartIndex);
        Assert.Equal("Skåne län".Length, c.Length);
        Assert.Equal("Skåne län", c.OriginalText);
    }

    [Fact]
    public void County_FindsBaseName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = SwedishCounty.FindCandidatesInText("Skåne har många invånare.");
        Assert.Single(results);
        Assert.Equal("12", results[0].NormalizedForm);
        Assert.Equal("Skåne län", results[0].FormattedForm);
        Assert.Equal("Skåne", results[0].OriginalText);
    }

    [Fact]
    public void County_FindsGenitiveBaseName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var results = SwedishCounty.FindCandidatesInText("Blekinges skärgård är vacker.");
        Assert.Single(results);
        Assert.Equal("10", results[0].NormalizedForm);
        Assert.Equal("Blekinge län", results[0].FormattedForm);
        Assert.Equal("Blekinges", results[0].OriginalText);
    }

    [Fact]
    public void County_FindsLowerCaseName()
    {
        var results = SwedishCounty.FindCandidatesInText("han bor i stockholms län sedan länge.");
        Assert.Single(results);
        Assert.Equal("01", results[0].NormalizedForm);
        Assert.Equal("stockholms län", results[0].OriginalText);
    }

    [Fact]
    public void County_NoMatchOnPlainText()
    {
        Assert.Empty(SwedishCounty.FindCandidatesInText("Det här är vanlig text."));
    }

    // --- Edge cases ---

    [Fact]
    public void FindCandidatesInText_NullInput_ReturnsEmpty_AllTypes()
    {
        Assert.Empty(EmailAddress.FindCandidatesInText(null!));
        Assert.Empty(SwedishPersonalIdentityNumber.FindCandidatesInText(null!));
        Assert.Empty(SwedishCoordinationNumber.FindCandidatesInText(null!));
        Assert.Empty(SwedishOrganizationNumber.FindCandidatesInText(null!));
        Assert.Empty(Iban.FindCandidatesInText(null!));
        Assert.Empty(EuVatNumber.FindCandidatesInText(null!));
        Assert.Empty(LeiCode.FindCandidatesInText(null!));
        Assert.Empty(SwedishBankgiroNumber.FindCandidatesInText(null!));
        Assert.Empty(SwedishPostgiroNumber.FindCandidatesInText(null!));
        Assert.Empty(SwedishVehicleRegistrationNumber.FindCandidatesInText(null!));
        Assert.Empty(VehicleIdentificationNumber.FindCandidatesInText(null!));
        Assert.Empty(Gtin13.FindCandidatesInText(null!));
        Assert.Empty(Gtin8.FindCandidatesInText(null!));
        Assert.Empty(PhoneNumber.FindCandidatesInText(null!));
        Assert.Empty(AddressZipCode.FindCandidatesInText(null!));
        Assert.Empty(Bic.FindCandidatesInText(null!));
        Assert.Empty(SwedishPropertyDesignation.FindCandidatesInText(null!));
        Assert.Empty(DunsNumber.FindCandidatesInText(null!));
        Assert.Empty(SwedishBankAccount.FindCandidatesInText(null!));
        Assert.Empty(SwedishOcrReferenceNumber.FindCandidatesInText(null!));
        Assert.Empty(Address.FindCandidatesInText(null!));
        Assert.Empty(Country.FindCandidatesInText(null!));
        Assert.Empty(SwedishMunicipality.FindCandidatesInText(null!));
        Assert.Empty(SwedishCounty.FindCandidatesInText(null!));
        Assert.Empty(Url.FindCandidatesInText(null!));
        Assert.Empty(ScreenSize.FindCandidatesInText(null!));
    }

    [Fact]
    public void FindCandidatesInText_EmptyInput_ReturnsEmpty_AllTypes()
    {
        Assert.Empty(EmailAddress.FindCandidatesInText(""));
        Assert.Empty(SwedishPersonalIdentityNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishCoordinationNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishOrganizationNumber.FindCandidatesInText(""));
        Assert.Empty(Iban.FindCandidatesInText(""));
        Assert.Empty(EuVatNumber.FindCandidatesInText(""));
        Assert.Empty(LeiCode.FindCandidatesInText(""));
        Assert.Empty(SwedishBankgiroNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishPostgiroNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishVehicleRegistrationNumber.FindCandidatesInText(""));
        Assert.Empty(VehicleIdentificationNumber.FindCandidatesInText(""));
        Assert.Empty(Gtin13.FindCandidatesInText(""));
        Assert.Empty(Gtin8.FindCandidatesInText(""));
        Assert.Empty(PhoneNumber.FindCandidatesInText(""));
        Assert.Empty(AddressZipCode.FindCandidatesInText(""));
        Assert.Empty(Bic.FindCandidatesInText(""));
        Assert.Empty(SwedishPropertyDesignation.FindCandidatesInText(""));
        Assert.Empty(DunsNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishBankAccount.FindCandidatesInText(""));
        Assert.Empty(SwedishOcrReferenceNumber.FindCandidatesInText(""));
        Assert.Empty(Address.FindCandidatesInText(""));
        Assert.Empty(Country.FindCandidatesInText(""));
        Assert.Empty(SwedishMunicipality.FindCandidatesInText(""));
        Assert.Empty(SwedishCounty.FindCandidatesInText(""));
        Assert.Empty(Url.FindCandidatesInText(""));
        Assert.Empty(ScreenSize.FindCandidatesInText(""));
    }

    [Fact]
    public void ScreenSize_FindsInchSymbolInProse()
    {
        var results = ScreenSize.FindCandidatesInText("The 15.6\" display is great.");
        Assert.Single(results);
        Assert.Equal("15.6 in", results[0].NormalizedForm);
        Assert.Equal(TextCandidateCategory.Product, results[0].Category);
    }

    // --- Whitespace position ---

    [Fact]
    public void MeasurementScan_DoesNotIncludeLeadingWhitespace()
    {
        var text = "resolution, 512 GB SSD";
        var results = DataSize.FindCandidatesInText(text);
        Assert.Single(results);
        Assert.Equal("512 GB", results[0].OriginalText);
        Assert.Equal(text.IndexOf('5'), results[0].StartIndex);
    }

    [Fact]
    public void Candidates_HaveCorrectPositions()
    {
        var text = "Prefix info@example.com suffix";
        var results = EmailAddress.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(text.Substring(c.StartIndex, c.Length), c.OriginalText);
    }

    [Fact]
    public void Candidates_HaveCorrectCategory()
    {
        Assert.Equal(TextCandidateCategory.Contact, EmailAddress.FindCandidatesInText("a@b.com")[0].Category);
        Assert.Equal(TextCandidateCategory.PersonalIdentifier, SwedishPersonalIdentityNumber.FindCandidatesInText("990807-2391")[0].Category);
        Assert.Equal(TextCandidateCategory.OrganizationIdentifier, SwedishOrganizationNumber.FindCandidatesInText("559246-0421")[0].Category);
        Assert.Equal(TextCandidateCategory.Financial, Iban.FindCandidatesInText("SE4550000000058398257466")[0].Category);
        Assert.Equal(TextCandidateCategory.Vehicle, SwedishVehicleRegistrationNumber.FindCandidatesInText("ABC 123")[0].Category);
        Assert.Equal(TextCandidateCategory.Product, Gtin13.FindCandidatesInText("5901234123457")[0].Category);
    }

    [Fact]
    public void Candidates_HaveNonEmptyMaskedForm()
    {
        var email = EmailAddress.FindCandidatesInText("user@example.com")[0];
        Assert.False(string.IsNullOrEmpty(email.MaskedForm));
        Assert.NotEqual(email.NormalizedForm, email.MaskedForm);

        var pin = SwedishPersonalIdentityNumber.FindCandidatesInText("990807-2391")[0];
        Assert.False(string.IsNullOrEmpty(pin.MaskedForm));
        Assert.NotEqual(pin.NormalizedForm, pin.MaskedForm);
    }

    // --- SwedishDrivingLicenseCategory ---

    [Fact]
    public void DrivingLicenseCategory_FindsCodeInProse()
    {
        var results = SwedishDrivingLicenseCategory.FindCandidatesInText("Har körkort B och C1E.");
        Assert.True(results.Count >= 2);
        Assert.Contains(results, r => r.Value.Code == "B");
        Assert.Contains(results, r => r.Value.Code == "C1E");
    }

    [Fact]
    public void DrivingLicenseCategory_FindsMultiCharCodes()
    {
        var results = SwedishDrivingLicenseCategory.FindCandidatesInText("Kategori AM och BE krävs.");
        Assert.Contains(results, r => r.Value.Code == "AM");
        Assert.Contains(results, r => r.Value.Code == "BE");
    }

    [Fact]
    public void DrivingLicenseCategory_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(SwedishDrivingLicenseCategory.FindCandidatesInText(""));
        Assert.Empty(SwedishDrivingLicenseCategory.FindCandidatesInText(null!));
    }

    // --- SwedishSwishNumber ---

    [Fact]
    public void SwedishSwishNumber_FindsSwish123InProse()
    {
        var results = SwedishSwishNumber.FindCandidatesInText("Swisha till 1236652895 för att stödja.");
        Assert.Single(results);
        Assert.Equal("1236652895", results[0].NormalizedForm);
        Assert.Equal(TextCandidateCategory.Financial, results[0].Category);
    }

    [Fact]
    public void SwedishSwishNumber_FindsFormattedSwish123()
    {
        var results = SwedishSwishNumber.FindCandidatesInText("Swish: 123-665 28 95");
        Assert.Single(results);
        Assert.Equal("1236652895", results[0].NormalizedForm);
    }

    [Fact]
    public void SwedishSwishNumber_Finds90NumberInProse()
    {
        var results = SwedishSwishNumber.FindCandidatesInText("Skänk via 902 00 33 till Rädda Barnen.");
        Assert.Single(results);
        Assert.Equal("1239020033", results[0].NormalizedForm);
    }

    [Fact]
    public void SwedishSwishNumber_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(SwedishSwishNumber.FindCandidatesInText(""));
        Assert.Empty(SwedishSwishNumber.FindCandidatesInText(null!));
    }

    [Fact]
    public void SwedishSwishNumber_NoFalsePositiveOnPlainText()
    {
        Assert.Empty(SwedishSwishNumber.FindCandidatesInText("Det här är vanlig text utan nummer."));
    }
}
