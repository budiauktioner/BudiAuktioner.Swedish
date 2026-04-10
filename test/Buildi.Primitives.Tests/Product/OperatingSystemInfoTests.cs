using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class OperatingSystemInfoTests
{
    [Theory]
    [InlineData("Windows 11")]
    [InlineData("windows 10")]
    [InlineData("macOS 14.5")]
    [InlineData("mac os x 10.15.7")]
    [InlineData("Ubuntu 22.04")]
    [InlineData("Android 14")]
    [InlineData("iOS 17.4.1")]
    [InlineData("Linux")]
    [InlineData("ChromeOS")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(OperatingSystemInfo.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("FooOS 5")]
    [InlineData("11")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(OperatingSystemInfo.IsValid(input));
    }

    [Theory]
    [InlineData("Windows 11", "Windows", "11")]
    [InlineData("macOS 14.5", "macOS", "14.5")]
    [InlineData("mac os x 10.15.7", "macOS", "10.15.7")]
    [InlineData("Ubuntu 22.04", "Ubuntu", "22.04")]
    [InlineData("Android 14", "Android", "14")]
    [InlineData("iOS 17.4.1", "iOS", "17.4.1")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedName, string expectedVersion)
    {
        Assert.True(OperatingSystemInfo.TryParse(input, out var result));
        Assert.Equal(expectedName, result!.Name.Value);
        Assert.NotNull(result.Version);
        Assert.Equal(expectedVersion, result.Version!.Value);
    }

    [Fact]
    public void TryParse_NameOnly_NoVersion()
    {
        Assert.True(OperatingSystemInfo.TryParse("Linux", out var result));
        Assert.Equal("Linux", result!.Name.Value);
        Assert.Null(result.Version);
    }

    [Theory]
    [InlineData("windows 11", "Windows 11")]
    [InlineData("mac os x 10.15.7", "macOS 10.15.7")]
    [InlineData("ubuntu 22.04", "Ubuntu 22.04")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OperatingSystemInfo.Format(input));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("11")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OperatingSystemInfo.Parse(input));
    }

    [Fact]
    public void ToString_ReturnsDisplayForm()
    {
        var os = OperatingSystemInfo.Parse("Windows 11");
        Assert.Equal("Windows 11", os.ToString());
    }

    [Fact]
    public void Equality_SameOs()
    {
        var a = OperatingSystemInfo.Parse("Windows 11");
        var b = OperatingSystemInfo.Parse("win 11");
        Assert.True(a == b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = OperatingSystemInfo.Parse("Android 14");
        var b = OperatingSystemInfo.Parse("Windows 11");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = OperatingSystemInfo.Parse("Ubuntu 22.04");
        Assert.Equal(1, a.CompareTo(null));
    }
}
