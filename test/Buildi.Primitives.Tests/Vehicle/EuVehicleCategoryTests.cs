using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EuVehicleCategoryTests
{
    [Theory]
    [InlineData("M1")]
    [InlineData("M2")]
    [InlineData("M3")]
    [InlineData("N1")]
    [InlineData("N1G")]
    [InlineData("N1g")]
    [InlineData("L3e")]
    [InlineData("L3e-A2")]
    [InlineData("O4")]
    [InlineData("T1")]
    [InlineData("C1")]
    [InlineData("R1")]
    [InlineData("S1")]
    [InlineData("m1")]
    [InlineData("n1g")]
    [InlineData(" M1 ")]
    [InlineData("L3E")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EuVehicleCategory.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X1")]
    [InlineData("M0")]
    [InlineData("M9")]
    [InlineData("MM1")]
    [InlineData("123")]
    [InlineData("L3")]
    [InlineData("M1e")]
    [InlineData("O1G")]
    [InlineData("L3eG")]
    [InlineData("T6")]
    [InlineData("C6")]
    [InlineData("S3")]
    [InlineData("O5")]
    [InlineData("L8e")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EuVehicleCategory.IsValid(input));
    }

    [Theory]
    [InlineData("M1", "M1", "M", 1, null, false)]
    [InlineData("M1G", "M1G", "M", 1, "G", true)]
    [InlineData("N1G", "N1G", "N", 1, "G", true)]
    [InlineData("n1g", "N1G", "N", 1, "G", true)]
    [InlineData("L3e", "L3e", "L", 3, "e", false)]
    [InlineData("L3e-A2", "L3e-A2", "L", 3, "e-A2", false)]
    [InlineData("O4", "O4", "O", 4, null, false)]
    [InlineData("T1", "T1", "T", 1, null, false)]
    [InlineData("m1", "M1", "M", 1, null, false)]
    [InlineData(" N2 ", "N2", "N", 2, null, false)]
    [InlineData("L3E", "L3e", "L", 3, "e", false)]
    [InlineData("R4", "R4", "R", 4, null, false)]
    [InlineData("S2", "S2", "S", 2, null, false)]
    [InlineData("C5", "C5", "C", 5, null, false)]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, string expectedValue, string expectedBase, int expectedNumber,
        string? expectedSuffix, bool expectedIsOffRoad)
    {
        var ok = EuVehicleCategory.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedBase, result.BaseCategory);
        Assert.Equal(expectedNumber, result.CategoryNumber);
        Assert.Equal(expectedSuffix, result.Suffix);
        Assert.Equal(expectedIsOffRoad, result.IsOffRoad);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X1")]
    [InlineData("M0")]
    [InlineData("MM1")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = EuVehicleCategory.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("X1")]
    [InlineData("M0")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EuVehicleCategory.Parse(input));
    }

    [Theory]
    [InlineData("M1", "Personbil ≤8+1 passagerare")]
    [InlineData("N1", "Lätt lastbil ≤3,5 ton")]
    [InlineData("N1G", "Lätt lastbil ≤3,5 ton (terrängfordon)")]
    [InlineData("L3e", "Tvåhjulig motorcykel")]
    [InlineData("L3e-A2", "Tvåhjulig motorcykel")]
    [InlineData("O4", "Tung släpvagn >10 ton")]
    [InlineData("T1", "Standardjordbrukstraktor")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EuVehicleCategory.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("invalid", EuVehicleCategory.Format(" invalid ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_WithFallback_ReturnsNull_ForWhitespace()
    {
        Assert.Null(EuVehicleCategory.Format(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("M1", "M1")]
    [InlineData("m1", "M1")]
    [InlineData("N1G", "N1G")]
    [InlineData("n1g", "N1G")]
    [InlineData("L3e", "L3e")]
    [InlineData("L3e-A2", "L3e-A2")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EuVehicleCategory.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("invalid", EuVehicleCategory.Normalize(" invalid ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(EuVehicleCategory.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(EuVehicleCategory.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("M1", true)]
    [InlineData("m1", false)]
    [InlineData("N1G", true)]
    [InlineData("n1g", false)]
    [InlineData("L3e", true)]
    [InlineData("L3E", false)]
    [InlineData(null, false)]
    [InlineData("invalid", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, EuVehicleCategory.IsNormalized(input));
    }

    [Theory]
    [InlineData("M1", "M1")]
    [InlineData("m1", "M1")]
    [InlineData("N1G", "N1G")]
    [InlineData("L3e-A2", "L3e-A2")]
    public void ToString_ReturnsCanonicalCode(string input, string expected)
    {
        var category = EuVehicleCategory.Parse(input);
        Assert.Equal(expected, category.ToString());
        Assert.Equal(expected, category.ToNormalizedString());
    }

    [Fact]
    public void TryParse_M1G_HasOffRoadDescription()
    {
        var ok = EuVehicleCategory.TryParse("M1G", out var result);
        Assert.True(ok);
        Assert.True(result!.IsOffRoad);
        Assert.Contains("off-road", result.EnglishDescription);
        Assert.Contains("terrängfordon", result.LocalizedDescription);
    }

    [Fact]
    public void TryParse_L3e_A2_HasSubSuffix()
    {
        var ok = EuVehicleCategory.TryParse("L3e-A2", out var result);
        Assert.True(ok);
        Assert.Equal("L3e-A2", result!.Value);
        Assert.Equal("L", result.BaseCategory);
        Assert.Equal(3, result.CategoryNumber);
        Assert.Equal("e-A2", result.Suffix);
        Assert.False(result.IsOffRoad);
    }

    [Fact]
    public void Equality_SameCategory()
    {
        var a = EuVehicleCategory.Parse("M1");
        var b = EuVehicleCategory.Parse("m1");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentCategories()
    {
        var a = EuVehicleCategory.Parse("M1");
        var b = EuVehicleCategory.Parse("N1");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersByValue()
    {
        var m1 = EuVehicleCategory.Parse("M1");
        var n1 = EuVehicleCategory.Parse("N1");
        Assert.True(m1.CompareTo(n1) < 0);
        Assert.True(n1.CompareTo(m1) > 0);
        Assert.Equal(0, m1.CompareTo(m1));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Assert.Equal(1, EuVehicleCategory.Parse("M1").CompareTo(null));
    }

    [Fact]
    public void Operators_LessThan_GreaterThan()
    {
        var m1 = EuVehicleCategory.Parse("M1");
        var n1 = EuVehicleCategory.Parse("N1");
        Assert.True(m1 < n1);
        Assert.True(n1 > m1);
        Assert.True(m1 <= n1);
        Assert.True(n1 >= m1);
        Assert.True(m1 <= EuVehicleCategory.Parse("M1"));
        Assert.True(m1 >= EuVehicleCategory.Parse("M1"));
    }
}
