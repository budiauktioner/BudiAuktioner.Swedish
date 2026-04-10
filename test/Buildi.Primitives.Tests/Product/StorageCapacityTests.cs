using Buildi.Primitives.Measurement;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class StorageCapacityTests
{
    [Theory]
    [InlineData("512")]
    [InlineData("512 GB")]
    [InlineData("1 TB")]
    [InlineData("256 GB")]
    [InlineData("2 TB")]
    [InlineData("128 GiB")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(StorageCapacity.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-5 GB")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(StorageCapacity.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToGB()
    {
        Assert.True(StorageCapacity.TryParse("512", out var result));
        Assert.Equal(512m, result!.Gigabytes);
        Assert.Equal("512 GB", result.Value);
    }

    [Fact]
    public void TryParse_WithUnit_UsesSpecifiedUnit()
    {
        Assert.True(StorageCapacity.TryParse("1 TB", out var result));
        Assert.Equal(1000m, result!.Gigabytes);
    }

    [Theory]
    [InlineData("512 GB", "512 GB")]
    [InlineData("1 TB", "1 TB")]
    [InlineData("512", "512 GB")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, StorageCapacity.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 GB", StorageCapacity.Format("1 TB", unit: DataSizeUnit.Gigabyte));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = StorageCapacity.Parse("256 GB");
        var b = StorageCapacity.Parse("1 TB");
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameCapacity()
    {
        var a = StorageCapacity.Parse("1000 GB");
        var b = StorageCapacity.Parse("1 TB");
        Assert.True(a == b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => StorageCapacity.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var sc = StorageCapacity.Create(512m, DataSizeUnit.Gigabyte);
        Assert.Equal(512m, sc.Gigabytes);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StorageCapacity.Create(0m, DataSizeUnit.Gigabyte));
        Assert.Throws<ArgumentOutOfRangeException>(() => StorageCapacity.Create(-1m, DataSizeUnit.Gigabyte));
    }

    [Fact]
    public void FromGigabytes_Works()
    {
        var sc = StorageCapacity.FromGigabytes(512);
        Assert.Equal(512m, sc.Gigabytes);
    }

    [Fact]
    public void FromTerabytes_Works()
    {
        var sc = StorageCapacity.FromTerabytes(1);
        Assert.Equal(1000m, sc.Gigabytes);
    }

    [Fact]
    public void FromMegabytes_Works()
    {
        var sc = StorageCapacity.FromMegabytes(256);
        Assert.True(sc.Bytes > 0);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = StorageCapacity.FromGigabytes(512);
        var fromString = StorageCapacity.Parse("512 GB");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void Operator_Add_CombinesCapacity()
    {
        var a = StorageCapacity.FromGigabytes(512);
        var b = StorageCapacity.FromGigabytes(256);
        var result = a + b;
        Assert.Equal(768m, result.Gigabytes);
    }

    [Fact]
    public void Operator_Subtract_Works()
    {
        var a = StorageCapacity.FromGigabytes(512);
        var b = StorageCapacity.FromGigabytes(256);
        Assert.Equal(256m, (a - b).Gigabytes);
    }

    [Fact]
    public void Operator_Multiply_ScalesCapacity()
    {
        var a = StorageCapacity.FromGigabytes(256);
        Assert.Equal(512m, (a * 2m).Gigabytes);
        Assert.Equal(512m, (2m * a).Gigabytes);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = StorageCapacity.FromGigabytes(512);
        Assert.Equal(256m, (a / 2m).Gigabytes);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = StorageCapacity.FromGigabytes(512);
        Assert.Equal(-512m, (-a).Gigabytes);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = StorageCapacity.Parse("256 GB");
        var b = StorageCapacity.Parse("512 GB");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = StorageCapacity.Parse("1 TB");
        Assert.Equal(1, a.CompareTo(null));
    }
}
