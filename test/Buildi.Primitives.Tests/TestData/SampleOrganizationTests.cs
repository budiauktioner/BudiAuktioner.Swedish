using Buildi.Primitives.SampleData;

namespace Buildi.Primitives.Tests.TestData;

/// <summary>
/// Validates that every <see cref="SampleOrganization"/> in the catalog
/// was successfully parsed at static initialization time (no exceptions)
/// and that required fields are populated.
/// </summary>
public class SampleOrganizationTests
{
    public static IEnumerable<object[]> AllSamples =>
        SampleOrganizations.All.Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(AllSamples))]
    public void Sample_HasNameAndSources(SampleOrganization sample)
    {
        Assert.NotNull(sample.Name);
        Assert.NotEmpty(sample.Sources);
    }

    [Theory]
    [MemberData(nameof(AllSamples))]
    public void Sample_HasOrganizationNumber(SampleOrganization sample)
    {
        Assert.NotNull(sample.OrganizationNumber);
    }

    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(26, SampleOrganizations.All.Count);
    }

    [Fact]
    public void PerType_BankgiroSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Banking.SwedishBankgiroNumberSampleData.BudiAB);
        Assert.NotNull(SampleData.Banking.SwedishBankgiroNumberSampleData.Vattenfall);
        Assert.NotNull(SampleData.Banking.SwedishBankgiroNumberSampleData.Telia);
        Assert.NotNull(SampleData.Banking.SwedishBankgiroNumberSampleData.SvenskaKyrkan);
        Assert.True(SampleData.Banking.SwedishBankgiroNumberSampleData.All.Count >= 4);
    }

    [Fact]
    public void PerType_PlusgiroSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Banking.SwedishPostgiroNumberSampleData.Vattenfall);
        Assert.NotNull(SampleData.Banking.SwedishPostgiroNumberSampleData.SvenskaKyrkan);
        Assert.True(SampleData.Banking.SwedishPostgiroNumberSampleData.All.Count >= 2);
    }

    [Fact]
    public void PerType_IbanSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Banking.IbanSampleData.Vattenfall);
        Assert.NotNull(SampleData.Banking.IbanSampleData.SwedishGeneric);
        Assert.NotNull(SampleData.Banking.IbanSampleData.GermanGeneric);
        Assert.NotNull(SampleData.Banking.IbanSampleData.BritishGeneric);
        Assert.True(SampleData.Banking.IbanSampleData.All.Count >= 4);
    }

    [Fact]
    public void PerType_BicSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Banking.BicSampleData.Nordea);
        Assert.NotNull(SampleData.Banking.BicSampleData.SEB);
        Assert.NotNull(SampleData.Banking.BicSampleData.Swedbank);
        Assert.NotNull(SampleData.Banking.BicSampleData.Handelsbanken);
        Assert.True(SampleData.Banking.BicSampleData.All.Count >= 4);
    }

    [Fact]
    public void PerType_OrganizationNumberSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.OrganizationNumberSampleData.BudiAB);
        Assert.NotNull(SampleData.Organization.OrganizationNumberSampleData.Vattenfall);
        Assert.NotNull(SampleData.Organization.OrganizationNumberSampleData.StockholmsKommun);
        Assert.NotNull(SampleData.Organization.OrganizationNumberSampleData.SvenskaKyrkan);
        Assert.NotNull(SampleData.Organization.OrganizationNumberSampleData.Folksam);
        Assert.True(SampleData.Organization.OrganizationNumberSampleData.All.Count >= 26);
    }

    [Fact]
    public void PerType_EuVatNumberSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.EuVatNumberSampleData.BudiAB);
        Assert.NotNull(SampleData.Organization.EuVatNumberSampleData.Vattenfall);
    }

    [Fact]
    public void PerType_SniCodeSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.SwedishSniCodeSampleData.BudiAB);
        Assert.NotNull(SampleData.Organization.SwedishSniCodeSampleData.Vattenfall);
    }

    [Fact]
    public void PerType_LeiCodeSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.LeiCodeSampleData.Vattenfall);
        Assert.NotNull(SampleData.Organization.LeiCodeSampleData.LKAB);
        Assert.NotNull(SampleData.Organization.LeiCodeSampleData.Folksam);
    }

    [Fact]
    public void PerType_DunsNumberSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.DunsNumberSampleData.BudiAB);
    }

    [Fact]
    public void PerType_PhoneNumberSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Contact.PhoneNumberSampleData.Systembolaget);
        Assert.NotNull(SampleData.Contact.PhoneNumberSampleData.Folksam);
        Assert.NotNull(SampleData.Contact.PhoneNumberSampleData.SwedishMobileTest);
        Assert.NotNull(SampleData.Contact.PhoneNumberSampleData.SwedishFixedTest);
        Assert.True(SampleData.Contact.PhoneNumberSampleData.All.Count >= 10);
    }

    [Fact]
    public void PerType_EmailSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Web.EmailSampleData.Samhall);
        Assert.NotNull(SampleData.Web.EmailSampleData.LKAB);
        Assert.NotNull(SampleData.Web.EmailSampleData.Example);
        Assert.NotNull(SampleData.Web.EmailSampleData.ExampleOrg);
        Assert.True(SampleData.Web.EmailSampleData.All.Count >= 4);
    }

    [Fact]
    public void PerType_GeographySamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Geography.SwedishCountySampleData.Stockholm);
        Assert.NotNull(SampleData.Geography.SwedishCountySampleData.Norrbotten);
        Assert.NotNull(SampleData.Geography.SwedishCountySampleData.VastraGotaland);
        Assert.NotNull(SampleData.Geography.SwedishCountySampleData.Skane);
        Assert.True(SampleData.Geography.SwedishCountySampleData.All.Count >= 5);

        Assert.NotNull(SampleData.Geography.SwedishMunicipalitySampleData.Stockholm);
        Assert.NotNull(SampleData.Geography.SwedishMunicipalitySampleData.Lulea);
        Assert.NotNull(SampleData.Geography.SwedishMunicipalitySampleData.Goteborg);
        Assert.NotNull(SampleData.Geography.SwedishMunicipalitySampleData.Malmo);
        Assert.True(SampleData.Geography.SwedishMunicipalitySampleData.All.Count >= 6);
    }

    [Fact]
    public void PerType_GeoCoordinateSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Geography.GeoCoordinateSampleData.Stockholm);
        Assert.NotNull(SampleData.Geography.GeoCoordinateSampleData.Gothenburg);
        Assert.NotNull(SampleData.Geography.GeoCoordinateSampleData.Malmo);
        Assert.NotNull(SampleData.Geography.GeoCoordinateSampleData.Lulea);
        Assert.NotNull(SampleData.Geography.GeoCoordinateSampleData.NullIsland);
        Assert.True(SampleData.Geography.GeoCoordinateSampleData.All.Count >= 5);
    }

    [Fact]
    public void PerType_PropertyDesignationSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Property.SwedishPropertyDesignationSampleData.UppsalaDomkyrka);
        Assert.NotNull(SampleData.Property.SwedishPropertyDesignationSampleData.GenericUrban);
        Assert.NotNull(SampleData.Property.SwedishPropertyDesignationSampleData.GenericRural);
        Assert.True(SampleData.Property.SwedishPropertyDesignationSampleData.All.Count >= 3);
    }

    [Fact]
    public void Myndigheter_AreAccessible()
    {
        Assert.NotNull(SampleOrganizations.Skatteverket);
        Assert.NotNull(SampleOrganizations.Forsakringskassan);
        Assert.NotNull(SampleOrganizations.Arbetsformedlingen);
        Assert.NotNull(SampleOrganizations.Bolagsverket);
        Assert.NotNull(SampleOrganizations.Trafikverket);
        Assert.NotNull(SampleOrganizations.Lantmateriet);
        Assert.NotNull(SampleOrganizations.Kronofogden);
        Assert.NotNull(SampleOrganizations.Transportstyrelsen);
        Assert.NotNull(SampleOrganizations.Migrationsverket);
        Assert.NotNull(SampleOrganizations.Tullverket);
    }

    [Fact]
    public void Myndigheter_InferCorrectOrganizationType()
    {
        var skatteverket = SampleOrganizations.Skatteverket;
        Assert.Equal(
            Primitives.Organization.SwedishOrganizationType.OffentligSektor,
            skatteverket.Name.InferredSwedishOrganizationType);
        Assert.True(skatteverket.Name.HasOrganizationIndicators);
    }

    [Fact]
    public void Aggregated_VattenfallHasAllBankingFields()
    {
        var v = SampleOrganizations.VattenfallAB;
        Assert.NotNull(v.Bankgiro);
        Assert.NotNull(v.Plusgiro);
        Assert.NotNull(v.Iban);
        Assert.NotNull(v.Bic);
    }

    [Fact]
    public void Aggregated_SvenskaKyrkanHasPropertyDesignation()
    {
        Assert.NotNull(SampleOrganizations.SvenskaKyrkan.PropertyDesignation);
    }

    [Fact]
    public void PerType_SwedishSwishNumberSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Banking.SwedishSwishNumberSampleData.RodaKorset);
        Assert.NotNull(SampleData.Banking.SwedishSwishNumberSampleData.RaddaBarnen);
        Assert.True(SampleData.Banking.SwedishSwishNumberSampleData.All.Count >= 2);
    }

    [Fact]
    public void PerType_IsinSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Finance.IsinSampleData.Ericsson);
        Assert.NotNull(SampleData.Finance.IsinSampleData.Telia);
        Assert.NotNull(SampleData.Finance.IsinSampleData.AppleInc);
        Assert.True(SampleData.Finance.IsinSampleData.All.Count >= 3);
    }

    [Fact]
    public void PerType_ElfCodeSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Organization.ElfCodeSampleData.Aktiebolag);
        Assert.NotNull(SampleData.Organization.ElfCodeSampleData.Handelsbolag);
        Assert.NotNull(SampleData.Organization.ElfCodeSampleData.EnskildFirma);
        Assert.NotNull(SampleData.Organization.ElfCodeSampleData.Stiftelse);
        Assert.True(SampleData.Organization.ElfCodeSampleData.All.Count >= 4);
    }

    [Fact]
    public void PerType_PropertyTaxationCodeSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Property.SwedishPropertyTaxationCodeSampleData.SmahusBebyggd);
        Assert.NotNull(SampleData.Property.SwedishPropertyTaxationCodeSampleData.HyreshusBostad);
        Assert.NotNull(SampleData.Property.SwedishPropertyTaxationCodeSampleData.IndustriBebyggd);
        Assert.True(SampleData.Property.SwedishPropertyTaxationCodeSampleData.All.Count >= 3);
    }

    [Fact]
    public void PerType_LanguageSamples_AreAccessible()
    {
        Assert.NotNull(SampleData.Geography.LanguageSampleData.Swedish);
        Assert.NotNull(SampleData.Geography.LanguageSampleData.English);
        Assert.NotNull(SampleData.Geography.LanguageSampleData.German);
        Assert.True(SampleData.Geography.LanguageSampleData.All.Count >= 9);
    }
}
