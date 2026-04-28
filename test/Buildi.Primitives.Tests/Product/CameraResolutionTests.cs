using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class CameraResolutionTests
{
    [Theory]
    [InlineData("12 MP")]
    [InlineData("12mp")]
    [InlineData("12 Mp")]
    [InlineData("12.2 MP")]
    [InlineData("108 megapixels")]
    [InlineData("108 megapixel")]
    [InlineData("48 Mpx")]
    [InlineData("48 mpix")]
    [InlineData("12 megapixlar")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(CameraResolution.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12")]
    [InlineData("12 pixels")]
    [InlineData("12 px")]
    [InlineData("abc")]
    [InlineData("MP")]
    [InlineData("-5 MP")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(CameraResolution.IsValid(input));
    }

    [Theory]
    [InlineData("12 MP", 12)]
    [InlineData("12.2 MP", 12.2)]
    [InlineData("108 megapixels", 108)]
    public void TryParse_ReturnsExpectedMegapixels(string input, double expected)
    {
        Assert.True(CameraResolution.TryParse(input, out var result));
        Assert.Equal((decimal)expected, result!.Megapixels);
    }

    [Fact]
    public void TotalPixels_ReturnsExpected()
    {
        var r = CameraResolution.FromMegapixels(12.2m);
        Assert.Equal(12_200_000L, r.TotalPixels);
    }

    [Fact]
    public void FromTotalPixels_ConvertsCorrectly()
    {
        var r = CameraResolution.FromTotalPixels(12_200_000);
        Assert.Equal(12.2m, r.Megapixels);
    }

    [Theory]
    [InlineData("12 MP", "12 MP")]
    [InlineData("12.2 MP", "12.2 MP")]
    [InlineData("108 megapixels", "108 MP")]
    public void Normalize_UsesCanonicalSuffix(string input, string expected) =>
        Assert.Equal(expected, CameraResolution.Normalize(input));

    [Theory]
    [InlineData("12 MP", true)]
    [InlineData("108 MP", true)]
    [InlineData("12mp", false)]
    [InlineData("108 megapixels", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected) =>
        Assert.Equal(expected, CameraResolution.IsNormalized(input));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => CameraResolution.Parse("abc"));

    [Fact]
    public void FromMegapixels_Throws_ForNegative() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CameraResolution.FromMegapixels(-1));

    [Fact]
    public void Equality_SameMegapixels()
    {
        var a = CameraResolution.FromMegapixels(12.2m);
        var b = CameraResolution.Parse("12.2 MP");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Comparison_OrdersByMegapixels()
    {
        var a = CameraResolution.FromMegapixels(12);
        var b = CameraResolution.FromMegapixels(48);
        Assert.True(a < b);
    }

    [Fact]
    public void ToMaskedString_ReturnsMaskedForm()
    {
        var r = CameraResolution.FromMegapixels(12.2m);
        Assert.Equal("*** MP", r.ToMaskedString());
    }

    [Fact]
    public void FindCandidatesInText_FindsMegapixelValues()
    {
        var text = "Main camera 50 MP, ultra-wide 12mp, and a 108 megapixel sensor on the back.";
        var candidates = CameraResolution.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 3);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Camera Resolution", CameraResolution.TypeInfo.EnglishName);
        Assert.Equal("Kameraupplösning", CameraResolution.TypeInfo.LocalizedName);
    }
}
