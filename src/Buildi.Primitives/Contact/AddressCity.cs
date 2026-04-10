using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Contact;

/// <summary>
/// The postal city (<c>postort</c>) is the locality associated with a Swedish postal code, as defined by PostNord. Normalization applies Swedish-aware title casing, correctly handling characters like Å, Ä, Ö and hyphenated place names.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.se/">PostNord</see> — Swedish postal service</description></item>
/// </list>
/// </remarks>
public sealed class AddressCity : IEquatable<AddressCity>, IComparable<AddressCity>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("City", "Postort", "🏙️", ["https://www.postnord.se/"]);

    private const int MaxInputLength = 100;

    private static CultureInfo DefaultCulture => PrimitivesDefaults.Culture;
    private static readonly Regex NamePattern = new(
        @"^(?:[\p{L}\p{M}]{2,}|['\p{L}\p{M}]{2,}|[\p{Lo}])(?:['\p{Zs}-][\p{L}\p{M}]+)*$",
        RegexOptions.Compiled);

    public string Value { get; }

    private AddressCity(string value) => Value = value;

    public static bool TryParse(string? input, out AddressCity? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length > MaxInputLength) return false;

        var capitalized = CapitalizeEachWord(cleaned);

        if (capitalized.Any(char.IsControl)) return false;
        if (capitalized.Any(c => char.GetUnicodeCategory(c) == UnicodeCategory.OtherNotAssigned)) return false;
        if (!NamePattern.IsMatch(capitalized)) return false;

        result = new AddressCity(capitalized);
        return true;
    }

    public static AddressCity Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid city name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the city in normalized Swedish-aware capitalization, for example <c>Åkersberga</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized city name, for example <c>Åkersberga</c>.
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
    /// Returns the normalized city name, for example <c>Åkersberga</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the city in normalized Swedish-aware capitalization, for example <c>Åkersberga</c>.
    /// </summary>
    public override string ToString() => Value;

    private static string CapitalizeEachWord(string input)
    {
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
        var lowered = DefaultCulture.TextInfo.ToLower(word);
        var chars = lowered.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetter(chars[i])) continue;
            chars[i] = DefaultCulture.TextInfo.ToUpper(chars[i].ToString())[0];
            return new string(chars);
        }
        return lowered;
    }

    public bool Equals(AddressCity? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is AddressCity other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(AddressCity? a, AddressCity? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AddressCity? a, AddressCity? b) => !(a == b);
    public int CompareTo(AddressCity? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(AddressCity left, AddressCity right) => left.CompareTo(right) < 0;
    public static bool operator >(AddressCity left, AddressCity right) => left.CompareTo(right) > 0;
    public static bool operator <=(AddressCity left, AddressCity right) => left.CompareTo(right) <= 0;
    public static bool operator >=(AddressCity left, AddressCity right) => left.CompareTo(right) >= 0;
}
