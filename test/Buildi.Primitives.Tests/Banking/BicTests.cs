using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Banking;

public class BicTests
{
    [Theory]
    [InlineData("NDEASESS", "NDEA", "SE", "SS", null, true)]
    [InlineData("NDEASESSXXX", "NDEA", "SE", "SS", "XXX", false)]
    [InlineData("DEUTDEFF500", "DEUT", "DE", "FF", "500", false)]
    public void TryParse_ReturnsExpectedComponents(
        string input,
        string institutionCode,
        string countryCode,
        string locationCode,
        string? branchCode,
        bool isPrimaryOffice)
    {
        var ok = Bic.TryParse(input, out var bic);

        Assert.True(ok);
        Assert.NotNull(bic);
        Assert.Equal(input, bic!.Code);
        Assert.Equal(institutionCode, bic.InstitutionCode);
        Assert.Equal(countryCode, bic.CountryCode);
        Assert.Equal(locationCode, bic.LocationCode);
        Assert.Equal(branchCode, bic.BranchCode);
        Assert.Equal(isPrimaryOffice, bic.IsPrimaryOffice);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NDEASE")]
    [InlineData("NDEASESSXX")]
    [InlineData("1234SESS")]
    [InlineData("NDEA00SS")]
    public void IsValid_ReturnsFalse_ForInvalidInput(string? input)
    {
        Assert.False(Bic.IsValid(input));
    }

    [Fact]
    public void Format_Normalize_And_ToString_ReturnExpectedValues()
    {
        var bic = Bic.Parse(" ndea sess xxx ");

        Assert.Equal("NDEASESSXXX", Bic.Format(" ndea sess xxx "));
        Assert.Equal("NDEASESSXXX", Bic.Normalize(" ndea sess xxx "));
        Assert.Equal("NDEASESSXXX", bic.ToNormalizedString());
        Assert.Equal("NDEASESSXXX", bic.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Bic.Parse("NDEASESS");
        var b = Bic.Parse("ndeasess");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Bic.Parse("NDEASESS");
        var b = Bic.Parse("DEUTDEFF500");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Bic.Parse("NDEASESS");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Bic.Parse("DEUTDEFF500");
        var b = Bic.Parse("NDEASESS");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Bic.Parse("NDEASESS");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("ABC", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("NDEAQQSS", false, ValidationErrorReason.UnknownCountryCode)]
    [InlineData("NDEASESS", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = Bic.Validate(input);

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

    [Fact]
    public void Validate_Issues_ContainBothLanguageDescriptions()
    {
        var result = Bic.Validate("ABC");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("NDEASESS")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("NDEAQQSS")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(Bic.IsValid(input), Bic.Validate(input).IsValid);
    }
}
