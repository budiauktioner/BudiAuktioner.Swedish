using System.Text;
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
/// The result of scanning unstructured text for structured Swedish value types.
/// Contains all found candidates, a resolved non-overlapping subset, and methods for
/// bulk replacement/masking.
/// </summary>
/// <remarks>
/// <para>Text scanning is heuristic-based. Candidates may be false positives, and valid
/// occurrences may be missed. No guarantee is made about completeness or accuracy.</para>
/// </remarks>
public sealed class TextScanResult
{
    /// <summary>All candidates sorted by <see cref="TextCandidate.StartIndex"/>, including overlapping ones.</summary>
    public IReadOnlyList<TextCandidate> All { get; }

    /// <summary>
    /// Non-overlapping subset after resolution. When candidates overlap or one contains another,
    /// the winner is chosen by: containment (longer enclosing match wins), then higher
    /// <see cref="TextMatchConfidence"/>, then longer span, then type priority.
    /// </summary>
    public IReadOnlyList<TextCandidate> ResolvedCandidates { get; }

    public IReadOnlyList<TextCandidate<EmailAddress>> Emails { get; }
    public IReadOnlyList<TextCandidate<PhoneNumber>> PhoneNumbers { get; }
    public IReadOnlyList<TextCandidate<AddressZipCode>> ZipCodes { get; }
    public IReadOnlyList<TextCandidate<Address>> Addresses { get; }
    public IReadOnlyList<TextCandidate<SwedishPersonalIdentityNumber>> PersonalIdentityNumbers { get; }
    public IReadOnlyList<TextCandidate<SwedishCoordinationNumber>> CoordinationNumbers { get; }
    public IReadOnlyList<TextCandidate<SwedishOrganizationNumber>> OrganizationNumbers { get; }
    public IReadOnlyList<TextCandidate<EuVatNumber>> EuVatNumbers { get; }
    public IReadOnlyList<TextCandidate<LeiCode>> LeiCodes { get; }
    public IReadOnlyList<TextCandidate<DunsNumber>> DunsNumbers { get; }
    public IReadOnlyList<TextCandidate<Iban>> Ibans { get; }
    public IReadOnlyList<TextCandidate<Bic>> Bics { get; }
    public IReadOnlyList<TextCandidate<SwedishBankgiroNumber>> BankgiroNumbers { get; }
    public IReadOnlyList<TextCandidate<SwedishPostgiroNumber>> PostgiroNumbers { get; }
    public IReadOnlyList<TextCandidate<SwedishBankAccount>> BankAccounts { get; }
    public IReadOnlyList<TextCandidate<SwedishOcrReferenceNumber>> OcrReferences { get; }
    public IReadOnlyList<TextCandidate<Isin>> Isins { get; }
    public IReadOnlyList<TextCandidate<SwedishVehicleRegistrationNumber>> VehicleRegistrationNumbers { get; }
    public IReadOnlyList<TextCandidate<VehicleIdentificationNumber>> VehicleIdentificationNumbers { get; }
    public IReadOnlyList<TextCandidate<OperatingHours>> OperatingHourValues { get; }
    public IReadOnlyList<TextCandidate<BoltPattern>> BoltPatterns { get; }
    public IReadOnlyList<TextCandidate<WheelRimDimension>> WheelRimDimensions { get; }
    public IReadOnlyList<TextCandidate<EuTypeApprovalNumber>> EuTypeApprovalNumbers { get; }
    public IReadOnlyList<TextCandidate<EnginePower>> EnginePowers { get; }
    public IReadOnlyList<TextCandidate<FuelConsumption>> FuelConsumptions { get; }
    public IReadOnlyList<TextCandidate<EmissionRate>> EmissionRates { get; }
    public IReadOnlyList<TextCandidate<SwedishPropertyDesignation>> PropertyDesignations { get; }
    public IReadOnlyList<TextCandidate<Gtin13>> Gtin13s { get; }
    public IReadOnlyList<TextCandidate<Gtin8>> Gtin8s { get; }
    public IReadOnlyList<TextCandidate<Gtin12>> Gtin12s { get; }
    public IReadOnlyList<TextCandidate<Gtin14>> Gtin14s { get; }
    public IReadOnlyList<TextCandidate<IpRating>> IpRatings { get; }
    public IReadOnlyList<TextCandidate<ElectricalPhase>> ElectricalPhases { get; }
    public IReadOnlyList<TextCandidate<Country>> Countries { get; }
    public IReadOnlyList<TextCandidate<SwedishMunicipality>> Municipalities { get; }
    public IReadOnlyList<TextCandidate<SwedishCounty>> Counties { get; }
    public IReadOnlyList<TextCandidate<Url>> Urls { get; }
    public IReadOnlyList<TextCandidate<Length>> Lengths { get; }
    public IReadOnlyList<TextCandidate<Area>> Areas { get; }
    public IReadOnlyList<TextCandidate<Volume>> Volumes { get; }
    public IReadOnlyList<TextCandidate<Weight>> Weights { get; }
    public IReadOnlyList<TextCandidate<Energy>> Energies { get; }
    public IReadOnlyList<TextCandidate<Power>> Powers { get; }
    public IReadOnlyList<TextCandidate<Voltage>> Voltages { get; }
    public IReadOnlyList<TextCandidate<ElectricCharge>> ElectricCharges { get; }
    public IReadOnlyList<TextCandidate<Torque>> Torques { get; }
    public IReadOnlyList<TextCandidate<Frequency>> Frequencies { get; }
    public IReadOnlyList<TextCandidate<Speed>> Speeds { get; }
    public IReadOnlyList<TextCandidate<Temperature>> Temperatures { get; }
    public IReadOnlyList<TextCandidate<DataSize>> DataSizes { get; }
    public IReadOnlyList<TextCandidate<Pressure>> Pressures { get; }
    public IReadOnlyList<TextCandidate<Percentage>> Percentages { get; }
    public IReadOnlyList<TextCandidate<ElectricCurrent>> ElectricCurrents { get; }
    public IReadOnlyList<TextCandidate<FlowRate>> FlowRates { get; }
    public IReadOnlyList<TextCandidate<LuminousFlux>> LuminousFluxes { get; }
    public IReadOnlyList<TextCandidate<RotationalSpeed>> RotationalSpeeds { get; }
    public IReadOnlyList<TextCandidate<SoundLevel>> SoundLevels { get; }
    public IReadOnlyList<TextCandidate<EuroEmissionClass>> EuroEmissionClasses { get; }
    public IReadOnlyList<TextCandidate<TireDimension>> TireDimensions { get; }
    public IReadOnlyList<TextCandidate<Color>> Colors { get; }
    public IReadOnlyList<TextCandidate<ScreenResolution>> ScreenResolutions { get; }
    public IReadOnlyList<TextCandidate<ScreenSize>> ScreenSizes { get; }
    public IReadOnlyList<TextCandidate<SwedishDrivingLicenseCategory>> DrivingLicenseCategories { get; }
    public IReadOnlyList<TextCandidate<SwedishSwishNumber>> SwedishSwishNumbers { get; }

    /// <summary>
    /// Convenience accessor: the subset of <see cref="Addresses"/> that qualify as Swedish
    /// (5-digit zip code, SE country). Derived from the same scan — not included separately in <see cref="All"/>.
    /// </summary>
    public IReadOnlyList<TextCandidate<SwedishAddress>> SwedishAddresses { get; }

    public int TotalCount => All.Count;

    internal TextScanResult(
        IReadOnlyList<TextCandidate<EmailAddress>> emails,
        IReadOnlyList<TextCandidate<PhoneNumber>> phoneNumbers,
        IReadOnlyList<TextCandidate<AddressZipCode>> zipCodes,
        IReadOnlyList<TextCandidate<Address>> addresses,
        IReadOnlyList<TextCandidate<SwedishPersonalIdentityNumber>> personalIdentityNumbers,
        IReadOnlyList<TextCandidate<SwedishCoordinationNumber>> coordinationNumbers,
        IReadOnlyList<TextCandidate<SwedishOrganizationNumber>> organizationNumbers,
        IReadOnlyList<TextCandidate<EuVatNumber>> vatNumbers,
        IReadOnlyList<TextCandidate<LeiCode>> leiCodes,
        IReadOnlyList<TextCandidate<DunsNumber>> dunsNumbers,
        IReadOnlyList<TextCandidate<Iban>> ibans,
        IReadOnlyList<TextCandidate<Bic>> bics,
        IReadOnlyList<TextCandidate<SwedishBankgiroNumber>> bankgiroNumbers,
        IReadOnlyList<TextCandidate<SwedishPostgiroNumber>> postgiroNumbers,
        IReadOnlyList<TextCandidate<SwedishBankAccount>> bankAccounts,
        IReadOnlyList<TextCandidate<SwedishOcrReferenceNumber>> ocrReferences,
        IReadOnlyList<TextCandidate<Isin>> isins,
        IReadOnlyList<TextCandidate<SwedishVehicleRegistrationNumber>> vehicleRegistrationNumbers,
        IReadOnlyList<TextCandidate<VehicleIdentificationNumber>> vehicleIdentificationNumbers,
        IReadOnlyList<TextCandidate<OperatingHours>> operatingHourValues,
        IReadOnlyList<TextCandidate<BoltPattern>> boltPatterns,
        IReadOnlyList<TextCandidate<WheelRimDimension>> wheelRimDimensions,
        IReadOnlyList<TextCandidate<EuTypeApprovalNumber>> euTypeApprovalNumbers,
        IReadOnlyList<TextCandidate<EnginePower>> enginePowers,
        IReadOnlyList<TextCandidate<FuelConsumption>> fuelConsumptions,
        IReadOnlyList<TextCandidate<EmissionRate>> emissionRates,
        IReadOnlyList<TextCandidate<SwedishPropertyDesignation>> propertyDesignations,
        IReadOnlyList<TextCandidate<Gtin13>> gtin13s,
        IReadOnlyList<TextCandidate<Gtin8>> gtin8s,
        IReadOnlyList<TextCandidate<Gtin12>> gtin12s,
        IReadOnlyList<TextCandidate<Gtin14>> gtin14s,
        IReadOnlyList<TextCandidate<IpRating>> ipRatings,
        IReadOnlyList<TextCandidate<ElectricalPhase>> electricalPhases,
        IReadOnlyList<TextCandidate<Country>> countries,
        IReadOnlyList<TextCandidate<SwedishMunicipality>> municipalities,
        IReadOnlyList<TextCandidate<SwedishCounty>> counties,
        IReadOnlyList<TextCandidate<Url>> urls,
        IReadOnlyList<TextCandidate<Length>> lengths,
        IReadOnlyList<TextCandidate<Area>> areas,
        IReadOnlyList<TextCandidate<Volume>> volumes,
        IReadOnlyList<TextCandidate<Weight>> weights,
        IReadOnlyList<TextCandidate<Energy>> energies,
        IReadOnlyList<TextCandidate<Power>> powers,
        IReadOnlyList<TextCandidate<Voltage>> voltages,
        IReadOnlyList<TextCandidate<ElectricCharge>> electricCharges,
        IReadOnlyList<TextCandidate<Torque>> torques,
        IReadOnlyList<TextCandidate<Frequency>> frequencies,
        IReadOnlyList<TextCandidate<Speed>> speeds,
        IReadOnlyList<TextCandidate<Temperature>> temperatures,
        IReadOnlyList<TextCandidate<DataSize>> dataSizes,
        IReadOnlyList<TextCandidate<Pressure>> pressures,
        IReadOnlyList<TextCandidate<Percentage>> percentages,
        IReadOnlyList<TextCandidate<ElectricCurrent>> electricCurrents,
        IReadOnlyList<TextCandidate<FlowRate>> flowRates,
        IReadOnlyList<TextCandidate<LuminousFlux>> luminousFluxes,
        IReadOnlyList<TextCandidate<RotationalSpeed>> rotationalSpeeds,
        IReadOnlyList<TextCandidate<SoundLevel>> soundLevels,
        IReadOnlyList<TextCandidate<EuroEmissionClass>> euroEmissionClasses,
        IReadOnlyList<TextCandidate<TireDimension>> tireDimensions,
        IReadOnlyList<TextCandidate<Color>> colors,
        IReadOnlyList<TextCandidate<ScreenResolution>> screenResolutions,
        IReadOnlyList<TextCandidate<ScreenSize>> screenSizes,
        IReadOnlyList<TextCandidate<SwedishDrivingLicenseCategory>> drivingLicenseCategories,
        IReadOnlyList<TextCandidate<SwedishSwishNumber>> swishNumbers)
    {
        Emails = emails;
        PhoneNumbers = phoneNumbers;
        ZipCodes = zipCodes;
        Addresses = addresses;
        PersonalIdentityNumbers = personalIdentityNumbers;
        CoordinationNumbers = coordinationNumbers;
        OrganizationNumbers = organizationNumbers;
        EuVatNumbers = vatNumbers;
        LeiCodes = leiCodes;
        DunsNumbers = dunsNumbers;
        Ibans = ibans;
        Bics = bics;
        BankgiroNumbers = bankgiroNumbers;
        PostgiroNumbers = postgiroNumbers;
        BankAccounts = bankAccounts;
        OcrReferences = ocrReferences;
        Isins = isins;
        VehicleRegistrationNumbers = vehicleRegistrationNumbers;
        VehicleIdentificationNumbers = vehicleIdentificationNumbers;
        OperatingHourValues = operatingHourValues;
        BoltPatterns = boltPatterns;
        WheelRimDimensions = wheelRimDimensions;
        EuTypeApprovalNumbers = euTypeApprovalNumbers;
        EnginePowers = enginePowers;
        FuelConsumptions = fuelConsumptions;
        EmissionRates = emissionRates;
        PropertyDesignations = propertyDesignations;
        Gtin13s = gtin13s;
        Gtin8s = gtin8s;
        Gtin12s = gtin12s;
        Gtin14s = gtin14s;
        IpRatings = ipRatings;
        ElectricalPhases = electricalPhases;
        Countries = countries;
        Municipalities = municipalities;
        Counties = counties;
        Urls = urls;
        Lengths = lengths;
        Areas = areas;
        Volumes = volumes;
        Weights = weights;
        Energies = energies;
        Powers = powers;
        Voltages = voltages;
        ElectricCharges = electricCharges;
        Torques = torques;
        Frequencies = frequencies;
        Speeds = speeds;
        Temperatures = temperatures;
        DataSizes = dataSizes;
        Pressures = pressures;
        Percentages = percentages;
        ElectricCurrents = electricCurrents;
        FlowRates = flowRates;
        LuminousFluxes = luminousFluxes;
        RotationalSpeeds = rotationalSpeeds;
        SoundLevels = soundLevels;
        EuroEmissionClasses = euroEmissionClasses;
        TireDimensions = tireDimensions;
        Colors = colors;
        ScreenResolutions = screenResolutions;
        ScreenSizes = screenSizes;
        DrivingLicenseCategories = drivingLicenseCategories;
        SwedishSwishNumbers = swishNumbers;

        var swedish = new List<TextCandidate<SwedishAddress>>();
        foreach (var a in addresses)
        {
            if (!SwedishAddress.TryParse(
                    a.Value.Street.Street,
                    a.Value.ZipCode?.Value,
                    a.Value.City?.Value,
                    a.Value.Country?.Alpha2Code,
                    out var sa))
                continue;

            swedish.Add(new TextCandidate<SwedishAddress>(
                a.StartIndex, a.Length, a.OriginalText,
                nameof(SwedishAddress), a.Category,
                sa!.ToNormalizedString(), sa.ToString(),
                sa.Address.ToMaskedString(), a.Confidence, sa));
        }
        SwedishAddresses = swedish;

        var all = new List<TextCandidate>();
        AddRange(all, emails);
        AddRange(all, phoneNumbers);
        AddRange(all, zipCodes);
        AddRange(all, addresses);
        AddRange(all, personalIdentityNumbers);
        AddRange(all, coordinationNumbers);
        AddRange(all, organizationNumbers);
        AddRange(all, vatNumbers);
        AddRange(all, leiCodes);
        AddRange(all, dunsNumbers);
        AddRange(all, ibans);
        AddRange(all, bics);
        AddRange(all, bankgiroNumbers);
        AddRange(all, postgiroNumbers);
        AddRange(all, bankAccounts);
        AddRange(all, ocrReferences);
        AddRange(all, isins);
        AddRange(all, vehicleRegistrationNumbers);
        AddRange(all, vehicleIdentificationNumbers);
        AddRange(all, operatingHourValues);
        AddRange(all, boltPatterns);
        AddRange(all, wheelRimDimensions);
        AddRange(all, euTypeApprovalNumbers);
        AddRange(all, enginePowers);
        AddRange(all, fuelConsumptions);
        AddRange(all, emissionRates);
        AddRange(all, propertyDesignations);
        AddRange(all, gtin13s);
        AddRange(all, gtin8s);
        AddRange(all, gtin12s);
        AddRange(all, gtin14s);
        AddRange(all, ipRatings);
        AddRange(all, electricalPhases);
        AddRange(all, countries);
        AddRange(all, municipalities);
        AddRange(all, counties);
        AddRange(all, urls);
        AddRange(all, lengths);
        AddRange(all, areas);
        AddRange(all, volumes);
        AddRange(all, weights);
        AddRange(all, energies);
        AddRange(all, powers);
        AddRange(all, voltages);
        AddRange(all, electricCharges);
        AddRange(all, torques);
        AddRange(all, frequencies);
        AddRange(all, speeds);
        AddRange(all, temperatures);
        AddRange(all, dataSizes);
        AddRange(all, pressures);
        AddRange(all, percentages);
        AddRange(all, electricCurrents);
        AddRange(all, flowRates);
        AddRange(all, luminousFluxes);
        AddRange(all, rotationalSpeeds);
        AddRange(all, soundLevels);
        AddRange(all, euroEmissionClasses);
        AddRange(all, tireDimensions);
        AddRange(all, colors);
        AddRange(all, screenResolutions);
        AddRange(all, screenSizes);
        AddRange(all, drivingLicenseCategories);
        AddRange(all, swishNumbers);
        all.Sort((a, b) => a.StartIndex.CompareTo(b.StartIndex));
        All = all;
        ResolvedCandidates = Resolve(all);
    }

    /// <summary>
    /// Returns the number of candidates in the given category.
    /// </summary>
    public int CountByCategory(TextCandidateCategory category) =>
        All.Count(c => c.Category == category);

    /// <summary>
    /// Returns the number of candidates at or above the given confidence level.
    /// </summary>
    public int CountByConfidence(TextMatchConfidence minimum) =>
        All.Count(c => c.Confidence >= minimum);

    /// <summary>
    /// Replaces each resolved candidate's span in <paramref name="text"/> with its
    /// <see cref="TextCandidate.MaskedForm"/>.
    /// </summary>
    public string MaskAll(string text) =>
        ReplaceAll(text, c => c.MaskedForm);

    /// <summary>
    /// Replaces each resolved candidate's span in <paramref name="text"/> with <paramref name="replacement"/>.
    /// </summary>
    public string RedactAll(string text, string replacement = "[REDACTED]") =>
        ReplaceAll(text, _ => replacement);

    /// <summary>
    /// Replaces each resolved candidate's span in <paramref name="text"/> using a custom delegate.
    /// Replacements are applied right-to-left on the original text so positions remain valid.
    /// </summary>
    public string ReplaceAll(string text, Func<TextCandidate, string> replacer)
    {
        if (ResolvedCandidates.Count == 0) return text;

        var sb = new StringBuilder(text);
        for (var i = ResolvedCandidates.Count - 1; i >= 0; i--)
        {
            var c = ResolvedCandidates[i];
            if (c.StartIndex + c.Length > sb.Length) continue;
            sb.Remove(c.StartIndex, c.Length);
            sb.Insert(c.StartIndex, replacer(c));
        }
        return sb.ToString();
    }

    private static readonly Dictionary<TextCandidateCategory, int> CategoryPriority = new()
    {
        [TextCandidateCategory.PersonalIdentifier] = 0,
        [TextCandidateCategory.Contact] = 1,
        [TextCandidateCategory.Financial] = 2,
        [TextCandidateCategory.OrganizationIdentifier] = 3,
        [TextCandidateCategory.Vehicle] = 4,
        [TextCandidateCategory.Property] = 5,
        [TextCandidateCategory.Product] = 6,
        [TextCandidateCategory.Geography] = 7,
        [TextCandidateCategory.Measurement] = 8,
    };

    private static IReadOnlyList<TextCandidate> Resolve(List<TextCandidate> sorted)
    {
        if (sorted.Count == 0) return [];

        var resolved = new List<TextCandidate>();
        foreach (var candidate in sorted)
        {
            var dominated = false;
            for (var i = resolved.Count - 1; i >= 0; i--)
            {
                var existing = resolved[i];
                if (!existing.Overlaps(candidate)) continue;

                var winner = PickWinner(existing, candidate);
                if (ReferenceEquals(winner, existing))
                {
                    dominated = true;
                    break;
                }

                resolved.RemoveAt(i);
            }

            if (!dominated)
                resolved.Add(candidate);
        }

        resolved.Sort((a, b) => a.StartIndex.CompareTo(b.StartIndex));
        return resolved;
    }

    private static TextCandidate PickWinner(TextCandidate a, TextCandidate b)
    {
        if (a.Contains(b)) return a;
        if (b.Contains(a)) return b;

        if (a.Confidence != b.Confidence)
            return a.Confidence > b.Confidence ? a : b;

        if (a.Length != b.Length)
            return a.Length > b.Length ? a : b;

        var pa = CategoryPriority.GetValueOrDefault(a.Category, 99);
        var pb = CategoryPriority.GetValueOrDefault(b.Category, 99);
        return pa <= pb ? a : b;
    }

    private static void AddRange<T>(List<TextCandidate> target, IReadOnlyList<TextCandidate<T>> source)
    {
        for (var i = 0; i < source.Count; i++)
            target.Add(source[i]);
    }
}
