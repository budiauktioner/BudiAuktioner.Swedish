using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A street address (<c>gatuadress</c>) is the house-level part of a Swedish postal address, including street name and house number. The parser also extracts care-of (c/o), apartment number (lgh/apt), and post box (Box) when present. Address data in Sweden is maintained by Lantmäteriet. Normalization handles whitespace, casing, and uppercase house number suffixes (for example <c>12a</c> becomes <c>12A</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.lantmateriet.se/">Lantmäteriet</see> — Swedish mapping, cadastral and land registration authority</description></item>
/// </list>
/// </remarks>
public sealed class AddressStreet : IEquatable<AddressStreet>, IComparable<AddressStreet>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Street Address", "Gatuadress", "🛣️", ["https://www.lantmateriet.se/"]);

    private const int MaxInputLength = 200;

    private static CultureInfo DefaultCulture => PrimitivesDefaults.Culture;
    private static readonly Regex AddressPattern = new(@"^[\p{L}0-9\s,.:'/\-]+$", RegexOptions.Compiled);

    private static readonly Regex CareOfPattern = new(
        @"^c[/.]o\.?\s+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ApartmentPattern = new(
        @"[,\s]+(?:lgh\.?|lägenhet|apt\.?)\s*(\d+\w*)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BoxPattern = new(
        @"^(?:p\.?o\.?\s*)?box\s+(\d{1,7})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex StreetNumberPattern = new(
        @"^(.*?)\s+(\d+[A-ZÅÄÖ]*)$",
        RegexOptions.Compiled);

    public string Street { get; }
    /// <summary>The street name without the house number, for example <c>Storgatan</c>. <see langword="null"/> for post boxes.</summary>
    public string? StreetName { get; }
    /// <summary>The house number including any letter suffix, for example <c>12A</c>. <see langword="null"/> for post boxes.</summary>
    public string? StreetNumber { get; }
    public string? ApartmentNumber { get; }
    public string? CareOf { get; }
    public string? PostBox { get; }
    public bool IsPostBox => PostBox != null;

    private AddressStreet(string street, string? streetName, string? streetNumber, string? apartmentNumber = null, string? careOf = null, string? postBox = null)
    {
        Street = street;
        StreetName = streetName;
        StreetNumber = streetNumber;
        ApartmentNumber = apartmentNumber;
        CareOf = careOf;
        PostBox = postBox;
    }

    public static bool TryParse(string? input, out AddressStreet? result)
        => TryParse(input, null, null, out result);

    /// <summary>
    /// Parse and normalize a street address, optionally stripping a trailing city and/or zip code.
    /// Extracts care-of, apartment number, and post box when present.
    /// </summary>
    public static bool TryParse(string? input, string? city, string? zipCode, out AddressStreet? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var text = InputSanitization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxInputLength) return false;

        // 1. Extract c/o prefix (split on first comma)
        string? careOf = null;
        var coMatch = CareOfPattern.Match(text);
        if (coMatch.Success)
        {
            var afterCo = text[coMatch.Length..];
            var commaIdx = afterCo.IndexOf(',');
            if (commaIdx >= 0)
            {
                careOf = afterCo[..commaIdx].Trim();
                text = afterCo[(commaIdx + 1)..].Trim();
            }
            else
            {
                careOf = afterCo.Trim();
                text = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(careOf))
                careOf = null;
        }

        // 2. Check for box address
        if (!string.IsNullOrWhiteSpace(text))
        {
            var boxText = InputSanitization.CollapseWhitespace(text.Replace(",", " "));
            var boxMatch = BoxPattern.Match(boxText);
            if (boxMatch.Success)
            {
                var postBox = boxMatch.Groups[1].Value;
                result = new AddressStreet("Box " + postBox, null, null, null, careOf, postBox);
                return true;
            }
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            if (careOf != null)
            {
                result = new AddressStreet("c/o " + careOf, null, null, null, careOf, null);
                return true;
            }
            return false;
        }

        // 3. Extract apartment number suffix
        string? apartment = null;
        var aptMatch = ApartmentPattern.Match(text);
        if (aptMatch.Success)
        {
            apartment = aptMatch.Groups[1].Value;
            text = text[..aptMatch.Index].Trim();
        }

        // 4. Normal street normalization
        var normalized = NormalizeStreet(text, city, zipCode);
        if (!ValidateStreet(normalized)) return false;

        string? streetName = null;
        string? streetNumber = null;
        var numberMatch = StreetNumberPattern.Match(normalized);
        if (numberMatch.Success)
        {
            streetName = numberMatch.Groups[1].Value;
            streetNumber = numberMatch.Groups[2].Value;
        }

        result = new AddressStreet(normalized, streetName, streetNumber, apartment, careOf, null);
        return true;
    }

    public static AddressStreet Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid street address.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the normalized street line only, for example <c>Storgatan 12A</c> or <c>Box 123</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Street : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized street line only, for example <c>Storgatan 12A</c> or <c>Box 123</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Street;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized street line only, for example <c>Storgatan 12A</c> or <c>Box 123</c>.
    /// </summary>
    public string ToNormalizedString() => Street;
    /// <summary>
    /// Returns the normalized street line only, for example <c>Storgatan 12A</c> or <c>Box 123</c>.
    /// </summary>
    public override string ToString() => Street;

    private static string NormalizeStreet(string? input, string? city, string? zipCode)
    {
        var cleaned = InputSanitization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(cleaned)) return string.Empty;

        cleaned = InputSanitization.CollapseWhitespace(cleaned.Replace(',', ' '));

        cleaned = Regex.Replace(cleaned, @"(\p{L})(\d+\p{L}*)$", "$1 $2");
        cleaned = InputSanitization.CollapseWhitespace(cleaned);

        var stripped = RemoveTrailingCityAndZip(cleaned, city, zipCode);

        if (ShouldSentenceCase(stripped))
            stripped = ToSentenceCase(stripped);

        stripped = UppercaseHouseNumberSuffixes(stripped);

        return stripped;
    }

    private static bool ValidateStreet(string? address)
    {
        if (address == null || string.IsNullOrWhiteSpace(address)) return false;
        if (address.Any(char.IsControl)) return false;
        if (address != address.Trim()) return false;
        if (address.Contains("  ")) return false;
        if (address.Length < 5 || address.Length > 50) return false;
        if (address.Any(c => char.GetUnicodeCategory(c) == UnicodeCategory.OtherNotAssigned)) return false;
        return AddressPattern.IsMatch(address);
    }

    private static bool ShouldSentenceCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var letters = input.Where(char.IsLetter).ToArray();
        if (letters.Length == 0) return false;

        var allLettersUpper = letters.All(c => !char.IsLower(c));

        var firstLetterIndex = -1;
        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsLetter(input[i]))
            {
                firstLetterIndex = i;
                break;
            }
        }

        if (firstLetterIndex == -1) return allLettersUpper;

        var startsWithUpper = char.IsUpper(input[firstLetterIndex]);
        return allLettersUpper || !startsWithUpper;
    }

    private static string ToSentenceCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var lowered = DefaultCulture.TextInfo.ToLower(input);
        var chars = lowered.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetter(chars[i])) continue;
            chars[i] = DefaultCulture.TextInfo.ToUpper(chars[i].ToString())[0];
            break;
        }

        return UppercaseHouseNumberSuffixes(new string(chars));
    }

    private static string UppercaseHouseNumberSuffixes(string input) =>
        Regex.Replace(
            input,
            @"(\d+)(\p{L}+)(?=[^\p{L}\d]|$)",
            m => m.Groups[1].Value + m.Groups[2].Value.ToUpper(DefaultCulture));

    private static string RemoveTrailingCityAndZip(string street, string? city, string? zipCode)
    {
        var cityNorm = InputSanitization.CollapseWhitespace(city);
        var zipNorm = InputSanitization.CollapseWhitespace(zipCode);

        var cityU = string.IsNullOrWhiteSpace(cityNorm) ? string.Empty : cityNorm.ToUpperInvariant();
        var zipU = string.IsNullOrWhiteSpace(zipNorm) ? string.Empty : zipNorm.ToUpperInvariant();
        var zipNoSpaceU = string.IsNullOrWhiteSpace(zipNorm) ? string.Empty : zipNorm.Replace(" ", string.Empty).ToUpperInvariant();

        var streetOrigNorm = InputSanitization.CollapseWhitespace(street.Replace(',', ' '));
        var streetUNorm = streetOrigNorm.ToUpperInvariant();

        static bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

        string RemoveTail(string s, int tailLength) =>
            tailLength <= 0 || tailLength > s.Length ? s : InputSanitization.CollapseWhitespace(s[..^tailLength]);

        if (HasValue(cityU) && HasValue(zipU) && streetUNorm.EndsWith(" " + zipU + " " + cityU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, (zipU + " " + cityU).Length);

        if (HasValue(cityU) && HasValue(zipNoSpaceU) && streetUNorm.EndsWith(" " + zipNoSpaceU + " " + cityU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, (zipNoSpaceU + " " + cityU).Length);

        if (HasValue(cityU) && HasValue(zipU) && streetUNorm.EndsWith(" " + cityU + " " + zipU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, (cityU + " " + zipU).Length);

        if (HasValue(cityU) && HasValue(zipNoSpaceU) && streetUNorm.EndsWith(" " + cityU + " " + zipNoSpaceU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, (cityU + " " + zipNoSpaceU).Length);

        if (HasValue(cityU) && streetUNorm.EndsWith(" " + cityU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, cityU.Length);

        if (HasValue(zipU) && streetUNorm.EndsWith(" " + zipU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, zipU.Length);

        if (HasValue(zipNoSpaceU) && streetUNorm.EndsWith(" " + zipNoSpaceU, StringComparison.Ordinal))
            return RemoveTail(streetOrigNorm, zipNoSpaceU.Length);

        return streetOrigNorm;
    }

    public bool Equals(AddressStreet? other) => other is not null && Street == other.Street;
    public override bool Equals(object? obj) => obj is AddressStreet other && Equals(other);
    public override int GetHashCode() => Street.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(AddressStreet? a, AddressStreet? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AddressStreet? a, AddressStreet? b) => !(a == b);
    public int CompareTo(AddressStreet? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(AddressStreet left, AddressStreet right) => left.CompareTo(right) < 0;
    public static bool operator >(AddressStreet left, AddressStreet right) => left.CompareTo(right) > 0;
    public static bool operator <=(AddressStreet left, AddressStreet right) => left.CompareTo(right) <= 0;
    public static bool operator >=(AddressStreet left, AddressStreet right) => left.CompareTo(right) >= 0;
}
