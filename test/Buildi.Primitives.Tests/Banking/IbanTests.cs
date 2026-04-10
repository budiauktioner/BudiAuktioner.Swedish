using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class IbanTests
{
    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00")]
    [InlineData("nl91abna0417164300")]
    [InlineData("DE89 3704 0044 0532 0130 00")]
    [InlineData("GB82 WEST 1234 5698 7654 32")]
    [InlineData("FR14 2004 1010 0505 0001 3M02 606")]
    [InlineData("SE45 5000 0000 0583 9825 7466")]
    [InlineData("IT60 X054 2811 1010 0000 0123 456")]
    [InlineData("AL47 2121 1009 0000 0002 3569 8741")]
    [InlineData("AD12 0001 2030 2003 5910 0100")]
    [InlineData("AT61 1904 3002 3457 3201")]
    [InlineData("BH67 BMAG 0000 1299 1234 56")]
    [InlineData("BE68 5390 0754 7034")]
    [InlineData("BA39 1290 0794 0102 8494")]
    [InlineData("BG80 BNBG 9661 1020 3456 78")]
    [InlineData("HR12 1001 0051 8630 0016 0")]
    [InlineData("CZ65 0800 0000 1920 0014 5399")]
    [InlineData("DK50 0040 0440 1162 43")]
    [InlineData("EE38 2200 2210 2014 5685")]
    [InlineData("ES91 2100 0418 4502 0005 1332")]
    [InlineData("FI21 1234 5600 0007 85")]
    [InlineData("DE41 5001 0517 0123 4567 89")]
    [InlineData("GI75 NWBK 0000 0000 7099 453")]
    [InlineData("GR16 0110 1250 0000 0001 2300 695")]
    [InlineData("HU42 1177 3016 1111 1018 0000 0000")]
    [InlineData("IE29 AIBK 9311 5212 3456 78")]
    [InlineData("IQ98 NBIQ 8501 2345 6789 012")]
    [InlineData("JO94 CBJO 0010 0000 0000 0131 0003 02")]
    [InlineData("KZ86 125K ZT50 0410 0100")]
    [InlineData("LI21 0881 0000 2324 013A A")]
    [InlineData("LT12 1000 0111 0100 1000")]
    [InlineData("LU28 0019 4006 4475 0000")]
    [InlineData("MT84 MALT 0110 0001 2345 MTLC AST0 01S")]
    [InlineData("MR13 0002 0001 0100 0012 3456 753")]
    [InlineData("MD24 AG00 0225 1000 1310 4168")]
    [InlineData("ME25 5050 0001 2345 6789 51")]
    [InlineData("MK07 2501 2000 0058 984")]
    [InlineData("NO93 8601 1117 947")]
    [InlineData("RO49 AAAA 1B31 0075 9384 0000")]
    [InlineData("RS35 2600 0560 1001 6113 79")]
    [InlineData("SA03 8000 0000 6080 1016 7519")]
    [InlineData("SM86 U032 2509 8000 0000 0270 100")]
    [InlineData("SK31 1200 0000 1987 4263 7541")]
    [InlineData("TL38 0080 0123 4567 8910 157")]
    [InlineData("TN59 1000 6035 1835 9847 8831")]
    [InlineData("TR33 0006 1005 1978 6457 8413 26")]
    [InlineData("GB71 HBUK 4012 7612 3456 78")]
    [InlineData("VG96 VPVG 0000 0123 4567 8901")]
    public void IsValid_ReturnsTrue_ForValidIbans(string input)
    {
        Assert.True(Iban.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NL12A")]
    [InlineData("US64SVBKUS6S3300958879")]
    [InlineData("ZZ12ABCD1234567890123456789")]
    [InlineData("NL91ABNA04171643000")]
    [InlineData("NL00 ABNA 0417 1643 00")]
    [InlineData("DE89 3704 0044 0532 0130 0012345678901234567890")]
    public void IsValid_ReturnsFalse_ForInvalidIbans(string? input)
    {
        Assert.False(Iban.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsComponents_ForValidIban()
    {
        var ok = Iban.TryParse(" nl91 abna 0417 1643 00 ", out var iban);

        Assert.True(ok);
        Assert.NotNull(iban);
        Assert.Equal("NL", iban!.CountryCode);
        Assert.Equal("NL91ABNA0417164300", iban.Value);
        Assert.Equal("NL91 ABNA 0417 1643 00", iban.Formatted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US64SVBKUS6S3300958879")]
    [InlineData("NL00 ABNA 0417 1643 00")]
    [InlineData("ZZ12ABCD1234567890")]
    [InlineData("NL91ABNA04171643000")]
    public void TryParse_ReturnsNull_ForInvalidIban(string? input)
    {
        var ok = Iban.TryParse(input, out var iban);

        Assert.False(ok);
        Assert.Null(iban);
    }

    [Theory]
    [InlineData(" nl91 abna 0417 1643 00 ", "NL91 ABNA 0417 1643 00")]
    [InlineData("de89370400440532013000", "DE89 3704 0044 0532 0130 00")]
    [InlineData("GB82-WEST-1234-5698-7654-32", "GB82 WEST 1234 5698 7654 32")]
    [InlineData("fr14-2004-1010-0505-0001-3m02-606", "FR14 2004 1010 0505 0001 3M02 606")]
    [InlineData("se4550000000058398257466", "SE45 5000 0000 0583 9825 7466")]
    [InlineData("it60x0542811101000000123456", "IT60 X054 2811 1010 0000 0123 456")]
    [InlineData("AL47 2121-1009 0000-0002 3569-8741", "AL47 2121 1009 0000 0002 3569 8741")]
    [InlineData("AD12-0001 2030-2003 5910-0100", "AD12 0001 2030 2003 5910 0100")]
    [InlineData(" at61 1904 3002 3457 3201 ", "AT61 1904 3002 3457 3201")]
    [InlineData("BH67BMAG00001299123456", "BH67 BMAG 0000 1299 1234 56")]
    [InlineData("  be68 5390 0754 7034  ", "BE68 5390 0754 7034")]
    [InlineData("BA39-1290-0794-0102-8494", "BA39 1290 0794 0102 8494")]
    [InlineData("bg80bnbg96611020345678", "BG80 BNBG 9661 1020 3456 78")]
    [InlineData("HR12 1001 0051-8630 0016 0", "HR12 1001 0051 8630 0016 0")]
    [InlineData("cz6508000000192000145399", "CZ65 0800 0000 1920 0014 5399")]
    [InlineData("dk50-0040-0440-1162-43", "DK50 0040 0440 1162 43")]
    [InlineData("ee38 2200 2210   2014 5685", "EE38 2200 2210 2014 5685")]
    [InlineData("es9121000418450200051332", "ES91 2100 0418 4502 0005 1332")]
    [InlineData("fi21-1234-5600-0007-85", "FI21 1234 5600 0007 85")]
    [InlineData("de41500105170123456789", "DE41 5001 0517 0123 4567 89")]
    [InlineData("GI75NWBK000000007099453", "GI75 NWBK 0000 0000 7099 453")]
    [InlineData("gr1601101250000000012300695", "GR16 0110 1250 0000 0001 2300 695")]
    [InlineData("hu42 1177 3016 1111 1018 0000 0000", "HU42 1177 3016 1111 1018 0000 0000")]
    [InlineData("ie29aibk93115212345678", "IE29 AIBK 9311 5212 3456 78")]
    [InlineData("IQ98-NBIQ-8501-2345-6789-012", "IQ98 NBIQ 8501 2345 6789 012")]
    [InlineData(" jo94cbjo0010000000000131000302 ", "JO94 CBJO 0010 0000 0000 0131 0003 02")]
    [InlineData("kz86 125kzt50 0410 0100", "KZ86 125K ZT50 0410 0100")]
    [InlineData("LI21-0881-0000-2324-013A-A", "LI21 0881 0000 2324 013A A")]
    [InlineData("lt121000011101001000", "LT12 1000 0111 0100 1000")]
    [InlineData("LU280019400644750000", "LU28 0019 4006 4475 0000")]
    [InlineData(" mt84malt011000012345mtlcast001s ", "MT84 MALT 0110 0001 2345 MTLC AST0 01S")]
    [InlineData("MR13-0002-0001-0100-0012-3456-753", "MR13 0002 0001 0100 0012 3456 753")]
    [InlineData("md24ag000225100013104168", "MD24 AG00 0225 1000 1310 4168")]
    [InlineData("ME25 5050 0001 2345 6789 51", "ME25 5050 0001 2345 6789 51")]
    [InlineData(" mk07 2501 2000 0058 984 ", "MK07 2501 2000 0058 984")]
    [InlineData("NO93-8601-1117-947", "NO93 8601 1117 947")]
    [InlineData("ro49aaaa1b31007593840000", "RO49 AAAA 1B31 0075 9384 0000")]
    [InlineData("RS35 2600 0560 1001 6113 79", "RS35 2600 0560 1001 6113 79")]
    [InlineData(" sa0380000000608010167519", "SA03 8000 0000 6080 1016 7519")]
    [InlineData("SM86U0322509800000000270100", "SM86 U032 2509 8000 0000 0270 100")]
    [InlineData("sk3112000000198742637541", "SK31 1200 0000 1987 4263 7541")]
    [InlineData("TL38-0080-0123-4567-8910-157", "TL38 0080 0123 4567 8910 157")]
    [InlineData("tn59 1000 6035 1835 9847 8831  ", "TN59 1000 6035 1835 9847 8831")]
    [InlineData("TR33 0006-1005-1978-6457-8413-26", "TR33 0006 1005 1978 6457 8413 26")]
    [InlineData("GB71HBUK40127612345678", "GB71 HBUK 4012 7612 3456 78")]
    [InlineData(" vg96vpvg0000012345678901 ", "VG96 VPVG 0000 0123 4567 8901")]
    public void TryParse_Formatted_ReturnsExpectedValue(string input, string expectedFormatted)
    {
        var ok = Iban.TryParse(input, out var iban);

        Assert.True(ok);
        Assert.Equal(expectedFormatted, iban!.Formatted);
    }

    [Theory]
    [InlineData("NL91ABNA0417164300", "NL91 ABNA 0417 1643 00")]
    [InlineData("DE89370400440532013000", "DE89 3704 0044 0532 0130 00")]
    [InlineData("SE4550000000058398257466", "SE45 5000 0000 0583 9825 7466")]
    public void Parse_Formatted_ReturnsExpectedValue(string input, string expected)
    {
        var iban = Iban.Parse(input);

        Assert.Equal(expected, iban.Formatted);
    }

    [Theory]
    [InlineData("US64SVBKUS6S3300958879")]
    [InlineData("NL00 ABNA 0417 1643 00")]
    [InlineData("NL91ABNA04171643000")]
    public void Parse_Throws_ForInvalidInput(string input)
    {
        Assert.Throws<ArgumentException>(() => Iban.Parse(input));
    }

    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00", "NL91ABNA0417164300")]
    [InlineData(" nl91 abna 0417 1643 00 ", "NL91ABNA0417164300")]
    [InlineData("de89370400440532013000", "DE89370400440532013000")]
    public void TryParse_Value_ReturnsNormalized(string input, string expectedValue)
    {
        var ok = Iban.TryParse(input, out var iban);

        Assert.True(ok);
        Assert.Equal(expectedValue, iban!.Value);
    }

    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00", "NL")]
    [InlineData("DE89 3704 0044 0532 0130 00", "DE")]
    [InlineData("SE45 5000 0000 0583 9825 7466", "SE")]
    [InlineData("GB82 WEST 1234 5698 7654 32", "GB")]
    public void TryParse_CountryCode_IsCorrect(string input, string expectedCountryCode)
    {
        var ok = Iban.TryParse(input, out var iban);

        Assert.True(ok);
        Assert.Equal(expectedCountryCode, iban!.CountryCode);
    }

    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00", "IBAN NL91 ABNA 0417 1643 00")]
    [InlineData("se4550000000058398257466", "IBAN SE45 5000 0000 0583 9825 7466")]
    public void ToDisplayString_ReturnsFullDisplay(string input, string expected)
    {
        var iban = Iban.Parse(input);

        Assert.Equal(expected, iban.ToDisplayString());
    }

    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00", "IBAN NL91 ABNA 0417 1643 00")]
    [InlineData("se4550000000058398257466", "IBAN SE45 5000 0000 0583 9825 7466")]
    public void ToShortDisplayString_ReturnsShortDisplay(string input, string expected)
    {
        var iban = Iban.Parse(input);

        Assert.Equal(expected, iban.ToShortDisplayString());
    }

    [Theory]
    [InlineData("NL91 ABNA 0417 1643 00", "NL91 ABNA 0417 1643 00")]
    [InlineData("se4550000000058398257466", "SE45 5000 0000 0583 9825 7466")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var iban = Iban.Parse(input);

        Assert.Equal(expected, iban.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Iban.Parse("nl91abna0417164300");
        var b = Iban.Parse("NL91 ABNA 0417 1643 00");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Iban.Parse("NL91 ABNA 0417 1643 00");
        var b = Iban.Parse("DE89 3704 0044 0532 0130 00");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Iban.Parse("NL91 ABNA 0417 1643 00");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Iban.Parse("DE89370400440532013000");
        var b = Iban.Parse("SE4550000000058398257466");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Iban.Parse("NL91 ABNA 0417 1643 00");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("SE45", false, ValidationErrorReason.InvalidLength)]
    [InlineData("XX4550000000058398257466", false, ValidationErrorReason.UnknownCountryCode)]
    [InlineData("SE4450000000058398257466", false, ValidationErrorReason.InvalidCheckDigit)]
    [InlineData("SE4550000000058398257466", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = Iban.Validate(input);

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
        var result = Iban.Validate("SE45");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("SE4550000000058398257466")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SE45")]
    [InlineData("XX4550000000058398257466")]
    [InlineData("SE4450000000058398257466")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(Iban.IsValid(input), Iban.Validate(input).IsValid);
    }
}
