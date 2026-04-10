using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class GeographyBenchmarks
{
    private const string ValidCountyCode = "01";
    private const string ValidCountyName = "Stockholms län";
    private const string InvalidCounty = "99";

    private const string ValidMunicipalityCode = "0180";
    private const string ValidMunicipalityName = "Stockholm";
    private const string InvalidMunicipality = "9999";

    [Benchmark] public bool County_TryParse_Code() => SwedishCounty.TryParse(ValidCountyCode, out _);
    [Benchmark] public bool County_TryParse_Name() => SwedishCounty.TryParse(ValidCountyName, out _);
    [Benchmark] public bool County_TryParse_Invalid() => SwedishCounty.TryParse(InvalidCounty, out _);

    [Benchmark] public bool Municipality_TryParse_Code() => SwedishMunicipality.TryParse(ValidMunicipalityCode, out _);
    [Benchmark] public bool Municipality_TryParse_Name() => SwedishMunicipality.TryParse(ValidMunicipalityName, out _);
    [Benchmark] public bool Municipality_TryParse_Invalid() => SwedishMunicipality.TryParse(InvalidMunicipality, out _);
}
