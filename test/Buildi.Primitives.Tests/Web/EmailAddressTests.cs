using Buildi.Primitives.Web;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Web;

public class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("User@Example.COM", "user@example.com")]
    [InlineData("  user@example.com  ", "user@example.com")]
    [InlineData("user+tag@example.com", "user+tag@example.com")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(EmailAddress.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_ExtractsLocalPartAndDomain()
    {
        Assert.True(EmailAddress.TryParse("user@example.com", out var result));
        Assert.Equal("user", result!.LocalPart);
        Assert.Equal("example.com", result.Domain);
    }

    [Fact]
    public void TryParse_ExtractsTopLevelDomain()
    {
        Assert.True(EmailAddress.TryParse("user@example.com", out var result));
        Assert.Equal("com", result!.TopLevelDomain);
    }

    [Fact]
    public void TryParse_MapsCountryCodeTld()
    {
        Assert.True(EmailAddress.TryParse("user@company.se", out var result));
        Assert.Equal("se", result!.TopLevelDomain);
        Assert.NotNull(result.Country);
        Assert.Equal("Sweden", result.Country!.EnglishName);
    }

    [Fact]
    public void TryParse_GenericTld_ReturnsNullCountry()
    {
        Assert.True(EmailAddress.TryParse("user@example.com", out var result));
        Assert.Null(result!.Country);
    }

    [Theory]
    [InlineData("user@gmail.com", PublicEmailProvider.Gmail)]
    [InlineData("user@hotmail.com", PublicEmailProvider.Outlook)]
    [InlineData("user@yahoo.com", PublicEmailProvider.Yahoo)]
    [InlineData("user@telia.com", PublicEmailProvider.Telia)]
    public void TryParse_DetectsPublicEmailProvider(string input, PublicEmailProvider expectedProvider)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expectedProvider, result!.Provider);
        Assert.True(result.IsPublicEmailProvider);
    }

    [Fact]
    public void TryParse_UnknownDomain_ReturnsNullProvider()
    {
        Assert.True(EmailAddress.TryParse("user@company.example", out var result));
        Assert.Null(result!.Provider);
        Assert.False(result.IsPublicEmailProvider);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("notanemail", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, EmailAddress.IsValid(input));
    }

    [Theory]
    [InlineData("User@Example.COM", "user@example.com")]
    [InlineData("notanemail", "notanemail")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EmailAddress.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !EmailAddress.IsValid(input)));
    }

    [Theory]
    [InlineData("User@Example.COM", "user@example.com")]
    [InlineData("notanemail", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EmailAddress.Normalize(input));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => EmailAddress.Parse("notanemail"));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var email = EmailAddress.Parse("user@example.com");
        Assert.Equal("user@example.com", email.ToString());
        Assert.Equal("user@example.com", email.ToNormalizedString());
    }

    [Theory]
    [InlineData("user@wp.pl", PublicEmailProvider.WirtualnaPolska)]
    [InlineData("user@onet.pl", PublicEmailProvider.Onet)]
    [InlineData("user@o2.pl", PublicEmailProvider.Onet)]
    [InlineData("user@interia.pl", PublicEmailProvider.Interia)]
    [InlineData("user@web.de", PublicEmailProvider.WebDe)]
    [InlineData("user@mail.ru", PublicEmailProvider.MailRu)]
    [InlineData("user@abv.bg", PublicEmailProvider.Abv)]
    [InlineData("user@inbox.lv", PublicEmailProvider.InboxLv)]
    [InlineData("user@seznam.cz", PublicEmailProvider.Seznam)]
    [InlineData("user@libero.it", PublicEmailProvider.Libero)]
    [InlineData("user@naver.com", PublicEmailProvider.Naver)]
    [InlineData("user@freemail.hu", PublicEmailProvider.FreemailHu)]
    [InlineData("user@ukr.net", PublicEmailProvider.UkrNet)]
    [InlineData("user@tuta.io", PublicEmailProvider.Tuta)]
    [InlineData("user@tutanota.com", PublicEmailProvider.Tuta)]
    [InlineData("user@hey.com", PublicEmailProvider.Hey)]
    [InlineData("user@duck.com", PublicEmailProvider.DuckDuckGoEmail)]
    [InlineData("user@simplelogin.com", PublicEmailProvider.SimpleLogin)]
    [InlineData("user@bredband2.com", PublicEmailProvider.Bredband2)]
    [InlineData("user@ownit.nu", PublicEmailProvider.Ownit)]
    [InlineData("user@online.no", PublicEmailProvider.OnlineNo)]
    [InlineData("user@kolumbus.fi", PublicEmailProvider.Kolumbus)]
    [InlineData("user@aland.net", PublicEmailProvider.Aland)]
    [InlineData("user@rocketmail.com", PublicEmailProvider.Yahoo)]
    [InlineData("user@windowslive.com", PublicEmailProvider.Outlook)]
    [InlineData("user@protonmail.ch", PublicEmailProvider.ProtonMail)]
    [InlineData("user@yahoo.it", PublicEmailProvider.Yahoo)]
    [InlineData("user@hotmail.it", PublicEmailProvider.Outlook)]
    [InlineData("user@live.fr", PublicEmailProvider.Outlook)]
    [InlineData("user@outlook.se", PublicEmailProvider.Outlook)]
    public void TryParse_DetectsNewPublicEmailProviders(string input, PublicEmailProvider expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Provider);
        Assert.True(result.IsPublicEmailProvider);
    }

    [Theory]
    [InlineData("user@gnail.com", "gmail.com")]
    [InlineData("user@gamil.com", "gmail.com")]
    [InlineData("user@gmai.com", "gmail.com")]
    [InlineData("user@gmil.com", "gmail.com")]
    [InlineData("user@gmsil.com", "gmail.com")]
    [InlineData("user@gmal.com", "gmail.com")]
    [InlineData("user@gmaill.com", "gmail.com")]
    [InlineData("user@gamail.com", "gmail.com")]
    [InlineData("user@gmail.con", "gmail.com")]
    [InlineData("user@gmail.vom", "gmail.com")]
    [InlineData("user@gmail.cim", "gmail.com")]
    [InlineData("user@gmail.comi", "gmail.com")]
    [InlineData("user@gmail.se", "gmail.com")]
    [InlineData("user@gmail.co", "gmail.com")]
    public void TryParse_WithTypoCorrection_FixesGmailMisspellings(string input, string expectedDomain)
    {
        Assert.True(EmailAddress.TryParse(input, tryCorrectTypos: true, out var result));
        Assert.Equal(expectedDomain, result!.Domain);
        Assert.True(result.WasCorrected);
        Assert.Equal(PublicEmailProvider.Gmail, result.Provider);
    }

    [Theory]
    [InlineData("user@hotmai.com", "hotmail.com")]
    [InlineData("user@hotmil.com", "hotmail.com")]
    [InlineData("user@hotmsil.com", "hotmail.com")]
    [InlineData("user@hotmal.com", "hotmail.com")]
    [InlineData("user@homail.com", "hotmail.com")]
    [InlineData("user@hotnail.com", "hotmail.com")]
    [InlineData("user@hitmail.com", "hotmail.com")]
    [InlineData("user@hptmail.com", "hotmail.com")]
    [InlineData("user@hmail.com", "hotmail.com")]
    [InlineData("user@hotmail.con", "hotmail.com")]
    [InlineData("user@hotmail.cpm", "hotmail.com")]
    [InlineData("user@hotmail.co", "hotmail.com")]
    [InlineData("user@hotmail.vom", "hotmail.com")]
    [InlineData("user@hotmail.cim", "hotmail.com")]
    [InlineData("user@hotmail.comm", "hotmail.com")]
    [InlineData("user@outlook.con", "outlook.com")]
    [InlineData("user@putlook.com", "outlook.com")]
    public void TryParse_WithTypoCorrection_FixesHotmailMisspellings(string input, string expectedDomain)
    {
        Assert.True(EmailAddress.TryParse(input, tryCorrectTypos: true, out var result));
        Assert.Equal(expectedDomain, result!.Domain);
        Assert.True(result.WasCorrected);
        Assert.Equal(PublicEmailProvider.Outlook, result.Provider);
    }

    [Theory]
    [InlineData("user@icloude.com", "icloud.com")]
    [InlineData("user@icloud.se", "icloud.com")]
    [InlineData("user@icloud.co", "icloud.com")]
    public void TryParse_WithTypoCorrection_FixesICloudMisspellings(string input, string expectedDomain)
    {
        Assert.True(EmailAddress.TryParse(input, tryCorrectTypos: true, out var result));
        Assert.Equal(expectedDomain, result!.Domain);
        Assert.True(result.WasCorrected);
        Assert.Equal(PublicEmailProvider.ICloud, result.Provider);
    }

    [Theory]
    [InlineData("user@02.pl", "o2.pl")]
    public void TryParse_WithTypoCorrection_FixesOnetMisspellings(string input, string expectedDomain)
    {
        Assert.True(EmailAddress.TryParse(input, tryCorrectTypos: true, out var result));
        Assert.Equal(expectedDomain, result!.Domain);
        Assert.True(result.WasCorrected);
        Assert.Equal(PublicEmailProvider.Onet, result.Provider);
    }

    [Fact]
    public void TryParse_WithTypoCorrection_PreservesOriginalDomain()
    {
        Assert.True(EmailAddress.TryParse("user@gnail.com", tryCorrectTypos: true, out var result));
        Assert.Equal("gnail.com", result!.OriginalDomain);
        Assert.Equal("gmail.com", result.Domain);
        Assert.Equal("user@gmail.com", result.Value);
    }

    [Fact]
    public void TryParse_WithTypoCorrection_ValidEmailIsNotCorrected()
    {
        Assert.True(EmailAddress.TryParse("user@gmail.com", tryCorrectTypos: true, out var result));
        Assert.False(result!.WasCorrected);
        Assert.Null(result.OriginalDomain);
    }

    [Fact]
    public void TryParse_WithoutTypoCorrection_DoesNotCorrect()
    {
        Assert.True(EmailAddress.TryParse("user@gnail.com", out var result));
        Assert.Equal("gnail.com", result!.Domain);
        Assert.False(result.WasCorrected);
        Assert.Null(result.OriginalDomain);
    }

    [Fact]
    public void Format_WithTypoCorrection_ReturnsCorrectedEmail()
    {
        Assert.Equal("user@gmail.com", EmailAddress.Format("user@gnail.com", tryCorrectTypos: true));
    }

    [Fact]
    public void Normalize_WithTypoCorrection_ReturnsCorrectedEmail()
    {
        Assert.Equal("user@gmail.com", EmailAddress.Normalize("user@gmail.con", tryCorrectTypos: true));
    }

    [Theory]
    [InlineData("mailto:user@example.com", "user@example.com")]
    [InlineData("MAILTO:user@example.com", "user@example.com")]
    [InlineData("mailto: user@example.com", "user@example.com")]
    [InlineData("mailto:oresundsplattsattning@gmail.com", "oresundsplattsattning@gmail.com")]
    public void TryParse_StripsMailtoPrefix(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("John Doe <john@example.com>", "john@example.com")]
    [InlineData("\"John Doe\" <john@example.com>", "john@example.com")]
    [InlineData("<john@example.com>", "john@example.com")]
    [InlineData("John Doe <JOHN@Example.COM>", "john@example.com")]
    [InlineData("John <john@example.com> ", "john@example.com")]
    public void TryParse_ExtractsEmailFromDisplayNameFormat(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("<mailto:john@example.com>", "john@example.com")]
    [InlineData("<mailto:john@example.com?subject=Hello>", "john@example.com")]
    public void TryParse_HandlesMailtoInsideAngleBrackets(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("mailto:user@example.com?subject=Hello", "user@example.com")]
    [InlineData("mailto:user@example.com?subject=Hello&body=World", "user@example.com")]
    [InlineData("mailto:user@example.com?subject=Hello%20World&cc=other@example.com", "user@example.com")]
    [InlineData("mailto:user@example.com?body=Check%20this%20out", "user@example.com")]
    [InlineData("MAILTO:User@Example.COM?Subject=Test", "user@example.com")]
    public void TryParse_StripsMailtoQueryString(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("user@example.com.", "user@example.com")]
    [InlineData("katarina.lofberg@enkoping.se.", "katarina.lofberg@enkoping.se")]
    [InlineData("tonylindqvist206@hotmail.com.", "tonylindqvist206@hotmail.com")]
    public void TryParse_StripsTrailingDot(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("user@example.com\u200B", "user@example.com")]
    [InlineData("\u200Buser@example.com", "user@example.com")]
    [InlineData("user\u200B@example.com", "user@example.com")]
    [InlineData("user@example\u200B.com", "user@example.com")]
    public void TryParse_StripsInvisibleCharacters(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("bamfo @hotmail.com", "bamfo@hotmail.com")]
    [InlineData("user @example.com", "user@example.com")]
    public void TryParse_RemovesInternalWhitespace(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("_anton@live.se", "_anton@live.se")]
    [InlineData("_user@example.com", "_user@example.com")]
    public void TryParse_AllowsLeadingUnderscore(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("stefan@högquist.se", "stefan@högquist.se")]
    [InlineData("micke@lindströmsskylt.se", "micke@lindströmsskylt.se")]
    [InlineData("info@flinksbetonghåltagning.se", "info@flinksbetonghåltagning.se")]
    [InlineData("jo@ohöinplattsattning.se", "jo@ohöinplattsattning.se")]
    public void TryParse_HandlesIdnDomains(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var result));
        Assert.Equal(expected, result!.Value);
    }

    [Fact]
    public void TryParse_IdnDomain_ExtractsCorrectProperties()
    {
        Assert.True(EmailAddress.TryParse("stefan@högquist.se", out var result));
        Assert.Equal("stefan", result!.LocalPart);
        Assert.Equal("högquist.se", result.Domain);
        Assert.Equal("se", result.TopLevelDomain);
        Assert.NotNull(result.Country);
        Assert.Equal("Sweden", result.Country!.EnglishName);
        Assert.True(result.IsInternationalizedDomain);
        Assert.NotEqual(result.Domain, result.DomainAscii);
        Assert.StartsWith("xn--", result.DomainAscii);
        Assert.EndsWith(".se", result.DomainAscii);
    }

    [Fact]
    public void TryParse_AsciiDomain_IsNotInternationalized()
    {
        Assert.True(EmailAddress.TryParse("user@example.com", out var result));
        Assert.False(result!.IsInternationalizedDomain);
        Assert.Equal("example.com", result.DomainAscii);
        Assert.Equal(result.Domain, result.DomainAscii);
    }

    [Fact]
    public void ToAsciiString_ReturnsValueWithPunycodeDomain()
    {
        Assert.True(EmailAddress.TryParse("user@örneholm.com", out var result));
        Assert.Equal("user@örneholm.com", result!.ToString());
        Assert.Equal("user@örneholm.com", result.ToNormalizedString());
        Assert.StartsWith("user@xn--", result.ToAsciiString());
        Assert.EndsWith(".com", result.ToAsciiString());
    }

    [Fact]
    public void ToAsciiString_ReturnsSameAsToString_ForAsciiDomains()
    {
        Assert.True(EmailAddress.TryParse("user@example.com", out var result));
        Assert.Equal(result!.ToString(), result.ToAsciiString());
    }

    [Theory]
    [InlineData("olles.@oktv.se")]
    [InlineData("fotografspider.@gmail.com")]
    [InlineData("rabei.taha.altaha.@gmail.com")]
    [InlineData("hlena..ua@gmail.com")]
    [InlineData("diana..persson@icloud.com")]
    [InlineData("biljana.celar@biljana.celar@hotmail.se")]
    [InlineData("magnus.lefvert@@lwab.se")]
    [InlineData("rooftop@gambia@gmail.com")]
    [InlineData("kenneth.markl####und@gmail.com")]
    [InlineData("david@alveskars.e")]
    [InlineData("leifgustafsson49@gmail.co.n")]
    [InlineData("lars-erik@zleep.se6")]
    [InlineData("mariasjodin(@gmail.com")]
    [InlineData("oldgus56@)gmail.com")]
    [InlineData("albert@£take1.se")]
    [InlineData("jennie.z@mac.")]
    [InlineData("10366")]
    [InlineData("moayad")]
    [InlineData("rasmus")]
    [InlineData("karolina")]
    [InlineData("mikaela")]
    [InlineData("eva74eva")]
    [InlineData("benedetto santoro")]
    [InlineData("mattias bratt")]
    [InlineData("christopher eriksson")]
    [InlineData("pdabyggab")]
    public void TryParse_RemainsInvalid_ForGenuinelyBadInputs(string input)
    {
        Assert.False(EmailAddress.TryParse(input, out _));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = EmailAddress.Parse("user@example.com");
        var b = EmailAddress.Parse("user@example.com");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = EmailAddress.Parse("user@example.com");
        var b = EmailAddress.Parse("user+tag@example.com");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = EmailAddress.Parse("user@example.com");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EmailAddress.Parse("user+tag@example.com");
        var b = EmailAddress.Parse("user@example.com");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EmailAddress.Parse("user@example.com");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("user", false, ValidationErrorReason.MissingAtSign)]
    [InlineData("user@", false, ValidationErrorReason.MissingAtSign)]
    [InlineData("@example.com", false, ValidationErrorReason.MissingAtSign)]
    [InlineData("user@.com", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("user@example.com", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = EmailAddress.Validate(input);

        Assert.Equal(input, result.RawInput);
        Assert.Equal(expectedIsValid, result.IsValid);

        if (expectedReason is not null)
        {
            Assert.Single(result.Issues);
            Assert.Equal(expectedReason.Value, result.Issues[0].Reason);
        }
        else
        {
            Assert.Empty(result.Issues);
        }
    }

    [Fact]
    public void Validate_Issues_ContainBothLanguageDescriptions()
    {
        var result = EmailAddress.Validate("user");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@.com")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(EmailAddress.IsValid(input), EmailAddress.Validate(input).IsValid);
    }
}
