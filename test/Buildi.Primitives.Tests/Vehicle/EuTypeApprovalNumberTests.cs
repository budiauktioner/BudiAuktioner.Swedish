using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EuTypeApprovalNumberTests
{
    [Theory]
    [InlineData("e9*2007/46*6364*09")]
    [InlineData("e5*2018/858*12345*01")]
    [InlineData("E9*2007/46*6364*09")]
    [InlineData(" e9*2007/46*6364*09 ")]
    [InlineData("e15*2007/46*1234*01")]
    [InlineData("e42*2018/858*999*03")]
    [InlineData("e58*2007/46*1*1")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EuTypeApprovalNumber.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("e0*2007/46*1*1")]
    [InlineData("9*2007/46*1*1")]
    [InlineData("e99*2007/46*1*1")]
    [InlineData("e59*2007/46*1*1")]
    [InlineData("x9*2007/46*1*1")]
    [InlineData("e9*2007/46*1")]
    [InlineData("e9 2007/46 1 1")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EuTypeApprovalNumber.IsValid(input));
    }

    [Theory]
    [InlineData("e9*2007/46*6364*09", 9, "Spain", "2007/46", "6364", "09")]
    [InlineData("e5*2018/858*12345*01", 5, "Sweden", "2018/858", "12345", "01")]
    [InlineData("E9*2007/46*6364*09", 9, "Spain", "2007/46", "6364", "09")]
    [InlineData("e1*2007/46*100*02", 1, "Germany", "2007/46", "100", "02")]
    [InlineData("e15*2007/46*1234*01", 15, null, "2007/46", "1234", "01")]
    [InlineData("e42*2018/858*999*03", 42, "EU", "2018/858", "999", "03")]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, int expectedCountryCode, string? expectedCountryName,
        string expectedDirective, string expectedTypeNumber, string expectedExtension)
    {
        var ok = EuTypeApprovalNumber.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedCountryCode, result!.ApprovalCountryCode);
        Assert.Equal(expectedCountryName, result.ApprovalCountryName);
        Assert.Equal(expectedDirective, result.Directive);
        Assert.Equal(expectedTypeNumber, result.TypeNumber);
        Assert.Equal(expectedExtension, result.Extension);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("e0*2007/46*1*1")]
    [InlineData("9*2007/46*1*1")]
    [InlineData("e99*2007/46*1*1")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = EuTypeApprovalNumber.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("e0*2007/46*1*1")]
    [InlineData("9*2007/46*1*1")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EuTypeApprovalNumber.Parse(input));
    }

    [Theory]
    [InlineData("e9*2007/46*6364*09", "e9*2007/46*6364*09")]
    [InlineData("E9*2007/46*6364*09", "e9*2007/46*6364*09")]
    [InlineData(" e5*2018/858*12345*01 ", "e5*2018/858*12345*01")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EuTypeApprovalNumber.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("invalid", EuTypeApprovalNumber.Format(" invalid ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_WithFallback_ReturnsNull_ForWhitespace()
    {
        Assert.Null(EuTypeApprovalNumber.Format(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("e9*2007/46*6364*09", "e9*2007/46*6364*09")]
    [InlineData("E9*2007/46*6364*09", "e9*2007/46*6364*09")]
    [InlineData(null, null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EuTypeApprovalNumber.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("invalid", EuTypeApprovalNumber.Normalize(" invalid ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(EuTypeApprovalNumber.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(EuTypeApprovalNumber.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("e9*2007/46*6364*09", true)]
    [InlineData("E9*2007/46*6364*09", false)]
    [InlineData(null, false)]
    [InlineData("invalid", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, EuTypeApprovalNumber.IsNormalized(input));
    }

    [Theory]
    [InlineData("e9*2007/46*6364*09", "e9*2007/46*6364*09")]
    [InlineData("E9*2007/46*6364*09", "e9*2007/46*6364*09")]
    public void ToString_ReturnsCanonicalForm(string input, string expected)
    {
        var approval = EuTypeApprovalNumber.Parse(input);
        Assert.Equal(expected, approval.ToString());
        Assert.Equal(expected, approval.ToNormalizedString());
    }

    [Fact]
    public void TryParse_UnassignedCountryCode_HasNullName()
    {
        var ok = EuTypeApprovalNumber.TryParse("e15*2007/46*1234*01", out var result);
        Assert.True(ok);
        Assert.Equal(15, result!.ApprovalCountryCode);
        Assert.Null(result.ApprovalCountryName);
    }

    [Fact]
    public void Value_NormalizesCountryCodeLeadingZero()
    {
        var ok = EuTypeApprovalNumber.TryParse("e09*2007/46*6364*09", out var result);
        Assert.True(ok);
        Assert.Equal("e9*2007/46*6364*09", result!.Value);
        Assert.Equal(9, result.ApprovalCountryCode);
    }

    [Fact]
    public void Equality_SameApproval()
    {
        var a = EuTypeApprovalNumber.Parse("e9*2007/46*6364*09");
        var b = EuTypeApprovalNumber.Parse("E9*2007/46*6364*09");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentApprovals()
    {
        var a = EuTypeApprovalNumber.Parse("e9*2007/46*6364*09");
        var b = EuTypeApprovalNumber.Parse("e5*2018/858*12345*01");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersByValue()
    {
        var a = EuTypeApprovalNumber.Parse("e5*2018/858*12345*01");
        var b = EuTypeApprovalNumber.Parse("e9*2007/46*6364*09");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Assert.Equal(1, EuTypeApprovalNumber.Parse("e9*2007/46*6364*09").CompareTo(null));
    }

    [Fact]
    public void Operators_LessThan_GreaterThan()
    {
        var a = EuTypeApprovalNumber.Parse("e5*2018/858*12345*01");
        var b = EuTypeApprovalNumber.Parse("e9*2007/46*6364*09");
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= b);
        Assert.True(b >= a);
        Assert.True(a <= EuTypeApprovalNumber.Parse("e5*2018/858*12345*01"));
        Assert.True(a >= EuTypeApprovalNumber.Parse("e5*2018/858*12345*01"));
    }
}
