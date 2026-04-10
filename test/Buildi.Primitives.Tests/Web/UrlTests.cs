using Buildi.Primitives.Web;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.Web;

public class UrlTests
{
    // --- IsValid ---

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com")]
    [InlineData("https://www.example.com/path")]
    [InlineData("https://example.com/path?key=value")]
    [InlineData("https://example.com/path#section")]
    [InlineData("ftp://files.example.com/doc.pdf")]
    [InlineData("https://example.com:8080/")]
    [InlineData("https://sub.domain.example.com")]
    [InlineData("example.com")]
    [InlineData("www.example.com")]
    [InlineData("mailto:user@example.com")]
    [InlineData("tel:+46701234567")]
    [InlineData("ssh://github.com/repo.git")]
    [InlineData("git://github.com/repo.git")]
    [InlineData("sms:+46701234567")]
    [InlineData("callto:+46701234567")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Url.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    [InlineData("invalid")]
    [InlineData("just-text")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Url.IsValid(input));
    }

    // --- TryParse ---

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("HTTP://EXAMPLE.COM", "http://example.com/")]
    [InlineData("  https://example.com  ", "https://example.com/")]
    [InlineData("example.com", "https://example.com/")]
    [InlineData("www.example.com/path", "https://www.example.com/path")]
    public void TryParse_ReturnsExpectedNormalizedValue(string input, string expected)
    {
        Assert.True(Url.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Url.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://example.com", "https")]
    [InlineData("http://example.com", "http")]
    [InlineData("ftp://example.com", "ftp")]
    [InlineData("ssh://github.com/repo", "ssh")]
    [InlineData("git://github.com/repo", "git")]
    [InlineData("mailto:user@example.com", "mailto")]
    [InlineData("tel:+1234567890", "tel")]
    [InlineData("sms:+1234567890", "sms")]
    public void TryParse_ExtractsScheme(string input, string expectedScheme)
    {
        Assert.True(Url.TryParse(input, out var result));
        Assert.Equal(expectedScheme, result!.Scheme);
    }

    [Fact]
    public void TryParse_MailtoScheme_ParsesAuthority()
    {
        Assert.True(Url.TryParse("mailto:user@example.com", out var result));
        Assert.Equal("mailto", result!.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal("com", result.TopLevelDomain);
        Assert.True(result.IsExampleDomain);
        Assert.False(result.IsSecure);
    }

    [Theory]
    [InlineData("tel:+46701234567")]
    [InlineData("sms:+46701234567")]
    public void TryParse_NonHierarchical_EmptyHost(string input)
    {
        Assert.True(Url.TryParse(input, out var result));
        Assert.Equal(string.Empty, result!.Host);
        Assert.Equal(string.Empty, result.TopLevelDomain);
        Assert.Null(result.Country);
        Assert.False(result.IsExampleDomain);
        Assert.False(result.IsIpAddress);
        Assert.False(result.IsSecure);
    }

    [Fact]
    public void TryParse_ExtractsHost()
    {
        Assert.True(Url.TryParse("https://www.example.com/path", out var result));
        Assert.Equal("www.example.com", result!.Host);
    }

    [Fact]
    public void TryParse_ExtractsExplicitPort()
    {
        Assert.True(Url.TryParse("https://example.com:8080/", out var result));
        Assert.Equal(8080, result!.Port);
    }

    [Fact]
    public void TryParse_DefaultPort_IsNull()
    {
        Assert.True(Url.TryParse("https://example.com/", out var result));
        Assert.Null(result!.Port);

        Assert.True(Url.TryParse("http://example.com/", out var http));
        Assert.Null(http!.Port);
    }

    [Fact]
    public void TryParse_ExtractsPath()
    {
        Assert.True(Url.TryParse("https://example.com/path/to/page", out var result));
        Assert.Equal("/path/to/page", result!.Path);
    }

    [Fact]
    public void TryParse_ExtractsQuery()
    {
        Assert.True(Url.TryParse("https://example.com/path?key=value&foo=bar", out var result));
        Assert.Equal("key=value&foo=bar", result!.Query);
    }

    [Fact]
    public void TryParse_NoQuery_IsNull()
    {
        Assert.True(Url.TryParse("https://example.com/path", out var result));
        Assert.Null(result!.Query);
    }

    [Fact]
    public void TryParse_ExtractsFragment()
    {
        Assert.True(Url.TryParse("https://example.com/path#section", out var result));
        Assert.Equal("section", result!.Fragment);
    }

    [Fact]
    public void TryParse_NoFragment_IsNull()
    {
        Assert.True(Url.TryParse("https://example.com/path", out var result));
        Assert.Null(result!.Fragment);
    }

    // --- TopLevelDomain & Country ---

    [Fact]
    public void TryParse_ExtractsTopLevelDomain()
    {
        Assert.True(Url.TryParse("https://www.example.se", out var result));
        Assert.Equal("se", result!.TopLevelDomain);
    }

    [Fact]
    public void TryParse_MapsCountryCodeTld()
    {
        Assert.True(Url.TryParse("https://www.example.se", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("SE", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_GenericTld_NoCountry()
    {
        Assert.True(Url.TryParse("https://example.com", out var result));
        Assert.Null(result!.Country);
    }

    [Fact]
    public void TryParse_NorwegianTld()
    {
        Assert.True(Url.TryParse("https://vg.no", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("NO", result.Country!.Alpha2Code);
    }

    // --- IsSecure ---

    [Fact]
    public void IsSecure_TrueForHttps()
    {
        Assert.True(Url.Parse("https://example.com").IsSecure);
    }

    [Fact]
    public void IsSecure_FalseForHttp()
    {
        Assert.False(Url.Parse("http://example.com").IsSecure);
    }

    // --- IsExampleDomain ---

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.org")]
    [InlineData("https://example.net")]
    [InlineData("https://sub.example.com")]
    [InlineData("https://test.test")]
    [InlineData("https://something.invalid")]
    [InlineData("https://app.localhost")]
    public void IsExampleDomain_True(string input)
    {
        Assert.True(Url.Parse(input).IsExampleDomain);
    }

    [Theory]
    [InlineData("https://google.com")]
    [InlineData("https://www.example.se")]
    public void IsExampleDomain_False(string input)
    {
        Assert.False(Url.Parse(input).IsExampleDomain);
    }

    // --- IsIpAddress ---

    [Fact]
    public void IsIpAddress_TrueForIp()
    {
        Assert.True(Url.Parse("http://192.168.1.1").IsIpAddress);
    }

    [Fact]
    public void IsIpAddress_FalseForDomain()
    {
        Assert.False(Url.Parse("https://example.com").IsIpAddress);
    }

    // --- Parse throws ---

    [Theory]
    [InlineData("not a url")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Url.Parse(input));
    }

    // --- Format / Normalize ---

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("HTTP://EXAMPLE.COM/PATH", "http://example.com/PATH")]
    [InlineData(null, null)]
    [InlineData("invalid", null)]
    [InlineData("not-a-url", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Url.Format(input));
    }

    [Fact]
    public void Format_FallbackReturnsInput()
    {
        Assert.Equal("not a url", Url.Format("not a url", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("https://example.com/", true)]
    [InlineData("HTTP://EXAMPLE.COM", false)]
    [InlineData("example.com", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, Url.IsNormalized(input));
    }

    // --- ToString ---

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    public void ToString_ReturnsNormalizedValue(string input, string expected)
    {
        Assert.Equal(expected, Url.Parse(input).ToString());
    }

    // --- Auto-prefix ---

    [Fact]
    public void TryParse_AddsHttpsWhenNoScheme()
    {
        Assert.True(Url.TryParse("example.com", out var result));
        Assert.Equal("https", result!.Scheme);
    }

    [Fact]
    public void TryParse_DomainWithPort_AutoPrefixes()
    {
        Assert.True(Url.TryParse("example.com:8080", out var result));
        Assert.Equal("https", result!.Scheme);
        Assert.Equal("example.com", result.Host);
        Assert.Equal(8080, result.Port);
    }

    // --- Hierarchical non-HTTP schemes ---

    [Fact]
    public void TryParse_SshScheme_ExtractsHost()
    {
        Assert.True(Url.TryParse("ssh://github.com/repo.git", out var result));
        Assert.Equal("ssh", result!.Scheme);
        Assert.Equal("github.com", result.Host);
        Assert.Equal("/repo.git", result.Path);
        Assert.Equal("com", result.TopLevelDomain);
    }

    // --- Masking ---

    [Fact]
    public void ToMaskedString_MasksPathAndQuery()
    {
        var url = Url.Parse("https://www.example.com/secret/page?key=val#top");
        var masked = url.ToMaskedString();
        Assert.Equal("https://www.example.com/***?***#***", masked);
    }

    [Fact]
    public void ToMaskedString_RootPath_Unchanged()
    {
        var url = Url.Parse("https://www.example.com");
        var masked = url.ToMaskedString();
        Assert.Equal("https://www.example.com/", masked);
    }

    [Fact]
    public void ToMaskedString_PreservesPort()
    {
        var url = Url.Parse("https://example.com:8080/path");
        var masked = url.ToMaskedString();
        Assert.Equal("https://example.com:8080/***", masked);
    }

    [Theory]
    [InlineData("mailto:user@example.com", "mailto:***")]
    [InlineData("tel:+46701234567", "tel:***")]
    [InlineData("sms:+46701234567", "sms:***")]
    public void ToMaskedString_NonHierarchical_ShowsSchemeOnly(string input, string expected)
    {
        var url = Url.Parse(input);
        Assert.Equal(expected, url.ToMaskedString());
    }

    // --- Scanning ---

    [Fact]
    public void FindCandidatesInText_FindsHttpsUrl()
    {
        var results = Url.FindCandidatesInText("Besök oss på https://www.example.com/info tack!");
        Assert.Single(results);
        Assert.Equal(TextMatchConfidence.High, results[0].Confidence);
        Assert.Equal("https://www.example.com/info", results[0].Value.Value);
    }

    [Fact]
    public void FindCandidatesInText_FindsWwwUrl()
    {
        var results = Url.FindCandidatesInText("Se www.example.com för mer info.");
        Assert.Single(results);
        Assert.Equal("www.example.com", results[0].OriginalText);
    }

    [Fact]
    public void FindCandidatesInText_FindsMultiple()
    {
        var results = Url.FindCandidatesInText("Kolla https://a.com och https://b.com");
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void FindCandidatesInText_TrimsTrailingPunctuation()
    {
        var results = Url.FindCandidatesInText("Besök https://example.com/page.");
        Assert.Single(results);
        Assert.Equal("https://example.com/page", results[0].OriginalText);
    }

    [Fact]
    public void FindCandidatesInText_PreservesBalancedParens()
    {
        var results = Url.FindCandidatesInText("Se https://en.wikipedia.org/wiki/URL_(disambiguation) för info.");
        Assert.Single(results);
        Assert.Contains("(disambiguation)", results[0].OriginalText);
    }

    [Fact]
    public void FindCandidatesInText_NullInput()
    {
        Assert.Empty(Url.FindCandidatesInText(null!));
    }

    [Fact]
    public void FindCandidatesInText_EmptyInput()
    {
        Assert.Empty(Url.FindCandidatesInText(""));
    }

    [Fact]
    public void FindCandidatesInText_NoUrls()
    {
        Assert.Empty(Url.FindCandidatesInText("Vanlig text utan webbadresser."));
    }

    [Fact]
    public void FindCandidatesInText_FindsMailtoLink()
    {
        var results = Url.FindCandidatesInText("Kontakta oss på mailto:info@example.com tack");
        Assert.Single(results);
        Assert.Equal("mailto", results[0].Value.Scheme);
        Assert.Equal("mailto:***", results[0].MaskedForm);
    }

    [Fact]
    public void FindCandidatesInText_FindsSshUrl()
    {
        var results = Url.FindCandidatesInText("Klona via ssh://git@github.com/repo.git");
        Assert.Single(results);
        Assert.Equal("ssh", results[0].Value.Scheme);
        Assert.Equal("github.com", results[0].Value.Host);
    }

    [Fact]
    public void FindCandidatesInText_FindsTelLink()
    {
        var results = Url.FindCandidatesInText("Ring tel:+46701234567 för support");
        Assert.Single(results);
        Assert.Equal("tel", results[0].Value.Scheme);
    }

    [Fact]
    public void FindCandidatesInText_FindsCustomScheme()
    {
        var results = Url.FindCandidatesInText("Öppna spotify://track/123abc");
        Assert.Single(results);
        Assert.Equal("spotify", results[0].Value.Scheme);
    }

    [Fact]
    public void FindCandidatesInText_PositionsAreCorrect()
    {
        var text = "Prefix https://example.com suffix";
        var results = Url.FindCandidatesInText(text);
        Assert.Single(results);
        var c = results[0];
        Assert.Equal(7, c.StartIndex);
        Assert.Equal("https://example.com", c.OriginalText);
    }

    [Fact]
    public void FindCandidatesInText_HasMaskedForm()
    {
        var results = Url.FindCandidatesInText("Se https://example.com/secret/path");
        Assert.Single(results);
        Assert.Contains("example.com", results[0].MaskedForm);
        Assert.Contains("***", results[0].MaskedForm);
    }

    // --- ToUri / FromUri / implicit conversions ---

    [Fact]
    public void ToUri_ReturnsEquivalentUri()
    {
        var url = Url.Parse("https://example.com/path?q=1#frag");
        Uri uri = url.ToUri();

        Assert.Equal("https", uri.Scheme);
        Assert.Equal("example.com", uri.Host);
        Assert.Equal("/path", uri.AbsolutePath);
        Assert.Equal("?q=1", uri.Query);
        Assert.Equal("#frag", uri.Fragment);
    }

    [Fact]
    public void FromUri_CreatesUrl()
    {
        var uri = new Uri("https://www.example.com/path");
        var url = Url.FromUri(uri);

        Assert.Equal("https://www.example.com/path", url.Value);
        Assert.Equal("www.example.com", url.Host);
    }

    [Fact]
    public void FromUri_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Url.FromUri(null!));
    }

    [Fact]
    public void ImplicitConversion_UrlToUri()
    {
        Url url = Url.Parse("https://example.com/page");
        Uri uri = url;

        Assert.Equal("https://example.com/page", uri.AbsoluteUri);
    }

    [Fact]
    public void ImplicitConversion_UriToUrl()
    {
        Uri uri = new("https://example.com/page");
        Url url = uri;

        Assert.Equal("https://example.com/page", url.Value);
        Assert.Equal("example.com", url.Host);
    }

    [Fact]
    public void ToUri_RoundTrips_WithFromUri()
    {
        var original = Url.Parse("https://www.example.se:8080/path?key=val#top");
        var roundTripped = Url.FromUri(original.ToUri());

        Assert.Equal(original.Value, roundTripped.Value);
        Assert.Equal(original.Host, roundTripped.Host);
        Assert.Equal(original.Port, roundTripped.Port);
        Assert.Equal(original.Query, roundTripped.Query);
        Assert.Equal(original.Fragment, roundTripped.Fragment);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Url.Parse("https://example.com");
        var b = Url.Parse("https://example.com");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Url.Parse("https://example.com");
        var b = Url.Parse("https://www.example.com/path");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Url.Parse("https://example.com");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Url.Parse("https://example.com");
        var b = Url.Parse("https://google.com");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Url.Parse("https://example.com");
        Assert.Equal(1, a.CompareTo(null));
    }
}
