using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishOrganizationIdentifierParserTests
{
    #region Sample Data
    
    // Swedish Organization Numbers (from sample data)
    private const string BudiAB = "559246-0421";
    private const string InsolvaAB = "559444-7210";
    private const string SundaHusLinkoepingAB = "556404-1373";
    
    // Swedish Personal Identity Numbers
    private const string Pin1 = "193803032394";
    private const string Pin2 = "199102152387";
    
    // VAT Numbers
    private const string SwedishVat = "SE556404137301"; // SundaHus
    private const string PolishVat1 = "PL5258567891";
    private const string PolishVat2 = "PL7389456124";
    private const string DanishVat1 = "DK47851234";
    private const string DanishVat2 = "DK86421357";
    private const string FinnishVat1 = "FI23456780"; // Valid MOD 11-2 checksum
    private const string FinnishVat2 = "FI87654321"; // Valid MOD 11-2 checksum
    
    // DUNS Numbers
    private const string Duns1 = "123456789";
    private const string Duns2 = "987654321";
    private const string BudiDuns = "350827673"; // Budi's D-U-N-S number
    
    // LEI Codes
    private const string Lei1 = "5493000IBP32UQZ0KL24";
    private const string Lei2 = "529900T8BM49AURSDO55";
    
    #endregion

    #region TryParse - Swedish Organization Numbers

    [Theory]
    [InlineData(BudiAB)]
    [InlineData(InsolvaAB)]
    [InlineData(SundaHusLinkoepingAB)]
    public void TryParse_SwedishOrganizationNumber_ShouldIdentifyCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishOrganizationNumber, orgNumber.Type);
        Assert.NotNull(orgNumber.SwedishOrganizationNumber);
        Assert.Null(orgNumber.EuVatNumber);
        Assert.Null(orgNumber.DunsNumber);
        Assert.Null(orgNumber.LeiCode);
    }

    [Theory]
    [InlineData(Pin1)]
    [InlineData(Pin2)]
    public void TryParse_SwedishPersonalIdentityNumber_ShouldIdentifyCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishPersonalIdentityNumber, orgNumber.Type);
        Assert.NotNull(orgNumber.SwedishOrganizationNumber);
        Assert.True(orgNumber.SwedishOrganizationNumber!.IsPerson);
    }

    [Fact]
    public void TryParse_SwedishOrgNumber_WithCompanyName_ShouldHintSwedishOrganizationType()
    {
        // Arrange
        var input = BudiAB;
        var name = "Budi AB";

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, name);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_SwedishPIN_WithoutHints_ShouldReturnEnskildFirmaEllerPrivatperson()
    {
        // Arrange
        var input = Pin1;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_SwedishPIN_WithIsPrivatePersonTrue_ShouldReturnPrivatperson()
    {
        // Arrange
        var input = Pin1;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, isPrivatePerson: true);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Privatperson, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_SwedishPIN_WithIsPrivatePersonFalse_ShouldReturnEnskildFirma()
    {
        // Arrange
        var input = Pin1;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, isPrivatePerson: false);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.EnskildFirmaEllerPrivatperson, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.EnskildFirma, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region TryParse - VAT Numbers

    [Theory]
    [InlineData(SwedishVat)]
    [InlineData(PolishVat1)]
    [InlineData(PolishVat2)]
    [InlineData(DanishVat1)]
    [InlineData(DanishVat2)]
    [InlineData(FinnishVat1)]
    [InlineData(FinnishVat2)]
    public void TryParse_EuVatNumber_ShouldIdentifyCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
        Assert.NotNull(orgNumber.EuVatNumber);
        Assert.Null(orgNumber.SwedishOrganizationNumber);
        Assert.Null(orgNumber.DunsNumber);
        Assert.Null(orgNumber.LeiCode);
    }

    [Fact]
    public void TryParse_SwedishEuVatNumber_ShouldExtractSwedishOrganizationTypeHint()
    {
        // Arrange
        var input = SwedishVat;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, "SundaHus AB");

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_NonSwedishEuVatNumber_ShouldHaveUnknownSwedishOrganizationType()
    {
        // Arrange
        var input = PolishVat1;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region TryParse - DUNS Numbers

    [Theory]
    [InlineData(Duns1)]
    [InlineData(Duns2)]
    [InlineData(BudiDuns)] // Budi's D-U-N-S number
    public void TryParse_DunsNumber_ShouldIdentifyCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.DunsNumber, orgNumber.Type);
        Assert.NotNull(orgNumber.DunsNumber);
        Assert.Null(orgNumber.SwedishOrganizationNumber);
        Assert.Null(orgNumber.EuVatNumber);
        Assert.Null(orgNumber.LeiCode);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region TryParse - LEI Codes

    [Theory]
    [InlineData(Lei1)]
    [InlineData(Lei2)]
    public void TryParse_LeiCode_ShouldIdentifyCorrectly(string input)
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.LeiCode, orgNumber.Type);
        Assert.NotNull(orgNumber.LeiCode);
        Assert.Null(orgNumber.SwedishOrganizationNumber);
        Assert.Null(orgNumber.EuVatNumber);
        Assert.Null(orgNumber.DunsNumber);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region TryParse - Invalid Input

    [Fact]
    public void TryParse_NullInput_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(null, out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_EmptyString_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse("", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_WhitespaceOnly_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse("   ", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_InvalidText_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse("INVALID", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_SpecialCharactersOnly_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse("!@#$%^&*()", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Unknown, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region Parse Tests

    [Fact]
    public void Parse_ValidSwedishOrganizationNumber_ShouldReturnOrganizationNumber()
    {
        // Act
        var orgNumber = SwedishOrganizationIdentifierParser.Parse(BudiAB);

        // Assert
        Assert.NotNull(orgNumber);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishOrganizationNumber, orgNumber.Type);
    }

    [Fact]
    public void Parse_ValidEuVatNumber_ShouldReturnOrganizationNumber()
    {
        // Act
        var orgNumber = SwedishOrganizationIdentifierParser.Parse(PolishVat1);

        // Assert
        Assert.NotNull(orgNumber);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
    }

    [Fact]
    public void Parse_InvalidInput_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwedishOrganizationIdentifierParser.Parse("INVALID"));
        Assert.Contains("Could not parse input as a known organization/identity number", exception.Message);
    }

    #endregion

    #region TryParseForCountry Tests

    [Fact]
    public void TryParseForCountry_SwedishOrgNumber_WithSE_ShouldParse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(BudiAB, "SE", out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishOrganizationNumber, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_SwedishOrgNumber_WithDifferentCountry_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(BudiAB, "NO", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_PolishVat_WithPL_ShouldParse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(PolishVat1, "PL", out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
        Assert.Equal("PL", orgNumber.EuVatNumber!.CountryCode);
    }

    [Fact]
    public void TryParseForCountry_PolishVat_WithDifferentCountry_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(PolishVat1, "SE", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_DunsNumber_ShouldNotParse()
    {
        // Act - DUNS numbers don't have country information
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(Duns1, "SE", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_LeiCode_ShouldNotParse()
    {
        // Act - LEI codes don't have country information
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(Lei1, "SE", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_NullCountryCode_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(BudiAB, null!, out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_EmptyCountryCode_ShouldReturnFalse()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(BudiAB, "", out var orgNumber);

        // Assert
        Assert.False(result);
        Assert.Equal(SwedishOrganizationIdentifierType.Unknown, orgNumber.Type);
    }

    [Fact]
    public void TryParseForCountry_SwedishVat_WithSE_ShouldExtractSwedishOrganizationTypeHint()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParseForCountry(SwedishVat, "SE", out var orgNumber, "SundaHus AB");

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.EuVatNumber, orgNumber.Type);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintCertain);
        Assert.Equal(SwedishOrganizationType.Aktiebolag, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion

    #region ParseForCountry Tests

    [Fact]
    public void ParseForCountry_ValidSwedishOrganizationNumber_WithSE_ShouldParse()
    {
        // Act
        var orgNumber = SwedishOrganizationIdentifierParser.ParseForCountry(BudiAB, "SE");

        // Assert
        Assert.NotNull(orgNumber);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishOrganizationNumber, orgNumber.Type);
    }

    [Fact]
    public void ParseForCountry_SwedishOrgNumber_WithDifferentCountry_ShouldThrow()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwedishOrganizationIdentifierParser.ParseForCountry(BudiAB, "NO"));
        Assert.Contains("Could not parse input as a known organization/identity number for country NO", exception.Message);
    }

    [Fact]
    public void ParseForCountry_InvalidInput_ShouldThrow()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwedishOrganizationIdentifierParser.ParseForCountry("INVALID", "SE"));
        Assert.Contains("Could not parse input as a known organization/identity number for country SE", exception.Message);
    }

    #endregion

    #region NormalizedValue Tests

    [Fact]
    public void TryParse_SwedishOrganizationNumber_ShouldHave12DigitNormalizedValue()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(BudiAB, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal("165592460421", orgNumber.NormalizedValue);
        Assert.Equal(12, orgNumber.NormalizedValue.Length);
    }

    [Fact]
    public void TryParse_EuVatNumber_ShouldHaveCountryCodePlusBodyNormalizedValue()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(PolishVat1, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.StartsWith("PL", orgNumber.NormalizedValue);
        Assert.Equal(orgNumber.EuVatNumber!.CountryCode + orgNumber.EuVatNumber.Body, orgNumber.NormalizedValue);
    }

    [Fact]
    public void TryParse_DunsNumber_ShouldHave9DigitNormalizedValue()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(Duns1, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(Duns1, orgNumber.NormalizedValue);
        Assert.Equal(9, orgNumber.NormalizedValue.Length);
    }

    [Fact]
    public void TryParse_LeiCode_ShouldHave20CharacterNormalizedValue()
    {
        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(Lei1, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(Lei1.ToUpperInvariant(), orgNumber.NormalizedValue);
        Assert.Equal(20, orgNumber.NormalizedValue.Length);
    }

    #endregion

    #region Priority Tests (testing parsing priority order)

    [Fact]
    public void TryParse_AmbiguousInput_ShouldPreferSwedishOrganizationNumber()
    {
        // Arrange - A Swedish org number might also match as DUNS if we're not careful
        var input = BudiAB;

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationIdentifierType.SwedishOrganizationNumber, orgNumber.Type);
        Assert.NotNull(orgNumber.SwedishOrganizationNumber);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TryParse_SwedishPIN_WithEFInName_ShouldInferEnskildFirma()
    {
        // Arrange
        var input = Pin1;
        var name = "Anna Svensson EF";

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, name);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.EnskildFirma, orgNumber.OrganizationTypeHintBestGuess);
    }

    [Fact]
    public void TryParse_SwedishPIN_WithPrivatpersonInName_ShouldInferPrivatperson()
    {
        // Arrange
        var input = Pin1;
        var name = "Anna Svensson privatperson";

        // Act
        var result = SwedishOrganizationIdentifierParser.TryParse(input, out var orgNumber, name);

        // Assert
        Assert.True(result);
        Assert.Equal(SwedishOrganizationType.Privatperson, orgNumber.OrganizationTypeHintBestGuess);
    }

    #endregion
}
