using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class OperatingSystemNameTests
{
    [Theory]
    [InlineData("Windows")]
    [InlineData("windows")]
    [InlineData("win")]
    [InlineData("macOS")]
    [InlineData("mac os")]
    [InlineData("Mac OS X")]
    [InlineData("OSX")]
    [InlineData("Linux")]
    [InlineData("Ubuntu")]
    [InlineData("Android")]
    [InlineData("iOS")]
    [InlineData("iPadOS")]
    [InlineData("ChromeOS")]
    [InlineData("chrome os")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(OperatingSystemName.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("FooOS")]
    [InlineData("ReactOS")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(OperatingSystemName.IsValid(input));
    }

    [Theory]
    [InlineData("windows", "Windows", OperatingSystemFamily.Windows)]
    [InlineData("win", "Windows", OperatingSystemFamily.Windows)]
    [InlineData("mac os x", "macOS", OperatingSystemFamily.MacOS)]
    [InlineData("osx", "macOS", OperatingSystemFamily.MacOS)]
    [InlineData("Ubuntu", "Ubuntu", OperatingSystemFamily.Linux)]
    [InlineData("rhel", "Red Hat", OperatingSystemFamily.Linux)]
    [InlineData("android", "Android", OperatingSystemFamily.Android)]
    [InlineData("iphone os", "iOS", OperatingSystemFamily.IOS)]
    [InlineData("ipad os", "iPadOS", OperatingSystemFamily.IPadOS)]
    [InlineData("chrome os", "ChromeOS", OperatingSystemFamily.ChromeOS)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedName, OperatingSystemFamily expectedFamily)
    {
        Assert.True(OperatingSystemName.TryParse(input, out var result));
        Assert.Equal(expectedName, result!.Value);
        Assert.Equal(expectedFamily, result.Family);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("ms-dos")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OperatingSystemName.Parse(input));
    }

    [Theory]
    [InlineData("win", "Windows")]
    [InlineData("mac os", "macOS")]
    [InlineData("ubuntu", "Ubuntu")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OperatingSystemName.Format(input));
    }

    [Theory]
    [InlineData("Windows", "Windows")]
    [InlineData("win", "Windows")]
    public void ToString_ReturnsCanonicalName(string input, string expected)
    {
        var os = OperatingSystemName.Parse(input);
        Assert.Equal(expected, os.ToString());
    }

    [Fact]
    public void Equality_SameOs()
    {
        var a = OperatingSystemName.Parse("win");
        var b = OperatingSystemName.Parse("Windows");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentOs()
    {
        var a = OperatingSystemName.Parse("Windows");
        var b = OperatingSystemName.Parse("Linux");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = OperatingSystemName.Parse("Android");
        var b = OperatingSystemName.Parse("Windows");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = OperatingSystemName.Parse("Linux");
        Assert.Equal(1, a.CompareTo(null));
    }
}
