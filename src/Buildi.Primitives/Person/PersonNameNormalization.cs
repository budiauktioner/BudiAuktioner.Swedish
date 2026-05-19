using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Person;

internal static class PersonNameNormalization
{
    internal static CultureInfo DefaultCulture => PrimitivesDefaults.Culture;

    // Single-letter name parts (e.g. "B N" returned by an identity provider) are
    // not officially valid full names in most jurisdictions, but they appear in
    // real-world data from Skatteverket, BankID and other identity providers.
    // Normalization must preserve them rather than discard the name.
    internal static readonly Regex SinglePartPattern = new(
        @"^(?:[\p{L}\p{M}]+|['\p{L}\p{M}]{2,}|[\p{Lo}])(?:['\p{Zs}-][\p{L}\p{M}]+)*$",
        RegexOptions.Compiled);

    internal static readonly Regex FullNamePattern = new(
        @"^[\p{L}\p{M}][\p{L}\p{M}']*(?:\s+[\p{L}\p{M}][\p{L}\p{M}']*)+$",
        RegexOptions.Compiled);

    private static readonly Regex HyphenWhitespaceRegex = new(@"\s*-\s*", RegexOptions.Compiled);

    private static readonly string[] Honorifics =
    [
        "fröken", "froken", "froeken",
        "prof", "miss", "mrs", "mr", "ms", "dr",
        "herr", "fru",
    ];

    internal static string CollapseWhitespace(string? input)
    {
        var result = InputSanitization.CollapseWhitespace(input);
        result = HyphenWhitespaceRegex.Replace(result, "-");
        var parts = result.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('.'))
            .Where(p => p.Length > 0);
        result = string.Join(" ", parts);
        return StripLeadingHonorific(result);
    }

    private static string StripLeadingHonorific(string input)
    {
        foreach (var h in Honorifics)
        {
            if (input.Length <= h.Length) continue;
            if (!input.StartsWith(h, StringComparison.OrdinalIgnoreCase)) continue;
            if (input[h.Length] != ' ') continue;
            return input[(h.Length + 1)..];
        }
        return input;
    }

    /// <summary>
    /// If every letter in <paramref name="input"/> has the same case (all lower or all upper),
    /// capitalize each word and hyphenated sub-word. Otherwise keep the original casing.
    /// </summary>
    internal static string NormalizeCasing(string input)
    {
        var letters = input.Where(char.IsLetter).ToArray();
        if (letters.Length == 0) return input;

        var allLower = letters.All(char.IsLower);
        var allUpper = letters.All(char.IsUpper);

        return allLower || allUpper ? CapitalizeEachWord(input) : input;
    }

    internal static bool ValidateNamePart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Any(char.IsControl)) return false;
        if (value.Any(c => char.GetUnicodeCategory(c) == UnicodeCategory.OtherNotAssigned)) return false;
        return SinglePartPattern.IsMatch(value);
    }

    internal static bool ValidateGivenNames(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && parts.All(ValidateNamePart);
    }

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
}
