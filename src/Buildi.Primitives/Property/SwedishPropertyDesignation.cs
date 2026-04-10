using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Property;

/// <summary>
/// A Swedish property designation (<c>fastighetsbeteckning</c>) is the official identifier for a property in the Swedish real property register. It combines the municipality and tract or quarter name with a register number such as <c>75:2</c>, which together uniquely identify the property.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.lantmateriet.se/sv/kartor/vara-karttjanster/min-karta/hitta-fastighetsbeteckning-i-min-karta/">Lantmateriet - Find property designation</see></description></item>
/// <item><description><see href="https://www2.lantmateriet.se/sv/fastighet-och-mark/information-om-fastigheter/Fastighetsregistret/registrets-innehall/">Lantmateriet - Property register contents</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Fastighetsbeteckning">Wikipedia - Fastighetsbeteckning</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishPropertyDesignation : IEquatable<SwedishPropertyDesignation>, IComparable<SwedishPropertyDesignation>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Property Designation", "Fastighetsbeteckning", "🏠", ["https://www.lantmateriet.se/sv/kartor/vara-karttjanster/min-karta/hitta-fastighetsbeteckning-i-min-karta/", "https://www2.lantmateriet.se/sv/fastighet-och-mark/information-om-fastigheter/Fastighetsregistret/registrets-innehall/", "https://sv.wikipedia.org/wiki/Fastighetsbeteckning"]);

    private const int MaxInputLength = 200;

    private static readonly CultureInfo SwedishCulture = CultureInfo.GetCultureInfo("sv-SE");
    private static readonly Regex Pattern = new(
        @"^(?<name>[\p{L}\p{M}\d][\p{L}\p{M}\d\s'\-]*?)\s+(?<block>\d{1,6}):(?<unit>\d{1,6})$",
        RegexOptions.Compiled);

    public string Value { get; }
    public string DesignationName { get; }
    public string RegisterNumber { get; }
    public int BlockNumber { get; }
    public int UnitNumber { get; }

    private SwedishPropertyDesignation(
        string value,
        string designationName,
        string registerNumber,
        int blockNumber,
        int unitNumber)
    {
        Value = value;
        DesignationName = designationName;
        RegisterNumber = registerNumber;
        BlockNumber = blockNumber;
        UnitNumber = unitNumber;
    }

    public static bool TryParse(string? input, out SwedishPropertyDesignation? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = NormalizeInput(input);
        if (normalized.Length > MaxInputLength) return false;
        var match = Pattern.Match(normalized);
        if (!match.Success) return false;

        var designationName = NormalizeName(match.Groups["name"].Value);
        if (string.IsNullOrWhiteSpace(designationName)) return false;

        if (!int.TryParse(match.Groups["block"].Value, out var blockNumber) || blockNumber <= 0) return false;
        if (!int.TryParse(match.Groups["unit"].Value, out var unitNumber) || unitNumber <= 0) return false;

        var registerNumber = $"{blockNumber}:{unitNumber}";
        var value = $"{designationName} {registerNumber}";

        result = new SwedishPropertyDesignation(value, designationName, registerNumber, blockNumber, unitNumber);
        return true;
    }

    public static SwedishPropertyDesignation Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish property designation.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the property designation in normalized display form, for example <c>Stockholm Söder 75:2</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;

    /// <summary>
    /// Returns the normalized property designation, for example <c>Stockholm Söder 75:2</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
    /// <summary>
    /// Returns the normalized property designation, for example <c>Stockholm Söder 75:2</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the property designation in normalized display form, for example <c>Stockholm Söder 75:2</c>.
    /// </summary>
    public override string ToString() => Value;

    private static string NormalizeInput(string input)
    {
        var cleaned = InputSanitization.CollapseWhitespace(input);
        cleaned = Regex.Replace(cleaned, @"\s*:\s*", ":");
        return cleaned;
    }

    private static string NormalizeName(string input)
    {
        var words = InputSanitization.CollapseWhitespace(input).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < words.Length; i++)
            words[i] = CapitalizeHyphenatedWord(words[i]);

        return string.Join(" ", words);
    }

    private static string CapitalizeHyphenatedWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        var parts = word.Split('-');
        for (var i = 0; i < parts.Length; i++)
            parts[i] = CapitalizeWord(parts[i]);

        return string.Join("-", parts);
    }

    private static string CapitalizeWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        var lowered = SwedishCulture.TextInfo.ToLower(word);
        var chars = lowered.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetter(chars[i])) continue;
            chars[i] = SwedishCulture.TextInfo.ToUpper(chars[i].ToString())[0];
            return new string(chars);
        }

        return lowered;
    }

    private static readonly Regex ScanPattern = new(
        @"\b(?<name>\p{Lu}[\p{L}\p{M}'\-]*(?:\s\p{Lu}[\p{L}\p{M}'\-]*)?)\s+(?<block>\d{1,6}):(?<unit>\d{1,6})(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish property designations (e.g. <c>Stockholm Söder 75:2</c>).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real property designation in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishPropertyDesignation>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishPropertyDesignation>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var prop)) continue;
            results.Add(new TextCandidate<SwedishPropertyDesignation>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishPropertyDesignation),
                TextCandidateCategory.Property,
                prop!.ToNormalizedString(),
                prop.ToString(),
                prop.ToMaskedString(),
                TextMatchConfidence.Medium,
                prop));
        }
        return results;
    }

    public bool Equals(SwedishPropertyDesignation? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishPropertyDesignation other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishPropertyDesignation? a, SwedishPropertyDesignation? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishPropertyDesignation? a, SwedishPropertyDesignation? b) => !(a == b);
    public int CompareTo(SwedishPropertyDesignation? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishPropertyDesignation left, SwedishPropertyDesignation right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishPropertyDesignation left, SwedishPropertyDesignation right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishPropertyDesignation left, SwedishPropertyDesignation right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishPropertyDesignation left, SwedishPropertyDesignation right) => left.CompareTo(right) >= 0;
}
