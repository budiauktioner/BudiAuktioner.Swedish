using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class EuOrganizationNameTests
{
    // --- Basic acceptance: same shapes the Swedish type accepts must also work here ---

    [Theory]
    [InlineData("Budi Auktioner AB", "Budi Auktioner AB")]
    [InlineData("  Budi  Auktioner  AB  ", "Budi Auktioner AB")]
    [InlineData("AB", "AB")]
    [InlineData("A & B Trading", "A & B Trading")]
    [InlineData("Café Björnen", "Café Björnen")]
    [InlineData("López Trading AB", "López Trading AB")]
    [InlineData("Müller & Partners", "Müller & Partners")]
    [InlineData("O'Malley's Pub AB", "O'Malley's Pub AB")]
    [InlineData("Kebab King / Pizza House", "Kebab King / Pizza House")]
    public void TryParse_AcceptsCommonNames(string input, string expected)
    {
        Assert.True(EuOrganizationName.TryParse(input, out var name));
        Assert.Equal(expected, name!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void TryParse_RejectsTooShortOrEmpty(string? input)
    {
        Assert.False(EuOrganizationName.TryParse(input, out var name));
        Assert.Null(name);
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => EuOrganizationName.Parse("A"));
    }

    [Fact]
    public void IsValid_ReturnsExpected()
    {
        Assert.True(EuOrganizationName.IsValid("Budi Auktioner AB"));
        Assert.False(EuOrganizationName.IsValid(null));
        Assert.False(EuOrganizationName.IsValid("A"));
    }

    [Theory]
    [InlineData("  Budi  Auktioner  AB  ", "Budi Auktioner AB")]
    [InlineData("A", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EuOrganizationName.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var name = EuOrganizationName.Parse("Budi Auktioner AB");
        Assert.Equal("Budi Auktioner AB", name.ToString());
        Assert.Equal("Budi Auktioner AB", name.ToNormalizedString());
    }

    // --- Combined LEGAL || TRADE form (Greek GEMI, some VIES feeds) ---

    [Theory]
    [InlineData("Volvo AB", "Volvo AB", null)]
    [InlineData("Budi Auktioner AB", "Budi Auktioner AB", null)]
    [InlineData("Volvo AB||Volvo Cars", "Volvo AB", "Volvo Cars")]
    [InlineData("Volvo AB | Volvo Cars", "Volvo AB", "Volvo Cars")]
    [InlineData("Volvo AB | | Volvo Cars", "Volvo AB", "Volvo Cars")]
    [InlineData("ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ||EXAMPLE TEXTILE", "ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ", "EXAMPLE TEXTILE")]
    [InlineData("Acme Group AB||Acme Retail||Acme Wholesale", "Acme Group AB", "Acme Retail | Acme Wholesale")]
    public void TryParse_SplitsLegalAndTrade(string input, string expectedLegal, string? expectedTrade)
    {
        Assert.True(EuOrganizationName.TryParse(input, out var name));
        Assert.Equal(expectedLegal, name!.LegalName);
        Assert.Equal(expectedTrade, name.TradeName);
        Assert.Equal(expectedTrade is not null, name.HasTradeName);
    }

    [Theory]
    [InlineData("||EXAMPLE TEXTILE")]
    [InlineData("Volvo AB||")]
    [InlineData("|||")]
    public void TryParse_OnlyOneSideHasContent_DoesNotSplit(string input)
    {
        Assert.True(EuOrganizationName.TryParse(input, out var name));
        Assert.Equal(name!.Value, name.LegalName);
        Assert.Null(name.TradeName);
        Assert.False(name.HasTradeName);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Volvo AB", true)]
    [InlineData("Volvo AB||Volvo Cars", true)]
    public void TrySplitLegalAndTrade_ReturnsExpected(string? input, bool expectedSuccess)
    {
        var ok = EuOrganizationName.TrySplitLegalAndTrade(input, out _, out _);
        Assert.Equal(expectedSuccess, ok);
    }

    [Fact]
    public void TrySplitLegalAndTrade_ExtractsParts()
    {
        Assert.True(EuOrganizationName.TrySplitLegalAndTrade(
            "ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ||EXAMPLE TEXTILE", out var legal, out var trade));
        Assert.Equal("ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ", legal);
        Assert.Equal("EXAMPLE TEXTILE", trade);
    }

    [Fact]
    public void TrySplitLegalAndTrade_NoSeparator_ReturnsFullNameAsLegal()
    {
        Assert.True(EuOrganizationName.TrySplitLegalAndTrade("Volvo AB", out var legal, out var trade));
        Assert.Equal("Volvo AB", legal);
        Assert.Null(trade);
    }

    // --- Baltic / Slavic double-quoted distinctive names ---

    [Theory]
    [InlineData("SIA \"EXAMPLE LV\"")]              // Latvia – limited liability
    [InlineData("AS \"EXAMPLE LV\"")]               // Latvia – joint-stock
    [InlineData("UAB \"EXAMPLE LT\"")]              // Lithuania
    [InlineData("AB \"EXAMPLE LT\"")]               // Lithuania – public company
    [InlineData("AS \"EXAMPLE EE\"")]               // Estonia
    [InlineData("OÜ \"EXAMPLE EE\"")]               // Estonia – private limited
    [InlineData("\"EXAMPLE PL\" Sp. z o.o.")]       // Poland – distinctive name first
    [InlineData("SIA \"EXAMPLE LV\" filiāle")]      // Latvia – branch suffix
    public void TryParse_AcceptsDoubleQuotedDistinctiveName(string input)
    {
        Assert.True(EuOrganizationName.IsValid(input));
        Assert.True(EuOrganizationName.TryParse(input, out var name));
        Assert.Equal(input, name!.Value);
    }

    [Theory]
    // Curly double quotes (U+201C / U+201D) – Word, mobile keyboards
    [InlineData("SIA \u201CEXAMPLE LV\u201D", "SIA \"EXAMPLE LV\"")]
    // German low-9 / high-reversed-9 quotes (U+201E / U+201F)
    [InlineData("SIA \u201EEXAMPLE LV\u201F", "SIA \"EXAMPLE LV\"")]
    // Guillemets (U+00AB / U+00BB) – Russian, Ukrainian, French registries
    [InlineData("\u00ABEXAMPLE\u00BB AS", "\"EXAMPLE\" AS")]
    public void TryParse_NormalizesTypographicDoubleQuotesToAscii(string input, string expectedValue)
    {
        Assert.True(EuOrganizationName.TryParse(input, out var name));
        Assert.Equal(expectedValue, name!.Value);
    }

    [Fact]
    public void TryParse_BalticName_PreservesDiacritics()
    {
        // Latvian diacritics ā ī ē are preserved (covered by \p{L}).
        Assert.True(EuOrganizationName.TryParse("SIA \"Vāca Pārtika\"", out var name));
        Assert.Equal("SIA \"Vāca Pārtika\"", name!.Value);
    }

    // --- Equality and ordering ---

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = EuOrganizationName.Parse("SIA \"Example LV\"");
        var b = EuOrganizationName.Parse("SIA \"Example LV\"");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = EuOrganizationName.Parse("SIA \"Example LV\"");
        var b = EuOrganizationName.Parse("UAB \"Example LT\"");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EuOrganizationName.Parse("AB");
        var b = EuOrganizationName.Parse("SIA \"Example LV\"");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
        Assert.Equal(1, a.CompareTo(null));
    }

    // --- Masking ---

    [Theory]
    [InlineData("Volvo Cars AB", "V**** C*** A*")]
    [InlineData("SIA \"Example LV\"", "S** \"E****** L*\"")]
    [InlineData("Volvo AB||Volvo Cars", "V**** A*||V**** C***")]
    public void ToMaskedString_PreservesStructuralSeparators(string input, string expected)
    {
        var name = EuOrganizationName.Parse(input);
        Assert.Equal(expected, name.ToMaskedString());
    }
}
