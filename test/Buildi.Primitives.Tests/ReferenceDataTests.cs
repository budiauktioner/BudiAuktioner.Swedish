using Buildi.Primitives.Finance;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests;

public class ReferenceDataTests
{
    [Fact]
    public void All_ContainsExpectedDatasets()
    {
        var all = ReferenceData.All;
        Assert.True(all.Count >= 10);
    }

    [Fact]
    public void All_DatasetsHaveNames()
    {
        foreach (var ds in ReferenceData.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(ds.Name), $"Dataset has empty name");
            Assert.False(string.IsNullOrWhiteSpace(ds.Description), $"{ds.Name} has empty description");
            Assert.False(string.IsNullOrWhiteSpace(ds.Source), $"{ds.Name} has empty source");
            Assert.NotNull(ds.SourceUrl);
        }
    }

    [Fact]
    public void SwedishCounties_HasExpectedMetadata()
    {
        var ds = ReferenceData.SwedishCounties;
        Assert.Equal("SwedishCounties", ds.Name);
        Assert.Equal(21, ds.EntryCount);
        Assert.Contains("SCB", ds.Source);
    }

    [Fact]
    public void SwedishMunicipalities_HasExpectedMetadata()
    {
        var ds = ReferenceData.SwedishMunicipalities;
        Assert.Equal(290, ds.EntryCount);
    }

    [Fact]
    public void Countries_EntryCountMatchesActualData()
    {
        Assert.Equal(Country.All.Count, ReferenceData.Countries.EntryCount);
    }

    [Fact]
    public void Currencies_EntryCountMatchesActualData()
    {
        Assert.Equal(Currency.All.Count, ReferenceData.Currencies.EntryCount);
    }

    [Fact]
    public void EuropeanUnionMembers_EntryCountMatchesActualData()
    {
        Assert.Equal(Country.All.Count(c => c.IsInEuropeanUnion), ReferenceData.EuropeanUnionMembers.EntryCount);
    }

    [Fact]
    public void EeaMembers_EntryCountMatchesActualData()
    {
        Assert.Equal(Country.All.Count(c => c.IsInEea), ReferenceData.EeaMembers.EntryCount);
    }

    [Fact]
    public void SchengenMembers_EntryCountMatchesActualData()
    {
        Assert.Equal(Country.All.Count(c => c.IsInSchengen), ReferenceData.SchengenMembers.EntryCount);
    }

    [Fact]
    public void SepaMembers_EntryCountMatchesActualData()
    {
        Assert.Equal(Country.All.Count(c => c.IsInSepa), ReferenceData.SepaMembers.EntryCount);
    }

    [Fact]
    public void LastVerified_IsReasonablyRecent()
    {
        var cutoff = new DateOnly(2024, 1, 1);
        foreach (var ds in ReferenceData.All)
            Assert.True(ds.LastVerified >= cutoff, $"{ds.Name} last verified {ds.LastVerified} is before {cutoff}");
    }

    [Fact]
    public void DatasetRecord_SupportsEquality()
    {
        var a = ReferenceData.SwedishCounties;
        var b = ReferenceData.SwedishCounties;
        Assert.Equal(a, b);
    }
}
