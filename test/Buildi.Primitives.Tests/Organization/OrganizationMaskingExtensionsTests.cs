using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class OrganizationMaskingExtensionsTests
{
    [Fact]
    public void OrgNumber_Person_DefaultMasksIndividualPart()
    {
        var org = SwedishOrganizationNumber.Parse("990807-2391");
        Assert.True(org.IsPerson);
        var masked = org.ToMaskedString();
        Assert.Equal("990807-****", masked);
    }

    [Fact]
    public void OrgNumber_Person_MaskBirthDate_MasksEverything()
    {
        var org = SwedishOrganizationNumber.Parse("990807-2391");
        var masked = org.ToMaskedString(maskBirthDate: true);
        Assert.Equal("******-****", masked);
    }

    [Fact]
    public void OrgNumber_LegalEntity_DefaultReturnsUnmasked()
    {
        var org = SwedishOrganizationNumber.Parse("559246-0421");
        Assert.False(org.IsPerson);
        var masked = org.ToMaskedString();
        Assert.Equal("559246-0421", masked);
    }

    [Fact]
    public void OrgNumber_LegalEntity_MaskOrganizationNumbers_MasksLastFour()
    {
        var org = SwedishOrganizationNumber.Parse("559246-0421");
        var masked = org.ToMaskedString(maskOrganizationNumbers: true);
        Assert.Equal("559246-****", masked);
    }

    [Fact]
    public void OrgNumber_LegalEntity_MaskBirthDateAlone_HasNoEffect()
    {
        var org = SwedishOrganizationNumber.Parse("559246-0421");
        var masked = org.ToMaskedString(maskBirthDate: true);
        Assert.Equal("559246-0421", masked);
    }

    [Fact]
    public void EuVatNumber_SePersonBased_DefaultMasksPersonPart()
    {
        var vat = EuVatNumber.Parse("SE990807239101");
        var masked = vat.ToMaskedString();
        Assert.Equal("SE990807****01", masked);
    }

    [Fact]
    public void EuVatNumber_SeNonPerson_DefaultReturnsUnmasked()
    {
        var vat = EuVatNumber.Parse("SE559246042101");
        var masked = vat.ToMaskedString();
        Assert.Equal("SE559246042101", masked);
    }

    [Fact]
    public void EuVatNumber_SeNonPerson_AlwaysMask_MasksOrgPart()
    {
        var vat = EuVatNumber.Parse("SE559246042101");
        var masked = vat.ToMaskedString(alwaysMask: true);
        Assert.Equal("SE559246****01", masked);
    }

    [Fact]
    public void EuVatNumber_NonSe_DefaultReturnsUnmasked()
    {
        var vat = EuVatNumber.Parse("DE123456789");
        var masked = vat.ToMaskedString();
        Assert.Equal("DE123456789", masked);
    }

    [Fact]
    public void EuVatNumber_NonSe_AlwaysMask_MasksBody()
    {
        var vat = EuVatNumber.Parse("DE123456789");
        var masked = vat.ToMaskedString(alwaysMask: true);
        Assert.Equal("DE1234*****", masked);
    }
}
