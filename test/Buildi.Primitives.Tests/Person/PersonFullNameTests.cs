using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Person;

public class PersonFullNameTests
{
    [Theory]
    [InlineData("Anna Andersson", "Anna Andersson")]
    [InlineData("anna andersson", "Anna Andersson")]
    [InlineData("ANNA ANDERSSON", "Anna Andersson")]
    [InlineData("Anna Maria Andersson", "Anna Maria Andersson")]
    [InlineData("anna maria elisabeth andersson", "Anna Maria Elisabeth Andersson")]
    [InlineData("  Anna   Andersson  ", "Anna Andersson")]
    [InlineData("Fatima Al-Rashid", "Fatima Al-Rashid")]
    [InlineData("mohammed hassan", "Mohammed Hassan")]
    [InlineData("YUSUF ABDI", "Yusuf Abdi")]
    [InlineData("josé garcía", "José García")]
    [InlineData("Linh Nguyễn", "Linh Nguyễn")]
    [InlineData("Priya Sharma", "Priya Sharma")]
    [InlineData("Amina Zahra Hassan", "Amina Zahra Hassan")]
    [InlineData("Björn Özdemir", "Björn Özdemir")]
    [InlineData("Bo Ek", "Bo Ek")]
    [InlineData("Li Wu", "Li Wu")]
    [InlineData("Jennifer Null", "Jennifer Null")]
    [InlineData("jennifer null", "Jennifer Null")]
    [InlineData("JENNIFER NULL", "Jennifer Null")]
    [InlineData("Ed True", "Ed True")]
    [InlineData("Karl Johan Wilhelm Sebastian Alexander Wolfeschlegelsteinhausenbergerdorff", "Karl Johan Wilhelm Sebastian Alexander Wolfeschlegelsteinhausenbergerdorff")]
    [InlineData("Herr Karl Andersson", "Karl Andersson")]
    [InlineData("herr karl andersson", "Karl Andersson")]
    [InlineData("HERR KARL ANDERSSON", "Karl Andersson")]
    [InlineData("Fru Anna Andersson", "Anna Andersson")]
    [InlineData("Mr. Karl Andersson", "Karl Andersson")]
    [InlineData("Mrs. Anna Andersson", "Anna Andersson")]
    [InlineData("Dr. Anna Maria Svensson", "Anna Maria Svensson")]
    [InlineData("Prof. Karl Andersson", "Karl Andersson")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(PersonFullName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Anna")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(PersonFullName.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_SplitsGivenAndFamilyName()
    {
        Assert.True(PersonFullName.TryParse("Anna Maria Elisabeth Andersson", out var full));
        Assert.Equal("Anna Maria Elisabeth", full!.GivenName.Value);
        Assert.Equal(3, full.GivenName.Names.Count);
        Assert.Equal("Anna", full.GivenName.Names[0]);
        Assert.Equal("Maria", full.GivenName.Names[1]);
        Assert.Equal("Elisabeth", full.GivenName.Names[2]);
        Assert.Equal("Andersson", full.FamilyName.Value);
    }

    [Fact]
    public void TryParse_TwoTokens_SingleGivenName()
    {
        Assert.True(PersonFullName.TryParse("Anna Andersson", out var full));
        Assert.Equal("Anna", full!.GivenName.Value);
        Assert.Single(full.GivenName.Names);
        Assert.Equal("Andersson", full.FamilyName.Value);
    }

    [Fact]
    public void PreferredName_IsNull_WhenNotSet()
    {
        var full = PersonFullName.Parse("Anna Maria Andersson");
        Assert.Null(full.PreferredName);
    }

    [Fact]
    public void TryParse_WithPreferredName_Succeeds()
    {
        Assert.True(PersonFullName.TryParse("Anna Maria Andersson", "Maria", out var full));
        Assert.Equal("Maria", full!.PreferredName);
    }

    [Fact]
    public void TryParse_WithInvalidPreferredName_Fails()
    {
        Assert.False(PersonFullName.TryParse("Anna Maria Andersson", "Elisabeth", out var full));
    }

    [Fact]
    public void WithPreferredName_ReturnsNewInstance()
    {
        var original = PersonFullName.Parse("Anna Maria Andersson");
        var withPref = original.WithPreferredName("Maria");

        Assert.Null(original.PreferredName);
        Assert.Equal("Maria", withPref.PreferredName);
        Assert.Equal("Anna Maria Andersson", withPref.Value);
    }

    [Fact]
    public void WithPreferredName_InvalidName_Throws()
    {
        var full = PersonFullName.Parse("Anna Maria Andersson");
        Assert.Throws<ArgumentException>(() => full.WithPreferredName("Elisabeth"));
    }

    [Fact]
    public void Create_FromParsedParts()
    {
        var given = PersonGivenName.Parse("Anna Maria", "Anna");
        var family = PersonFamilyName.Parse("Andersson");
        var full = PersonFullName.Create(given, family);

        Assert.Equal("Anna Maria Andersson", full.Value);
        Assert.Equal("Anna Maria", full.GivenName.Value);
        Assert.Equal("Andersson", full.FamilyName.Value);
        Assert.Equal("Anna", full.PreferredName);
    }

    [Theory]
    [InlineData("Anna Andersson", true)]
    [InlineData("Anna Maria Andersson", true)]
    [InlineData("Fatima Al-Rashid", true)]
    [InlineData("José García", true)]
    [InlineData("Linh Nguyễn", true)]
    [InlineData("Bo Ek", true)]
    [InlineData("Jennifer Null", true)]
    [InlineData("Anna", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PersonFullName.IsValid(input));
    }

    [Fact]
    public void MixedCase_IsPreserved()
    {
        var full = PersonFullName.Parse("Anna MacDonald");
        Assert.Equal("Anna MacDonald", full.Value);
        Assert.Equal("MacDonald", full.FamilyName.Value);
    }

    [Fact]
    public void Null_AsLastName_IsHandledCorrectly()
    {
        var full = PersonFullName.Parse("Jennifer Null");
        Assert.Equal("Jennifer Null", full.Value);
        Assert.Equal("Jennifer", full.GivenName.Value);
        Assert.Equal("Null", full.FamilyName.Value);
    }

    [Fact]
    public void ShortNames_TwoLetterParts_AreValid()
    {
        var full = PersonFullName.Parse("Bo Ek");
        Assert.Equal("Bo", full.GivenName.Value);
        Assert.Equal("Ek", full.FamilyName.Value);
    }

    [Fact]
    public void TryParse_WithHonorific_DoesNotIncludeInGivenNames()
    {
        Assert.True(PersonFullName.TryParse("Herr Karl Andersson", out var full));
        Assert.Equal("Karl", full!.GivenName.Value);
        Assert.Single(full.GivenName.Names);
        Assert.Equal("Andersson", full.FamilyName.Value);
    }

    [Fact]
    public void LongNames_ManyGivenNames_AreValid()
    {
        var full = PersonFullName.Parse("Karl Johan Wilhelm Sebastian Alexander Andersson");
        Assert.Equal(5, full.GivenName.Names.Count);
        Assert.Equal("Andersson", full.FamilyName.Value);
    }

    [Theory]
    [InlineData("anna andersson", "Anna Andersson")]
    [InlineData("ANNA MARIA ANDERSSON", "Anna Maria Andersson")]
    [InlineData("fatima al-rashid", "Fatima Al-Rashid")]
    [InlineData("josé garcía", "José García")]
    [InlineData("jennifer null", "Jennifer Null")]
    [InlineData("bo ek", "Bo Ek")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonFullName.Normalize(input));
    }

    [Theory]
    [InlineData("anna andersson", "Anna Andersson")]
    [InlineData("ANNA ANDERSSON", "Anna Andersson")]
    [InlineData("Anna", "Anna")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonFullName.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !PersonFullName.IsValid(input)));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var full = PersonFullName.Parse("Anna Andersson");
        Assert.Equal("Anna Andersson", full.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var full = PersonFullName.Parse("Anna Andersson");
        Assert.Equal("Anna Andersson", full.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = PersonFullName.Parse("Anna Andersson");
        var b = PersonFullName.Parse("Anna Andersson");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = PersonFullName.Parse("Anna Andersson");
        var b = PersonFullName.Parse("Bo Ek");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = PersonFullName.Parse("Anna Andersson");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = PersonFullName.Parse("Anna Andersson");
        var b = PersonFullName.Parse("Bo Ek");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = PersonFullName.Parse("Anna Andersson");
        Assert.Equal(1, a.CompareTo(null));
    }
}
