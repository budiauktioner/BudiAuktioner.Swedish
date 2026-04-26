using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// The electrochemical chemistry of a battery (<c>batterikemi</c>),
/// e.g. <c>Li-ion</c>, <c>LiFePO4</c>, <c>AGM</c>, <c>Pb-Acid</c>, <c>Gel</c>.
/// </summary>
/// <remarks>
/// <para>Captures the most common chemistries used in caravans, RVs, marine, automotive starter
/// batteries, off-grid storage, and consumer portable electronics. <see cref="NominalCellVoltageV"/>
/// gives the typical nominal voltage of a single cell, useful when correlating with
/// pack-level voltage figures.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iec.ch/">IEC battery standards</see></description></item>
/// </list>
/// </remarks>
public sealed class BatteryChemistry : IEquatable<BatteryChemistry>, IComparable<BatteryChemistry>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Battery Chemistry",
        "Batterikemi",
        "🔋",
        ["https://www.iec.ch/"]);

    private static readonly Lazy<Dictionary<string, BatteryChemistry>> Lookup = new(BuildLookup);

    /// <summary>Canonical short code, e.g. <c>Li-ion</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Lithium-ion</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Litium-jon</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Whether the chemistry is rechargeable (secondary cell).</summary>
    public bool IsRechargeable { get; }

    /// <summary>Nominal cell voltage in volts, e.g. <c>3.7</c> for Li-ion, <c>3.2</c> for LiFePO4, <c>2.0</c> for lead-acid.</summary>
    public decimal NominalCellVoltageV { get; }

    public static readonly BatteryChemistry LithiumIon = new(
        "Li-ion", "Lithium-ion", "Litium-jon", true, 3.7m);

    public static readonly BatteryChemistry LiFePO4 = new(
        "LiFePO4", "Lithium iron phosphate", "Litiumjärnfosfat", true, 3.2m);

    public static readonly BatteryChemistry LithiumPolymer = new(
        "LiPo", "Lithium polymer", "Litium-polymer", true, 3.7m);

    public static readonly BatteryChemistry LithiumTitanate = new(
        "LTO", "Lithium titanate", "Litium-titanat", true, 2.4m);

    public static readonly BatteryChemistry NickelMetalHydride = new(
        "NiMH", "Nickel-metal hydride", "Nickel-metallhydrid", true, 1.2m);

    public static readonly BatteryChemistry NickelCadmium = new(
        "NiCd", "Nickel-cadmium", "Nickel-kadmium", true, 1.2m);

    public static readonly BatteryChemistry FloodedLeadAcid = new(
        "Pb-Acid", "Flooded lead-acid", "Blybatteri (flytande syra)", true, 2.0m);

    public static readonly BatteryChemistry AbsorbedGlassMat = new(
        "AGM", "Absorbed Glass Mat", "AGM-batteri", true, 2.0m);

    public static readonly BatteryChemistry GelLeadAcid = new(
        "Gel", "Gel lead-acid", "Gelbatteri", true, 2.0m);

    public static readonly BatteryChemistry Alkaline = new(
        "Alkaline", "Alkaline", "Alkaliskt", false, 1.5m);

    public static readonly BatteryChemistry ZincCarbon = new(
        "Zinc-carbon", "Zinc-carbon", "Zinkkol", false, 1.5m);

    /// <summary>All predefined chemistries.</summary>
    public static IReadOnlyList<BatteryChemistry> All { get; } =
    [
        LithiumIon, LiFePO4, LithiumPolymer, LithiumTitanate,
        NickelMetalHydride, NickelCadmium,
        FloodedLeadAcid, AbsorbedGlassMat, GelLeadAcid,
        Alkaline, ZincCarbon
    ];

    private BatteryChemistry(string value, string englishName, string localizedName, bool isRechargeable, decimal nominalCellVoltageV)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        IsRechargeable = isRechargeable;
        NominalCellVoltageV = nominalCellVoltageV;
    }

    public static bool TryParse(string? input, out BatteryChemistry? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var v))
        {
            result = v;
            return true;
        }
        return false;
    }

    public static BatteryChemistry Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid battery chemistry.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Litium-jon</c> (Swedish) or
    /// <c>Lithium-ion</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical short code, e.g. <c>Li-ion</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical short code, e.g. <c>Li-ion</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, BatteryChemistry> d, BatteryChemistry value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, BatteryChemistry> BuildLookup()
    {
        var d = new Dictionary<string, BatteryChemistry>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in All)
        {
            AddKey(d, c, c.Value);
            AddKey(d, c, c.EnglishName);
            AddKey(d, c, c.LocalizedName);
        }

        AddKey(d, LithiumIon, "Lithium ion");
        AddKey(d, LithiumIon, "Litiumjon");
        AddKey(d, LithiumIon, "Litium ion");
        AddKey(d, LithiumIon, "Lithium-Ion");
        AddKey(d, LithiumIon, "Li-Ion");
        AddKey(d, LiFePO4, "LFP");
        AddKey(d, LiFePO4, "Litium järnfosfat");
        AddKey(d, LiFePO4, "Lithium iron phosphate");
        AddKey(d, LithiumPolymer, "Li-poly");
        AddKey(d, LithiumPolymer, "Litiumpolymer");
        AddKey(d, NickelMetalHydride, "Nickel metal hydride");
        AddKey(d, NickelMetalHydride, "Nickelmetallhydrid");
        AddKey(d, NickelCadmium, "Nickelkadmium");
        AddKey(d, FloodedLeadAcid, "Lead-acid");
        AddKey(d, FloodedLeadAcid, "Lead acid");
        AddKey(d, FloodedLeadAcid, "Bly");
        AddKey(d, FloodedLeadAcid, "Blybatteri");
        AddKey(d, FloodedLeadAcid, "Pb");
        AddKey(d, FloodedLeadAcid, "Wet cell");
        AddKey(d, FloodedLeadAcid, "Våtcell");
        AddKey(d, AbsorbedGlassMat, "Absorbent glass mat");
        AddKey(d, AbsorbedGlassMat, "AGM-batteri");
        AddKey(d, GelLeadAcid, "Gel cell");
        AddKey(d, GelLeadAcid, "Gel-batteri");
        AddKey(d, Alkaline, "Alkalisk");
        AddKey(d, ZincCarbon, "Zink-kol");
        AddKey(d, ZincCarbon, "Zinkkol");

        return d;
    }

    public static bool operator ==(BatteryChemistry? a, BatteryChemistry? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(BatteryChemistry? a, BatteryChemistry? b) => !(a == b);

    public bool Equals(BatteryChemistry? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is BatteryChemistry other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(BatteryChemistry? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(BatteryChemistry a, BatteryChemistry b) => a.CompareTo(b) < 0;
    public static bool operator >(BatteryChemistry a, BatteryChemistry b) => a.CompareTo(b) > 0;
    public static bool operator <=(BatteryChemistry a, BatteryChemistry b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BatteryChemistry a, BatteryChemistry b) => a.CompareTo(b) >= 0;
}
