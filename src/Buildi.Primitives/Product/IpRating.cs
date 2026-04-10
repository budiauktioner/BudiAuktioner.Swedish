using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// Ingress Protection rating (<c>kapslingsklass</c> / <c>IP-klass</c>) per IEC 60529,
/// e.g. <c>IP65</c>, <c>IPX4</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/IP_code">Wikipedia — IP code</see></description></item>
/// <item><description><see href="https://webstore.iec.ch/en/publication/2452">IEC 60529 — Degrees of protection</see></description></item>
/// </list>
/// </remarks>
public sealed class IpRating : IEquatable<IpRating>, IComparable<IpRating>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("IP Rating", "IP-klass", "🛡️",
        ["https://en.wikipedia.org/wiki/IP_code", "https://webstore.iec.ch/en/publication/2452"]);

    private static readonly Regex ParsePattern = new(
        @"^\s*[Ii][Pp]\s*[-/]?\s*([0-6Xx])\s*([0-9Xx])\s*$",
        RegexOptions.Compiled);

    /// <summary>Solids (first digit): <c>'0'</c>–<c>'6'</c> or <c>'X'</c>.</summary>
    public char SolidsProtection { get; }

    /// <summary>Liquids (second digit): <c>'0'</c>–<c>'9'</c> or <c>'X'</c>.</summary>
    public char LiquidsProtection { get; }

    /// <summary>Normalized code, e.g. <c>IP65</c>.</summary>
    public string Value { get; }

    /// <summary>English description of the solids protection level, e.g. <c>Dust tight</c>.</summary>
    public string SolidsDescription => GetSolidsDescription(SolidsProtection);

    /// <summary>English description of the liquids protection level, e.g. <c>Water jets</c>.</summary>
    public string LiquidsDescription => GetLiquidsDescription(LiquidsProtection);

    private IpRating(char solids, char liquids)
    {
        SolidsProtection = solids;
        LiquidsProtection = liquids;
        Value = $"IP{solids}{liquids}";
    }

    public static bool TryParse(string? input, out IpRating? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var match = ParsePattern.Match(trimmed);
        if (!match.Success) return false;

        var solids = char.ToUpperInvariant(match.Groups[1].Value[0]);
        var liquids = char.ToUpperInvariant(match.Groups[2].Value[0]);

        result = new IpRating(solids, liquids);
        return true;
    }

    public static IpRating Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid IP rating.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the canonical form, e.g. <c>IP65</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the canonical form, e.g. <c>IP65</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals its normalized form.</summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns <c>IP65</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns <c>IP65</c>.</summary>
    public override string ToString() => Value;

    private static string GetSolidsDescription(char c) => c switch
    {
        'X' => "Not tested",
        '0' => "No protection",
        '1' => "Objects >50 mm",
        '2' => "Objects >12.5 mm",
        '3' => "Objects >2.5 mm",
        '4' => "Objects >1 mm",
        '5' => "Dust protected",
        '6' => "Dust tight",
        _ => "Unknown"
    };

    private static string GetLiquidsDescription(char c) => c switch
    {
        'X' => "Not tested",
        '0' => "No protection",
        '1' => "Dripping water",
        '2' => "Dripping water (15° tilted)",
        '3' => "Spraying water",
        '4' => "Splashing water",
        '5' => "Water jets",
        '6' => "Powerful water jets",
        '7' => "Temporary immersion",
        '8' => "Continuous immersion",
        '9' => "High-pressure/steam cleaning",
        _ => "Unknown"
    };

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\bIP\s*[-/]?\s*([0-6Xx])([0-9Xx])\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like IP protection ratings (e.g. <c>IP65</c>).
    /// The <c>IP</c> prefix makes matches fairly distinctive but scanning remains heuristic-based.
    /// </summary>
    public static IReadOnlyList<TextCandidate<IpRating>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<IpRating>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ip)) continue;
            results.Add(new TextCandidate<IpRating>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(IpRating), TextCandidateCategory.Product,
                ip!.ToNormalizedString(), ip.ToString(),
                ip.ToMaskedString(),
                TextMatchConfidence.Medium,
                ip));
        }
        return results;
    }

    public static bool operator ==(IpRating? a, IpRating? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(IpRating? a, IpRating? b) => !(a == b);

    public int CompareTo(IpRating? other)
    {
        if (other is null) return 1;
        var c = SolidsProtection.CompareTo(other.SolidsProtection);
        return c != 0 ? c : LiquidsProtection.CompareTo(other.LiquidsProtection);
    }

    public static bool operator <(IpRating left, IpRating right) => left.CompareTo(right) < 0;
    public static bool operator >(IpRating left, IpRating right) => left.CompareTo(right) > 0;
    public static bool operator <=(IpRating left, IpRating right) => left.CompareTo(right) <= 0;
    public static bool operator >=(IpRating left, IpRating right) => left.CompareTo(right) >= 0;

    public bool Equals(IpRating? other) =>
        other is not null && SolidsProtection == other.SolidsProtection && LiquidsProtection == other.LiquidsProtection;
    public override bool Equals(object? obj) => obj is IpRating other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(SolidsProtection, LiquidsProtection);
}
