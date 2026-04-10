using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishCfarNumberTests
{
    [Theory]
    [InlineData("55667788")]
    [InlineData(" 55667788 ")]
    public void TryParse_ReturnsNumber_ForValidInput(string input)
    {
        var ok = SwedishCfarNumber.TryParse(input, out var cfar);

        Assert.True(ok);
        Assert.NotNull(cfar);
        Assert.Equal("55667788", cfar!.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("5566778")]
    [InlineData("556677889")]
    [InlineData("00000000")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(SwedishCfarNumber.IsValid(input));
    }

    [Fact]
    public void Format_Normalize_And_ToString_ReturnExpectedValues()
    {
        var cfar = SwedishCfarNumber.Parse("55667788");

        Assert.Equal("55667788", SwedishCfarNumber.Format("55667788"));
        Assert.Equal("55667788", SwedishCfarNumber.Normalize("55667788"));
        Assert.Equal("55667788", cfar.ToNormalizedString());
        Assert.Equal("55667788", cfar.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishCfarNumber.Parse("55667788");
        var b = SwedishCfarNumber.Parse("55667788");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishCfarNumber.Parse("55667788");
        var b = SwedishCfarNumber.Parse("12345678");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishCfarNumber.Parse("55667788");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishCfarNumber.Parse("12345678");
        var b = SwedishCfarNumber.Parse("55667788");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishCfarNumber.Parse("55667788");
        Assert.Equal(1, a.CompareTo(null));
    }
}
