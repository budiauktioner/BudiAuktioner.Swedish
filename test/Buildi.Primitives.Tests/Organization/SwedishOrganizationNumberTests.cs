using Buildi.Primitives.Organization;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishOrganizationNumberTests
{
    #region Sample Data
    
    // Valid Swedish Company Organization Numbers (from sample data)
    private const string BudiAB = "559246-0421";
    private const string InsolvaAB = "559444-7210";
    private const string MalardalensObestandstjanstAB = "559442-6677";
    private const string SundaHusLinkoepingAB = "556404-1373";
    private const string ActiveSolutionSverigeAB = "556557-3895";
    private const string OrneholmAB = "556984-7709";
    private const string ConnyOrneholmManagementAB = "559474-6686";

    // Valid Swedish Personal Identity Numbers
    private const string Pin1 = "193803032394";
    private const string Pin2 = "199102152387";
    private const string Pin3 = "200102192390";
    
    // Dödsbo (Estate) - starts with 1
    // Using valid number from README example
    private const string Dodsbo1 = "123456-7890";
    
    // Known Dödsbo organizations
    private const string DodsboBengtKarlsson_OrgNumber = "105555-5559";
    private const string DodsboBengtKarlsson_Name = "Bengt Karlsson";
    
    // Known BRF organizations
    private const string BrfNytorp1_OrgNumber = "769621-2716";
    private const string BrfNytorp1_Name = "BRF Nytorp 1";
    
    #endregion

    #region TryParse Tests

    [Theory]
    [InlineData(BudiAB)]
    [InlineData(InsolvaAB)]
    [InlineData(MalardalensObestandstjanstAB)]
    [InlineData(SundaHusLinkoepingAB)]
    [InlineData(ActiveSolutionSverigeAB)]
    [InlineData(OrneholmAB)]
    [InlineData(ConnyOrneholmManagementAB)]
    public void TryParse_ValidCompanyNumbers_ShouldReturnTrue(string input)
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.NotNull(orgNumber);
        Assert.False(orgNumber!.IsPerson);
    }

    [Theory]
    [InlineData(Pin1)]
    [InlineData(Pin2)]
    [InlineData(Pin3)]
    public void TryParse_ValidPersonalIdentityNumbers_ShouldReturnTrue(string input)
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.NotNull(orgNumber);
        Assert.True(orgNumber!.IsPerson);
    }

    [Theory]
    [InlineData("5592460421")] // 10 digits without separator
    [InlineData("559246-0421")] // 10 digits with dash
    [InlineData("16559246042 1")] // 12 digits with space
    [InlineData("16 559246-0421")] // 12 digits with space and dash
    public void TryParse_ValidFormats_ShouldParseCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.NotNull(orgNumber);
        Assert.Equal("5592460421", orgNumber!.To10DigitsOnly());
    }

    [Fact]
    public void TryParse_NullInput_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse(null, out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_EmptyString_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_WhitespaceOnly_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("   ", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_NonNumericText_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("invalid", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_TooShortInput_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("123", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_TooLongInput_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("12345678901234567890", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_TenDigitsWithInvalidChecksum_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationNumber.TryParse("1234567890", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    [Fact]
    public void TryParse_ValidFormatButWrongChecksum_ShouldReturnFalse()
    {
        // Arrange - should be 0421, not 0422
        var result = SwedishOrganizationNumber.TryParse("5592460422", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    #endregion

    #region Parse Tests

    [Fact]
    public void Parse_ValidInput_ShouldReturnOrganizationNumber()
    {
        // Act
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Assert
        Assert.NotNull(orgNumber);
        Assert.False(orgNumber.IsPerson);
    }

    [Fact]
    public void Parse_InvalidInput_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwedishOrganizationNumber.Parse("invalid"));
        Assert.Contains("Invalid Swedish organization number", exception.Message);
    }

    #endregion

    #region Formatting Tests

    [Fact]
    public void To10DigitsOnly_ShouldReturnDigitsWithoutSeparator()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Act
        var result = orgNumber.To10DigitsOnly();

        // Assert
        Assert.Equal("5592460421", result);
        Assert.DoesNotContain("-", result);
        Assert.DoesNotContain("+", result);
    }

    [Fact]
    public void To10DigitString_ForLegalEntity_ShouldReturnWithDash()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Act
        var result = orgNumber.To10DigitString();

        // Assert
        Assert.Equal("559246-0421", result);
    }

    [Fact]
    public void To12DigitString_ForLegalEntity_ShouldStartWith16()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Act
        var result = orgNumber.To12DigitString();

        // Assert
        Assert.StartsWith("16", result);
        Assert.Equal("165592460421", result);
    }

    [Fact]
    public void To12DigitString_ForPersonalIdentityNumber_ShouldStartWithCentury()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act
        var result = orgNumber.To12DigitString();

        // Assert
        Assert.StartsWith("19", result);
        Assert.Equal(12, result.Length);
    }

    #endregion

    #region Organization Type Hint Tests

    [Fact]
    public void GetSwedishOrganizationTypeHint_Aktiebolag_ShouldIdentifyCorrectly()
    {
        // Arrange - Organizations starting with 5 are typically AB
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Budi AB");

        // Assert
        Assert.Equal(SwedishOrganizationType.Aktiebolag, result.Certain);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Dodsbo_ShouldIdentifyCorrectly()
    {
        // Arrange - Numbers starting with 1 are dödsbo (estates)
        var orgNumber = SwedishOrganizationNumber.Parse(DodsboBengtKarlsson_OrgNumber);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint();

        // Assert
        Assert.Equal(SwedishOrganizationType.Dodsbo, result.Certain);
        Assert.Equal(SwedishOrganizationType.Dodsbo, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Kommun_ShouldIdentifyFromName()
    {
        // Arrange - Organizations starting with 2 are public sector
        var orgNumber = SwedishOrganizationNumber.Parse("212000-0142"); // Stockholm kommun

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Stockholms kommun");

        // Assert
        Assert.Equal(SwedishOrganizationType.OffentligSektor, result.Certain);
        Assert.Equal(SwedishOrganizationType.Kommun, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Bostadsrattsforening_ShouldIdentifyFromName()
    {
        // Arrange - Organizations starting with 7 are economic associations
        var orgNumber = SwedishOrganizationNumber.Parse(BrfNytorp1_OrgNumber);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("BRF Testgatan");

        // Assert
        Assert.Equal(SwedishOrganizationType.EkonomiskForening, result.Certain);
        Assert.Equal(SwedishOrganizationType.Bostadsrattsforening, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_IdeellForening_ShouldIdentifyCorrectly()
    {
        // Arrange - Organizations starting with 8 are non-profit/foundation
        // Using valid number: 882600-2035 (Stiftelsen Vansbrohem)
        var orgNumber = SwedishOrganizationNumber.Parse("882600-2035");

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Testföreningen");

        // Assert
        Assert.Equal(SwedishOrganizationType.IdeellForening, result.Certain);
        Assert.Equal(SwedishOrganizationType.IdeellForening, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Stiftelse_ShouldIdentifyFromName()
    {
        // Arrange - Organizations starting with 8 are non-profit/foundation
        // Using valid number: 882600-2035 (Stiftelsen Vansbrohem)
        var orgNumber = SwedishOrganizationNumber.Parse("882600-2035");

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Teststiftelsen");

        // Assert
        Assert.Equal(SwedishOrganizationType.IdeellForening, result.Certain);
        Assert.Equal(SwedishOrganizationType.Stiftelse, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_PersonalIdentityNumber_WithoutHints_ShouldReturnEnskildFirmaOrPrivatperson()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint();

        // Assert
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, result.Certain);
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_PersonalIdentityNumber_WithIsPrivatePersonTrue_ShouldReturnPrivatperson()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint(isPrivatePerson: true);

        // Assert
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, result.Certain);
        Assert.Equal(SwedishOrganizationType.Privatperson, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_PersonalIdentityNumber_WithIsPrivatePersonFalse_ShouldReturnEnskildFirma()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint(isPrivatePerson: false);

        // Assert
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, result.Certain);
        Assert.Equal(SwedishOrganizationType.EnskildFirma, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_PersonalIdentityNumber_WithEFInName_ShouldReturnEnskildFirma()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Test Företag EF");

        // Assert
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, result.Certain);
        Assert.Equal(SwedishOrganizationType.EnskildFirma, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Handelsbolag_ShouldIdentifyFromName()
    {
        // Arrange - Organizations starting with 6 or 9 can be HB/KB
        // Using valid number: 969782-5447 (Meta Bytes Handelsbolag)
        var orgNumber = SwedishOrganizationNumber.Parse("969782-5447");

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Test HB");

        // Assert
        Assert.Equal(SwedishOrganizationType.HandelsbolagEllerKommanditbolag, result.Certain);
        Assert.Equal(SwedishOrganizationType.Handelsbolag, result.BestGuess);
    }

    [Fact]
    public void GetSwedishOrganizationTypeHint_Kommanditbolag_ShouldIdentifyFromName()
    {
        // Arrange - Organizations starting with 6 or 9 can be HB/KB
        // Using valid number: 916463-3001 (Masmästaren Dalarna Kommanditbolag)
        var orgNumber = SwedishOrganizationNumber.Parse("916463-3001");

        // Act
        var result = orgNumber.GetSwedishOrganizationTypeHint("Test KB");

        // Assert
        Assert.Equal(SwedishOrganizationType.HandelsbolagEllerKommanditbolag, result.Certain);
        Assert.Equal(SwedishOrganizationType.Kommanditbolag, result.BestGuess);
    }

    #endregion

    #region IsPerson Tests

    [Fact]
    public void IsPerson_ForLegalEntity_ShouldReturnFalse()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(BudiAB);

        // Act & Assert
        Assert.False(orgNumber.IsPerson);
    }

    [Fact]
    public void IsPerson_ForPersonalIdentityNumber_ShouldReturnTrue()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse(Pin1);

        // Act & Assert
        Assert.True(orgNumber.IsPerson);
    }

    #endregion

    #region TenDigits Property Tests

    [Fact]
    public void TenDigits_ShouldReturnTenDigitsWithoutSeparator()
    {
        // Arrange
        var orgNumber = SwedishOrganizationNumber.Parse("559246-0421");

        // Act
        var result = orgNumber.To10DigitsOnly();

        // Assert
        Assert.Equal("5592460421", result);
        Assert.Equal(10, result.Length);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TryParse_WithSpaces_ShouldParseCorrectly()
    {
        // Arrange
        var input = "559 246 - 04 21";

        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal("5592460421", orgNumber!.To10DigitsOnly());
    }

    [Fact]
    public void TryParse_TwelveDigitsWithPrefix_ShouldParseCorrectly()
    {
        // Arrange
        var input = "165592460421";

        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal("5592460421", orgNumber!.To10DigitsOnly());
        Assert.Equal("165592460421", orgNumber.To12DigitString());
    }

    [Fact]
    public void TryParse_PersonalIdentityNumberWith10Digits_ShouldParseCorrectly()
    {
        // Arrange
        var input = "380303-2394"; // 10-digit format

        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.True(orgNumber!.IsPerson);
        Assert.StartsWith("19", orgNumber.To12DigitString());
    }

    [Fact]
    public void TryParse_InvalidLuhnChecksum_ShouldReturnFalse()
    {
        // Arrange
        var input = "559246-0422"; // Wrong checksum (should be 0421)

        // Act
        var result = SwedishOrganizationNumber.TryParse(input, out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Null(orgNumber);
    }

    #endregion

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishOrganizationNumber.Parse("559246-0421");
        var b = SwedishOrganizationNumber.Parse("559246-0421");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishOrganizationNumber.Parse("559246-0421");
        var b = SwedishOrganizationNumber.Parse("559444-7210");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishOrganizationNumber.Parse("559246-0421");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishOrganizationNumber.Parse("559246-0421");
        var b = SwedishOrganizationNumber.Parse("559444-7210");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishOrganizationNumber.Parse("559246-0421");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData(" ", false, ValidationErrorReason.InputIsEmpty)]
    [InlineData("ABC", false, ValidationErrorReason.InvalidFormat)]
    [InlineData("12345", false, ValidationErrorReason.InvalidLength)]
    [InlineData("0292460421", false, ValidationErrorReason.InvalidEntityPattern)]
    [InlineData("5592460422", false, ValidationErrorReason.InvalidCheckDigit)]
    [InlineData("5592460421", true, null)]
    [InlineData("165592460421", true, null)]
    [InlineData("199908072391", true, null)]
    public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)
    {
        var result = SwedishOrganizationNumber.Validate(input);

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
        var result = SwedishOrganizationNumber.Validate("ABC");

        Assert.Single(result.Issues);
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].EnglishDescription));
        Assert.False(string.IsNullOrWhiteSpace(result.Issues[0].LocalizedDescription));
    }

    [Theory]
    [InlineData("5592460421")]
    [InlineData("165592460421")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("5592460422")]
    public void Validate_IsValid_MatchesIsValid(string? input)
    {
        Assert.Equal(SwedishOrganizationNumber.IsValid(input), SwedishOrganizationNumber.Validate(input).IsValid);
    }
}
