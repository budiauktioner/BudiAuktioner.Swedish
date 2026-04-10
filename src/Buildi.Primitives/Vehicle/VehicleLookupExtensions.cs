namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Extension methods that produce public lookup URLs for vehicle identifiers.
/// </summary>
public static class VehicleLookupExtensions
{
    /// <summary>
    /// Returns a Biluppgifter.se lookup URL, e.g.
    /// <c>https://biluppgifter.se/fordon/ABC123</c>.
    /// </summary>
    public static string ToBiluppgifterUrl(this SwedishVehicleRegistrationNumber regNumber) =>
        $"https://biluppgifter.se/fordon/{regNumber.ToNormalizedString()}";
}
