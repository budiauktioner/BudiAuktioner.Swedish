using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A European vehicle emission standard (<c>Euroklass</c>) with the related Swedish
/// environmental class label (<c>miljöklass</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://commission.europa.eu/energy-climate-change-environment/standards-tools-and-labels/labels-certificates-and-standards/vehicles-and-transport-labelling-standards/european-emission-standards_en">European Commission — European emission standards</see></description></item>
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen — fordonsregler</see> (Swedish environmental class labels)</description></item>
/// </list>
/// <para>The generic Swedish label <c>Miljöklass 2005</c> applies to several Euro levels; when parsing that label without a Euro stage, it is mapped to <see cref="Euro6"/> as a representative modern default.</para>
/// </remarks>
public sealed class EuroEmissionClass : IEquatable<EuroEmissionClass>, IComparable<EuroEmissionClass>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Euro Emission Class", "Euroklass", "🌿", ["https://commission.europa.eu/energy-climate-change-environment/standards-tools-and-labels/labels-certificates-and-standards/vehicles-and-transport-labelling-standards/european-emission-standards_en", "https://www.transportstyrelsen.se/"]);

    private static readonly Lazy<Dictionary<string, EuroEmissionClass>> Lookup = new(BuildLookup);

    private static readonly Regex EuroTokenPattern = new(
        @"^\s*(?:euro|eu)\s*([1-7])\s*([a-z][a-z-]*)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex ScanPattern = new(
        @"\bEuro\s*[1-7](?!\d)(?:\s*[a-z][a-z-]*)?\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, int> RomanNumerals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["I"] = 1, ["II"] = 2, ["III"] = 3, ["IV"] = 4, ["V"] = 5, ["VI"] = 6, ["VII"] = 7
    };

    private static readonly Regex RomanPattern = new(
        @"^\s*(?:euro\s*)?([IV]{1,4})\s*([a-z][a-z-]*)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Display Euro stage, e.g. <c>Euro 6d</c> or <c>El</c>.</summary>
    public string EuroClass { get; }

    /// <summary>Main numeric Euro level (1–7), or <c>0</c> for <see cref="El"/>.</summary>
    public int Level { get; }

    /// <summary>Optional letter or compound suffix after the level, e.g. <c>d</c>, <c>d-temp</c>, <c>b</c>, or <see langword="null"/>.</summary>
    public string? SubLevel { get; }

    /// <summary>Swedish miljöklass label, e.g. <c>Miljöklass 2005</c> or <c>Miljöklass El</c>.</summary>
    public string SwedishMiljoklass { get; }

    /// <summary>Typical first introduction year for the standard in the EU light-duty framework, or <c>0</c> for <see cref="El"/>.</summary>
    public int IntroductionYear { get; }

    /// <summary>Canonical string form; same as <see cref="EuroClass"/>.</summary>
    public string Value => EuroClass;

    public static readonly EuroEmissionClass Euro1 = new("Euro 1", 1, null, "Miljöklass 1", 1992);
    public static readonly EuroEmissionClass Euro2 = new("Euro 2", 2, null, "Miljöklass 2", 1996);
    public static readonly EuroEmissionClass Euro3 = new("Euro 3", 3, null, "Miljöklass 2005", 2000);
    public static readonly EuroEmissionClass Euro4 = new("Euro 4", 4, null, "Miljöklass 2005", 2005);
    public static readonly EuroEmissionClass Euro5 = new("Euro 5", 5, null, "Miljöklass 2005", 2009);
    public static readonly EuroEmissionClass Euro5a = new("Euro 5a", 5, "a", "Miljöklass 2005", 2009);
    public static readonly EuroEmissionClass Euro5b = new("Euro 5b", 5, "b", "Miljöklass 2005", 2011);
    public static readonly EuroEmissionClass Euro6 = new("Euro 6", 6, null, "Miljöklass 2005", 2014);
    public static readonly EuroEmissionClass Euro6b = new("Euro 6b", 6, "b", "Miljöklass 2005", 2014);
    public static readonly EuroEmissionClass Euro6c = new("Euro 6c", 6, "c", "Miljöklass 2005", 2017);
    public static readonly EuroEmissionClass Euro6dTemp = new("Euro 6d-temp", 6, "d-temp", "Miljöklass 2005", 2017);
    public static readonly EuroEmissionClass Euro6d = new("Euro 6d", 6, "d", "Miljöklass 2005", 2020);
    public static readonly EuroEmissionClass Euro6e = new("Euro 6e", 6, "e", "Miljöklass 2005", 2023);
    public static readonly EuroEmissionClass Euro7 = new("Euro 7", 7, null, "Miljöklass 2005", 2025);
    public static readonly EuroEmissionClass El = new("El", 0, null, "Miljöklass El", 0);

    /// <summary>All predefined emission classes, ordered from <see cref="Euro1"/> through <see cref="Euro7"/>, then <see cref="El"/>.</summary>
    public static IReadOnlyList<EuroEmissionClass> All { get; } =
    [
        Euro1, Euro2, Euro3, Euro4, Euro5, Euro5a, Euro5b,
        Euro6, Euro6b, Euro6c, Euro6dTemp, Euro6d, Euro6e,
        Euro7, El
    ];

    private EuroEmissionClass(string euroClass, int level, string? subLevel, string swedishMiljoklass, int introductionYear)
    {
        EuroClass = euroClass;
        Level = level;
        SubLevel = subLevel;
        SwedishMiljoklass = swedishMiljoklass;
        IntroductionYear = introductionYear;
    }

    /// <summary>
    /// Attempts to parse a European emission class or Swedish miljöklass synonym (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out EuroEmissionClass? result)
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

        var m = EuroTokenPattern.Match(trimmed);
        if (m.Success)
        {
            var level = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var subRaw = m.Groups[2].Success ? m.Groups[2].Value.ToLowerInvariant() : null;
            if (TryMapLevelSub(level, subRaw, out var mapped))
            {
                result = mapped;
                return true;
            }
        }

        var rm = RomanPattern.Match(trimmed);
        if (rm.Success)
        {
            var romanStr = rm.Groups[1].Value.ToUpperInvariant();
            if (RomanNumerals.TryGetValue(romanStr, out var level))
            {
                var subRaw = rm.Groups[2].Success ? rm.Groups[2].Value.ToLowerInvariant() : null;
                if (TryMapLevelSub(level, subRaw, out var mapped))
                {
                    result = mapped;
                    return true;
                }
            }
        }

        return false;
    }

    public static EuroEmissionClass Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid European emission class.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical Euro class label, e.g. <c>Euro 6d</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.EuroClass : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical Euro class label, e.g. <c>Euro 6d</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.EuroClass;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its normalized Euro class label.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical Euro class label, e.g. <c>Euro 6d</c>.</summary>
    public string ToNormalizedString() => EuroClass;

    /// <summary>Returns the canonical Euro class label, e.g. <c>Euro 6d</c>.</summary>
    public override string ToString() => EuroClass;

    /// <summary>
    /// Scans unstructured text for substrings that look like Euro stage labels (e.g. <c>Euro 6</c>, <c>Euro 6d-temp</c>).
    /// Results are heuristic-based candidates and may include false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EuroEmissionClass>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EuroEmissionClass>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ec)) continue;
            var emission = ec!;
            results.Add(new TextCandidate<EuroEmissionClass>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(EuroEmissionClass),
                TextCandidateCategory.Vehicle,
                emission.ToNormalizedString(),
                emission.ToString(),
                emission.EuroClass,
                TextMatchConfidence.Medium,
                emission));
        }

        return results;
    }

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("ö", "o");
        folded = Regex.Replace(folded, @"\s+", " ", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, EuroEmissionClass> d, EuroEmissionClass value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d[k] = value;
    }

    private static Dictionary<string, EuroEmissionClass> BuildLookup()
    {
        var d = new Dictionary<string, EuroEmissionClass>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in All)
        {
            AddKey(d, e, e.EuroClass);
            var compact = Regex.Replace(e.EuroClass.ToLowerInvariant(), @"\s+", "", RegexOptions.CultureInvariant);
            AddKey(d, e, compact);
        }

        AddKey(d, Euro1, "Miljöklass 1");
        AddKey(d, Euro2, "Miljöklass 2");
        AddKey(d, Euro6, "Miljöklass 2005");
        AddKey(d, Euro6, "MK2005");
        AddKey(d, El, "Miljöklass El");
        AddKey(d, El, "Elbil");

        AddKey(d, Euro5, "2005PM");
        AddKey(d, Euro5, "Miljöklass 2005PM");
        AddKey(d, Euro5b, "2008");
        AddKey(d, Euro5b, "2008PM");
        AddKey(d, Euro5b, "Miljöklass 2008");
        AddKey(d, Euro5, "EEV");

        return d;
    }

    private static bool TryMapLevelSub(int level, string? subLower, out EuroEmissionClass? result)
    {
        result = null;
        var sub = subLower ?? "";

        EuroEmissionClass? assigned = null;
        var ok = false;
        switch (level)
        {
            case 1 when sub.Length == 0:
                assigned = Euro1;
                ok = true;
                break;
            case 2 when sub.Length == 0:
                assigned = Euro2;
                ok = true;
                break;
            case 3 when sub.Length == 0:
                assigned = Euro3;
                ok = true;
                break;
            case 4 when sub.Length == 0:
                assigned = Euro4;
                ok = true;
                break;
            case 5 when sub.Length == 0:
                assigned = Euro5;
                ok = true;
                break;
            case 5 when sub == "a":
                assigned = Euro5a;
                ok = true;
                break;
            case 5 when sub == "b":
                assigned = Euro5b;
                ok = true;
                break;
            case 6 when sub.Length == 0:
                assigned = Euro6;
                ok = true;
                break;
            case 6 when sub == "b":
                assigned = Euro6b;
                ok = true;
                break;
            case 6 when sub == "c":
                assigned = Euro6c;
                ok = true;
                break;
            case 6 when sub == "d":
                assigned = Euro6d;
                ok = true;
                break;
            case 6 when sub == "d-temp":
                assigned = Euro6dTemp;
                ok = true;
                break;
            case 6 when sub == "e":
                assigned = Euro6e;
                ok = true;
                break;
            case 7 when sub.Length == 0:
                assigned = Euro7;
                ok = true;
                break;
        }

        if (ok)
            result = assigned;
        return ok;
    }

    public static bool operator ==(EuroEmissionClass? a, EuroEmissionClass? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EuroEmissionClass? a, EuroEmissionClass? b) => !(a == b);

    public bool Equals(EuroEmissionClass? other) => other is not null && string.Equals(EuroClass, other.EuroClass, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is EuroEmissionClass other && Equals(other);
    public override int GetHashCode() => EuroClass.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(EuroEmissionClass? other) => other is null ? 1 : Level.CompareTo(other.Level);
    public static bool operator <(EuroEmissionClass a, EuroEmissionClass b) => a.Level < b.Level;
    public static bool operator >(EuroEmissionClass a, EuroEmissionClass b) => a.Level > b.Level;
    public static bool operator <=(EuroEmissionClass a, EuroEmissionClass b) => a.Level <= b.Level;
    public static bool operator >=(EuroEmissionClass a, EuroEmissionClass b) => a.Level >= b.Level;
}
