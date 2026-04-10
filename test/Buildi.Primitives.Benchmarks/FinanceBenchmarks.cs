using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Finance;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class FinanceBenchmarks
{
    private const string ValidCurrencyCode = "SEK";
    private const string ValidCurrencyName = "Kronor";
    private const string InvalidCurrency = "XYZ";

    private const string ValidMoney = "1 234,50 SEK";
    private const string InvalidMoney = "not money";

    [Benchmark] public bool Currency_TryParse_Code() => Currency.TryParse(ValidCurrencyCode, out _);
    [Benchmark] public bool Currency_TryParse_Name() => Currency.TryParse(ValidCurrencyName, out _);
    [Benchmark] public bool Currency_TryParse_Invalid() => Currency.TryParse(InvalidCurrency, out _);

    [Benchmark] public bool MoneyAmount_TryParse_Valid() => MoneyAmount.TryParse(ValidMoney, out _);
    [Benchmark] public bool MoneyAmount_TryParse_Invalid() => MoneyAmount.TryParse(InvalidMoney, out _);
}
