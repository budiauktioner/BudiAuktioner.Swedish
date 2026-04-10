using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ProductBenchmarks
{
    private const string ValidGtin8 = "96385074";
    private const string InvalidGtin8 = "00000000";

    private const string ValidGtin12 = "012345678905";
    private const string InvalidGtin12 = "000000000000";

    private const string ValidGtin13 = "5901234123457";
    private const string InvalidGtin13 = "0000000000000";

    private const string ValidGtin14 = "15901234123451";
    private const string InvalidGtin14 = "00000000000000";

    private const string ValidGtinAny = "5901234123457";
    private const string InvalidGtinAny = "XYZ";

    private const string ValidHsCode = "8471.30";
    private const string InvalidHsCode = "0000";

    private const string ValidGoogleCategory = "166";
    private const string InvalidGoogleCategory = "0";

    [Benchmark] public bool Gtin8_TryParse_Valid() => Gtin8.TryParse(ValidGtin8, out _);
    [Benchmark] public bool Gtin8_TryParse_Invalid() => Gtin8.TryParse(InvalidGtin8, out _);

    [Benchmark] public bool Gtin12_TryParse_Valid() => Gtin12.TryParse(ValidGtin12, out _);
    [Benchmark] public bool Gtin12_TryParse_Invalid() => Gtin12.TryParse(InvalidGtin12, out _);

    [Benchmark] public bool Gtin13_TryParse_Valid() => Gtin13.TryParse(ValidGtin13, out _);
    [Benchmark] public bool Gtin13_TryParse_Invalid() => Gtin13.TryParse(InvalidGtin13, out _);

    [Benchmark] public bool Gtin14_TryParse_Valid() => Gtin14.TryParse(ValidGtin14, out _);
    [Benchmark] public bool Gtin14_TryParse_Invalid() => Gtin14.TryParse(InvalidGtin14, out _);

    [Benchmark] public bool Gtin_TryParse_Valid() => Gtin.TryParse(ValidGtinAny, out _);
    [Benchmark] public bool Gtin_TryParse_Invalid() => Gtin.TryParse(InvalidGtinAny, out _);

    [Benchmark] public bool HsCode_TryParse_Valid() => HsCode.TryParse(ValidHsCode, out _);
    [Benchmark] public bool HsCode_TryParse_Invalid() => HsCode.TryParse(InvalidHsCode, out _);

    [Benchmark] public bool GoogleCategory_TryParse_Valid() => GoogleProductCategory.TryParse(ValidGoogleCategory, out _);
    [Benchmark] public bool GoogleCategory_TryParse_Invalid() => GoogleProductCategory.TryParse(InvalidGoogleCategory, out _);
}
