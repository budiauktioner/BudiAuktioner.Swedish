using System.Text;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Organization;

/// <summary>
/// A Legal Entity Identifier (LEI) is a 20-character identifier for legal entities used in financial and regulatory contexts. This type validates the ISO 17442 structure and checksum.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gleif.org/en/about-lei/iso-17442-the-lei-code-structure/">GLEIF — Legal Entity Identifier (LEI)</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Legal_Entity_Identifier">Wikipedia — Legal Entity Identifier</see></description></item>
/// </list>
/// </remarks>
public sealed class LeiCode : IEquatable<LeiCode>, IComparable<LeiCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("LEI Code", "LEI-kod", "🏛️", ["https://www.gleif.org/en/about-lei/iso-17442-the-lei-code-structure/", "https://en.wikipedia.org/wiki/Legal_Entity_Identifier"]);

    private const int MaxInputLength = 30;

    public string Value { get; }

    /// <summary>
    /// The LOU (Local Operating Unit) prefix (first 4 characters).
    /// Identifies the issuer of the LEI.
    /// </summary>
    public string LouPrefix => Value.Substring(0, 4);

    /// <summary>
    /// Reserved characters (positions 5-6, typically "00").
    /// </summary>
    public string Reserved => Value.Substring(4, 2);

    /// <summary>
    /// Entity-specific identifier (positions 7-18, randomized).
    /// </summary>
    public string EntitySpecific => Value.Substring(6, 12);

    /// <summary>
    /// Check digits (positions 19-20).
    /// Used for ISO 17442 Mod-97 validation.
    /// </summary>
    public string CheckDigits => Value.Substring(18, 2);

    private LeiCode(string value)
    {
        Value = value;
    }

    public static bool TryParse(string? input, out LeiCode? lei)
    {
        lei = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        var source = InputSanitization.SanitizeInput(input!);
        var cleaned = new StringBuilder(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            var c = source[i];
            if (char.IsLetterOrDigit(c))
            {
                cleaned.Append(char.ToUpperInvariant(c));
            }
        }
        var value = cleaned.ToString();
        if (value.Length > MaxInputLength) return false;
        if (value.Length != 20)
        {
            return false;
        }

        // Validate Mod 97
        if (!OrganizationValidationUtils.IsValidIso7064Mod97(value))
        {
            return false;
        }

        lei = new LeiCode(value);
        return true;
    }

    public static LeiCode Parse(string input)
    {
        if (!TryParse(input, out var lei))
        {
            throw new ArgumentException("Invalid LEI code (length or checksum invalid).", nameof(input));
        }
        return lei!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the LEI as 20 uppercase characters, for example <c>5493001KJTIIGC8Y1R12</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized LEI as 20 uppercase characters, for example <c>5493001KJTIIGC8Y1R12</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized LEI as 20 uppercase characters, for example <c>5493001KJTIIGC8Y1R12</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the LEI as 20 uppercase characters, for example <c>5493001KJTIIGC8Y1R12</c>.
    /// </summary>
    public override string ToString() => Value;

    private static readonly Regex ScanPattern = new(
        @"\b[A-Z0-9]{18}\d{2}\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential LEI codes.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real LEI in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<LeiCode>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<LeiCode>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var lei)) continue;
            results.Add(new TextCandidate<LeiCode>(
                match.Index,
                match.Length,
                match.Value,
                nameof(LeiCode),
                TextCandidateCategory.OrganizationIdentifier,
                lei!.ToNormalizedString(),
                lei.ToString(),
                lei.ToMaskedString(),
                TextMatchConfidence.High,
                lei));
        }
        return results;
    }

    public bool Equals(LeiCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is LeiCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(LeiCode? a, LeiCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(LeiCode? a, LeiCode? b) => !(a == b);
    public int CompareTo(LeiCode? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(LeiCode left, LeiCode right) => left.CompareTo(right) < 0;
    public static bool operator >(LeiCode left, LeiCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(LeiCode left, LeiCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(LeiCode left, LeiCode right) => left.CompareTo(right) >= 0;
}
