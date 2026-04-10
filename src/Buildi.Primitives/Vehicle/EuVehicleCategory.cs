using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// EU vehicle category code (<c>fordonskategori</c>) per Regulation (EU) 2018/858 and Directive 2007/46/EC.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32018R0858">Regulation (EU) 2018/858</see> — EU vehicle category framework</description></item>
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonskategorier</description></item>
/// </list>
/// </remarks>
public sealed class EuVehicleCategory : IEquatable<EuVehicleCategory>, IComparable<EuVehicleCategory>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("EU Vehicle Category", "Fordonskategori", "🚛", ["https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32018R0858", "https://www.transportstyrelsen.se/"]);

    private static readonly Regex CategoryPattern = new(
        @"^([MNOLTCRS])(\d)(e)?(-[A-Z]\d?)?(G)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, (string English, string Localized)> KnownCategories = new(StringComparer.Ordinal)
    {
        ["M1"] = ("Passenger vehicle ≤8+1 seats", "Personbil ≤8+1 passagerare"),
        ["M2"] = ("Bus/coach >8+1 seats, ≤5 t", "Buss >8+1 passagerare, ≤5 ton"),
        ["M3"] = ("Bus/coach >8+1 seats, >5 t", "Buss >8+1 passagerare, >5 ton"),
        ["N1"] = ("Light commercial vehicle ≤3.5 t", "Lätt lastbil ≤3,5 ton"),
        ["N2"] = ("Medium goods vehicle 3.5–12 t", "Lastbil 3,5–12 ton"),
        ["N3"] = ("Heavy goods vehicle >12 t", "Tung lastbil >12 ton"),
        ["O1"] = ("Light trailer ≤0.75 t", "Lätt släpvagn ≤0,75 ton"),
        ["O2"] = ("Trailer 0.75–3.5 t", "Släpvagn 0,75–3,5 ton"),
        ["O3"] = ("Trailer 3.5–10 t", "Släpvagn 3,5–10 ton"),
        ["O4"] = ("Heavy trailer >10 t", "Tung släpvagn >10 ton"),
        ["L1e"] = ("Two-wheel moped", "Tvåhjulig moped"),
        ["L2e"] = ("Three-wheel moped", "Trehjulig moped"),
        ["L3e"] = ("Two-wheel motorcycle", "Tvåhjulig motorcykel"),
        ["L4e"] = ("Motorcycle with sidecar", "Motorcykel med sidvagn"),
        ["L5e"] = ("Motor tricycle", "Motordriven trehjuling"),
        ["L6e"] = ("Light quadricycle", "Lätt fyrhjuling"),
        ["L7e"] = ("Heavy quadricycle", "Tung fyrhjuling"),
        ["T1"] = ("Standard agricultural tractor", "Standardjordbrukstraktor"),
        ["T2"] = ("Narrow-track tractor", "Smalspårig traktor"),
        ["T3"] = ("Low-clearance tractor", "Lågbyggd traktor"),
        ["T4"] = ("Special-purpose tractor", "Specialtraktor"),
        ["T5"] = ("Fast tractor", "Snabb traktor"),
        ["C1"] = ("Track-laying tractor (as T1)", "Bandtraktor (som T1)"),
        ["C2"] = ("Track-laying tractor (as T2)", "Bandtraktor (som T2)"),
        ["C3"] = ("Track-laying tractor (as T3)", "Bandtraktor (som T3)"),
        ["C4"] = ("Track-laying tractor (as T4)", "Bandtraktor (som T4)"),
        ["C5"] = ("Track-laying tractor (as T5)", "Bandtraktor (som T5)"),
        ["R1"] = ("Agricultural trailer ≤1.5 t", "Jordbrukssläpvagn ≤1,5 ton"),
        ["R2"] = ("Agricultural trailer 1.5–3.5 t", "Jordbrukssläpvagn 1,5–3,5 ton"),
        ["R3"] = ("Agricultural trailer 3.5–21 t", "Jordbrukssläpvagn 3,5–21 ton"),
        ["R4"] = ("Agricultural trailer >21 t", "Jordbrukssläpvagn >21 ton"),
        ["S1"] = ("Towed machinery ≤3.5 t", "Utbytbar dragen maskin ≤3,5 ton"),
        ["S2"] = ("Towed machinery >3.5 t", "Utbytbar dragen maskin >3,5 ton"),
    };

    /// <summary>Canonical category code, e.g. <c>N1</c>, <c>M1G</c>, <c>L3e-A2</c>.</summary>
    public string Value { get; }

    /// <summary>Base category letter, e.g. <c>M</c>, <c>N</c>, <c>L</c>.</summary>
    public string BaseCategory { get; }

    /// <summary>Numeric part of the category, e.g. <c>1</c>, <c>3</c>.</summary>
    public int CategoryNumber { get; }

    /// <summary>Optional suffix such as <c>G</c> (off-road), <c>e</c> (L categories), or <c>e-A2</c>.</summary>
    public string? Suffix { get; }

    /// <summary>English description, e.g. <c>Passenger vehicle ≤8+1 seats</c>.</summary>
    public string EnglishDescription { get; }

    /// <summary>Localized (Swedish) description, e.g. <c>Personbil ≤8+1 passagerare</c>.</summary>
    public string LocalizedDescription { get; }

    /// <summary><see langword="true"/> if the category has a <c>G</c> suffix indicating an off-road variant.</summary>
    public bool IsOffRoad { get; }

    /// <summary>Culture-aware description based on <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>.</summary>
    public string Description => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedDescription : EnglishDescription;

    private EuVehicleCategory(string value, string baseCategory, int categoryNumber, string? suffix,
        string englishDescription, string localizedDescription, bool isOffRoad)
    {
        Value = value;
        BaseCategory = baseCategory;
        CategoryNumber = categoryNumber;
        Suffix = suffix;
        EnglishDescription = englishDescription;
        LocalizedDescription = localizedDescription;
        IsOffRoad = isOffRoad;
    }

    /// <summary>
    /// Attempts to parse an EU vehicle category code such as <c>M1</c>, <c>N1G</c>, or <c>L3e-A2</c>.
    /// </summary>
    public static bool TryParse(string? input, out EuVehicleCategory? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = input!.Trim();
        var match = CategoryPattern.Match(trimmed);
        if (!match.Success) return false;

        var baseLetter = match.Groups[1].Value.ToUpperInvariant();
        var number = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var hasE = match.Groups[3].Success;
        var subSuffix = match.Groups[4].Success ? match.Groups[4].Value.ToUpperInvariant() : null;
        var hasG = match.Groups[5].Success;

        if (baseLetter == "L" && !hasE) return false;
        if (baseLetter != "L" && hasE) return false;
        if (hasG && baseLetter is not "M" and not "N") return false;
        if (subSuffix is not null && !hasE) return false;

        var lookupKey = hasE ? $"{baseLetter}{number}e" : $"{baseLetter}{number}";
        if (!KnownCategories.TryGetValue(lookupKey, out var descriptions)) return false;

        string? suffix = null;
        if (hasE && subSuffix is not null) suffix = $"e{subSuffix}";
        else if (hasE) suffix = "e";
        else if (hasG) suffix = "G";

        var value = $"{baseLetter}{number}{suffix}";

        var english = descriptions.English;
        var localized = descriptions.Localized;
        if (hasG)
        {
            english += " (off-road)";
            localized += " (terrängfordon)";
        }

        result = new EuVehicleCategory(value, baseLetter, number, suffix, english, localized, hasG);
        return true;
    }

    /// <summary>
    /// Parses an EU vehicle category code. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static EuVehicleCategory Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid EU vehicle category.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid EU vehicle category code.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the category description, e.g. <c>Passenger vehicle ≤8+1 seats</c> or <c>Lätt lastbil ≤3,5 ton</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.Description
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical category code, e.g. <c>M1</c>, <c>L3e-A2</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical category code.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical category code, e.g. <c>M1</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical category code, e.g. <c>M1</c>.</summary>
    public override string ToString() => Value;

    public static bool operator ==(EuVehicleCategory? a, EuVehicleCategory? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(EuVehicleCategory? a, EuVehicleCategory? b) => !(a == b);

    public bool Equals(EuVehicleCategory? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is EuVehicleCategory other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(EuVehicleCategory? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(EuVehicleCategory left, EuVehicleCategory right) => left.CompareTo(right) < 0;
    public static bool operator >(EuVehicleCategory left, EuVehicleCategory right) => left.CompareTo(right) > 0;
    public static bool operator <=(EuVehicleCategory left, EuVehicleCategory right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EuVehicleCategory left, EuVehicleCategory right) => left.CompareTo(right) >= 0;
}
