using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Person;

public class PersonFamilyNameTests
{
    [Theory]
    [InlineData("Andersson", "Andersson")]
    [InlineData("andersson", "Andersson")]
    [InlineData("ANDERSSON", "Andersson")]
    [InlineData("von Transen", "von Transen")]
    [InlineData("  Andersson  ", "Andersson")]
    [InlineData("Svensson-Berg", "Svensson-Berg")]
    [InlineData("Al-Rashid", "Al-Rashid")]
    [InlineData("garcía", "García")]
    [InlineData("GARCÍA", "García")]
    [InlineData("Nguyễn", "Nguyễn")]
    [InlineData("O'Brien", "O'Brien")]
    [InlineData("Özdemir", "Özdemir")]
    [InlineData("hassan", "Hassan")]
    [InlineData("Kowalski", "Kowalski")]
    [InlineData("Johansson-García", "Johansson-García")]
    [InlineData("Ek", "Ek")]
    [InlineData("Li", "Li")]
    [InlineData("Wu", "Wu")]
    [InlineData("Ng", "Ng")]
    [InlineData("Oh", "Oh")]
    [InlineData("Null", "Null")]
    [InlineData("null", "Null")]
    [InlineData("NULL", "Null")]
    [InlineData("True", "True")]
    [InlineData("False", "False")]
    [InlineData("None", "None")]
    [InlineData("Wolfeschlegelsteinhausenbergerdorff", "Wolfeschlegelsteinhausenbergerdorff")]
    [InlineData("Brod\u2019en", "Brod'en")]
    [InlineData("L\u2019Obry", "L'Obry")]
    [InlineData("O\u2018Brien", "O'Brien")]
    [InlineData("Ekstr\u00F6m - Lindqvist", "Ekström-Lindqvist")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(PersonFamilyName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(PersonFamilyName.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ValidInput()
    {
        var family = PersonFamilyName.Parse("Andersson");
        Assert.Equal("Andersson", family.Value);
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => PersonFamilyName.Parse(""));
    }

    [Theory]
    [InlineData("Andersson", true)]
    [InlineData("Al-Rashid", true)]
    [InlineData("García", true)]
    [InlineData("Nguyễn", true)]
    [InlineData("O'Brien", true)]
    [InlineData("Özdemir", true)]
    [InlineData("Ek", true)]
    [InlineData("Li", true)]
    [InlineData("Ng", true)]
    [InlineData("Null", true)]
    [InlineData("A", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PersonFamilyName.IsValid(input));
    }

    [Fact]
    public void MixedCase_IsPreserved()
    {
        var family = PersonFamilyName.Parse("MacDonald");
        Assert.Equal("MacDonald", family.Value);
    }

    [Fact]
    public void Null_AsLastName_IsHandledCorrectly()
    {
        var family = PersonFamilyName.Parse("Null");
        Assert.Equal("Null", family.Value);
    }

    [Fact]
    public void ShortNames_TwoLetters_AreValid()
    {
        Assert.True(PersonFamilyName.IsValid("Ek"));
        Assert.True(PersonFamilyName.IsValid("Li"));
        Assert.True(PersonFamilyName.IsValid("Ng"));
    }

    [Fact]
    public void LongNames_AreValid()
    {
        var family = PersonFamilyName.Parse("Wolfeschlegelsteinhausenbergerdorff");
        Assert.Equal("Wolfeschlegelsteinhausenbergerdorff", family.Value);
    }

    [Theory]
    [InlineData("andersson", "Andersson")]
    [InlineData("ANDERSSON", "Andersson")]
    [InlineData("MacDonald", "MacDonald")]
    [InlineData("garcía", "García")]
    [InlineData("HASSAN", "Hassan")]
    [InlineData("özdemir", "Özdemir")]
    [InlineData("null", "Null")]
    [InlineData("ek", "Ek")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonFamilyName.Normalize(input));
    }

    [Theory]
    [InlineData("andersson", "Andersson")]
    [InlineData("123", "123")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonFamilyName.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !PersonFamilyName.IsValid(input)));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var family = PersonFamilyName.Parse("Andersson");
        Assert.Equal("Andersson", family.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var family = PersonFamilyName.Parse("Andersson");
        Assert.Equal("Andersson", family.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = PersonFamilyName.Parse("Andersson");
        var b = PersonFamilyName.Parse("Andersson");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = PersonFamilyName.Parse("Andersson");
        var b = PersonFamilyName.Parse("García");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = PersonFamilyName.Parse("Andersson");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = PersonFamilyName.Parse("Andersson");
        var b = PersonFamilyName.Parse("García");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = PersonFamilyName.Parse("Andersson");
        Assert.Equal(1, a.CompareTo(null));
    }
}
