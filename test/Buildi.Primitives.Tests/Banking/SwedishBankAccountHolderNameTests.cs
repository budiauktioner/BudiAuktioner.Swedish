using Buildi.Primitives.Banking;

namespace Buildi.Primitives.Tests.Banking;

public class SwedishBankAccountHolderNameTests
{
    [Theory]
    [InlineData("Anna Andersson")]
    [InlineData("ANNA ANDERSSON")]
    [InlineData("Volvo AB")]
    [InlineData("VOLVO AB")]
    [InlineData("ICA Gruppen AB")]
    [InlineData("BRF Solgläntan")]
    [InlineData("Stiftelsen Riksbankens Jubileumsfond")]
    [InlineData("Stockholms kommun")]
    [InlineData("Erik von Sydow")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishBankAccountHolderName.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData("123")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishBankAccountHolderName.IsValid(input));
    }

    [Theory]
    [InlineData("ANNA ANDERSSON", BankAccountHolderType.Person)]
    [InlineData("Anna Andersson", BankAccountHolderType.Person)]
    [InlineData("Erik von Sydow", BankAccountHolderType.Person)]
    [InlineData("VOLVO AB", BankAccountHolderType.Organization)]
    [InlineData("Volvo AB", BankAccountHolderType.Organization)]
    [InlineData("ICA GRUPPEN AB", BankAccountHolderType.Organization)]
    [InlineData("FASTIGHETS HB", BankAccountHolderType.Organization)]
    [InlineData("BRF Solgläntan", BankAccountHolderType.Organization)]
    [InlineData("STIFTELSEN RIKSBANKENS JUBILEUMSFOND", BankAccountHolderType.Organization)]
    [InlineData("STOCKHOLMS KOMMUN", BankAccountHolderType.Organization)]
    [InlineData("VÄSTRA GÖTALANDS REGION", BankAccountHolderType.Organization)]
    [InlineData("Ekonomisk förening Test", BankAccountHolderType.Organization)]
    public void TryParse_DetectsHolderType(string input, BankAccountHolderType expectedType)
    {
        Assert.True(SwedishBankAccountHolderName.TryParse(input, out var result));
        Assert.Equal(expectedType, result!.HolderType);
    }

    [Theory]
    [InlineData("ANNA ANDERSSON", "ANNA ANDERSSON")]
    [InlineData("anna andersson", "anna andersson")]
    [InlineData("Anna Andersson", "Anna Andersson")]
    [InlineData("  Anna   Andersson  ", "Anna Andersson")]
    public void TryParse_PreservesOriginalCasing_ForPersonName(string input, string expected)
    {
        Assert.True(SwedishBankAccountHolderName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("VOLVO AB", "VOLVO AB")]
    [InlineData("volvo ab", "volvo ab")]
    [InlineData("ICA GRUPPEN AB", "ICA GRUPPEN AB")]
    [InlineData("FASTIGHETS HB", "FASTIGHETS HB")]
    [InlineData("BOSTADSRÄTTSFÖRENINGEN BRF", "BOSTADSRÄTTSFÖRENINGEN BRF")]
    public void TryParse_PreservesOriginalCasing_ForOrgName(string input, string expected)
    {
        Assert.True(SwedishBankAccountHolderName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Fact]
    public void TryParse_MixedCase_PreservedForOrg()
    {
        Assert.True(SwedishBankAccountHolderName.TryParse("Volvo AB", out var result));
        Assert.Equal("Volvo AB", result!.Value);
    }

    [Fact]
    public void TryParse_Person_ParsesPersonFullName()
    {
        Assert.True(SwedishBankAccountHolderName.TryParse("ANNA MARIA ANDERSSON", out var result));
        Assert.Equal(BankAccountHolderType.Person, result!.HolderType);
        Assert.NotNull(result.PersonName);
        Assert.Equal("Andersson", result.PersonName!.FamilyName.Value);
        Assert.Null(result.OrganizationName);
    }

    [Fact]
    public void TryParse_Organization_ParsesOrganizationName()
    {
        Assert.True(SwedishBankAccountHolderName.TryParse("VOLVO AB", out var result));
        Assert.Equal(BankAccountHolderType.Organization, result!.HolderType);
        Assert.NotNull(result.OrganizationName);
        Assert.Null(result.PersonName);
    }

    [Fact]
    public void TryParse_SingleWord_FallsBackToOrganization()
    {
        Assert.True(SwedishBankAccountHolderName.TryParse("Madonna", out var result));
        Assert.Equal(BankAccountHolderType.Organization, result!.HolderType);
        Assert.Null(result.PersonName);
        Assert.NotNull(result.OrganizationName);
    }

    [Theory]
    [InlineData("VOLVO AB", "VOLVO AB")]
    [InlineData("ANNA ANDERSSON", "ANNA ANDERSSON")]
    [InlineData("Volvo AB", "Volvo AB")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishBankAccountHolderName.Format(input));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishBankAccountHolderName.Parse(input));
    }

    [Fact]
    public void Equality_SameNameDifferentCase()
    {
        var a = SwedishBankAccountHolderName.Parse("VOLVO AB");
        var b = SwedishBankAccountHolderName.Parse("Volvo AB");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentNames()
    {
        var a = SwedishBankAccountHolderName.Parse("Volvo AB");
        var b = SwedishBankAccountHolderName.Parse("Scania AB");
        Assert.True(a != b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishBankAccountHolderName.Parse("Anna Andersson");
        var b = SwedishBankAccountHolderName.Parse("Volvo AB");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishBankAccountHolderName.Parse("ICA Gruppen AB");
        Assert.Equal(1, a.CompareTo(null));
    }
}
