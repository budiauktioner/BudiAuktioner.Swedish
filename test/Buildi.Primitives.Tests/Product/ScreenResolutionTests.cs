using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ScreenResolutionTests
{
    [Theory]
    [InlineData("1920x1080", 1920, 1080, "Full HD", "16:9")]
    [InlineData("1920 x 1080", 1920, 1080, "Full HD", "16:9")]
    [InlineData("1920X1080", 1920, 1080, "Full HD", "16:9")]
    [InlineData("1280x720", 1280, 720, "HD", "16:9")]
    [InlineData("1366x768", 1366, 768, "HD", "683:384")]
    [InlineData("3440x1440", 3440, 1440, "UWQHD", "43:18")]
    public void TryParse_WxH_ReturnsExpected(string input, int w, int h, string? name, string aspect)
    {
        var ok = ScreenResolution.TryParse(input, out var r);

        Assert.True(ok);
        Assert.NotNull(r);
        Assert.Equal(w, r!.Width);
        Assert.Equal(h, r.Height);
        Assert.Equal(name, r.Name);
        Assert.Equal(aspect, r.AspectRatio);
        Assert.Equal($"{w}x{h}", r.Value);
    }

    [Theory]
    [InlineData("Full HD", 1920, 1080)]
    [InlineData("FHD", 1920, 1080)]
    [InlineData("1080p", 1920, 1080)]
    [InlineData("4K", 3840, 2160)]
    [InlineData("UHD", 3840, 2160)]
    [InlineData("720p", 1280, 720)]
    [InlineData("HD", 1280, 720)]
    [InlineData("8K", 7680, 4320)]
    [InlineData("WQXGA", 2560, 1600)]
    [InlineData("UWQHD", 3440, 1440)]
    public void TryParse_Named_ReturnsExpectedPixels(string input, int w, int h)
    {
        var ok = ScreenResolution.TryParse(input, out var r);

        Assert.True(ok);
        Assert.Equal(w, r!.Width);
        Assert.Equal(h, r.Height);
    }

    [Fact]
    public void TotalPixels_IsWidthTimesHeight()
    {
        var r = ScreenResolution.Parse("1920x1080");

        Assert.Equal(1920L * 1080, r.TotalPixels);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0x1080")]
    [InlineData("-1x720")]
    [InlineData("not a resolution")]
    public void TryParse_ReturnsNull_ForInvalid(string? input)
    {
        var ok = ScreenResolution.TryParse(input, out var r);

        Assert.False(ok);
        Assert.Null(r);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => ScreenResolution.Parse("xyz"));
    }

    [Theory]
    [InlineData("1920x1080", "Full HD")]
    [InlineData("800x600", "800 x 600")]
    public void Format_ReturnsNameOrWxH(string input, string expected)
    {
        Assert.Equal(expected, ScreenResolution.Format(input));
    }

    [Theory]
    [InlineData("1920 x 1080", "1920x1080")]
    [InlineData("Full HD", "1920x1080")]
    [InlineData("1080p", "1920x1080")]
    public void Normalize_ReturnsCompactLowercaseX(string input, string expected)
    {
        Assert.Equal(expected, ScreenResolution.Normalize(input));
    }

    [Fact]
    public void IsNormalized_ReturnsTrue_ForCompactForm()
    {
        Assert.True(ScreenResolution.IsNormalized("1920x1080"));
    }

    [Theory]
    [InlineData("1920X1080")]
    [InlineData("1920 x 1080")]
    [InlineData("Full HD")]
    public void IsNormalized_ReturnsFalse_WhenNotCompact(string input)
    {
        Assert.False(ScreenResolution.IsNormalized(input));
    }

    [Fact]
    public void ToString_UsesNameWhenKnown()
    {
        var r = ScreenResolution.Parse("1920x1080");

        Assert.Equal("Full HD", r.ToString());
    }

    [Fact]
    public void ToNormalizedString_MatchesValue()
    {
        var r = ScreenResolution.Parse("1920x1080");

        Assert.Equal("1920x1080", r.ToNormalizedString());
        Assert.Equal(r.Value, r.ToNormalizedString());
    }

    [Fact]
    public void FindCandidatesInText_FindsWxH()
    {
        const string text = "Panel 1920x1080 and also 1280 x 720 here.";
        var c = ScreenResolution.FindCandidatesInText(text);

        Assert.Equal(2, c.Count);
        Assert.Contains(c, x => x.Value.Width == 1920 && x.Value.Height == 1080);
        Assert.Contains(c, x => x.Value.Width == 1280 && x.Value.Height == 720);
    }

    [Fact]
    public void FindCandidatesInText_ReturnsEmpty_ForNullOrEmpty()
    {
        Assert.Empty(ScreenResolution.FindCandidatesInText(""));
        Assert.Empty(ScreenResolution.FindCandidatesInText(null!));
    }

    [Fact]
    public void ToMaskedString_ReturnsPlaceholder()
    {
        var r = ScreenResolution.Parse("3840x2160");

        Assert.Equal("*** x ***", r.ToMaskedString());
    }

    [Fact]
    public void Equality_SameResolution()
    {
        var a = ScreenResolution.Parse("1920x1080");
        var b = ScreenResolution.Parse("Full HD");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_LowerToHigher()
    {
        var hd = ScreenResolution.Parse("720p");
        var uhd = ScreenResolution.Parse("4K");
        Assert.True(hd < uhd);
        Assert.True(uhd > hd);
    }

    [Fact]
    public void Create_FromWidthAndHeight_Works()
    {
        var sr = ScreenResolution.Create(1920, 1080);
        Assert.Equal(1920, sr.Width);
        Assert.Equal(1080, sr.Height);
        Assert.Equal("Full HD", sr.Name);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScreenResolution.Create(0, 1080));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScreenResolution.Create(1920, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScreenResolution.Create(-1, 1080));
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = ScreenResolution.Create(1920, 1080);
        var fromString = ScreenResolution.Parse("1920x1080");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ScreenResolution.Parse("1280x720");
        var b = ScreenResolution.Parse("1920x1080");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ScreenResolution.Parse("1920x1080");
        Assert.Equal(1, a.CompareTo(null));
    }
}
