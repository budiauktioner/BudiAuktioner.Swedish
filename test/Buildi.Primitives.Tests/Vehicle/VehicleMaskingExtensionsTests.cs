using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class VehicleMaskingExtensionsTests
{
    [Fact]
    public void RegistrationNumber_MasksLettersAndDigits()
    {
        var reg = SwedishVehicleRegistrationNumber.Parse("ABC 123");
        var masked = reg.ToMaskedString();
        Assert.Equal("*** ***", masked);
    }

    [Fact]
    public void RegistrationNumber_PreservesSpace()
    {
        var reg = SwedishVehicleRegistrationNumber.Parse("ABC123");
        var masked = reg.ToMaskedString();
        Assert.Contains(" ", masked);
    }

    [Fact]
    public void RegistrationNumber_MaskedLengthIsConsistent()
    {
        var reg = SwedishVehicleRegistrationNumber.Parse("XYZ 789");
        var masked = reg.ToMaskedString();
        Assert.Equal(7, masked.Length);
    }

    [Fact]
    public void Vin_ShowsWmiAndMasksRest()
    {
        var vin = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        var masked = vin.ToMaskedString();
        Assert.StartsWith("WBA", masked);
        Assert.Equal(17, masked.Length);
        Assert.Equal("WBA**************", masked);
    }

    [Fact]
    public void Vin_MaskedLengthMatchesOriginal()
    {
        var vin = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        var masked = vin.ToMaskedString();
        Assert.Equal(vin.Value.Length, masked.Length);
    }

    [Fact]
    public void Vin_OnlyWmiIsVisible()
    {
        var vin = VehicleIdentificationNumber.Parse("WBA3A5C55CF256789");
        var masked = vin.ToMaskedString();
        Assert.Equal("WBA", masked[..3]);
        Assert.All(masked[3..], c => Assert.Equal('*', c));
    }
}
