using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Person;

public class PersonGivenNameTests
{
    [Theory]
    [InlineData("Anna", "Anna")]
    [InlineData("anna", "Anna")]
    [InlineData("ANNA", "Anna")]
    [InlineData("Anna-Lisa", "Anna-Lisa")]
    [InlineData("anna-lisa", "Anna-Lisa")]
    [InlineData("ANNA-LISA", "Anna-Lisa")]
    [InlineData("  Anna  ", "Anna")]
    [InlineData("Anna Maria", "Anna Maria")]
    [InlineData("anna maria", "Anna Maria")]
    [InlineData("ANNA MARIA", "Anna Maria")]
    [InlineData("Anna Maria Elisabeth", "Anna Maria Elisabeth")]
    [InlineData("Fatima", "Fatima")]
    [InlineData("mohammed", "Mohammed")]
    [InlineData("YUSUF", "Yusuf")]
    [InlineData("Amina Zahra", "Amina Zahra")]
    [InlineData("José", "José")]
    [InlineData("Linh", "Linh")]
    [InlineData("Björn-Erik", "Björn-Erik")]
    [InlineData("Priya", "Priya")]
    [InlineData("Abdi", "Abdi")]
    [InlineData("Ngọc", "Ngọc")]
    [InlineData("Bo", "Bo")]
    [InlineData("Li", "Li")]
    [InlineData("Ed", "Ed")]
    [InlineData("Ai", "Ai")]
    [InlineData("Null", "Null")]
    [InlineData("null", "Null")]
    [InlineData("NULL", "Null")]
    [InlineData("True", "True")]
    [InlineData("Karl Johan Wilhelm Sebastian Alexander", "Karl Johan Wilhelm Sebastian Alexander")]
    [InlineData("Lars - Olof", "Lars-Olof")]
    [InlineData("Sven -Erik", "Sven-Erik")]
    [InlineData("Britt -Marie", "Britt-Marie")]
    [InlineData("Nils- Arne", "Nils-Arne")]
    [InlineData(".Gunnar", "Gunnar")]
    [InlineData("Astrid.", "Astrid")]
    [InlineData("Ingvar.", "Ingvar")]
    [InlineData("Rolf. Bertil.", "Rolf Bertil")]
    [InlineData("\u202AElsa", "Elsa")]
    [InlineData("\u200FGunhild", "Gunhild")]
    [InlineData("\uFEFFTorsten", "Torsten")]
    [InlineData("\u00A0Hilma\u00A0", "Hilma")]
    [InlineData("Dagny\tSignild", "Dagny Signild")]
    [InlineData("Folke\nValdemar", "Folke Valdemar")]
    [InlineData("\0Tyra", "Tyra")]
    [InlineData("Herr Karl", "Karl")]
    [InlineData("herr karl", "Karl")]
    [InlineData("HERR KARL", "Karl")]
    [InlineData("Fru Anna", "Anna")]
    [InlineData("FRU ANNA MARIA", "Anna Maria")]
    [InlineData("Fröken Astrid", "Astrid")]
    [InlineData("Mr Karl", "Karl")]
    [InlineData("Mr. Karl", "Karl")]
    [InlineData("Mrs Anna", "Anna")]
    [InlineData("Mrs. Anna", "Anna")]
    [InlineData("Ms Anna", "Anna")]
    [InlineData("Dr Karl-Erik", "Karl-Erik")]
    [InlineData("Dr. Karl-Erik", "Karl-Erik")]
    [InlineData("Prof Anna", "Anna")]
    [InlineData("Prof. Anna", "Anna")]
    [InlineData("Miss Anna", "Anna")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(PersonGivenName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("123")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(PersonGivenName.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Names_ReturnsIndividualNames()
    {
        var given = PersonGivenName.Parse("Anna Maria Elisabeth");
        Assert.Equal(3, given.Names.Count);
        Assert.Equal("Anna", given.Names[0]);
        Assert.Equal("Maria", given.Names[1]);
        Assert.Equal("Elisabeth", given.Names[2]);
    }

    [Fact]
    public void Names_SingleName()
    {
        var given = PersonGivenName.Parse("Anna");
        Assert.Single(given.Names);
        Assert.Equal("Anna", given.Names[0]);
    }

    [Fact]
    public void PreferredName_IsNull_WhenNotSet()
    {
        var given = PersonGivenName.Parse("Anna Maria");
        Assert.Null(given.PreferredName);
    }

    [Fact]
    public void TryParse_WithPreferredName_Succeeds()
    {
        Assert.True(PersonGivenName.TryParse("Anna Maria", "Maria", out var result));
        Assert.Equal("Maria", result!.PreferredName);
        Assert.Equal("Anna Maria", result.Value);
    }

    [Fact]
    public void TryParse_WithPreferredName_CaseInsensitive()
    {
        Assert.True(PersonGivenName.TryParse("Anna Maria", "maria", out var result));
        Assert.Equal("Maria", result!.PreferredName);
    }

    [Fact]
    public void TryParse_WithInvalidPreferredName_Fails()
    {
        Assert.False(PersonGivenName.TryParse("Anna Maria", "Elisabeth", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithPreferredName()
    {
        var given = PersonGivenName.Parse("Anna Maria", "Anna");
        Assert.Equal("Anna", given.PreferredName);
    }

    [Fact]
    public void Parse_WithInvalidPreferredName_Throws()
    {
        Assert.Throws<ArgumentException>(() => PersonGivenName.Parse("Anna Maria", "Elisabeth"));
    }

    [Fact]
    public void WithPreferredName_ReturnsNewInstance()
    {
        var original = PersonGivenName.Parse("Anna Maria");
        var withPref = original.WithPreferredName("Maria");

        Assert.Null(original.PreferredName);
        Assert.Equal("Maria", withPref.PreferredName);
        Assert.Equal("Anna Maria", withPref.Value);
    }

    [Fact]
    public void WithPreferredName_InvalidName_Throws()
    {
        var given = PersonGivenName.Parse("Anna Maria");
        Assert.Throws<ArgumentException>(() => given.WithPreferredName("Elisabeth"));
    }

    [Theory]
    [InlineData("Anna", true)]
    [InlineData("Anna Maria", true)]
    [InlineData("Fatima", true)]
    [InlineData("José", true)]
    [InlineData("Amina Zahra", true)]
    [InlineData("Bo", true)]
    [InlineData("Li", true)]
    [InlineData("Null", true)]
    [InlineData("True", true)]
    [InlineData("A", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PersonGivenName.IsValid(input));
    }

    [Fact]
    public void MixedCase_IsPreserved()
    {
        var given = PersonGivenName.Parse("MacDonald");
        Assert.Equal("MacDonald", given.Value);
    }

    [Theory]
    [InlineData("anna", "Anna")]
    [InlineData("ANNA MARIA", "Anna Maria")]
    [InlineData("MacDonald", "MacDonald")]
    [InlineData("fatima", "Fatima")]
    [InlineData("MOHAMMED", "Mohammed")]
    [InlineData("josé", "José")]
    [InlineData("null", "Null")]
    [InlineData("bo", "Bo")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonGivenName.Normalize(input));
    }

    [Theory]
    [InlineData("anna", "Anna")]
    [InlineData("ANNA", "Anna")]
    [InlineData("123", "123")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonGivenName.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !PersonGivenName.IsValid(input)));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var given = PersonGivenName.Parse("Anna Maria");
        Assert.Equal("Anna Maria", given.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var given = PersonGivenName.Parse("Anna Maria");
        Assert.Equal("Anna Maria", given.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = PersonGivenName.Parse("Anna");
        var b = PersonGivenName.Parse("Anna");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = PersonGivenName.Parse("Anna");
        var b = PersonGivenName.Parse("Bo");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = PersonGivenName.Parse("Anna");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = PersonGivenName.Parse("Anna");
        var b = PersonGivenName.Parse("Bo");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = PersonGivenName.Parse("Anna");
        Assert.Equal(1, a.CompareTo(null));
    }
}
