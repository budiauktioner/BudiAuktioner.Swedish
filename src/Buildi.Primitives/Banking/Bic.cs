using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A BIC (<c>Business Identifier Code</c> / SWIFT code) identifies a financial institution in international payment messaging. It is often used together with IBAN for cross-border payments and incoming international transfers to Swedish bank accounts.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/cms/render/live/en/sites/isoorg/contents/data/standard/08/41/84108.html">ISO 9362</see> — BIC standard</description></item>
/// <item><description><see href="https://www.swift.com/standards/data-standards/bic-business-identifier-code">SWIFT - Business Identifier Code (BIC)</see></description></item>
/// </list>
/// </remarks>
public sealed class Bic : IEquatable<Bic>, IComparable<Bic>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("BIC", "BIC", "🔄", ["https://www.iso.org/cms/render/live/en/sites/isoorg/contents/data/standard/08/41/84108.html", "https://www.swift.com/standards/data-standards/bic-business-identifier-code"]);

    private const int MaxInputLength = 20;

    private static readonly Regex Pattern = new(
        @"^(?<institution>[A-Z]{4})(?<country>[A-Z]{2})(?<location>[A-Z0-9]{2})(?<branch>[A-Z0-9]{3})?$",
        RegexOptions.Compiled);

    public string Code { get; }
    public string InstitutionCode { get; }
    public string CountryCode { get; }
    public string LocationCode { get; }
    public string? BranchCode { get; }
    public bool IsPrimaryOffice => BranchCode == null;

    private Bic(string code, string institutionCode, string countryCode, string locationCode, string? branchCode)
    {
        Code = code;
        InstitutionCode = institutionCode;
        CountryCode = countryCode;
        LocationCode = locationCode;
        BranchCode = branchCode;
    }

    public static bool TryParse(string? input, out Bic? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = NormalizeInternal(InputSanitization.SanitizeInput(input!));
        if (normalized.Length > MaxInputLength) return false;
        var match = Pattern.Match(normalized);
        if (!match.Success) return false;
        if (!Buildi.Primitives.Geography.Country.TryParse(match.Groups["country"].Value, out _)) return false;

        result = new Bic(
            normalized,
            match.Groups["institution"].Value,
            match.Groups["country"].Value,
            match.Groups["location"].Value,
            match.Groups["branch"].Success ? match.Groups["branch"].Value : null);
        return true;
    }

    public static Bic Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid BIC.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the BIC as uppercase 8 or 11 characters, for example <c>NDEASESS</c> or <c>NDEASESSXXX</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.Code : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized BIC as uppercase 8 or 11 characters, for example <c>NDEASESS</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized BIC as uppercase 8 or 11 characters, for example <c>NDEASESS</c>.
    /// </summary>
    public string ToNormalizedString() => Code;
    /// <summary>
    /// Returns the BIC as uppercase 8 or 11 characters, for example <c>NDEASESS</c>.
    /// </summary>
    public override string ToString() => Code;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a well-formed BIC.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var normalized = NormalizeInternal(InputSanitization.SanitizeInput(input!));

        if (normalized.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        var match = Pattern.Match(normalized);
        if (!match.Success)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "BIC has an invalid format (must be 8 or 11 alphanumeric characters).",
                "BIC har ett ogiltigt format (måste vara 8 eller 11 alfanumeriska tecken).");

        if (!Buildi.Primitives.Geography.Country.TryParse(match.Groups["country"].Value, out _))
            return ValidationResult.Invalid(input, ValidationErrorReason.UnknownCountryCode,
                "Unknown BIC country code.", "Okänd BIC-landskod.");

        return ValidationResult.Valid(input);
    }

    private static readonly Regex ScanPattern = new(
        @"\b[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}(?:[A-Z0-9]{3})?\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential BIC/SWIFT codes.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real BIC in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Bic>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Bic>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            var original = match.Value;
            if (!IsAllUpperAscii(original)) continue;
            if (!TryParse(original, out var bic)) continue;
            results.Add(new TextCandidate<Bic>(
                match.Index,
                match.Length,
                original,
                nameof(Bic),
                TextCandidateCategory.Financial,
                bic!.ToNormalizedString(),
                bic.ToString(),
                bic.ToMaskedString(),
                TextMatchConfidence.Medium,
                bic));
        }
        return results;
    }

    private static bool IsAllUpperAscii(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c is >= 'a' and <= 'z') return false;
        }
        return true;
    }

    private static string NormalizeInternal(string input)
    {
        var buffer = new char[input.Length];
        var length = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsWhiteSpace(c) || c == '-') continue;

            buffer[length++] = char.ToUpperInvariant(c);
        }

        return new string(buffer, 0, length);
    }

    public bool Equals(Bic? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is Bic other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Bic? a, Bic? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Bic? a, Bic? b) => !(a == b);
    public int CompareTo(Bic? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Bic left, Bic right) => left.CompareTo(right) < 0;
    public static bool operator >(Bic left, Bic right) => left.CompareTo(right) > 0;
    public static bool operator <=(Bic left, Bic right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Bic left, Bic right) => left.CompareTo(right) >= 0;
}
