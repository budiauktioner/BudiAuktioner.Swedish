using Buildi.Primitives.Measurement;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class RamCapacityTests
{
    [Theory]
    [InlineData("16")]
    [InlineData("16 GB")]
    [InlineData("32 GB")]
    [InlineData("8 GiB")]
    [InlineData("4096 MB")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(RamCapacity.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-8 GB")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(RamCapacity.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToGB()
    {
        Assert.True(RamCapacity.TryParse("16", out var result));
        Assert.Equal(16m, result!.Gigabytes);
        Assert.Equal("16 GB", result.Value);
    }

    [Fact]
    public void TryParse_WithUnit_UsesSpecifiedUnit()
    {
        Assert.True(RamCapacity.TryParse("4096 MB", out var result));
        Assert.Equal(4096m, result!.Megabytes);
    }

    [Theory]
    [InlineData("16 GB", "16 GB")]
    [InlineData("4096 MB", "4096 MB")]
    [InlineData("16", "16 GB")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, RamCapacity.Format(input));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = RamCapacity.Parse("8 GB");
        var b = RamCapacity.Parse("16 GB");
        Assert.True(a < b);
    }

    [Fact]
    public void Equality_SameCapacity()
    {
        var a = RamCapacity.Parse("16");
        var b = RamCapacity.Parse("16 GB");
        Assert.True(a == b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => RamCapacity.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var rc = RamCapacity.Create(16m, DataSizeUnit.Gigabyte);
        Assert.Equal(16m, rc.Gigabytes);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RamCapacity.Create(0m, DataSizeUnit.Gigabyte));
        Assert.Throws<ArgumentOutOfRangeException>(() => RamCapacity.Create(-1m, DataSizeUnit.Gigabyte));
    }

    [Fact]
    public void FromGigabytes_Works()
    {
        var rc = RamCapacity.FromGigabytes(16);
        Assert.Equal(16m, rc.Gigabytes);
    }

    [Fact]
    public void FromMegabytes_Works()
    {
        var rc = RamCapacity.FromMegabytes(512);
        Assert.True(rc.Bytes > 0);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = RamCapacity.FromGigabytes(16);
        var fromString = RamCapacity.Parse("16 GB");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void Operator_Add_CombinesCapacity()
    {
        var a = RamCapacity.FromGigabytes(16);
        var b = RamCapacity.FromGigabytes(16);
        var result = a + b;
        Assert.Equal(32m, result.Gigabytes);
    }

    [Fact]
    public void Operator_Subtract_Works()
    {
        var a = RamCapacity.FromGigabytes(32);
        var b = RamCapacity.FromGigabytes(16);
        var result = a - b;
        Assert.Equal(16m, result.Gigabytes);
    }

    [Fact]
    public void Operator_Multiply_ScalesCapacity()
    {
        var a = RamCapacity.FromGigabytes(8);
        Assert.Equal(16m, (a * 2m).Gigabytes);
        Assert.Equal(16m, (2m * a).Gigabytes);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = RamCapacity.FromGigabytes(32);
        Assert.Equal(16m, (a / 2m).Gigabytes);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = RamCapacity.FromGigabytes(16);
        Assert.Equal(-16m, (-a).Gigabytes);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = RamCapacity.Parse("8 GB");
        var b = RamCapacity.Parse("16 GB");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = RamCapacity.Parse("32 GB");
        Assert.Equal(1, a.CompareTo(null));
    }
}
