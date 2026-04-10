using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Property;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PropertyBenchmarks
{
    private const string ValidDesignation = "Stockholm Södermalm 1:1";
    private const string InvalidDesignation = "";

    [Benchmark] public bool PropertyDesignation_TryParse_Valid() => SwedishPropertyDesignation.TryParse(ValidDesignation, out _);
    [Benchmark] public bool PropertyDesignation_TryParse_Invalid() => SwedishPropertyDesignation.TryParse(InvalidDesignation, out _);
}
