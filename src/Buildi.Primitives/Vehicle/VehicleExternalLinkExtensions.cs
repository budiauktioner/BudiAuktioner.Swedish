namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Extension methods that generate lookup URLs for vehicle-related types.
/// </summary>
public static class VehicleExternalLinkExtensions
{
    /// <summary>
    /// Returns the Biluppgifter.se vehicle information URL for this registration number,
    /// e.g. <c>https://biluppgifter.se/fordon/ABC123</c>.
    /// </summary>
    public static Uri GetBiluppgifterUrl(this SwedishVehicleRegistrationNumber reg)
        => new($"https://biluppgifter.se/fordon/{reg.Value}");

    /// <summary>
    /// Returns the Car.info vehicle information URL for this registration number,
    /// e.g. <c>https://www.car.info/sv-se/license-plate/S/ABC123</c>.
    /// </summary>
    public static Uri GetCarInfoUrl(this SwedishVehicleRegistrationNumber reg)
        => new($"https://www.car.info/sv-se/license-plate/S/{reg.Value}");
}
