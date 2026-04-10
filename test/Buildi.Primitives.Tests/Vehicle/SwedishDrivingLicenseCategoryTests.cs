using System.Globalization;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

[Collection("CultureSensitive")]
public class SwedishDrivingLicenseCategoryTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(15, SwedishDrivingLicenseCategory.All.Count);
    }

    [Theory]
    [InlineData("AM", "Moped klass I", "Class I moped", 15, SwedishDrivingLicenseVehicleGroup.Moped, false, 10)]
    [InlineData("A1", "Lätt motorcykel", "Light motorcycle", 16, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10)]
    [InlineData("A2", "Mellanstor motorcykel", "Medium motorcycle", 18, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10)]
    [InlineData("A", "Tung motorcykel", "Heavy motorcycle", 24, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10)]
    [InlineData("B", "Personbil och lätt lastbil", "Car and light lorry", 18, SwedishDrivingLicenseVehicleGroup.Car, false, 10)]
    [InlineData("B96", "Personbil utökad", "Car extended", 18, SwedishDrivingLicenseVehicleGroup.Car, false, 10)]
    [InlineData("BE", "Personbil med tungt släp", "Car with heavy trailer", 18, SwedishDrivingLicenseVehicleGroup.Car, true, 10)]
    [InlineData("C1", "Medeltung lastbil", "Medium heavy lorry", 18, SwedishDrivingLicenseVehicleGroup.Truck, false, 5)]
    [InlineData("C1E", "Medeltung lastbil med tungt släp", "Medium heavy lorry with heavy trailer", 18, SwedishDrivingLicenseVehicleGroup.Truck, true, 5)]
    [InlineData("C", "Tung lastbil", "Heavy lorry", 21, SwedishDrivingLicenseVehicleGroup.Truck, false, 5)]
    [InlineData("CE", "Tung lastbil med tungt släp", "Heavy lorry with heavy trailer", 21, SwedishDrivingLicenseVehicleGroup.Truck, true, 5)]
    [InlineData("D1", "Mellanstor buss", "Medium bus", 21, SwedishDrivingLicenseVehicleGroup.Bus, false, 5)]
    [InlineData("D1E", "Mellanstor buss med tungt släp", "Medium bus with heavy trailer", 21, SwedishDrivingLicenseVehicleGroup.Bus, true, 5)]
    [InlineData("D", "Buss", "Bus", 24, SwedishDrivingLicenseVehicleGroup.Bus, false, 5)]
    [InlineData("DE", "Buss med tungt släp", "Bus with heavy trailer", 24, SwedishDrivingLicenseVehicleGroup.Bus, true, 5)]
    public void StaticInstances_HaveExpectedProperties(
        string code, string swedishName, string englishName, int minimumAge,
        SwedishDrivingLicenseVehicleGroup vehicleGroup, bool isTrailer, int validityYears)
    {
        var cat = SwedishDrivingLicenseCategory.All.Single(x => x.Code == code);
        Assert.Equal(swedishName, cat.LocalizedName);
        Assert.Equal(englishName, cat.EnglishName);
        Assert.Equal(minimumAge, cat.MinimumAge);
        Assert.Equal(vehicleGroup, cat.VehicleGroup);
        Assert.Equal(isTrailer, cat.IsTrailerCategory);
        Assert.Equal(validityYears, cat.ValidityYears);
        Assert.Equal(code, cat.Value);
        Assert.NotEmpty(cat.Description);
    }

    [Theory]
    [InlineData("AM")]
    [InlineData("A1")]
    [InlineData("A2")]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("B96")]
    [InlineData("BE")]
    [InlineData("C1")]
    [InlineData("C1E")]
    [InlineData("C")]
    [InlineData("CE")]
    [InlineData("D1")]
    [InlineData("D1E")]
    [InlineData("D")]
    [InlineData("DE")]
    [InlineData("am")]
    [InlineData("b")]
    [InlineData("c1e")]
    [InlineData("de")]
    [InlineData("  B  ")]
    [InlineData("Personbil")]
    [InlineData("Moped")]
    [InlineData("MC")]
    [InlineData("Motorcykel")]
    [InlineData("Buss")]
    [InlineData("Lastbil")]
    [InlineData("B utökad")]
    [InlineData("B extended")]
    [InlineData("Tung motorcykel")]
    [InlineData("Lätt motorcykel")]
    [InlineData("Mellanstor motorcykel")]
    [InlineData("Medeltung lastbil")]
    [InlineData("Mellanstor buss")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishDrivingLicenseCategory.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("F")]
    [InlineData("B1")]
    [InlineData("B2")]
    [InlineData("E")]
    [InlineData("C2")]
    [InlineData("ABC")]
    [InlineData("Flygplan")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishDrivingLicenseCategory.IsValid(input));
    }

    [Theory]
    [InlineData("b", "B")]
    [InlineData("am", "AM")]
    [InlineData("c1e", "C1E")]
    [InlineData("  B  ", "B")]
    [InlineData("Personbil", "B")]
    [InlineData("Moped", "AM")]
    [InlineData("MC", "A")]
    [InlineData("Motorcykel", "A")]
    [InlineData("Buss", "D")]
    [InlineData("Lastbil", "C")]
    [InlineData("B utökad", "B96")]
    [InlineData("B extended", "B96")]
    [InlineData("Tung motorcykel", "A")]
    [InlineData("Lätt motorcykel", "A1")]
    [InlineData("Mellanstor motorcykel", "A2")]
    [InlineData("Medeltung lastbil", "C1")]
    [InlineData("Mellanstor buss", "D1")]
    [InlineData("Bil", "B")]
    public void TryParse_ReturnsExpectedCategory(string input, string expectedCode)
    {
        var ok = SwedishDrivingLicenseCategory.TryParse(input, out var cat);
        Assert.True(ok);
        Assert.NotNull(cat);
        Assert.Equal(expectedCode, cat.Code);
        Assert.Same(SwedishDrivingLicenseCategory.All.First(x => x.Code == expectedCode), cat);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Z")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishDrivingLicenseCategory.TryParse(input, out var cat);
        Assert.False(ok);
        Assert.Null(cat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Z")]
    [InlineData("B1")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishDrivingLicenseCategory.Parse(input));
    }

    [Theory]
    [InlineData("B", "B — Personbil och lätt lastbil")]
    [InlineData("am", "AM — Moped klass I")]
    [InlineData("c1e", "C1E — Medeltung lastbil med tungt släp")]
    [InlineData("Personbil", "B — Personbil och lätt lastbil")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        Assert.Equal(expected, SwedishDrivingLicenseCategory.Format(input));
    }

    [Theory]
    [InlineData("invalid", "invalid")]
    [InlineData("  invalid  ", "invalid")]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Format_WithFallback_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishDrivingLicenseCategory.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("B", "B")]
    [InlineData("b", "B")]
    [InlineData("Personbil", "B")]
    [InlineData("am", "AM")]
    [InlineData("c1e", "C1E")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishDrivingLicenseCategory.Normalize(input));
    }

    [Theory]
    [InlineData("B")]
    [InlineData("AM")]
    [InlineData("C1E")]
    [InlineData("B96")]
    public void IsNormalized_ReturnsTrue_ForNormalizedInputs(string input)
    {
        Assert.True(SwedishDrivingLicenseCategory.IsNormalized(input));
    }

    [Theory]
    [InlineData("b")]
    [InlineData("am")]
    [InlineData("Personbil")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void IsNormalized_ReturnsFalse_ForNonNormalizedInputs(string? input)
    {
        Assert.False(SwedishDrivingLicenseCategory.IsNormalized(input));
    }

    [Theory]
    [InlineData("B", "B")]
    [InlineData("AM", "AM")]
    [InlineData("C1E", "C1E")]
    public void ToNormalizedString_ReturnsCode(string input, string expected)
    {
        var cat = SwedishDrivingLicenseCategory.Parse(input);
        Assert.Equal(expected, cat.ToNormalizedString());
    }

    [Theory]
    [InlineData("B", "B — Personbil och lätt lastbil")]
    [InlineData("AM", "AM — Moped klass I")]
    public void ToString_ReturnsCodeAndLocalizedName(string input, string expected)
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var cat = SwedishDrivingLicenseCategory.Parse(input);
        Assert.Equal(expected, cat.ToString());
    }

    [Theory]
    [InlineData("B", "Personbil och lätt lastbil")]
    [InlineData("AM", "Moped klass I")]
    public void ToLocalString_ReturnsLocalizedName(string input, string expected)
    {
        var cat = SwedishDrivingLicenseCategory.Parse(input);
        Assert.Equal(expected, cat.ToLocalString());
    }

    [Theory]
    [InlineData("B", "Car and light lorry")]
    [InlineData("AM", "Class I moped")]
    public void ToEnglishString_ReturnsEnglishName(string input, string expected)
    {
        var cat = SwedishDrivingLicenseCategory.Parse(input);
        Assert.Equal(expected, cat.ToEnglishString());
    }

    [Fact]
    public void Equality_SameCode_AreEqual()
    {
        var a = SwedishDrivingLicenseCategory.Parse("B");
        var b = SwedishDrivingLicenseCategory.Parse("b");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        Assert.NotEqual(SwedishDrivingLicenseCategory.B, SwedishDrivingLicenseCategory.C);
        Assert.True(SwedishDrivingLicenseCategory.B != SwedishDrivingLicenseCategory.C);
    }

    [Fact]
    public void TrailerCategories_AreCorrect()
    {
        var trailerCats = SwedishDrivingLicenseCategory.All.Where(c => c.IsTrailerCategory).Select(c => c.Code).ToList();
        Assert.Equal(["BE", "C1E", "CE", "D1E", "DE"], trailerCats);
    }

    [Fact]
    public void VehicleGroups_CoverAllCategories()
    {
        foreach (var cat in SwedishDrivingLicenseCategory.All)
        {
            Assert.NotEqual(SwedishDrivingLicenseVehicleGroup.Unknown, cat.VehicleGroup);
        }
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishDrivingLicenseCategory.Parse("A1");
        var b = SwedishDrivingLicenseCategory.Parse("B");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishDrivingLicenseCategory.Parse("B");
        Assert.Equal(1, a.CompareTo(null));
    }
}
