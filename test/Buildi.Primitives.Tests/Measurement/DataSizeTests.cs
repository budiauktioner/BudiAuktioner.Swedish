using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class DataSizeTests
{
    [Theory]
    [InlineData("1024 B")]
    [InlineData("1 KB")]
    [InlineData("1 KiB")]
    [InlineData("2.5 MB")]
    [InlineData("500 megabytes")]
    [InlineData("1 GiB")]
    [InlineData("10 TB")]
    [InlineData("1 PiB")]
    [InlineData("100 bytes")]
    [InlineData("1 EB")]
    [InlineData("1 EiB")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(DataSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("MB 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(DataSize.IsValid(input));
    }

    [Theory]
    [InlineData("1 KB", 1000)]
    [InlineData("1 KiB", 1024)]
    [InlineData("2 MB", 2_000_000)]
    [InlineData("1 GB", 1_000_000_000)]
    public void TryParse_ReturnsExpected_Bytes(string input, double expectedBytes)
    {
        Assert.True(DataSize.TryParse(input, out var result));
        Assert.Equal((decimal)expectedBytes, result!.Bytes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(DataSize.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => DataSize.Parse(input));
    }

    [Theory]
    [InlineData("10 MB", "10 MB")]
    [InlineData("1 KiB", "1 KiB")]
    [InlineData("5.5 GB", "5.5 GB")]
    [InlineData("1 EB", "1 EB")]
    [InlineData("1 EiB", "1 EiB")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DataSize.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 MB", DataSize.Format("1 GB", unit: DataSizeUnit.Megabyte));
        Assert.Equal("1 GB", DataSize.Format("1000 MB", unit: DataSizeUnit.Gigabyte));
        Assert.Equal("1 GB", DataSize.Format("1 GB"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("3 GB", DataSize.Format("2.567 GB", decimals: 0));
        Assert.Equal("2.6 GB", DataSize.Format("2.567 GB", decimals: 1));
        Assert.Equal("2.567 GB", DataSize.Format("2.567 GB"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var d = DataSize.Parse("2.567 GB");
        Assert.Equal("3 GB", d.ToString(DataSizeUnit.Gigabyte, decimals: 0));
        Assert.Equal("2.6 GB", d.ToString(DataSizeUnit.Gigabyte, decimals: 1));
    }

    [Theory]
    [InlineData("1 KB", "1000 B")]
    [InlineData("1 KiB", "1024 B")]
    [InlineData("1000 B", "1000 B")]
    [InlineData("2 MB", "2000000 B")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DataSize.Normalize(input));
    }

    [Theory]
    [InlineData("10 MB", "10 MB")]
    [InlineData("1 KiB", "1 KiB")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var size = DataSize.Parse(input);
        Assert.Equal(expected, size.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var size = DataSize.FromKilobytes(1);
        Assert.Equal("1000 B", size.ToString(DataSizeUnit.Byte));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var size = DataSize.FromMegabytes(1);
        Assert.Equal(1_000_000m, size.Bytes);
        Assert.Equal(1m, size.Megabytes);
        Assert.Equal(1000m, size.Kilobytes);
        Assert.Equal(1_000_000m / 1024m, size.Kibibytes);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var size = DataSize.FromKibibytes(1);
        Assert.Equal(1024m, size.In(DataSizeUnit.Byte));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = DataSize.FromBytes(100);
        var b = DataSize.FromBytes(200);
        Assert.Equal(300m, (a + b).Bytes);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = DataSize.FromBytes(200);
        var b = DataSize.FromBytes(50);
        Assert.Equal(150m, (a - b).Bytes);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = DataSize.FromBytes(100);
        Assert.Equal(300m, (a * 3).Bytes);
        Assert.Equal(300m, (3 * a).Bytes);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = DataSize.FromBytes(300);
        Assert.Equal(100m, (a / 3).Bytes);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = DataSize.FromBytes(100);
        var b = DataSize.FromBytes(200);
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
        var a = DataSize.FromKilobytes(1);
        var b = DataSize.FromBytes(1000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var size = DataSize.Parse("5 MB");
        Assert.Same(DataSizeUnit.Megabyte, size.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsDataSizes()
    {
        var text = "The log is 10 MB and the image is 2 GiB.";
        var candidates = DataSize.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var size = DataSize.Parse("10 MB");
        Assert.Equal("*** MB", size.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(DataSize.IsNormalized("512 B"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(DataSize.IsNormalized("1 MB"));
    }

    [Theory]
    [InlineData("B", "B")]
    [InlineData("KB", "KB")]
    [InlineData("KiB", "KiB")]
    [InlineData("megabyte", "MB")]
    [InlineData("mebibyte", "MiB")]
    [InlineData("bytes", "B")]
    [InlineData("kilobytes", "KB")]
    [InlineData("megabytes", "MB")]
    [InlineData("gigabytes", "GB")]
    [InlineData("terabytes", "TB")]
    [InlineData("petabytes", "PB")]
    [InlineData("kibibytes", "KiB")]
    [InlineData("mebibytes", "MiB")]
    [InlineData("gibibytes", "GiB")]
    [InlineData("tebibytes", "TiB")]
    [InlineData("pebibytes", "PiB")]
    [InlineData("EB", "EB")]
    [InlineData("exabyte", "EB")]
    [InlineData("exabytes", "EB")]
    [InlineData("EiB", "EiB")]
    [InlineData("exbibyte", "EiB")]
    [InlineData("exbibytes", "EiB")]
    public void DataSizeUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(DataSizeUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 GB")]
    [InlineData("2.5 TB")]
    [InlineData("0,5 MB")]
    [InlineData("3.14 KB")]
    [InlineData("1 000 B")]
    [InlineData("1,5 GiB")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(DataSize.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 KB", 5500)]
    [InlineData("2.5 MB", 2500000)]
    [InlineData("1 000 B", 1000)]
    [InlineData("1,5 GiB", 1610612736)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedBytes)
    {
        Assert.True(DataSize.TryParse(input, out var result));
        Assert.Equal((decimal)expectedBytes, result!.Bytes);
    }

    [Theory]
    [InlineData("5,5 GB", "5.5 GB")]
    [InlineData("  512  MB  ", "512 MB")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, DataSize.Format(input));
    }

    [Theory]
    [InlineData("2.5 KB", "2500 B")]
    [InlineData("5,5 B", "5.5 B")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, DataSize.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = DataSize.FromKilobytes(1.5m);
        var b = DataSize.FromBytes(500);
        Assert.Equal(2000m, (a + b).Bytes);
    }

    [Fact]
    public void Conversions_ExabyteToBytes()
    {
        var size = DataSize.Parse("1 EB");
        Assert.Equal(1_000_000_000_000_000_000m, size.Bytes);
    }

    [Fact]
    public void Conversions_ExbibyteToBytes()
    {
        var size = DataSize.Parse("1 EiB");
        Assert.Equal(1152921504606846976m, size.Bytes);
    }

    [Fact]
    public void FromFactory_Exabytes()
    {
        var size = DataSize.FromExabytes(2);
        Assert.Equal(2m, size.Exabytes);
    }

    [Fact]
    public void FromFactory_Exbibytes()
    {
        var size = DataSize.FromExbibytes(1);
        Assert.Equal(1m, size.Exbibytes);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(DataSize.TryParse("99999999999999 EB", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = DataSize.Parse("1 GB");
        var b = DataSize.Parse("2 GB");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = DataSize.Parse("1 GB");
        Assert.Equal(1, a.CompareTo(null));
    }
}
