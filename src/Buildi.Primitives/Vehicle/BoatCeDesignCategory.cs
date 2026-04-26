using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// CE design category for recreational craft (<c>CE-konstruktionskategori</c>) per
/// the EU Recreational Craft Directive (RCD) 2013/53/EU and ISO 12217:
/// <c>A</c> Ocean, <c>B</c> Offshore, <c>C</c> Inshore, <c>D</c> Sheltered waters.
/// </summary>
/// <remarks>
/// <para>The CE design category is the maximum sea condition for which a boat is
/// designed and certified, and is normally embossed on the boat's CE plate.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32013L0053">Directive 2013/53/EU</see> — recreational craft and personal watercraft</description></item>
/// <item><description><see href="https://www.iso.org/standard/56707.html">ISO 12217-1</see> — small craft stability and buoyancy assessment</description></item>
/// </list>
/// </remarks>
public sealed class BoatCeDesignCategory : IEquatable<BoatCeDesignCategory>, IComparable<BoatCeDesignCategory>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Boat CE Design Category",
        "Båt CE-konstruktionskategori",
        "⛵",
        ["https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32013L0053", "https://www.iso.org/standard/56707.html"]);

    private static readonly Lazy<Dictionary<string, BoatCeDesignCategory>> Lookup = new(BuildLookup);

    /// <summary>Single-letter code, e.g. <c>A</c>.</summary>
    public string Value { get; }

    /// <summary>English short name of the operating environment, e.g. <c>Ocean</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Swedish short name of the operating environment, e.g. <c>Hav</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Maximum significant wave height in metres for which the category is rated.</summary>
    public decimal MaxSignificantWaveHeightM { get; }

    /// <summary>Maximum sustained Beaufort wind force for which the category is rated.</summary>
    public int MaxBeaufortWindForce { get; }

    /// <summary>English description of the intended operating environment.</summary>
    public string EnglishDescription { get; }

    /// <summary>Swedish description of the intended operating environment.</summary>
    public string LocalizedDescription { get; }

    public static readonly BoatCeDesignCategory A = new(
        "A", "Ocean", "Hav", 7.0m, 10,
        "Designed for extended voyages where conditions may exceed wind force 8 (Beaufort) and significant wave heights of 4 m and above, but excluding abnormal conditions, and vessels largely self-sufficient.",
        "För längre färder där vindhastigheten kan överskrida 8 Beaufort och signifikant våghöjd överstiga 4 m, undantaget abnorma förhållanden. Båten är i stort sett självförsörjande.");

    public static readonly BoatCeDesignCategory B = new(
        "B", "Offshore", "Utomskärs", 4.0m, 8,
        "Designed for offshore voyages where conditions up to and including wind force 8 and significant wave heights up to and including 4 m may be experienced.",
        "För färder utomskärs där vindar upp till 8 Beaufort och signifikanta våghöjder upp till 4 m kan förekomma.");

    public static readonly BoatCeDesignCategory C = new(
        "C", "Inshore", "Inomskärs", 2.0m, 6,
        "Designed for inshore voyages in coastal waters, large bays, estuaries, lakes and rivers where conditions up to and including wind force 6 and significant wave heights up to and including 2 m may be experienced.",
        "För färder inomskärs och i kustnära farvatten, större vikar, flodmynningar, sjöar och floder där vindar upp till 6 Beaufort och signifikanta våghöjder upp till 2 m kan förekomma.");

    public static readonly BoatCeDesignCategory D = new(
        "D", "Sheltered waters", "Skyddade vatten", 0.3m, 4,
        "Designed for voyages on sheltered coastal waters, small bays, small lakes, rivers and canals where conditions up to and including wind force 4 and significant wave heights up to and including 0.3 m may be experienced, with occasional waves up to 0.5 m, e.g. from passing vessels.",
        "För färder i skyddade kustnära vatten, små vikar, mindre sjöar, floder och kanaler där vindar upp till 4 Beaufort och signifikanta våghöjder upp till 0,3 m kan förekomma, med tillfälliga vågor upp till 0,5 m, t.ex. från passerande fartyg.");

    /// <summary>All predefined CE design categories.</summary>
    public static IReadOnlyList<BoatCeDesignCategory> All { get; } = [A, B, C, D];

    private BoatCeDesignCategory(string value, string englishName, string localizedName,
        decimal maxSignificantWaveHeightM, int maxBeaufortWindForce,
        string englishDescription, string localizedDescription)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        MaxSignificantWaveHeightM = maxSignificantWaveHeightM;
        MaxBeaufortWindForce = maxBeaufortWindForce;
        EnglishDescription = englishDescription;
        LocalizedDescription = localizedDescription;
    }

    public static bool TryParse(string? input, out BoatCeDesignCategory? result)
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

    public static BoatCeDesignCategory Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid boat CE design category.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical letter code, e.g. <c>A</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical letter code, e.g. <c>A</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical letter code, e.g. <c>A</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical letter code, e.g. <c>A</c>.</summary>
    public override string ToString() => Value;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, BoatCeDesignCategory> d, BoatCeDesignCategory value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, BoatCeDesignCategory> BuildLookup()
    {
        var d = new Dictionary<string, BoatCeDesignCategory>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in All)
        {
            AddKey(d, c, c.Value);
            AddKey(d, c, $"CE {c.Value}");
            AddKey(d, c, $"CE-{c.Value}");
            AddKey(d, c, $"Kategori {c.Value}");
            AddKey(d, c, $"Category {c.Value}");
            AddKey(d, c, $"Class {c.Value}");
            AddKey(d, c, c.EnglishName);
            AddKey(d, c, c.LocalizedName);
        }

        AddKey(d, A, "Ocean-going");
        AddKey(d, A, "Havsgående");
        AddKey(d, B, "Offshore");
        AddKey(d, C, "Inshore/coastal");
        AddKey(d, C, "Kustnära");
        AddKey(d, D, "Sheltered");
        AddKey(d, D, "Insjö");

        return d;
    }

    public static bool operator ==(BoatCeDesignCategory? a, BoatCeDesignCategory? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(BoatCeDesignCategory? a, BoatCeDesignCategory? b) => !(a == b);

    public bool Equals(BoatCeDesignCategory? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is BoatCeDesignCategory other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(BoatCeDesignCategory? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(BoatCeDesignCategory a, BoatCeDesignCategory b) => a.CompareTo(b) < 0;
    public static bool operator >(BoatCeDesignCategory a, BoatCeDesignCategory b) => a.CompareTo(b) > 0;
    public static bool operator <=(BoatCeDesignCategory a, BoatCeDesignCategory b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BoatCeDesignCategory a, BoatCeDesignCategory b) => a.CompareTo(b) >= 0;
}
