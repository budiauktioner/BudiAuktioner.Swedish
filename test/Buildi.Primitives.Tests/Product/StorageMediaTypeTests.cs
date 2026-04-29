using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class StorageMediaTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(8, StorageMediaType.All.Count);

    [Theory]
    [InlineData("HDD")]
    [InlineData("hdd")]
    [InlineData("Hard disk")]
    [InlineData("Hard disk drive")]
    [InlineData("Hårddisk")]
    [InlineData("Mekanisk hårddisk")]
    [InlineData("Mechanical drive")]
    [InlineData("Spinning disk")]
    [InlineData("SSHD")]
    [InlineData("Hybriddisk")]
    [InlineData("Hybrid drive")]
    [InlineData("SSD")]
    [InlineData("Solid state")]
    [InlineData("Solid state drive")]
    [InlineData("Solid-state drive")]
    [InlineData("SATA SSD")]
    [InlineData("M.2 SATA")]
    [InlineData("NVMe")]
    [InlineData("nvme")]
    [InlineData("NVM Express")]
    [InlineData("PCIe SSD")]
    [InlineData("M.2 NVMe")]
    [InlineData("eMMC")]
    [InlineData("emmc")]
    [InlineData("e-MMC")]
    [InlineData("Embedded MMC")]
    [InlineData("UFS")]
    [InlineData("ufs")]
    [InlineData("Universal Flash Storage")]
    [InlineData("UFS 3.1")]
    [InlineData("Flash")]
    [InlineData("Flash storage")]
    [InlineData("Flashminne")]
    [InlineData("NAND flash")]
    [InlineData("Optane")]
    [InlineData("3D XPoint")]
    [InlineData("  SSD  ")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(StorageMediaType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("nope")]
    [InlineData("RAM")]
    [InlineData("DDR4")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(StorageMediaType.IsValid(input));

    [Theory]
    [InlineData("Hårddisk", "HDD")]
    [InlineData("Hard drive", "HDD")]
    [InlineData("Solid state", "SSD")]
    [InlineData("SATA SSD", "SSD")]
    [InlineData("PCIe SSD", "NVMe")]
    [InlineData("NVM Express", "NVMe")]
    [InlineData("M.2 NVMe", "NVMe")]
    [InlineData("e-MMC", "eMMC")]
    [InlineData("Universal Flash Storage", "UFS")]
    [InlineData("Flashminne", "Flash")]
    [InlineData("3D XPoint", "Optane")]
    [InlineData("Hybriddisk", "SSHD")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, StorageMediaType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(StorageMediaType.Nvme, StorageMediaType.Parse("PCIe SSD"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => StorageMediaType.Parse("nope"));

    [Fact]
    public void Family_GroupsRelatedStorage()
    {
        Assert.Equal("HDD", StorageMediaType.Hdd.Family);
        Assert.Equal("Hybrid", StorageMediaType.Sshd.Family);
        Assert.Equal("SSD", StorageMediaType.Ssd.Family);
        Assert.Equal("SSD", StorageMediaType.Nvme.Family);
        Assert.Equal("SSD", StorageMediaType.Optane.Family);
        Assert.Equal("Flash", StorageMediaType.EMmc.Family);
        Assert.Equal("Flash", StorageMediaType.Ufs.Family);
        Assert.Equal("Flash", StorageMediaType.Flash.Family);
    }

    [Fact]
    public void IsSolidState_TrueForFlashAndSsdFamilies()
    {
        Assert.False(StorageMediaType.Hdd.IsSolidState);
        Assert.False(StorageMediaType.Sshd.IsSolidState);
        Assert.True(StorageMediaType.Ssd.IsSolidState);
        Assert.True(StorageMediaType.Nvme.IsSolidState);
        Assert.True(StorageMediaType.EMmc.IsSolidState);
        Assert.True(StorageMediaType.Ufs.IsSolidState);
        Assert.True(StorageMediaType.Flash.IsSolidState);
        Assert.True(StorageMediaType.Optane.IsSolidState);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("****", StorageMediaType.Nvme.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = StorageMediaType.Parse("SSD");
        var b = StorageMediaType.Parse("Solid state");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Storage Media Type", StorageMediaType.TypeInfo.EnglishName);
        Assert.Equal("Lagringsmedia", StorageMediaType.TypeInfo.LocalizedName);
    }
}
