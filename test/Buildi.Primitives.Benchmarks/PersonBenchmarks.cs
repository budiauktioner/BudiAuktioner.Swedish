using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PersonBenchmarks
{
    private const string ValidGivenName = "Anna-Karin";
    private const string InvalidGivenName = "";

    private const string ValidFamilyName = "Andersson";
    private const string InvalidFamilyName = "";

    private const string ValidFullName = "Anna-Karin Andersson";
    private const string InvalidFullName = "";

    private const string ValidPnr = "198507099805";
    private const string InvalidPnr = "0000000000";

    private const string ValidCoordination = "198507699818";
    private const string InvalidCoordination = "0000000000";

    [Benchmark] public bool GivenName_TryParse_Valid() => PersonGivenName.TryParse(ValidGivenName, out _);
    [Benchmark] public bool GivenName_TryParse_Invalid() => PersonGivenName.TryParse(InvalidGivenName, out _);

    [Benchmark] public bool FamilyName_TryParse_Valid() => PersonFamilyName.TryParse(ValidFamilyName, out _);
    [Benchmark] public bool FamilyName_TryParse_Invalid() => PersonFamilyName.TryParse(InvalidFamilyName, out _);

    [Benchmark] public bool FullName_TryParse_Valid() => PersonFullName.TryParse(ValidFullName, out _);
    [Benchmark] public bool FullName_TryParse_Invalid() => PersonFullName.TryParse(InvalidFullName, out _);

    [Benchmark] public bool PersonalId_TryParse_Valid() => SwedishPersonalIdentityNumber.TryParse(ValidPnr, out _);
    [Benchmark] public bool PersonalId_TryParse_Invalid() => SwedishPersonalIdentityNumber.TryParse(InvalidPnr, out _);

    [Benchmark] public bool CoordNumber_TryParse_Valid() => SwedishCoordinationNumber.TryParse(ValidCoordination, out _);
    [Benchmark] public bool CoordNumber_TryParse_Invalid() => SwedishCoordinationNumber.TryParse(InvalidCoordination, out _);
}
