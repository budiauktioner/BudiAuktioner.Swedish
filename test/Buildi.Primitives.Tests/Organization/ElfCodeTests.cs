using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class ElfCodeTests
{
    [Theory]
    [InlineData("XTIQ")]
    [InlineData("N2GY")]
    [InlineData("FR3V")]
    [InlineData("CLBQ")]
    [InlineData("9GQP")]
    [InlineData("ZZZZ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ElfCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XTI")]
    [InlineData("XTIQA")]
    [InlineData("XT-Q")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ElfCode.IsValid(input));
    }

    [Theory]
    [InlineData("XTIQ", "XTIQ", "Limited company", "Aktiebolag", true)]
    [InlineData("N2GY", "N2GY", "General partnership", "Handelsbolag", true)]
    [InlineData("WJEL", "WJEL", "Sole proprietorship", "Enskild näringsidkare", true)]
    [InlineData("ZZZZ", "ZZZZ", null, null, false)]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, string expectedCode, string? expectedEnglish, string? expectedSwedish, bool expectedIsKnown)
    {
        var ok = ElfCode.TryParse(input, out var elf);

        Assert.True(ok);
        Assert.NotNull(elf);
        Assert.Equal(expectedCode, elf!.Code);
        Assert.Equal(expectedEnglish, elf.EnglishName);
        Assert.Equal(expectedSwedish, elf.LocalizedName);
        Assert.Equal(expectedIsKnown, elf.IsKnown);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XTI")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = ElfCode.TryParse(input, out var elf);

        Assert.False(ok);
        Assert.Null(elf);
    }

    [Theory]
    [InlineData("XTI")]
    [InlineData("XTIQA")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ElfCode.Parse(input));
    }

    [Theory]
    [InlineData("xtiq", "XTIQ")]
    [InlineData("  XTIQ  ", "XTIQ")]
    [InlineData("Xtiq", "XTIQ")]
    public void TryParse_NormalizesInput(string input, string expectedCode)
    {
        var ok = ElfCode.TryParse(input, out var elf);

        Assert.True(ok);
        Assert.Equal(expectedCode, elf!.Code);
    }

    [Fact]
    public void Format_ReturnsDisplayName_ForKnownCode()
    {
        Assert.Equal("Aktiebolag", ElfCode.Format("XTIQ"));
    }

    [Fact]
    public void Format_ReturnsCode_ForUnknownValidCode()
    {
        Assert.Equal("ZZZZ", ElfCode.Format("ZZZZ"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("XTI", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, ElfCode.Format(input));
    }

    [Theory]
    [InlineData("XTIQ", "XTIQ")]
    [InlineData("xtiq", "XTIQ")]
    [InlineData(null, null)]
    [InlineData("XTI", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElfCode.Normalize(input));
    }

    [Theory]
    [InlineData("XTIQ", true)]
    [InlineData("xtiq", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, ElfCode.IsNormalized(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalid()
    {
        Assert.Equal("XTI", ElfCode.Format("  XTI  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(ElfCode.Format(null, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void ToString_ReturnsDisplayName_ForKnown()
    {
        var elf = ElfCode.Parse("XTIQ");
        Assert.Equal("Aktiebolag", elf.ToString());
    }

    [Fact]
    public void ToString_ReturnsCode_ForUnknown()
    {
        var elf = ElfCode.Parse("ZZZZ");
        Assert.Equal("ZZZZ", elf.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsCode()
    {
        var elf = ElfCode.Parse("XTIQ");
        Assert.Equal("XTIQ", elf.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = ElfCode.Parse("XTIQ");
        var b = ElfCode.Parse("XTIQ");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = ElfCode.Parse("XTIQ");
        var b = ElfCode.Parse("N2GY");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = ElfCode.Parse("XTIQ");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ElfCode.Parse("FR3V");
        var b = ElfCode.Parse("XTIQ");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ElfCode.Parse("XTIQ");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Fact]
    public void AllKnownSwedishCodes_AreValid()
    {
        string[] knownCodes = ["XTIQ", "N2GY", "FR3V", "V2YH", "WJEL", "CLBQ", "F85L", "O9FH",
            "H0PO", "2HBR", "EVKQ", "R7GX", "KQM9", "L5CF", "LRQE", "J4GF", "9GQP"];

        foreach (var code in knownCodes)
        {
            Assert.True(ElfCode.IsValid(code), $"Expected {code} to be valid");
            var elf = ElfCode.Parse(code);
            Assert.True(elf.IsKnown, $"Expected {code} to be known");
            Assert.NotNull(elf.EnglishName);
            Assert.NotNull(elf.LocalizedName);
        }
    }
}
