using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// A refrigerant designation (<c>köldmedium</c>) following the ASHRAE Standard 34
/// numbering system, with associated GWP (Global Warming Potential) metadata.
/// </summary>
/// <remarks>
/// <para>Used by refrigeration units, heat pumps, air conditioning systems, and chillers.
/// Captures the most common civilian and commercial refrigerants in Europe and Sweden,
/// including legacy CFC/HCFC refrigerants (R12, R22) that may appear in older equipment,
/// HFCs (R134a, R404A, R410A, R452A, R507A, R513A), HFOs (R1234yf, R1234ze, R1233zd),
/// natural refrigerants (R290 propane, R600a isobutane, R744 CO₂, R717 ammonia, R1270 propylene),
/// and HFC/HFO blends.</para>
/// <para>GWP values are 100-year AR5 (IPCC Fifth Assessment Report) figures used by the EU F-gas regulation.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.ashrae.org/technical-resources/standards-and-guidelines/standards-addenda/standard-34-designation-and-safety-classification-of-refrigerants">ASHRAE Standard 34</see> — Designation and Safety Classification of Refrigerants</description></item>
/// <item><description><see href="https://eur-lex.europa.eu/eli/reg/2014/517/oj">EU Regulation No 517/2014</see> — F-gas Regulation</description></item>
/// </list>
/// </remarks>
public sealed class Refrigerant : IEquatable<Refrigerant>, IComparable<Refrigerant>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Refrigerant",
        "Köldmedium",
        "❄️",
        [
            "https://www.ashrae.org/technical-resources/standards-and-guidelines/standards-addenda/standard-34-designation-and-safety-classification-of-refrigerants",
            "https://eur-lex.europa.eu/eli/reg/2014/517/oj"
        ]);

    private static readonly Lazy<Dictionary<string, Refrigerant>> Lookup = new(BuildLookup);

    /// <summary>Broad chemical family classification for a refrigerant.</summary>
    public enum RefrigerantFamily
    {
        /// <summary>Unknown or unspecified refrigerant family.</summary>
        Unknown = 0,
        /// <summary>Chlorofluorocarbon (banned under Montreal Protocol; legacy only).</summary>
        Cfc = 1,
        /// <summary>Hydrochlorofluorocarbon (phased out for ozone depletion).</summary>
        Hcfc,
        /// <summary>Hydrofluorocarbon (high GWP; subject to F-gas phase-down).</summary>
        Hfc,
        /// <summary>Hydrofluoroolefin (low GWP, mildly flammable).</summary>
        Hfo,
        /// <summary>Blend of HFCs and HFOs.</summary>
        HfcHfoBlend,
        /// <summary>Natural refrigerant (hydrocarbon, ammonia, CO₂, water).</summary>
        Natural
    }

    /// <summary>Canonical ASHRAE designation, e.g. <c>R-134a</c>.</summary>
    public string Value { get; }

    /// <summary>Compact form without hyphen, e.g. <c>R134a</c>.</summary>
    public string CompactCode { get; }

    /// <summary>Common chemical or trade name, e.g. <c>1,1,1,2-Tetrafluoroethane</c> for R-134a, <c>Propane</c> for R-290.</summary>
    public string Composition { get; }

    /// <summary>Chemical family classification.</summary>
    public RefrigerantFamily Family { get; }

    /// <summary>100-year Global Warming Potential (AR5). <c>0</c> for natural refrigerants such as CO₂, propane, ammonia (effectively zero for regulatory purposes).</summary>
    public int Gwp100Year { get; }

    /// <summary>ASHRAE safety classification (e.g. <c>A1</c>, <c>A2L</c>, <c>A3</c>, <c>B1</c>, <c>B2L</c>).</summary>
    public string SafetyClass { get; }

    /// <summary>Whether the refrigerant is banned or restricted under the EU F-gas regulation in new equipment.</summary>
    public bool IsRestrictedInEu { get; }

    public static readonly Refrigerant R12 = new("R-12", "R12", "Dichlorodifluoromethane", RefrigerantFamily.Cfc, 10900, "A1", true);
    public static readonly Refrigerant R22 = new("R-22", "R22", "Chlorodifluoromethane", RefrigerantFamily.Hcfc, 1810, "A1", true);
    public static readonly Refrigerant R32 = new("R-32", "R32", "Difluoromethane", RefrigerantFamily.Hfc, 675, "A2L", false);
    public static readonly Refrigerant R134a = new("R-134a", "R134a", "1,1,1,2-Tetrafluoroethane", RefrigerantFamily.Hfc, 1430, "A1", false);
    public static readonly Refrigerant R290 = new("R-290", "R290", "Propane", RefrigerantFamily.Natural, 3, "A3", false);
    public static readonly Refrigerant R404A = new("R-404A", "R404A", "HFC blend (R-125/R-143a/R-134a)", RefrigerantFamily.Hfc, 3922, "A1", true);
    public static readonly Refrigerant R407C = new("R-407C", "R407C", "HFC blend (R-32/R-125/R-134a)", RefrigerantFamily.Hfc, 1774, "A1", false);
    public static readonly Refrigerant R407F = new("R-407F", "R407F", "HFC blend (R-32/R-125/R-134a)", RefrigerantFamily.Hfc, 1825, "A1", false);
    public static readonly Refrigerant R410A = new("R-410A", "R410A", "HFC blend (R-32/R-125)", RefrigerantFamily.Hfc, 2088, "A1", false);
    public static readonly Refrigerant R448A = new("R-448A", "R448A", "HFC/HFO blend (R-32/R-125/R-134a/R-1234yf/R-1234ze)", RefrigerantFamily.HfcHfoBlend, 1387, "A1", false);
    public static readonly Refrigerant R449A = new("R-449A", "R449A", "HFC/HFO blend (R-32/R-125/R-134a/R-1234yf)", RefrigerantFamily.HfcHfoBlend, 1397, "A1", false);
    public static readonly Refrigerant R452A = new("R-452A", "R452A", "HFC/HFO blend (R-32/R-125/R-1234yf)", RefrigerantFamily.HfcHfoBlend, 2140, "A1", false);
    public static readonly Refrigerant R507A = new("R-507A", "R507A", "HFC blend (R-125/R-143a)", RefrigerantFamily.Hfc, 3985, "A1", true);
    public static readonly Refrigerant R513A = new("R-513A", "R513A", "HFC/HFO blend (R-1234yf/R-134a)", RefrigerantFamily.HfcHfoBlend, 631, "A1", false);
    public static readonly Refrigerant R600a = new("R-600a", "R600a", "Isobutane", RefrigerantFamily.Natural, 3, "A3", false);
    public static readonly Refrigerant R717 = new("R-717", "R717", "Ammonia", RefrigerantFamily.Natural, 0, "B2L", false);
    public static readonly Refrigerant R718 = new("R-718", "R718", "Water", RefrigerantFamily.Natural, 0, "A1", false);
    public static readonly Refrigerant R744 = new("R-744", "R744", "Carbon dioxide", RefrigerantFamily.Natural, 1, "A1", false);
    public static readonly Refrigerant R1234yf = new("R-1234yf", "R1234yf", "2,3,3,3-Tetrafluoropropene", RefrigerantFamily.Hfo, 4, "A2L", false);
    public static readonly Refrigerant R1234ze = new("R-1234ze", "R1234ze", "trans-1,3,3,3-Tetrafluoropropene", RefrigerantFamily.Hfo, 7, "A2L", false);
    public static readonly Refrigerant R1233zd = new("R-1233zd", "R1233zd", "trans-1-Chloro-3,3,3-trifluoropropene", RefrigerantFamily.Hfo, 1, "A1", false);
    public static readonly Refrigerant R1270 = new("R-1270", "R1270", "Propylene", RefrigerantFamily.Natural, 2, "A3", false);

    /// <summary>All predefined refrigerants.</summary>
    public static IReadOnlyList<Refrigerant> All { get; } =
    [
        R12, R22, R32, R134a, R290, R404A, R407C, R407F, R410A,
        R448A, R449A, R452A, R507A, R513A,
        R600a, R717, R718, R744,
        R1234yf, R1234ze, R1233zd, R1270
    ];

    private Refrigerant(string value, string compactCode, string composition, RefrigerantFamily family, int gwp100Year, string safetyClass, bool isRestrictedInEu)
    {
        Value = value;
        CompactCode = compactCode;
        Composition = composition;
        Family = family;
        Gwp100Year = gwp100Year;
        SafetyClass = safetyClass;
        IsRestrictedInEu = isRestrictedInEu;
    }

    public static bool TryParse(string? input, out Refrigerant? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var fromDict))
        {
            result = fromDict;
            return true;
        }

        return false;
    }

    public static Refrigerant Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid refrigerant.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical ASHRAE designation, e.g. <c>R-134a</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical ASHRAE designation, e.g. <c>R-134a</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical ASHRAE designation, e.g. <c>R-134a</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical ASHRAE designation, e.g. <c>R-134a</c>.</summary>
    public override string ToString() => Value;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = Regex.Replace(folded, @"[\s\-_]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, Refrigerant> d, Refrigerant value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, Refrigerant> BuildLookup()
    {
        var d = new Dictionary<string, Refrigerant>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in All)
        {
            AddKey(d, r, r.Value);
            AddKey(d, r, r.CompactCode);
        }

        AddKey(d, R744, "CO2");
        AddKey(d, R744, "CO₂");
        AddKey(d, R744, "Carbon dioxide");
        AddKey(d, R744, "Koldioxid");
        AddKey(d, R290, "Propane");
        AddKey(d, R290, "Propan");
        AddKey(d, R600a, "Isobutane");
        AddKey(d, R600a, "Isobutan");
        AddKey(d, R717, "Ammonia");
        AddKey(d, R717, "Ammoniak");
        AddKey(d, R717, "NH3");
        AddKey(d, R718, "Water");
        AddKey(d, R718, "Vatten");
        AddKey(d, R718, "H2O");
        AddKey(d, R1270, "Propylene");
        AddKey(d, R1270, "Propen");

        return d;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\bR-?\d{2,4}[A-Za-z]?\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like ASHRAE refrigerant designations
    /// (e.g. <c>R-134a</c>, <c>R134a</c>, <c>R290</c>, <c>R744</c>). This is heuristic-based and
    /// may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Refrigerant>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Refrigerant>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var r)) continue;
            results.Add(new TextCandidate<Refrigerant>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Refrigerant), TextCandidateCategory.Product,
                r!.ToNormalizedString(), r.ToString(),
                r.ToMaskedString(),
                TextMatchConfidence.Medium,
                r));
        }
        return results;
    }

    public static bool operator ==(Refrigerant? a, Refrigerant? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Refrigerant? a, Refrigerant? b) => !(a == b);

    public bool Equals(Refrigerant? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Refrigerant other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(Refrigerant? other) =>
        other is null ? 1 : string.Compare(CompactCode, other.CompactCode, StringComparison.OrdinalIgnoreCase);

    public static bool operator <(Refrigerant a, Refrigerant b) => a.CompareTo(b) < 0;
    public static bool operator >(Refrigerant a, Refrigerant b) => a.CompareTo(b) > 0;
    public static bool operator <=(Refrigerant a, Refrigerant b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Refrigerant a, Refrigerant b) => a.CompareTo(b) >= 0;
}
