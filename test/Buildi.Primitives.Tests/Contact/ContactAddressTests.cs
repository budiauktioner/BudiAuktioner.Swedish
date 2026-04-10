using Buildi.Primitives.Contact;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Contact;

[Collection("CultureSensitive")]
public class ContactAddressTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();

    [Fact]
    public void Create_WithAllComponents_SetsAllProperties()
    {
        var person = PersonFullName.Parse("Anna Andersson");
        var org = SwedishOrganizationName.Parse("Budi AB");
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

        var contact = ContactAddress.Create(person, org, address);

        Assert.True(contact.HasPersonName);
        Assert.True(contact.HasOrganizationName);
        Assert.True(contact.HasAddress);
        Assert.Equal("Anna Andersson", contact.PersonName!.Value);
        Assert.Equal("Budi AB", contact.OrganizationName!.Value);
        Assert.NotNull(contact.Address);
    }

    [Fact]
    public void Create_WithOnlyPersonName_Works()
    {
        var person = PersonFullName.Parse("Anna Andersson");

        var contact = ContactAddress.Create(personName: person);

        Assert.True(contact.HasPersonName);
        Assert.False(contact.HasOrganizationName);
        Assert.False(contact.HasAddress);
    }

    [Fact]
    public void Create_WithOnlyOrganizationName_Works()
    {
        var org = SwedishOrganizationName.Parse("Budi AB");

        var contact = ContactAddress.Create(organizationName: org);

        Assert.False(contact.HasPersonName);
        Assert.True(contact.HasOrganizationName);
        Assert.False(contact.HasAddress);
    }

    [Fact]
    public void Create_WithOnlyAddress_Works()
    {
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

        var contact = ContactAddress.Create(address: address);

        Assert.False(contact.HasPersonName);
        Assert.False(contact.HasOrganizationName);
        Assert.True(contact.HasAddress);
    }

    [Fact]
    public void Create_WithNoComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => ContactAddress.Create());
    }

    [Fact]
    public void ToString_ReturnsSingleLineSwedish()
    {
        var person = PersonFullName.Parse("Anna Andersson");
        var org = SwedishOrganizationName.Parse("Budi AB");
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

        var contact = ContactAddress.Create(person, org, address);

        var result = contact.ToString();
        Assert.Contains("Anna Andersson", result);
        Assert.Contains("Budi AB", result);
        Assert.Contains("Storgatan 12", result);
        Assert.DoesNotContain("Sverige", result);
    }

    [Fact]
    public void ToEnglishString_ReturnsCountryInEnglish()
    {
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");
        var contact = ContactAddress.Create(address: address);

        var result = contact.ToEnglishString();
        Assert.Contains("Sweden", result);
        Assert.DoesNotContain("Sverige", result);
    }

    [Fact]
    public void ToMultilineString_IncludesNameLinesAboveAddress()
    {
        var person = PersonFullName.Parse("Anna Andersson");
        var org = SwedishOrganizationName.Parse("Budi AB");
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

        var contact = ContactAddress.Create(person, org, address);
        var lines = contact.ToMultilineString().Split(Environment.NewLine);

        Assert.Equal("Anna Andersson", lines[0]);
        Assert.Equal("Budi AB", lines[1]);
        Assert.Contains("Storgatan 12", lines[2]);
    }

    [Fact]
    public void ToMultilineString_WithEnglishUICulture_UsesEnglishCountry()
    {
        PrimitivesDefaults.UICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");
        var contact = ContactAddress.Create(address: address);

        var result = contact.ToMultilineString();
        Assert.Contains("Sweden", result);
    }

    [Fact]
    public void ToMultilineString_WithOnlyPersonName_ReturnsSingleLine()
    {
        var person = PersonFullName.Parse("Anna Andersson");
        var contact = ContactAddress.Create(personName: person);

        Assert.Equal("Anna Andersson", contact.ToMultilineString());
    }

    [Fact]
    public void Builder_WithRawStrings_ParsesAll()
    {
        var ok = ContactAddress.Builder()
            .WithPersonName("Anna Andersson")
            .WithOrganizationName("Budi AB")
            .WithAddress("Storgatan 12", "114 53", "Stockholm", "SE")
            .TryBuild(out var contact);

        Assert.True(ok);
        Assert.NotNull(contact);
        Assert.Equal("Anna Andersson", contact!.PersonName!.Value);
        Assert.Equal("Budi AB", contact.OrganizationName!.Value);
        Assert.NotNull(contact.Address);
    }

    [Fact]
    public void Builder_WithFreeTextAddress_ParsesAddress()
    {
        var ok = ContactAddress.Builder()
            .WithPersonName("Anna Andersson")
            .WithAddress("Storgatan 12, 114 53 Stockholm")
            .TryBuild(out var contact);

        Assert.True(ok);
        Assert.NotNull(contact!.Address);
        Assert.Equal("Anna Andersson", contact.PersonName!.Value);
    }

    [Fact]
    public void Builder_WithParsedInstances_Works()
    {
        var person = PersonFullName.Parse("Anna Andersson");
        var org = SwedishOrganizationName.Parse("Budi AB");

        var ok = ContactAddress.Builder()
            .WithPersonName(person)
            .WithOrganizationName(org)
            .TryBuild(out var contact);

        Assert.True(ok);
        Assert.Same(person, contact!.PersonName);
        Assert.Same(org, contact.OrganizationName);
    }

    [Fact]
    public void Builder_WithNoValidInput_ReturnsFalse()
    {
        var ok = ContactAddress.Builder()
            .WithPersonName((string?)null)
            .WithOrganizationName((string?)null)
            .WithAddress((string?)null)
            .TryBuild(out var contact);

        Assert.False(ok);
        Assert.Null(contact);
    }

    [Fact]
    public void Builder_Build_ThrowsWhenEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ContactAddress.Builder().Build());
    }

    [Fact]
    public void Builder_IgnoresUnparseableValues()
    {
        var ok = ContactAddress.Builder()
            .WithPersonName("")
            .WithOrganizationName("Budi AB")
            .TryBuild(out var contact);

        Assert.True(ok);
        Assert.Null(contact!.PersonName);
        Assert.NotNull(contact.OrganizationName);
    }

    [Fact]
    public void Create_GermanOrganizationWithAddress()
    {
        var org = SwedishOrganizationName.Parse("Müller & Partners GmbH");
        var address = Address.Parse("Friedrichstraße 42", "10117", "Berlin", "DE");

        var contact = ContactAddress.Create(organizationName: org, address: address);

        Assert.True(contact.HasOrganizationName);
        Assert.True(contact.HasAddress);
        Assert.Equal("Müller & Partners GmbH", contact.OrganizationName!.Value);
        Assert.Equal("DE", contact.Address!.Country!.Alpha2Code);
    }

    [Fact]
    public void Builder_FrenchPersonWithAddress()
    {
        var ok = ContactAddress.Builder()
            .WithPersonName("José García")
            .WithAddress("Rue de la Paix 15", "75002", "Paris", "FR")
            .TryBuild(out var contact);

        Assert.True(ok);
        Assert.Equal("José García", contact!.PersonName!.Value);
        Assert.Equal("FR", contact.Address!.Country!.Alpha2Code);
    }

    [Fact]
    public void ToMultilineString_NonSwedishCountry_IncludesCountryName()
    {
        PrimitivesDefaults.UICulture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE");
        var org = SwedishOrganizationName.Parse("López Trading");
        var address = Address.Parse("Calle Mayor 25", "28013", "Madrid", "ES");

        var contact = ContactAddress.Create(organizationName: org, address: address);
        var result = contact.ToMultilineString();

        Assert.Contains("López Trading", result);
        Assert.Contains("Calle Mayor 25", result);
        Assert.Contains("Spanien", result);
    }
}
