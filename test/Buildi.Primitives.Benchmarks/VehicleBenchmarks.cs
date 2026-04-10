using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class VehicleBenchmarks
{
    private const string ValidRegNumber = "ABC 123";
    private const string InvalidRegNumber = "00 000";

    private const string ValidVin = "WBA3A5C55CF256789";
    private const string InvalidVin = "00000000000000000";

    [Benchmark] public bool RegNumber_TryParse_Valid() => SwedishVehicleRegistrationNumber.TryParse(ValidRegNumber, out _);
    [Benchmark] public bool RegNumber_TryParse_Invalid() => SwedishVehicleRegistrationNumber.TryParse(InvalidRegNumber, out _);

    [Benchmark] public bool Vin_TryParse_Valid() => VehicleIdentificationNumber.TryParse(ValidVin, out _);
    [Benchmark] public bool Vin_TryParse_Invalid() => VehicleIdentificationNumber.TryParse(InvalidVin, out _);
}
