using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// An electrical phase configuration (<c>fas</c>) used for electrical equipment,
/// such as single-phase (<c>enfas</c>) or three-phase (<c>trefas</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.elsakerhetsverket.se/">Elsäkerhetsverket</see> — Swedish Electrical Safety Authority</description></item>
/// </list>
/// </remarks>
public sealed class ElectricalPhase : IEquatable<ElectricalPhase>, IComparable<ElectricalPhase>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Electrical Phase", "Fas", "⚡", ["https://www.elsakerhetsverket.se/"]);

    private static readonly Lazy<Dictionary<string, ElectricalPhase>> Lookup = new(BuildLookup);

    /// <summary>Canonical form, e.g. <c>1-phase</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Single-phase</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Enfas</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Number of phases: 1, 2, or 3.</summary>
    public int PhaseCount { get; }

    public static readonly ElectricalPhase SinglePhase = new("1-phase", "Single-phase", "Enfas", 1);
    public static readonly ElectricalPhase TwoPhase = new("2-phase", "Two-phase", "Tvåfas", 2);
    public static readonly ElectricalPhase ThreePhase = new("3-phase", "Three-phase", "Trefas", 3);

    /// <summary>All predefined electrical phases.</summary>
    public static IReadOnlyList<ElectricalPhase> All { get; } =
    [
        SinglePhase, TwoPhase, ThreePhase
    ];

    private ElectricalPhase(string value, string englishName, string localizedName, int phaseCount)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        PhaseCount = phaseCount;
    }

    /// <summary>
    /// Attempts to parse an electrical phase from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out ElectricalPhase? result)
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

    /// <summary>
    /// Parses an electrical phase. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static ElectricalPhase Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid electrical phase.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized electrical phase.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Enfas</c> (Swedish) or <c>Single-phase</c> (English).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical value, e.g. <c>1-phase</c>, <c>3-phase</c>.
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
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical value, e.g. <c>1-phase</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture, e.g. <c>Enfas</c> or <c>Single-phase</c>.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-~]+", "", RegexOptions.CultureInvariant).Trim();
        return folded;
    }

    private static void AddKey(Dictionary<string, ElectricalPhase> d, ElectricalPhase value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, ElectricalPhase> BuildLookup()
    {
        var d = new Dictionary<string, ElectricalPhase>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in All)
        {
            AddKey(d, p, p.Value);
            AddKey(d, p, p.EnglishName);
            AddKey(d, p, p.LocalizedName);
        }

        // Single-phase aliases
        AddKey(d, SinglePhase, "1-fas");
        AddKey(d, SinglePhase, "1fas");
        AddKey(d, SinglePhase, "1 fas");
        AddKey(d, SinglePhase, "1-phase");
        AddKey(d, SinglePhase, "1phase");
        AddKey(d, SinglePhase, "1 phase");
        AddKey(d, SinglePhase, "Single phase");
        AddKey(d, SinglePhase, "1P");
        AddKey(d, SinglePhase, "1F");
        AddKey(d, SinglePhase, "230V 1-fas");
        AddKey(d, SinglePhase, "L1");
        AddKey(d, SinglePhase, "1~");

        // Two-phase aliases
        AddKey(d, TwoPhase, "2-fas");
        AddKey(d, TwoPhase, "2fas");
        AddKey(d, TwoPhase, "2 fas");
        AddKey(d, TwoPhase, "2-phase");
        AddKey(d, TwoPhase, "2phase");
        AddKey(d, TwoPhase, "2 phase");
        AddKey(d, TwoPhase, "Two phase");
        AddKey(d, TwoPhase, "2P");
        AddKey(d, TwoPhase, "2F");
        AddKey(d, TwoPhase, "2~");

        // Three-phase aliases
        AddKey(d, ThreePhase, "3-fas");
        AddKey(d, ThreePhase, "3fas");
        AddKey(d, ThreePhase, "3 fas");
        AddKey(d, ThreePhase, "3-phase");
        AddKey(d, ThreePhase, "3phase");
        AddKey(d, ThreePhase, "3 phase");
        AddKey(d, ThreePhase, "Three phase");
        AddKey(d, ThreePhase, "3P");
        AddKey(d, ThreePhase, "3F");
        AddKey(d, ThreePhase, "400V 3-fas");
        AddKey(d, ThreePhase, "L1L2L3");
        AddKey(d, ThreePhase, "3~");

        return d;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\b(?:[123]-?fas(?:ig)?|enfas(?:ig)?|trefas(?:ig)?|tvåfas(?:ig)?|single-?phase|three-?phase|two-?phase|[123]~)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like electrical phase configurations
    /// (e.g. <c>3-fas</c>, <c>enfas</c>, <c>three-phase</c>).
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<ElectricalPhase>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<ElectricalPhase>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ep)) continue;
            results.Add(new TextCandidate<ElectricalPhase>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(ElectricalPhase), TextCandidateCategory.Product,
                ep!.ToNormalizedString(), ep.ToString(),
                ep.ToMaskedString(),
                TextMatchConfidence.Medium,
                ep));
        }
        return results;
    }

    public static bool operator ==(ElectricalPhase? a, ElectricalPhase? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(ElectricalPhase? a, ElectricalPhase? b) => !(a == b);

    public bool Equals(ElectricalPhase? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ElectricalPhase other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(ElectricalPhase? other) =>
        other is null ? 1 : PhaseCount.CompareTo(other.PhaseCount);

    public static bool operator <(ElectricalPhase left, ElectricalPhase right) => left.CompareTo(right) < 0;
    public static bool operator >(ElectricalPhase left, ElectricalPhase right) => left.CompareTo(right) > 0;
    public static bool operator <=(ElectricalPhase left, ElectricalPhase right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ElectricalPhase left, ElectricalPhase right) => left.CompareTo(right) >= 0;
}
