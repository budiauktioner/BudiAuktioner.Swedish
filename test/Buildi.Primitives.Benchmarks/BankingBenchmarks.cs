using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Banking;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class BankingBenchmarks
{
    private const string ValidBankgiro = "58056201";
    private const string InvalidBankgiro = "0000000";

    private const string ValidPostgiro = "1234567";
    private const string InvalidPostgiro = "000";

    private const string ValidOcr = "12345682";
    private const string InvalidOcr = "00000001";

    private const string ValidBic = "ESSESESS";
    private const string InvalidBic = "XYZ";

    private const string ValidIbanSE = "SE4550000000058398257466";
    private const string ValidIbanDE = "DE89370400440532013000";
    private const string InvalidIban = "XX00INVALID";

    private const string ValidClearing = "5331";
    private const string InvalidClearing = "0000";

    private const string ValidBankAccount = "5331-0111922137";
    private const string InvalidBankAccount = "0000-0000000000";

    [Benchmark] public bool Bankgiro_TryParse_Valid() => SwedishBankgiroNumber.TryParse(ValidBankgiro, out _);
    [Benchmark] public bool Bankgiro_TryParse_Invalid() => SwedishBankgiroNumber.TryParse(InvalidBankgiro, out _);

    [Benchmark] public bool Postgiro_TryParse_Valid() => SwedishPostgiroNumber.TryParse(ValidPostgiro, out _);
    [Benchmark] public bool Postgiro_TryParse_Invalid() => SwedishPostgiroNumber.TryParse(InvalidPostgiro, out _);

    [Benchmark] public bool Ocr_TryParse_Valid() => SwedishOcrReferenceNumber.TryParse(ValidOcr, out _);
    [Benchmark] public bool Ocr_TryParse_Invalid() => SwedishOcrReferenceNumber.TryParse(InvalidOcr, out _);

    [Benchmark] public bool Bic_TryParse_Valid() => Bic.TryParse(ValidBic, out _);
    [Benchmark] public bool Bic_TryParse_Invalid() => Bic.TryParse(InvalidBic, out _);

    [Benchmark] public bool Iban_TryParse_SE() => Iban.TryParse(ValidIbanSE, out _);
    [Benchmark] public bool Iban_TryParse_DE() => Iban.TryParse(ValidIbanDE, out _);
    [Benchmark] public bool Iban_TryParse_Invalid() => Iban.TryParse(InvalidIban, out _);

    [Benchmark] public bool Clearing_TryParse_Valid() => SwedishBankClearingNumber.TryParse(ValidClearing, out _);
    [Benchmark] public bool Clearing_TryParse_Invalid() => SwedishBankClearingNumber.TryParse(InvalidClearing, out _);

    [Benchmark] public bool BankAccount_TryParse_Valid() => SwedishBankAccount.TryParse(ValidBankAccount, out _);
    [Benchmark] public bool BankAccount_TryParse_Invalid() => SwedishBankAccount.TryParse(InvalidBankAccount, out _);
}
