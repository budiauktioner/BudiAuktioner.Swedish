using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Finance;
using Buildi.Primitives.Web;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;
using Buildi.Primitives.Product;
using Buildi.Primitives.Property;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.TextScanning;

/// <summary>
/// Scans unstructured text for all supported structured Swedish value types in a single pass
/// and returns an aggregate <see cref="TextScanResult"/>.
/// </summary>
/// <remarks>
/// <para>Text scanning is heuristic-based. Results may contain false positives, and valid
/// values may be missed entirely. No guarantee is made about completeness or accuracy.
/// Use <see cref="TextScannerOptions"/> to filter by category or minimum confidence.</para>
/// </remarks>
public sealed class TextScanner
{
    /// <summary>
    /// Scans <paramref name="text"/> for all supported types, returning every candidate found.
    /// </summary>
    public TextScanResult Scan(string text) => Scan(text, new TextScannerOptions());

    /// <summary>
    /// Scans <paramref name="text"/> for supported types matching the given <paramref name="options"/>.
    /// </summary>
    public TextScanResult Scan(string text, TextScannerOptions options)
    {
        var emails = ScanType(text, options, TextCandidateCategory.Contact, EmailAddress.FindCandidatesInText);
        var phones = ScanType(text, options, TextCandidateCategory.Contact, PhoneNumber.FindCandidatesInText);
        var zips = ScanType(text, options, TextCandidateCategory.Contact, AddressZipCode.FindCandidatesInText);
        var addresses = ScanType(text, options, TextCandidateCategory.Contact, Address.FindCandidatesInText);

        var pins = ScanType(text, options, TextCandidateCategory.PersonalIdentifier, SwedishPersonalIdentityNumber.FindCandidatesInText);
        var coords = ScanType(text, options, TextCandidateCategory.PersonalIdentifier, SwedishCoordinationNumber.FindCandidatesInText);

        var orgs = ScanType(text, options, TextCandidateCategory.OrganizationIdentifier, SwedishOrganizationNumber.FindCandidatesInText);
        var vats = ScanType(text, options, TextCandidateCategory.OrganizationIdentifier, EuVatNumber.FindCandidatesInText);
        var leis = ScanType(text, options, TextCandidateCategory.OrganizationIdentifier, LeiCode.FindCandidatesInText);
        var duns = ScanType(text, options, TextCandidateCategory.OrganizationIdentifier, DunsNumber.FindCandidatesInText);

        var ibans = ScanType(text, options, TextCandidateCategory.Financial, Iban.FindCandidatesInText);
        var bics = ScanType(text, options, TextCandidateCategory.Financial, Bic.FindCandidatesInText);
        var bgs = ScanType(text, options, TextCandidateCategory.Financial, SwedishBankgiroNumber.FindCandidatesInText);
        var pgs = ScanType(text, options, TextCandidateCategory.Financial, SwedishPostgiroNumber.FindCandidatesInText);
        var accounts = ScanType(text, options, TextCandidateCategory.Financial, SwedishBankAccount.FindCandidatesInText);
        var ocrs = ScanType(text, options, TextCandidateCategory.Financial, SwedishOcrReferenceNumber.FindCandidatesInText);
        var isins = ScanType(text, options, TextCandidateCategory.Financial, Isin.FindCandidatesInText);

        var regs = ScanType(text, options, TextCandidateCategory.Vehicle, SwedishVehicleRegistrationNumber.FindCandidatesInText);
        var vins = ScanType(text, options, TextCandidateCategory.Vehicle, VehicleIdentificationNumber.FindCandidatesInText);
        var operatingHours = ScanType(text, options, TextCandidateCategory.Vehicle, OperatingHours.FindCandidatesInText);
        var boltPatterns = ScanType(text, options, TextCandidateCategory.Vehicle, BoltPattern.FindCandidatesInText);
        var wheelRimDimensions = ScanType(text, options, TextCandidateCategory.Vehicle, WheelRimDimension.FindCandidatesInText);
        var euTypeApprovals = ScanType(text, options, TextCandidateCategory.Vehicle, EuTypeApprovalNumber.FindCandidatesInText);
        var enginePowers = ScanType(text, options, TextCandidateCategory.Vehicle, EnginePower.FindCandidatesInText);
        var fuelConsumptions = ScanType(text, options, TextCandidateCategory.Vehicle, FuelConsumption.FindCandidatesInText);
        var emissionRates = ScanType(text, options, TextCandidateCategory.Vehicle, EmissionRate.FindCandidatesInText);

        var props = ScanType(text, options, TextCandidateCategory.Property, SwedishPropertyDesignation.FindCandidatesInText);

        var gtin13s = ScanType(text, options, TextCandidateCategory.Product, Gtin13.FindCandidatesInText);
        var gtin8s = ScanType(text, options, TextCandidateCategory.Product, Gtin8.FindCandidatesInText);
        var gtin12s = ScanType(text, options, TextCandidateCategory.Product, Gtin12.FindCandidatesInText);
        var gtin14s = ScanType(text, options, TextCandidateCategory.Product, Gtin14.FindCandidatesInText);
        var ipRatings = ScanType(text, options, TextCandidateCategory.Product, IpRating.FindCandidatesInText);
        var electricalPhases = ScanType(text, options, TextCandidateCategory.Product, ElectricalPhase.FindCandidatesInText);

        var countries = ScanType(text, options, TextCandidateCategory.Geography, Country.FindCandidatesInText);
        var municipalities = ScanType(text, options, TextCandidateCategory.Geography, SwedishMunicipality.FindCandidatesInText);
        var counties = ScanType(text, options, TextCandidateCategory.Geography, SwedishCounty.FindCandidatesInText);

        var urls = ScanType(text, options, TextCandidateCategory.Contact, Url.FindCandidatesInText);

        var lengths = ScanType(text, options, TextCandidateCategory.Measurement, Length.FindCandidatesInText);
        var areas = ScanType(text, options, TextCandidateCategory.Measurement, Area.FindCandidatesInText);
        var volumes = ScanType(text, options, TextCandidateCategory.Measurement, Volume.FindCandidatesInText);
        var weights = ScanType(text, options, TextCandidateCategory.Measurement, Weight.FindCandidatesInText);
        var energies = ScanType(text, options, TextCandidateCategory.Measurement, Energy.FindCandidatesInText);
        var powers = ScanType(text, options, TextCandidateCategory.Measurement, Power.FindCandidatesInText);
        var voltages = ScanType(text, options, TextCandidateCategory.Measurement, Voltage.FindCandidatesInText);
        var electricCharges = ScanType(text, options, TextCandidateCategory.Measurement, ElectricCharge.FindCandidatesInText);
        var torques = ScanType(text, options, TextCandidateCategory.Measurement, Torque.FindCandidatesInText);
        var frequencies = ScanType(text, options, TextCandidateCategory.Measurement, Frequency.FindCandidatesInText);
        var speeds = ScanType(text, options, TextCandidateCategory.Measurement, Speed.FindCandidatesInText);
        var temperatures = ScanType(text, options, TextCandidateCategory.Measurement, Temperature.FindCandidatesInText);
        var dataSizes = ScanType(text, options, TextCandidateCategory.Measurement, DataSize.FindCandidatesInText);
        var pressures = ScanType(text, options, TextCandidateCategory.Measurement, Pressure.FindCandidatesInText);
        var percentages = ScanType(text, options, TextCandidateCategory.Measurement, Percentage.FindCandidatesInText);
        var electricCurrents = ScanType(text, options, TextCandidateCategory.Measurement, ElectricCurrent.FindCandidatesInText);
        var flowRates = ScanType(text, options, TextCandidateCategory.Measurement, FlowRate.FindCandidatesInText);
        var luminousFluxes = ScanType(text, options, TextCandidateCategory.Measurement, LuminousFlux.FindCandidatesInText);
        var rotationalSpeeds = ScanType(text, options, TextCandidateCategory.Measurement, RotationalSpeed.FindCandidatesInText);
        var soundLevels = ScanType(text, options, TextCandidateCategory.Measurement, SoundLevel.FindCandidatesInText);
        var euroEmissionClasses = ScanType(text, options, TextCandidateCategory.Vehicle, EuroEmissionClass.FindCandidatesInText);
        var tireDimensions = ScanType(text, options, TextCandidateCategory.Vehicle, TireDimension.FindCandidatesInText);
        var drivingLicenseCategories = ScanType(text, options, TextCandidateCategory.Vehicle, SwedishDrivingLicenseCategory.FindCandidatesInText);
        var swishNumbers = ScanType(text, options, TextCandidateCategory.Financial, SwedishSwishNumber.FindCandidatesInText);
        var colors = ScanType(text, options, TextCandidateCategory.Product, Color.FindCandidatesInText);
        var screenResolutions = ScanType(text, options, TextCandidateCategory.Product, ScreenResolution.FindCandidatesInText);
        var screenSizes = ScanType(text, options, TextCandidateCategory.Product, ScreenSize.FindCandidatesInText);

        return new TextScanResult(
            emails, phones, zips, addresses,
            pins, coords,
            orgs, vats, leis, duns,
            ibans, bics, bgs, pgs, accounts, ocrs, isins,
            regs, vins, operatingHours, boltPatterns,
            wheelRimDimensions, euTypeApprovals, enginePowers, fuelConsumptions, emissionRates,
            props,
            gtin13s, gtin8s, gtin12s, gtin14s, ipRatings, electricalPhases,
            countries, municipalities, counties,
            urls,
            lengths, areas, volumes, weights, energies, powers,
            voltages, electricCharges, torques, frequencies,
            speeds, temperatures, dataSizes, pressures, percentages,
            electricCurrents, flowRates, luminousFluxes,
            rotationalSpeeds, soundLevels,
            euroEmissionClasses, tireDimensions,
            colors, screenResolutions, screenSizes,
            drivingLicenseCategories, swishNumbers);
    }

    private static IReadOnlyList<TextCandidate<T>> ScanType<T>(
        string text,
        TextScannerOptions options,
        TextCandidateCategory category,
        Func<string, IReadOnlyList<TextCandidate<T>>> scanner)
    {
        if (!options.ShouldScan(category)) return [];

        var candidates = scanner(text);
        if (options.MinimumConfidence <= TextMatchConfidence.Low)
            return candidates;

        return candidates.Where(c => c.Confidence >= options.MinimumConfidence).ToList();
    }
}
