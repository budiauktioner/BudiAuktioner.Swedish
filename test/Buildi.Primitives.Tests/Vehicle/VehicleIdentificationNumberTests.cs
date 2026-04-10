using Buildi.Primitives.Validation;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class VehicleIdentificationNumberTests
{
    [Theory]
    [InlineData("WBA3A5C55CF256789")]
    [InlineData("wba3a5c55cf256789")]
    [InlineData("1HGCM82633A004352")]
    [InlineData("  WBA3A5C55CF256789  ")]
    [InlineData("WBA-3A5C5-5CF2-56789")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(VehicleIdentificationNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("WBA3A5C55CF25678")]
    [InlineData("WBA3A5C55CF2567890")]
    [InlineData("WBA3A5C55CF25678I")]
    [InlineData("WBA3A5C55CF25678O")]
    [InlineData("WBA3A5C55CF25678Q")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(VehicleIdentificationNumber.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsExpectedProperties()
    {
        var ok = VehicleIdentificationNumber.TryParse("WBA3A5C55CF256789", out var vin);

        Assert.True(ok);
        Assert.NotNull(vin);
        Assert.Equal("WBA3A5C55CF256789", vin!.Value);
        Assert.Equal("WBA", vin.Wmi);
        Assert.Equal("3A5C5", vin.Vds);
        Assert.Equal('5', vin.CheckDigit);
        Assert.Equal("CF256789", vin.Vis);
        Assert.Equal('C', vin.ModelYearCode);
        Assert.Equal('F', vin.AssemblyPlantCode);
        Assert.Equal("256789", vin.SequentialNumber);
        Assert.Equal([1982, 2012], vin.ModelYears);
    }

    [Theory]
    [InlineData("WBA3A5C55CF256789", 'C', 1982, 2012)] // C → 1982/2012
    [InlineData("1HGCM82633A004352", '3', 2003, 2033)] // 3 → 2003/2033
    public void TryParse_ModelYears_DecodesCorrectly(string input, char expectedCode, int year1, int year2)
    {
        Assert.True(VehicleIdentificationNumber.TryParse(input, out var vin));
        Assert.Equal(expectedCode, vin!.ModelYearCode);
        Assert.Equal(2, vin.ModelYears.Count);
        Assert.Contains(year1, vin.ModelYears);
        Assert.Contains(year2, vin.ModelYears);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TOOLONG12345678901")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = VehicleIdentificationNumber.TryParse(input, out var vin);

        Assert.False(ok);
        Assert.Null(vin);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => VehicleIdentificationNumber.Parse("INVALID"));
    }

    [Theory]
    [InlineData("wba3a5c55cf256789", "WBA3A5C55CF256789")]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, VehicleIdentificationNumber.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !VehicleIdentificationNumber.IsValid(input)));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("invalid", VehicleIdentificationNumber.Format("invalid", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("wba3a5c55cf256789", "WBA3A5C55CF256789")]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, VehicleIdentificationNumber.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var vin = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");

        Assert.Equal("WBA3A5C55CF256789", vin.ToString());
        Assert.Equal("WBA3A5C55CF256789", vin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        var b = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        var b = VehicleIdentificationNumber.Parse("1HGCM82633A004352");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = VehicleIdentificationNumber.Parse("1HGCM82633A004352");
        var b = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("ABC", false, ValidationErrorReason.InvalidLength)]
    [InlineData("WBA3A5C55CF25678I", false, ValidationErrorReason.InvalidCharacters)]
    [InlineData("WBA3A5C55CF256789", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = VehicleIdentificationNumber.Validate(input);

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
}
