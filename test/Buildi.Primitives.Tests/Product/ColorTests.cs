using Buildi.Primitives.Product;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.Product;

public class ColorTests
{
    [Theory]
    [InlineData("red", 255, 0, 0, "red", "röd")]
    [InlineData("Röd", 255, 0, 0, "red", "röd")]
    [InlineData("BLUE", 0, 0, 255, "blue", "blå")]
    [InlineData("svart", 0, 0, 0, "black", "svart")]
    [InlineData("vit", 255, 255, 255, "white", "vit")]
    [InlineData("grön", 0, 128, 0, "green", "grön")]
    [InlineData("marinblå", 0, 0, 128, "navy", "marinblå")]
    [InlineData("karmosinröd", 220, 20, 60, "crimson", "karmosinröd")]
    public void TryParse_NamedColors_EnAndSv(string input, byte r, byte g, byte b, string en, string sv)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.NotNull(c);
        Assert.Equal(r, c.R);
        Assert.Equal(g, c.G);
        Assert.Equal(b, c.B);
        Assert.Equal(en, c.NameEnglish);
        Assert.Equal(sv, c.NameSwedish);
    }

    [Theory]
    [InlineData("#FF0000", 255, 0, 0)]
    [InlineData("#ff0000", 255, 0, 0)]
    [InlineData("#F00", 255, 0, 0)]
    [InlineData("#1a2B3c", 26, 43, 60)]
    public void TryParse_Hex_SixAndThreeDigit(string input, byte r, byte g, byte b)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.Equal(r, c!.R);
        Assert.Equal(g, c.G);
        Assert.Equal(b, c.B);
    }

    [Theory]
    [InlineData("rgb(255,0,0)", 255, 0, 0)]
    [InlineData("RGB(255, 0, 0)", 255, 0, 0)]
    [InlineData("rgb(0, 128, 0)", 0, 128, 0)]
    public void TryParse_RgbFunction(string input, byte r, byte g, byte b)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.Equal(r, c!.R);
        Assert.Equal(g, c.G);
        Assert.Equal(b, c.B);
    }

    [Theory]
    [InlineData("hsl(0,100%,50%)", 255, 0, 0)]
    [InlineData("hsl(0, 100%, 50%)", 255, 0, 0)]
    [InlineData("HSL(240,100%,50%)", 0, 0, 255)]
    public void TryParse_HslFunction(string input, byte r, byte g, byte b)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.Equal(r, c!.R);
        Assert.Equal(g, c.G);
        Assert.Equal(b, c.B);
    }

    [Fact]
    public void Properties_RedNamed_HasExpectedHexAndHsl()
    {
        var c = Color.Parse("red");

        Assert.Equal("#FF0000", c.Hex);
        Assert.Equal(0, c.H);
        Assert.Equal(100, c.S);
        Assert.Equal(50, c.L);
        Assert.Equal("red", c.NameEnglish);
        Assert.Equal("röd", c.NameSwedish);
    }

    [Fact]
    public void Properties_PurpleFromHex_HasNames()
    {
        var c = Color.Parse("#800080");

        Assert.Equal(128, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(128, c.B);
        Assert.Equal("#800080", c.Hex);
        Assert.Equal("purple", c.NameEnglish);
        Assert.Equal("lila", c.NameSwedish);
    }

    [Fact]
    public void Properties_CustomRgb_HasNullNames()
    {
        var c = Color.Parse("rgb(1,2,3)");

        Assert.Equal(1, c.R);
        Assert.Equal(2, c.G);
        Assert.Equal(3, c.B);
        Assert.Equal("#010203", c.Hex);
        Assert.Null(c.NameEnglish);
        Assert.Null(c.NameSwedish);
    }

    [Theory]
    [InlineData("red", "red")]
    [InlineData("#010203", "#010203")]
    [InlineData("  blue ", "blue")]
    public void Format_ReturnsEnglishNameOrHex(string input, string expected)
    {
        Assert.Equal(expected, Color.Format(input));
    }

    [Theory]
    [InlineData("#ff0000", "#FF0000")]
    [InlineData("red", "#FF0000")]
    [InlineData("hsl(0,100%,50%)", "#FF0000")]
    public void Normalize_ReturnsUppercaseHex(string? input, string expected)
    {
        Assert.Equal(expected, Color.Normalize(input));
    }

    [Theory]
    [InlineData("#FF0000", true)]
    [InlineData("#ff0000", false)]
    [InlineData("red", false)]
    [InlineData(null, false)]
    public void IsNormalized_OnlyUppercaseHex(string? input, bool expected)
    {
        Assert.Equal(expected, Color.IsNormalized(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("#ff")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("rgb(256,0,0)")]
    [InlineData("rgb(-1,0,0)")]
    [InlineData("hsl(0,101%,50%)")]
    [InlineData("hsl(0,100%,101%)")]
    [InlineData("not-a-color")]
    public void IsValid_ReturnsFalse_ForInvalid(string? input)
    {
        Assert.False(Color.IsValid(input));
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Color.Parse("#gg0000"));
    }

    [Fact]
    public void FindCandidatesInText_FindsValidHexOnly()
    {
        var text = "Use #F00 or #aabbcc and invalid #gg and #12 ";
        var candidates = Color.FindCandidatesInText(text);

        Assert.Equal(2, candidates.Count);
        Assert.All(candidates, c => Assert.Equal(nameof(Color), c.TypeName));
        Assert.All(candidates, c => Assert.Equal(TextCandidateCategory.Product, c.Category));
        Assert.All(candidates, c => Assert.Equal(TextMatchConfidence.Medium, c.Confidence));
        Assert.Contains(candidates, c => c.Value.Hex == "#FF0000");
        Assert.Contains(candidates, c => c.Value.Hex == "#AABBCC");
    }

    [Theory]
    [InlineData("ljusblå", "light blue", "ljusblå")]
    [InlineData("mörkblå", "dark blue", "mörkblå")]
    [InlineData("ljusgrön", "light green", "ljusgrön")]
    [InlineData("mörkgrön", "dark green", "mörkgrön")]
    [InlineData("ljusröd", "light red", "ljusröd")]
    [InlineData("mörkröd", "dark red", "mörkröd")]
    [InlineData("ljusgrå", "light grey", "ljusgrå")]
    [InlineData("mörkgrå", "dark grey", "mörkgrå")]
    [InlineData("ljusrosa", "light pink", "ljusrosa")]
    [InlineData("mörkbrun", "dark brown", "mörkbrun")]
    [InlineData("ljuslila", "light purple", "ljuslila")]
    [InlineData("blekrosa", "pale pink", "blekrosa")]
    [InlineData("light blue", "light blue", "ljusblå")]
    [InlineData("dark green", "dark green", "mörkgrön")]
    [InlineData("pale pink", "pale pink", "blekrosa")]
    [InlineData("Ljusblå", "light blue", "ljusblå")]
    [InlineData("MÖRKBLÅ", "dark blue", "mörkblå")]
    [InlineData("ljus blå", "light blue", "ljusblå")]
    [InlineData("mörk-blå", "dark blue", "mörkblå")]
    [InlineData("ljusorange", "light orange", "ljusorange")]
    [InlineData("mörkturkos", "dark turquoise", "mörkturkos")]
    [InlineData("ljusmarinblå", "light navy", "ljusmarinblå")]
    public void TryParse_PrefixedColors_ReturnsCorrectNames(string input, string expectedEn, string expectedSv)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.NotNull(c);
        Assert.Equal(expectedEn, c.NameEnglish);
        Assert.Equal(expectedSv, c.NameSwedish);
    }

    [Fact]
    public void TryParse_LjusBlue_LightensColor()
    {
        var baseOk = Color.TryParse("blå", out var baseColor);
        var lightOk = Color.TryParse("ljusblå", out var lightColor);

        Assert.True(baseOk);
        Assert.True(lightOk);
        Assert.True(lightColor!.R > baseColor!.R);
        Assert.True(lightColor.G > baseColor.G);
        Assert.Equal(baseColor.B, lightColor.B);
    }

    [Fact]
    public void TryParse_MorkBlue_DarkensColor()
    {
        var baseOk = Color.TryParse("blå", out var baseColor);
        var darkOk = Color.TryParse("mörkblå", out var darkColor);

        Assert.True(baseOk);
        Assert.True(darkOk);
        Assert.Equal(baseColor!.R, darkColor!.R);
        Assert.Equal(baseColor.G, darkColor.G);
        Assert.True(darkColor.B < baseColor.B);
    }

    [Theory]
    [InlineData("ljusblå")]
    [InlineData("mörkröd")]
    [InlineData("blekgrön")]
    [InlineData("light blue")]
    [InlineData("dark red")]
    public void IsValid_ReturnsTrue_ForPrefixedColors(string input)
    {
        Assert.True(Color.IsValid(input));
    }

    [Theory]
    [InlineData("ljus")]
    [InlineData("mörk")]
    [InlineData("ljusfoo")]
    [InlineData("mörkxyz")]
    [InlineData("l")]
    [InlineData("m")]
    [InlineData("lxyz")]
    [InlineData("mxyz")]
    public void IsValid_ReturnsFalse_ForInvalidPrefixedColors(string? input)
    {
        Assert.False(Color.IsValid(input));
    }

    [Theory]
    [InlineData("Mgrå", "dark grey", "mörkgrå")]
    [InlineData("Mblå", "dark blue", "mörkblå")]
    [InlineData("MGRÖN", "dark green", "mörkgrön")]
    [InlineData("MRÖD", "dark red", "mörkröd")]
    [InlineData("Lgrå", "light grey", "ljusgrå")]
    [InlineData("LGRÅ", "light grey", "ljusgrå")]
    [InlineData("LBLÅ", "light blue", "ljusblå")]
    [InlineData("LBRUN", "light brown", "ljusbrun")]
    [InlineData("Lgrön", "light green", "ljusgrön")]
    [InlineData("Lgul", "light yellow", "ljusgul")]
    [InlineData("LRÖD", "light red", "ljusröd")]
    [InlineData("M grå", "dark grey", "mörkgrå")]
    [InlineData("L blå", "light blue", "ljusblå")]
    [InlineData("M-grå", "dark grey", "mörkgrå")]
    public void TryParse_SingleLetterAbbreviationPrefixes(string input, string expectedEn, string expectedSv)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.NotNull(c);
        Assert.Equal(expectedEn, c.NameEnglish);
        Assert.Equal(expectedSv, c.NameSwedish);
    }

    [Theory]
    [InlineData("Lila")]
    [InlineData("Lime")]
    [InlineData("Lavender")]
    [InlineData("Lavendel")]
    [InlineData("Magenta")]
    [InlineData("Maroon")]
    [InlineData("Marinblå")]
    public void TryParse_SingleLetterPrefix_DoesNotShadowNamedColors(string input)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.NotNull(c);
        Assert.NotNull(c.NameEnglish);
        Assert.False(c.NameEnglish!.StartsWith("light", StringComparison.OrdinalIgnoreCase));
        Assert.False(c.NameEnglish.StartsWith("dark", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("vinröd", 0x80, 0x00, 0x20, "burgundy", "vinröd")]
    [InlineData("burgundy", 0x80, 0x00, 0x20, "burgundy", "vinröd")]
    [InlineData("Vinröd", 0x80, 0x00, 0x20, "burgundy", "vinröd")]
    public void TryParse_Burgundy(string input, byte r, byte g, byte b, string en, string sv)
    {
        var ok = Color.TryParse(input, out var c);

        Assert.True(ok);
        Assert.NotNull(c);
        Assert.Equal(r, c.R);
        Assert.Equal(g, c.G);
        Assert.Equal(b, c.B);
        Assert.Equal(en, c.NameEnglish);
        Assert.Equal(sv, c.NameSwedish);
    }

    [Fact]
    public void TryParse_PrefixedColor_DeterministicRgb()
    {
        Color.TryParse("ljusblå", out var a);
        Color.TryParse("ljusblå", out var b);
        Color.TryParse("light blue", out var c);

        Assert.Equal(a!.R, b!.R);
        Assert.Equal(a.G, b.G);
        Assert.Equal(a.B, b.B);
        Assert.Equal(a.R, c!.R);
        Assert.Equal(a.G, c.G);
        Assert.Equal(a.B, c.B);
    }

    [Fact]
    public void RoundTrip_NameToHexToName()
    {
        var fromName = Color.Parse("turquoise");
        var hex = fromName.Hex;

        Assert.True(Color.TryParse(hex, out var fromHex));
        Assert.Equal("turquoise", fromHex!.NameEnglish);
        Assert.Equal("turkos", fromHex.NameSwedish);
    }

    [Fact]
    public void RoundTrip_HexToName_ForNamedRgb()
    {
        var fromHex = Color.Parse("#FFD700");
        Assert.Equal("gold", fromHex.NameEnglish);
        Assert.Equal("guld", fromHex.NameSwedish);

        Assert.Equal("gold", Color.Format("#FFD700"));
    }

    [Fact]
    public void ToString_And_ToNormalizedString()
    {
        var named = Color.Parse("olive");
        Assert.Equal("olive", named.ToString());
        Assert.Equal("#808000", named.ToNormalizedString());

        var custom = Color.Parse("#102030");
        Assert.Equal("#102030", custom.ToString());
        Assert.Equal("#102030", custom.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_ReturnsPlaceholder()
    {
        var c = Color.Parse("navy");
        Assert.Equal("#******", c.ToMaskedString());
    }

    [Fact]
    public void Equality_SameColor()
    {
        var a = Color.Parse("#FF0000");
        var b = Color.Parse("red");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentColors()
    {
        var a = Color.Parse("#FF0000");
        var b = Color.Parse("#0000FF");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Color.Parse("blue");
        var b = Color.Parse("red");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Color.Parse("navy");
        Assert.Equal(1, a.CompareTo(null));
    }
}
