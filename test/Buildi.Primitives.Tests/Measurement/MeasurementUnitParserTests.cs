using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

/// <summary>
/// Tests the shared number+unit parsing logic indirectly through <see cref="Length.TryParse"/>,
/// since <c>MeasurementUnitParser</c> is internal.
/// </summary>
public class MeasurementUnitParserTests
{
    [Theory]
    [InlineData("10 m", 10)]
    [InlineData("5.5 m", 5.5)]
    [InlineData("5,5 m", 5.5)]
    [InlineData("3.14 m", 3.14)]
    [InlineData("3,14 m", 3.14)]
    [InlineData("0.001 km", 1)]
    [InlineData("0,001 km", 1)]
    [InlineData("100cm", 1)]
    [InlineData("100 cm", 1)]
    [InlineData("  10  m  ", 10)]
    [InlineData("-5 m", -5)]
    [InlineData("+5 m", 5)]
    [InlineData("1 000 m", 1000)]
    [InlineData("1.000,5 km", 1000500)]
    [InlineData("1,000.5 km", 1000500)]
    [InlineData("1 000,50 m", 1000.50)]
    public void DecimalParsing_VariousFormats(string input, double expectedMeters)
    {
        Assert.True(Length.TryParse(input, out var result), $"Failed to parse: {input}");
        Assert.Equal((decimal)expectedMeters, result!.Meters);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("km")]
    [InlineData("km 10")]
    public void InvalidInputs_ReturnFalse(string? input)
    {
        Assert.False(Length.TryParse(input, out _));
    }

    [Theory]
    [InlineData("5,5 m", "5.5 m")]
    [InlineData("3,14 m", "3.14 m")]
    [InlineData("1 000 m", "1000 m")]
    [InlineData("1.000,5 m", "1000.5 m")]
    [InlineData("1,000.5 m", "1000.5 m")]
    public void CommaDecimal_FormatsWithPeriod(string input, string expectedFormat)
    {
        Assert.Equal(expectedFormat, Length.Format(input));
    }

    [Theory]
    [InlineData("85%", 0.85)]
    [InlineData("85,5%", 0.855)]
    [InlineData("0.5%", 0.005)]
    [InlineData("0,5%", 0.005)]
    [InlineData("99,99%", 0.9999)]
    public void Percentage_CommaDecimalParsing(string input, double expectedValue)
    {
        Assert.True(Percentage.TryParse(input, out var result), $"Failed to parse: {input}");
        Assert.Equal((decimal)expectedValue, result!.Value);
    }
}
