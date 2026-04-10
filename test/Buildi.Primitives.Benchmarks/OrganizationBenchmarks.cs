using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class OrganizationBenchmarks
{
    private const string ValidOrgNumber = "5592460421";
    private const string InvalidOrgNumber = "1234567890";

    private const string ValidVatSE = "SE559246042101";
    private const string ValidVatDE = "DE123456789";
    private const string InvalidVat = "XX000";

    private const string ValidDuns = "362498394";
    private const string InvalidDuns = "12345";

    private const string ValidLei = "529900T8BM49AURSDO55";
    private const string InvalidLei = "INVALID";

    private const string ValidSni = "62010";
    private const string InvalidSni = "00000";

    private const string ValidCfar = "12345678";
    private const string InvalidCfar = "1234";

    private const string ValidOrgName = "Budi Auktioner AB";
    private const string InvalidOrgName = "";

    [Benchmark] public bool OrgNumber_TryParse_Valid() => SwedishOrganizationNumber.TryParse(ValidOrgNumber, out _);
    [Benchmark] public bool OrgNumber_TryParse_Invalid() => SwedishOrganizationNumber.TryParse(InvalidOrgNumber, out _);

    [Benchmark] public bool EuVatNumber_TryParse_SE() => EuVatNumber.TryParse(ValidVatSE, out _);
    [Benchmark] public bool EuVatNumber_TryParse_DE() => EuVatNumber.TryParse(ValidVatDE, out _);
    [Benchmark] public bool EuVatNumber_TryParse_Invalid() => EuVatNumber.TryParse(InvalidVat, out _);

    [Benchmark] public bool DunsNumber_TryParse_Valid() => DunsNumber.TryParse(ValidDuns, out _);
    [Benchmark] public bool DunsNumber_TryParse_Invalid() => DunsNumber.TryParse(InvalidDuns, out _);

    [Benchmark] public bool LeiCode_TryParse_Valid() => LeiCode.TryParse(ValidLei, out _);
    [Benchmark] public bool LeiCode_TryParse_Invalid() => LeiCode.TryParse(InvalidLei, out _);

    [Benchmark] public bool SniCode_TryParse_Valid() => SwedishSniCode.TryParse(ValidSni, out _);
    [Benchmark] public bool SniCode_TryParse_Invalid() => SwedishSniCode.TryParse(InvalidSni, out _);

    [Benchmark] public bool CfarNumber_TryParse_Valid() => SwedishCfarNumber.TryParse(ValidCfar, out _);
    [Benchmark] public bool CfarNumber_TryParse_Invalid() => SwedishCfarNumber.TryParse(InvalidCfar, out _);

    [Benchmark] public bool OrgName_TryParse_Valid() => SwedishOrganizationName.TryParse(ValidOrgName, out _);
    [Benchmark] public bool OrgName_TryParse_Invalid() => SwedishOrganizationName.TryParse(InvalidOrgName, out _);
}
