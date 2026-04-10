using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class TransmissionTypeTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(7, TransmissionType.All.Count);
    }

    [Theory]
    [InlineData("Manual")]
    [InlineData("Automatic")]
    [InlineData("CVT")]
    [InlineData("Dual clutch")]
    [InlineData("Sequential")]
    [InlineData("Semi-automatic")]
    [InlineData("AMT")]
    [InlineData("Manuell")]
    [InlineData("manuell")]
    [InlineData("MANUELL")]
    [InlineData("M/T")]
    [InlineData("Automat")]
    [InlineData("automat")]
    [InlineData("A/T")]
    [InlineData("Auto")]
    [InlineData("Tiptronic")]
    [InlineData("Steglös")]
    [InlineData("Variator")]
    [InlineData("DCT")]
    [InlineData("DSG")]
    [InlineData("PDK")]
    [InlineData("S-tronic")]
    [InlineData("SMG")]
    [InlineData("Halvautomatisk")]
    [InlineData("EasyTronic")]
    [InlineData("Robotized")]
    [InlineData("  Manual  ")]
    [InlineData("Handväxlad")]
    [InlineData("Stick")]
    [InlineData("Standard")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(TransmissionType.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Diesel")]
    [InlineData("Hybrid")]
    [InlineData("Turbo")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(TransmissionType.IsValid(input));
    }

    [Theory]
    [InlineData("Manual", "Manual", "Manual", "Manuell")]
    [InlineData("Manuell", "Manual", "Manual", "Manuell")]
    [InlineData("M/T", "Manual", "Manual", "Manuell")]
    [InlineData("Handväxlad", "Manual", "Manual", "Manuell")]
    [InlineData("Stick", "Manual", "Manual", "Manuell")]
    [InlineData("Automat", "Automatic", "Automatic", "Automat")]
    [InlineData("A/T", "Automatic", "Automatic", "Automat")]
    [InlineData("Tiptronic", "Automatic", "Automatic", "Automat")]
    [InlineData("Momentomvandlare", "Automatic", "Automatic", "Automat")]
    [InlineData("Steglös", "CVT", "CVT", "CVT")]
    [InlineData("Variator", "CVT", "CVT", "CVT")]
    [InlineData("Xtronic", "CVT", "CVT", "CVT")]
    [InlineData("e-CVT", "CVT", "CVT", "CVT")]
    [InlineData("DSG", "Dual clutch", "Dual clutch", "Dubbelkoppling")]
    [InlineData("PDK", "Dual clutch", "Dual clutch", "Dubbelkoppling")]
    [InlineData("S-tronic", "Dual clutch", "Dual clutch", "Dubbelkoppling")]
    [InlineData("SMG", "Sequential", "Sequential", "Sekventiell")]
    [InlineData("Halvautomatisk", "Semi-automatic", "Semi-automatic", "Halvautomatisk")]
    [InlineData("Automated manual", "Semi-automatic", "Semi-automatic", "Halvautomatisk")]
    [InlineData("EasyTronic", "AMT", "AMT", "AMT")]
    [InlineData("iMT", "AMT", "AMT", "AMT")]
    [InlineData("Robotized", "AMT", "AMT", "AMT")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedEnglish, string expectedSwedish)
    {
        var ok = TransmissionType.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedEnglish, result.EnglishName);
        Assert.Equal(expectedSwedish, result.LocalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Diesel")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = TransmissionType.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Diesel")]
    [InlineData("Turbo")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => TransmissionType.Parse(input));
    }

    [Theory]
    [InlineData("Manual", "Manual")]
    [InlineData("Manuell", "Manual")]
    [InlineData("DSG", "Dual clutch")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, TransmissionType.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", TransmissionType.Normalize(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(TransmissionType.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(TransmissionType.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", TransmissionType.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("Manual", true)]
    [InlineData("manual", false)]
    [InlineData("Manuell", false)]
    [InlineData("CVT", true)]
    [InlineData("Dual clutch", true)]
    [InlineData("AMT", true)]
    [InlineData("nope", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, TransmissionType.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        var manual = TransmissionType.Parse("Manual");
        var display = manual.ToString();
        Assert.True(display == "Manual" || display == "Manuell");
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var auto = TransmissionType.Parse("Automat");
        Assert.Equal("Automatic", auto.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameType()
    {
        var a = TransmissionType.Parse("Manual");
        var b = TransmissionType.Parse("Manuell");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentTypes()
    {
        var a = TransmissionType.Parse("Manual");
        var b = TransmissionType.Parse("Automatic");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_Null()
    {
        var a = TransmissionType.Parse("Manual");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = TransmissionType.Parse("Manual");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Fact]
    public void Format_ReturnsNull_ForInvalidInput()
    {
        Assert.Null(TransmissionType.Format(null));
        Assert.Null(TransmissionType.Format(""));
        Assert.Null(TransmissionType.Format("nope"));
    }

    [Fact]
    public void Parse_ReturnsSameInstance()
    {
        var a = TransmissionType.Parse("Manual");
        Assert.Same(TransmissionType.Manual, a);
    }
}
