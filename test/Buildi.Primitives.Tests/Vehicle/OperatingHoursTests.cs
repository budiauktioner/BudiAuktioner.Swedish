using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class OperatingHoursTests
{
    [Theory]
    [InlineData("1234")]
    [InlineData("1234 h")]
    [InlineData("5600 timmar")]
    [InlineData("100 tim")]
    [InlineData("50 hours")]
    [InlineData("0")]
    [InlineData("12345.5 h")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(OperatingHours.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10 km")]
    [InlineData("10 V")]
    [InlineData("-5")]
    [InlineData("-100 h")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(OperatingHours.IsValid(input));
    }

    [Theory]
    [InlineData("1234", 1234)]
    [InlineData("1234 h", 1234)]
    [InlineData("5600 timmar", 5600)]
    [InlineData("100 tim", 100)]
    [InlineData("50 hours", 50)]
    [InlineData("12345.5 h", 12345.5)]
    public void TryParse_ReturnsExpected_Hours(string input, double expectedHours)
    {
        Assert.True(OperatingHours.TryParse(input, out var result));
        Assert.Equal((decimal)expectedHours, result!.Hours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(OperatingHours.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 km")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OperatingHours.Parse(input));
    }

    [Theory]
    [InlineData("1234", "1234 h")]
    [InlineData("1234 h", "1234 h")]
    [InlineData("5600 timmar", "5600 h")]
    [InlineData("100 tim", "100 h")]
    [InlineData("  1234  h  ", "1234 h")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OperatingHours.Format(input));
    }

    [Theory]
    [InlineData("1234", "1234 h")]
    [InlineData("5600 timmar", "5600 h")]
    [InlineData("100 h", "100 h")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OperatingHours.Normalize(input));
    }

    [Theory]
    [InlineData("1234 h", "1234 h")]
    [InlineData("5600 timmar", "5600 h")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var hours = OperatingHours.Parse(input);
        Assert.Equal(expected, hours.ToString());
    }

    [Fact]
    public void Create_FromDecimal()
    {
        var hours = OperatingHours.Create(1234m);
        Assert.Equal(1234m, hours.Hours);
        Assert.Equal("1234 h", hours.Value);
    }

    [Fact]
    public void Create_FromInt()
    {
        var hours = OperatingHours.Create(500);
        Assert.Equal(500m, hours.Hours);
    }

    [Fact]
    public void Create_ThrowsForNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OperatingHours.Create(-1));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = OperatingHours.FromHours(100);
        var b = OperatingHours.FromHours(200);
        Assert.Equal(300m, (a + b).Hours);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = OperatingHours.FromHours(200);
        var b = OperatingHours.FromHours(50);
        Assert.Equal(150m, (a - b).Hours);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = OperatingHours.FromHours(100);
        Assert.Equal(300m, (a * 3).Hours);
        Assert.Equal(300m, (3 * a).Hours);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = OperatingHours.FromHours(300);
        Assert.Equal(100m, (a / 3).Hours);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = OperatingHours.FromHours(100);
        var b = OperatingHours.FromHours(200);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= b);
        Assert.True(b >= a);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = OperatingHours.FromHours(100);
        var b = OperatingHours.Create(100);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void IsNormalized_True_ForNormalizedInput()
    {
        Assert.True(OperatingHours.IsNormalized("1234 h"));
    }

    [Fact]
    public void IsNormalized_False_ForUnnormalizedInput()
    {
        Assert.False(OperatingHours.IsNormalized("1234 timmar"));
        Assert.False(OperatingHours.IsNormalized("1234"));
    }

    [Theory]
    [InlineData("5,5 h")]
    [InlineData("2.5 timmar")]
    [InlineData("1 000 h")]
    [InlineData("1 000")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(OperatingHours.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 h", 5.5)]
    [InlineData("1 000 h", 1000)]
    [InlineData("1 000", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedHours)
    {
        Assert.True(OperatingHours.TryParse(input, out var result));
        Assert.Equal((decimal)expectedHours, result!.Hours);
    }

    [Fact]
    public void ToNormalizedString_ReturnsHoursFormat()
    {
        var hours = OperatingHours.Parse("5600 timmar");
        Assert.Equal("5600 h", hours.ToNormalizedString());
    }
}
