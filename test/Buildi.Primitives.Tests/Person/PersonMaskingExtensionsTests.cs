using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Person;

public class PersonMaskingExtensionsTests
{
    [Theory]
    [InlineData("990807-2391", false, "990807-****")]
    [InlineData("199908072391", false, "990807-****")]
    [InlineData("990807-2391", true, "******-****")]
    [InlineData("199908072391", true, "******-****")]
    public void Pin_ToMaskedString_ReturnsExpected(string input, bool maskBirthDate, string expected)
    {
        var pin = SwedishPersonalIdentityNumber.Parse(input);
        Assert.Equal(expected, pin.ToMaskedString(maskBirthDate));
    }

    [Fact]
    public void Pin_ToMaskedString_DefaultMasksBirthNumberOnly()
    {
        var pin = SwedishPersonalIdentityNumber.Parse("990807-2391");
        var masked = pin.ToMaskedString();
        Assert.StartsWith("990807", masked);
        Assert.EndsWith("****", masked);
        Assert.DoesNotContain("2391", masked);
    }

    [Theory]
    [InlineData("196801642395", false, "680164-****")]
    [InlineData("680164-2395", false, "680164-****")]
    [InlineData("196801642395", true, "******-****")]
    [InlineData("680164-2395", true, "******-****")]
    public void Cn_ToMaskedString_ReturnsExpected(string input, bool maskBirthDate, string expected)
    {
        var cn = SwedishCoordinationNumber.Parse(input);
        Assert.Equal(expected, cn.ToMaskedString(maskBirthDate));
    }

    [Fact]
    public void Cn_ToMaskedString_DefaultMasksBirthNumberOnly()
    {
        var cn = SwedishCoordinationNumber.Parse("680164-2395");
        var masked = cn.ToMaskedString();
        Assert.StartsWith("680164", masked);
        Assert.EndsWith("****", masked);
        Assert.DoesNotContain("2395", masked);
    }

    [Theory]
    [InlineData("Anna Maria", "*** ***")]
    [InlineData("Peter", "***")]
    [InlineData("Karl Johan Erik", "*** *** ***")]
    public void GivenName_ToMaskedString_DefaultMasksAllNames(string input, string expected)
    {
        var name = PersonGivenName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString());
    }

    [Theory]
    [InlineData("Anna Maria", "A. M.")]
    [InlineData("Peter", "P.")]
    [InlineData("Karl Johan Erik", "K. J. E.")]
    public void GivenName_ToMaskedString_UseInitials(string input, string expected)
    {
        var name = PersonGivenName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString(useInitials: true));
    }

    [Theory]
    [InlineData("Andersson", "***")]
    [InlineData("von Essen", "***")]
    public void FamilyName_ToMaskedString_DefaultMasks(string input, string expected)
    {
        var name = PersonFamilyName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString());
    }

    [Theory]
    [InlineData("Andersson", "A.")]
    [InlineData("von Essen", "v.")]
    public void FamilyName_ToMaskedString_UseInitials(string input, string expected)
    {
        var name = PersonFamilyName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString(useInitials: true));
    }

    [Theory]
    [InlineData("Anna Andersson", "*** ***")]
    [InlineData("Anna Maria Andersson", "*** *** ***")]
    [InlineData("Peter Andersson", "*** ***")]
    public void FullName_ToMaskedString_DefaultMasksAllParts(string input, string expected)
    {
        var name = PersonFullName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString());
    }

    [Theory]
    [InlineData("Anna Andersson", "A. A.")]
    [InlineData("Anna Maria Andersson", "A. M. A.")]
    [InlineData("Peter Andersson", "P. A.")]
    public void FullName_ToMaskedString_UseInitials(string input, string expected)
    {
        var name = PersonFullName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString(useInitials: true));
    }

    [Theory]
    [InlineData("Anna Andersson", false, "Anna ***")]
    [InlineData("Anna Maria Andersson", false, "Anna Maria ***")]
    [InlineData("Anna Andersson", true, "Anna A.")]
    [InlineData("Anna Maria Andersson", true, "Anna Maria A.")]
    public void FullName_ToMaskedString_ShowGivenName(string input, bool useInitials, string expected)
    {
        var name = PersonFullName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString(useInitials: useInitials, showGivenName: true));
    }

    [Fact]
    public void FullName_ToMaskedString_DefaultHidesAllNames()
    {
        var name = PersonFullName.Parse("Anna Maria Andersson");
        var masked = name.ToMaskedString();
        Assert.DoesNotContain("Anna", masked);
        Assert.DoesNotContain("Maria", masked);
        Assert.DoesNotContain("Andersson", masked);
        Assert.DoesNotContain("A.", masked);
        Assert.DoesNotContain("M.", masked);
    }
}
