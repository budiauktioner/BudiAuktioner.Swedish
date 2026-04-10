using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishSniCodeTests
{
    [Theory]
    [InlineData("62010", "62010", "62.010")]
    [InlineData("62.010", "62010", "62.010")]
    [InlineData("  62 010 ", "62010", "62.010")]
    public void TryParse_ReturnsExpectedValues(string input, string expectedCode, string expectedFormatted)
    {
        var ok = SwedishSniCode.TryParse(input, out var code);

        Assert.True(ok);
        Assert.NotNull(code);
        Assert.Equal(expectedCode, code!.Code);
        Assert.Equal(expectedFormatted, code.Formatted);
        Assert.Equal("62", code.DivisionCode);
        Assert.Equal("620", code.GroupCode);
        Assert.Equal("6201", code.SubGroupCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("6201")]
    [InlineData("620100")]
    [InlineData("00000")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(SwedishSniCode.IsValid(input));
    }

    [Fact]
    public void Format_Normalize_And_ToString_ReturnExpectedValues()
    {
        var code = SwedishSniCode.Parse("62010");

        Assert.Equal("62.010", SwedishSniCode.Format("62010"));
        Assert.Equal("62.010", SwedishSniCode.Normalize("62010"));
        Assert.Equal("62.010", SwedishSniCode.Normalize("62.010"));
        Assert.Equal("62.010", code.ToNormalizedString());
        Assert.Equal("62.010", code.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishSniCode.Parse("62010");
        var b = SwedishSniCode.Parse("62010");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishSniCode.Parse("62010");
        var b = SwedishSniCode.Parse("47917");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishSniCode.Parse("62010");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishSniCode.Parse("47917");
        var b = SwedishSniCode.Parse("62010");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishSniCode.Parse("62010");
        Assert.Equal(1, a.CompareTo(null));
    }
}
