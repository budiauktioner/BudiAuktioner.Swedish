using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class DrivetrainTypeTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(3, DrivetrainType.All.Count);

    [Theory]
    [InlineData("AWD")]
    [InlineData("awd")]
    [InlineData("All-wheel drive")]
    [InlineData("All wheel drive")]
    [InlineData("4WD")]
    [InlineData("4-WD")]
    [InlineData("4x4")]
    [InlineData("4 x 4")]
    [InlineData("Four-wheel drive")]
    [InlineData("Four wheel drive")]
    [InlineData("Fyrhjulsdrift")]
    [InlineData("fyrhjulsdrift")]
    [InlineData("Fyrhjulsdriven")]
    [InlineData("Fyrhjul")]
    [InlineData("4Motion")]
    [InlineData("4Matic")]
    [InlineData("quattro")]
    [InlineData("xDrive")]
    [InlineData("FWD")]
    [InlineData("fwd")]
    [InlineData("Front-wheel drive")]
    [InlineData("Front wheel drive")]
    [InlineData("Framhjulsdrift")]
    [InlineData("Framhjulsdriven")]
    [InlineData("Framhjul")]
    [InlineData("Framdrift")]
    [InlineData("RWD")]
    [InlineData("rwd")]
    [InlineData("Rear-wheel drive")]
    [InlineData("Rear wheel drive")]
    [InlineData("Bakhjulsdrift")]
    [InlineData("Bakhjulsdriven")]
    [InlineData("Bakhjul")]
    [InlineData("  AWD  ")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(DrivetrainType.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("nope")]
    [InlineData("8WD")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(DrivetrainType.IsValid(input));

    [Theory]
    [InlineData("Fyrhjulsdrift", "AWD")]
    [InlineData("Fyrhjulsdriven", "AWD")]
    [InlineData("All wheel drive", "AWD")]
    [InlineData("4WD", "AWD")]
    [InlineData("4x4", "AWD")]
    [InlineData("Four-wheel drive", "AWD")]
    [InlineData("4Motion", "AWD")]
    [InlineData("quattro", "AWD")]
    [InlineData("xDrive", "AWD")]
    [InlineData("Framhjulsdrift", "FWD")]
    [InlineData("Framhjul", "FWD")]
    [InlineData("Front wheel drive", "FWD")]
    [InlineData("Bakhjulsdrift", "RWD")]
    [InlineData("Rear wheel drive", "RWD")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, DrivetrainType.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(DrivetrainType.Awd, DrivetrainType.Parse("quattro"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => DrivetrainType.Parse("nope"));

    [Fact]
    public void DrivenAxleCount_MatchesLayout()
    {
        Assert.Equal(2, DrivetrainType.Awd.DrivenAxleCount);
        Assert.Equal(1, DrivetrainType.Fwd.DrivenAxleCount);
        Assert.Equal(1, DrivetrainType.Rwd.DrivenAxleCount);
    }

    [Fact]
    public void CompareTo_OrdersBySortOrder()
    {
        Assert.True(DrivetrainType.Awd < DrivetrainType.Fwd);
        Assert.True(DrivetrainType.Fwd < DrivetrainType.Rwd);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("***", DrivetrainType.Awd.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = DrivetrainType.Parse("AWD");
        var b = DrivetrainType.Parse("Fyrhjulsdrift");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Drivetrain Type", DrivetrainType.TypeInfo.EnglishName);
        Assert.Equal("Drivning", DrivetrainType.TypeInfo.LocalizedName);
    }
}
