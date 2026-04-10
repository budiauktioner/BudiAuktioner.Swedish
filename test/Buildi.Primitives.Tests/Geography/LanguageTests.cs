using System.Globalization;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

[Collection("CultureSensitive")]
public class LanguageTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();

    [Theory]
    [InlineData("sv")]
    [InlineData("SV")]
    [InlineData("en")]
    [InlineData("de")]
    [InlineData("ar")]
    [InlineData("zh")]
    [InlineData("swe")]
    [InlineData("eng")]
    [InlineData("deu")]
    [InlineData("Swedish")]
    [InlineData("swedish")]
    [InlineData("English")]
    [InlineData("German")]
    [InlineData("Svenska")]
    [InlineData("Tyska")]
    [InlineData("Deutsch")]
    [InlineData("Français")]
    [InlineData(" sv ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Language.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("xx")]
    [InlineData("xyz")]
    [InlineData("Klingon")]
    [InlineData("abcdefghij")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Language.IsValid(input));
    }

    [Theory]
    [InlineData("sv", "sv", "swe", "Swedish", "Svenska", "Svenska")]
    [InlineData("en", "en", "eng", "English", "Engelska", "English")]
    [InlineData("de", "de", "deu", "German", "Tyska", "Deutsch")]
    [InlineData("fr", "fr", "fra", "French", "Franska", "Français")]
    [InlineData("ar", "ar", "ara", "Arabic", "Arabiska", "العربية")]
    [InlineData("zh", "zh", "zho", "Chinese", "Kinesiska", "中文")]
    [InlineData("ja", "ja", "jpn", "Japanese", "Japanska", "日本語")]
    [InlineData("fi", "fi", "fin", "Finnish", "Finska", "Suomi")]
    [InlineData("da", "da", "dan", "Danish", "Danska", "Dansk")]
    [InlineData("no", "no", "nor", "Norwegian", "Norska", "Norsk")]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, string expectedAlpha2, string expectedAlpha3,
        string expectedEnglish, string expectedLocalized, string expectedNative)
    {
        var ok = Language.TryParse(input, out var result);

        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedAlpha2, result!.Alpha2Code);
        Assert.Equal(expectedAlpha3, result.Alpha3Code);
        Assert.Equal(expectedEnglish, result.EnglishName);
        Assert.Equal(expectedLocalized, result.LocalizedName);
        Assert.Equal(expectedNative, result.NativeName);
    }

    [Theory]
    [InlineData("swe", "sv")]
    [InlineData("deu", "de")]
    [InlineData("fra", "fr")]
    [InlineData("eng", "en")]
    [InlineData("jpn", "ja")]
    public void TryParse_FromAlpha3_ResolvesToAlpha2(string alpha3Input, string expectedAlpha2)
    {
        var ok = Language.TryParse(alpha3Input, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAlpha2, result!.Alpha2Code);
    }

    [Theory]
    [InlineData("Swedish", "sv")]
    [InlineData("Tyska", "de")]
    [InlineData("Deutsch", "de")]
    [InlineData("Français", "fr")]
    [InlineData("日本語", "ja")]
    [InlineData("中文", "zh")]
    [InlineData("العربية", "ar")]
    public void TryParse_FromName_ResolvesToAlpha2(string name, string expectedAlpha2)
    {
        var ok = Language.TryParse(name, out var result);

        Assert.True(ok);
        Assert.Equal(expectedAlpha2, result!.Alpha2Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("xx")]
    [InlineData("Klingon")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Language.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("xx")]
    [InlineData("Klingon")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Language.Parse(input));
    }

    [Theory]
    [InlineData("sv", "sv")]
    [InlineData("SV", "sv")]
    [InlineData("swe", "sv")]
    [InlineData("Swedish", "sv")]
    [InlineData("Svenska", "sv")]
    [InlineData("de", "de")]
    [InlineData("Deutsch", "de")]
    [InlineData(" sv ", "sv")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("xx", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Language.Normalize(input));
    }

    [Theory]
    [InlineData("xx", "xx")]
    [InlineData("Klingon", "Klingon")]
    [InlineData(" bogus ", "bogus")]
    public void Normalize_WithFallback_ReturnsTrimmedInput(string input, string expected)
    {
        Assert.Equal(expected, Language.Normalize(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("sv", true)]
    [InlineData("en", true)]
    [InlineData("de", true)]
    [InlineData("SV", false)]
    [InlineData("Swedish", false)]
    [InlineData("swe", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, Language.IsNormalized(input));
    }

    [Fact]
    public void Format_ReturnsEnglishName_WhenEnglishCulture()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");

        Assert.Equal("Swedish", Language.Format("sv"));
        Assert.Equal("German", Language.Format("de"));
        Assert.Equal("Arabic", Language.Format("ar"));
    }

    [Fact]
    public void Format_ReturnsLocalizedName_WhenSwedishCulture()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        Assert.Equal("Svenska", Language.Format("sv"));
        Assert.Equal("Tyska", Language.Format("de"));
        Assert.Equal("Arabiska", Language.Format("ar"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("xx", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, Language.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput()
    {
        Assert.Equal("Klingon", Language.Format("Klingon", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        Assert.Equal("Svenska", Language.Swedish.ToString());

        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("Swedish", Language.Swedish.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsAlpha2Code()
    {
        Assert.Equal("sv", Language.Swedish.ToNormalizedString());
        Assert.Equal("en", Language.English.ToNormalizedString());
        Assert.Equal("de", Language.German.ToNormalizedString());
    }

    [Fact]
    public void ToDisplayString_FollowsUICulture()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        Assert.Equal("Tyska", Language.German.ToDisplayString());

        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("German", Language.German.ToDisplayString());
    }

    [Fact]
    public void ToEnglishString_ReturnsEnglishName()
    {
        Assert.Equal("Swedish", Language.Swedish.ToEnglishString());
        Assert.Equal("German", Language.German.ToEnglishString());
        Assert.Equal("Arabic", Language.Arabic.ToEnglishString());
    }

    [Fact]
    public void ToNativeString_ReturnsNativeName()
    {
        Assert.Equal("Svenska", Language.Swedish.ToNativeString());
        Assert.Equal("Deutsch", Language.German.ToNativeString());
        Assert.Equal("العربية", Language.Arabic.ToNativeString());
        Assert.Equal("日本語", Language.Japanese.ToNativeString());
        Assert.Equal("中文", Language.Chinese.ToNativeString());
    }

    [Theory]
    [InlineData("ar", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("he", WritingScript.Hebrew, TextDirection.RightToLeft)]
    [InlineData("fa", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("ur", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("yi", WritingScript.Hebrew, TextDirection.RightToLeft)]
    [InlineData("ps", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("sd", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("ug", WritingScript.Arabic, TextDirection.RightToLeft)]
    [InlineData("dv", WritingScript.Thaana, TextDirection.RightToLeft)]
    public void RtlLanguages_HaveCorrectDirectionAndScript(string alpha2, WritingScript expectedScript, TextDirection expectedDir)
    {
        var lang = Language.Parse(alpha2);

        Assert.Equal(expectedScript, lang.Script);
        Assert.Equal(expectedDir, lang.Direction);
    }

    [Theory]
    [InlineData("sv", WritingScript.Latin)]
    [InlineData("en", WritingScript.Latin)]
    [InlineData("de", WritingScript.Latin)]
    [InlineData("ru", WritingScript.Cyrillic)]
    [InlineData("el", WritingScript.Greek)]
    [InlineData("ja", WritingScript.Han)]
    [InlineData("ko", WritingScript.Hangul)]
    [InlineData("th", WritingScript.Thai)]
    [InlineData("ka", WritingScript.Georgian)]
    [InlineData("hy", WritingScript.Armenian)]
    [InlineData("hi", WritingScript.Devanagari)]
    [InlineData("bn", WritingScript.Bengali)]
    [InlineData("ta", WritingScript.Tamil)]
    [InlineData("te", WritingScript.Telugu)]
    [InlineData("kn", WritingScript.Kannada)]
    [InlineData("ml", WritingScript.Malayalam)]
    [InlineData("si", WritingScript.Sinhala)]
    [InlineData("km", WritingScript.Khmer)]
    [InlineData("lo", WritingScript.Lao)]
    [InlineData("my", WritingScript.Myanmar)]
    [InlineData("am", WritingScript.Ethiopic)]
    [InlineData("bo", WritingScript.Tibetan)]
    [InlineData("gu", WritingScript.Gujarati)]
    [InlineData("pa", WritingScript.Gurmukhi)]
    public void LtrLanguages_HaveCorrectScript(string alpha2, WritingScript expectedScript)
    {
        var lang = Language.Parse(alpha2);

        Assert.Equal(expectedScript, lang.Script);
        Assert.Equal(TextDirection.LeftToRight, lang.Direction);
    }

    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.True(Language.All.Count >= 180);
    }

    [Fact]
    public void All_HasUniqueAlpha2Codes()
    {
        var codes = Language.All.Select(l => l.Alpha2Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void StaticProperties_ReturnExpectedLanguages()
    {
        Assert.Equal("sv", Language.Swedish.Alpha2Code);
        Assert.Equal("en", Language.English.Alpha2Code);
        Assert.Equal("de", Language.German.Alpha2Code);
        Assert.Equal("fr", Language.French.Alpha2Code);
        Assert.Equal("es", Language.Spanish.Alpha2Code);
        Assert.Equal("no", Language.Norwegian.Alpha2Code);
        Assert.Equal("da", Language.Danish.Alpha2Code);
        Assert.Equal("fi", Language.Finnish.Alpha2Code);
        Assert.Equal("is", Language.Icelandic.Alpha2Code);
        Assert.Equal("nl", Language.Dutch.Alpha2Code);
        Assert.Equal("it", Language.Italian.Alpha2Code);
        Assert.Equal("pt", Language.Portuguese.Alpha2Code);
        Assert.Equal("pl", Language.Polish.Alpha2Code);
        Assert.Equal("ar", Language.Arabic.Alpha2Code);
        Assert.Equal("zh", Language.Chinese.Alpha2Code);
        Assert.Equal("ja", Language.Japanese.Alpha2Code);
        Assert.Equal("ko", Language.Korean.Alpha2Code);
        Assert.Equal("ru", Language.Russian.Alpha2Code);
        Assert.Equal("hi", Language.Hindi.Alpha2Code);
        Assert.Equal("tr", Language.Turkish.Alpha2Code);
    }

    [Fact]
    public void Equality_SameLanguage_AreEqual()
    {
        var a = Language.Parse("sv");
        var b = Language.Parse("Swedish");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equality_DifferentLanguages_AreNotEqual()
    {
        Assert.NotEqual(Language.Swedish, Language.English);
        Assert.True(Language.Swedish != Language.English);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        Assert.True(Language.Danish.CompareTo(Language.Swedish) < 0);
        Assert.True(Language.Swedish.CompareTo(Language.Danish) > 0);
        Assert.Equal(0, Language.Swedish.CompareTo(Language.Swedish));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Assert.Equal(1, Language.Swedish.CompareTo(null));
    }

    [Fact]
    public void Value_ReturnsAlpha2Code()
    {
        Assert.Equal("sv", Language.Swedish.Value);
        Assert.Equal("en", Language.English.Value);
    }

    [Theory]
    [InlineData("ks", TextDirection.RightToLeft)]
    public void Kashmiri_IsRtl(string alpha2, TextDirection expectedDir)
    {
        var lang = Language.Parse(alpha2);
        Assert.Equal(expectedDir, lang.Direction);
    }
}
