using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishOrganizationNameTests
{
    [Theory]
    [InlineData("Budi Auktioner AB", "Budi Auktioner AB")]
    [InlineData("  Budi  Auktioner  AB  ", "Budi Auktioner AB")]
    [InlineData("AB", "AB")]
    [InlineData("A & B Trading", "A & B Trading")]
    [InlineData("Al-Salam Handel AB", "Al-Salam Handel AB")]
    [InlineData("Café Björnen", "Café Björnen")]
    [InlineData("López Trading AB", "López Trading AB")]
    [InlineData("Nguyễn Restaurang", "Nguyễn Restaurang")]
    [InlineData("Özkan Bygg och Renovering", "Özkan Bygg och Renovering")]
    [InlineData("O'Malley's Pub AB", "O'Malley's Pub AB")]
    [InlineData("Müller & Partners", "Müller & Partners")]
    [InlineData("Kebab King / Pizza House", "Kebab King / Pizza House")]
    [InlineData("ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ||EXAMPLE TEXTILE", "ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ||EXAMPLE TEXTILE")]
    [InlineData("Legal Name AB | Trade Name", "Legal Name AB | Trade Name")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(SwedishOrganizationName.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(SwedishOrganizationName.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Budi Auktioner AB", true)]
    [InlineData("Al-Salam Handel AB", true)]
    [InlineData("Café Björnen", true)]
    [InlineData("Nguyễn Restaurang", true)]
    [InlineData("Özkan Bygg och Renovering", true)]
    [InlineData("A", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.IsValid(input));
    }

    [Theory]
    [InlineData("  Budi  Auktioner  ", "Budi Auktioner")]
    [InlineData("A", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !SwedishOrganizationName.IsValid(input)));
    }

    [Theory]
    [InlineData("  Budi  Auktioner  AB  ", "Budi Auktioner AB")]
    [InlineData("  Al-Salam  Handel  AB  ", "Al-Salam Handel AB")]
    [InlineData("  Café   Björnen  ", "Café Björnen")]
    [InlineData("A", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.Normalize(input));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => SwedishOrganizationName.Parse("A"));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var name = SwedishOrganizationName.Parse("Budi Auktioner AB");
        Assert.Equal("Budi Auktioner AB", name.ToString());
        Assert.Equal("Budi Auktioner AB", name.ToNormalizedString());
    }

    [Theory]
    [InlineData("Volvo AB", true)]
    [InlineData("VOLVO AB", true)]
    [InlineData("Fastighets HB", true)]
    [InlineData("Test KB", true)]
    [InlineData("BRF Solgläntan", true)]
    [InlineData("HSB Riksförbund", true)]
    [InlineData("Enskild firma EF", true)]
    [InlineData("Stiftelsen Riksbankens Jubileumsfond", true)]
    [InlineData("Stockholms kommun", true)]
    [InlineData("Västra Götalands Region", true)]
    [InlineData("Test Myndighet", true)]
    [InlineData("Test Ekonomisk Förening", true)]
    [InlineData("Test Ideell Förening", true)]
    [InlineData("Anna Andersson", false)]
    [InlineData("Erik Svensson", false)]
    [InlineData("Spotify", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void HasOrganizationIndicators_ReturnsExpected(string? input, bool expected)
    {
        if (SwedishOrganizationName.TryParse(input, out var orgName) && orgName is not null)
            Assert.Equal(expected, orgName.HasOrganizationIndicators);
        else
            Assert.False(expected);
    }

    [Theory]
    [InlineData("Volvo AB", SwedishOrganizationType.Aktiebolag)]
    [InlineData("Test HB", SwedishOrganizationType.Handelsbolag)]
    [InlineData("Test KB", SwedishOrganizationType.Kommanditbolag)]
    [InlineData("BRF Solgläntan", SwedishOrganizationType.Bostadsrattsforening)]
    [InlineData("Stiftelsen Nobel", SwedishOrganizationType.Stiftelse)]
    [InlineData("Stockholms kommun", SwedishOrganizationType.Kommun)]
    [InlineData("Region Stockholm", SwedishOrganizationType.Region)]
    [InlineData("Test Ekonomisk Förening", SwedishOrganizationType.EkonomiskForening)]
    [InlineData("Test Ideell Förening", SwedishOrganizationType.IdeellForening)]
    [InlineData("HSB Riksförbund", SwedishOrganizationType.EkonomiskForening)]
    [InlineData("Test HSB", SwedishOrganizationType.EkonomiskForening)]
    [InlineData("Samfälligheten Ekbacken", SwedishOrganizationType.Samfallighetsforening)]
    [InlineData("Test Dödsbo", SwedishOrganizationType.Dodsbo)]
    [InlineData("Test EF", SwedishOrganizationType.EnskildFirma)]
    [InlineData("Test Enskild Firma", SwedishOrganizationType.EnskildFirma)]
    [InlineData("EEIG Europagruppen", SwedishOrganizationType.EuropeiskEkonomiskIntressegruppering)]
    [InlineData("Test Filial", SwedishOrganizationType.Filial)]
    [InlineData("Skatteverket", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Försäkringskassan", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Arbetsförmedlingen", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Bolagsverket", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Trafikverket", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Lantmäteriet", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Kronofogden", SwedishOrganizationType.OffentligSektor)]
    [InlineData("Spotify", SwedishOrganizationType.Unknown)]
    [InlineData(null, SwedishOrganizationType.Unknown)]
    public void InferSwedishOrganizationType_ReturnsExpected(string? input, SwedishOrganizationType expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.InferSwedishOrganizationType(input));
    }

    [Fact]
    public void InferSwedishOrganizationType_StripsPublSuffix()
    {
        Assert.Equal(SwedishOrganizationType.Aktiebolag, SwedishOrganizationName.InferSwedishOrganizationType("Volvo AB (publ)"));
        Assert.Equal(SwedishOrganizationType.Aktiebolag, SwedishOrganizationName.InferSwedishOrganizationType("VOLVO AB (PUBL)"));
    }

    [Theory]
    [InlineData("Skatteverket", true)]
    [InlineData("SKATTEVERKET", true)]
    [InlineData("Försäkringskassan", true)]
    [InlineData("Lantmäteriet", true)]
    [InlineData("Volvo AB", false)]
    [InlineData("Spotify", false)]
    [InlineData(null, false)]
    public void IsKnownGovernmentAgency_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.IsKnownGovernmentAgency(input));
    }

    [Fact]
    public void TryParse_KnownAgency_SetsIndicatorsAndType()
    {
        Assert.True(SwedishOrganizationName.TryParse("Skatteverket", out var orgName));
        Assert.True(orgName!.HasOrganizationIndicators);
        Assert.Equal(SwedishOrganizationType.OffentligSektor, orgName.InferredSwedishOrganizationType);
    }

    [Fact]
    public void TryParse_WithPublSuffix_InfersCorrectType()
    {
        Assert.True(SwedishOrganizationName.TryParse("Volvo AB (publ)", out var orgName));
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgName!.InferredSwedishOrganizationType);
        Assert.True(orgName.HasOrganizationIndicators);
    }

    [Fact]
    public void TryParse_ExposesInferredSwedishOrganizationType()
    {
        Assert.True(SwedishOrganizationName.TryParse("Volvo AB", out var orgName));
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgName!.InferredSwedishOrganizationType);
        Assert.True(orgName.HasOrganizationIndicators);
    }

    [Fact]
    public void TryParse_ExposesUnknownType_ForGenericName()
    {
        Assert.True(SwedishOrganizationName.TryParse("Spotify", out var orgName));
        Assert.Equal(SwedishOrganizationType.Unknown, orgName!.InferredSwedishOrganizationType);
        Assert.False(orgName.HasOrganizationIndicators);
    }

    [Fact]
    public void TryParse_ExposesCasingNormalizedValue()
    {
        Assert.True(SwedishOrganizationName.TryParse("VOLVO AB", out var orgName));
        Assert.Equal("VOLVO AB", orgName!.Value);
        Assert.Equal("Volvo AB", orgName.CasingNormalizedValue);
    }

    [Theory]
    [InlineData("VOLVO AB", "Volvo AB")]
    [InlineData("volvo ab", "Volvo AB")]
    [InlineData("ICA GRUPPEN AB", "Ica Gruppen AB")]
    [InlineData("FASTIGHETS HB", "Fastighets HB")]
    [InlineData("BOSTADSRÄTTSFÖRENINGEN BRF", "Bostadsrättsföreningen BRF")]
    [InlineData("test hsb", "Test HSB")]
    [InlineData("Volvo AB", "Volvo AB")]
    [InlineData("McDonald's AB", "McDonald's AB")]
    public void NormalizeCasing_ReturnsExpected(string input, string expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.NormalizeCasing(input));
    }

    [Fact]
    public void NormalizeCasing_MixedCase_PreservesAsIs()
    {
        Assert.Equal("Volvo AB", SwedishOrganizationName.NormalizeCasing("Volvo AB"));
        Assert.Equal("McDonald's", SwedishOrganizationName.NormalizeCasing("McDonald's"));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishOrganizationName.Parse("Budi Auktioner AB");
        var b = SwedishOrganizationName.Parse("Budi Auktioner AB");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishOrganizationName.Parse("Budi Auktioner AB");
        var b = SwedishOrganizationName.Parse("Volvo AB");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishOrganizationName.Parse("Budi Auktioner AB");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishOrganizationName.Parse("AB");
        var b = SwedishOrganizationName.Parse("Budi Auktioner AB");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishOrganizationName.Parse("Budi Auktioner AB");
        Assert.Equal(1, a.CompareTo(null));
    }

    // --- LegalName / TradeName splitting (LEGAL || TRADE convention) ---

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
        Assert.True(SwedishOrganizationName.TryParse(input, out var name));
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
        Assert.True(SwedishOrganizationName.TryParse(input, out var name));
        Assert.Equal(name!.Value, name.LegalName);
        Assert.Null(name.TradeName);
        Assert.False(name.HasTradeName);
    }

    [Fact]
    public void TryParse_PipeSeparated_RunsInferenceOnLegalPortion()
    {
        Assert.True(SwedishOrganizationName.TryParse("Volvo AB||Volvo Cars", out var name));
        Assert.Equal(SwedishOrganizationType.Aktiebolag, name!.InferredSwedishOrganizationType);
        Assert.True(name.HasOrganizationIndicators);
    }

    [Fact]
    public void TryParse_PipeSeparated_TradeNameAlone_DoesNotInferType()
    {
        // "Generic Brand" (legal) has no Swedish indicator; the trade portion "Volvo AB" must NOT
        // leak into the inference, otherwise we would falsely classify this as Aktiebolag.
        Assert.True(SwedishOrganizationName.TryParse("Generic Brand||Volvo AB", out var name));
        Assert.Equal(SwedishOrganizationType.Unknown, name!.InferredSwedishOrganizationType);
        Assert.False(name.HasOrganizationIndicators);
    }

    [Fact]
    public void InferSwedishOrganizationType_Static_UsesLegalPortion()
    {
        Assert.Equal(SwedishOrganizationType.Aktiebolag,
            SwedishOrganizationName.InferSwedishOrganizationType("Volvo AB||Volvo Cars"));
        Assert.Equal(SwedishOrganizationType.Unknown,
            SwedishOrganizationName.InferSwedishOrganizationType("Generic Brand||Volvo AB"));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Volvo AB", true)]
    [InlineData("Volvo AB||Volvo Cars", true)]
    public void TrySplitLegalAndTrade_ReturnsExpected(string? input, bool expectedSuccess)
    {
        var ok = SwedishOrganizationName.TrySplitLegalAndTrade(input, out _, out _);
        Assert.Equal(expectedSuccess, ok);
    }

    [Fact]
    public void TrySplitLegalAndTrade_ExtractsParts()
    {
        Assert.True(SwedishOrganizationName.TrySplitLegalAndTrade(
            "ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ||EXAMPLE TEXTILE", out var legal, out var trade));
        Assert.Equal("ΑΦΟΙ ΠΑΠΑΔΟΠΟΥΛΟΥ ΟΕ", legal);
        Assert.Equal("EXAMPLE TEXTILE", trade);
    }

    [Fact]
    public void TrySplitLegalAndTrade_NoSeparator_ReturnsFullNameAsLegal()
    {
        Assert.True(SwedishOrganizationName.TrySplitLegalAndTrade("Volvo AB", out var legal, out var trade));
        Assert.Equal("Volvo AB", legal);
        Assert.Null(trade);
    }

    [Theory]
    [InlineData("VOLVO AB||EXAMPLE TEXTILE", "Volvo AB||Example Textile")]
    [InlineData("VOLVO AB || EXAMPLE TEXTILE", "Volvo AB || Example Textile")]
    [InlineData("volvo ab||example textile", "Volvo AB||Example Textile")]
    public void NormalizeCasing_TreatsPipeAsWordBoundary(string input, string expected)
    {
        Assert.Equal(expected, SwedishOrganizationName.NormalizeCasing(input));
    }
}
