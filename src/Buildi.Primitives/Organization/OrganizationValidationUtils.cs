using System.Numerics;
using System.Text;

namespace Buildi.Primitives.Organization;

/// <summary>
/// Shared helper methods for organization validation (internal use).
/// </summary>
internal static class OrganizationValidationUtils
{
    /// <summary>
    /// Validates ISO 17442 Mod-97 checksum (used by LEI).
    /// Input should be alphanumeric. Letters are converted to numbers (A=10, B=11..).
    /// </summary>
    internal static bool IsValidIso7064Mod97(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        var sb = new StringBuilder(input.Length * 2);
        foreach (var c in input)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append(c);
            }
            else if (c >= 'A' && c <= 'Z')
            {
                sb.Append((int)c - 55); // 'A' is 65 -> 10
            }
            else if (c >= 'a' && c <= 'z')
            {
                sb.Append((int)c - 87); // 'a' is 97 -> 10
            }
            else
            {
                return false; // Invalid char
            }
        }

        if (!BigInteger.TryParse(sb.ToString(), out var number))
        {
            return false;
        }

        return (number % 97) == 1;
    }

    /// <summary>
    /// Cleans common characters from a VAT/Org string (spaces, dashes, nbsp).
    /// </summary>
    internal static string CleanSeparators(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var sb = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c != ' ' && c != '-' && c != '\u00A0')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    
    /// <summary>
    /// cleans specific strings from input like (publ)
    /// </summary>
    internal static string CleanName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var s = name!.Trim().ToUpperInvariant();
        if (s.EndsWith("(PUBL)", StringComparison.Ordinal))
        {
            s = s.Substring(0, s.Length - 6).Trim();
        }
        return s;
    }

    internal static bool NameContains(string haystack, params string[] needles)
    {
        if (string.IsNullOrEmpty(haystack)) return false;
        foreach (var needle in needles)
            if (haystack.IndexOf(needle, StringComparison.Ordinal) >= 0)
                return true;
        return false;
    }

    internal static bool NameContainsStandalone(string haystack, params string[] needles)
    {
        if (string.IsNullOrEmpty(haystack)) return false;
        foreach (var needle in needles)
        {
            if (haystack.Equals(needle, StringComparison.Ordinal)) return true;
            if (haystack.StartsWith(needle + " ", StringComparison.Ordinal)) return true;
            if (haystack.EndsWith(" " + needle, StringComparison.Ordinal)) return true;
            if (haystack.IndexOf(" " + needle + " ", StringComparison.Ordinal) >= 0) return true;
        }
        return false;
    }

    internal static bool NameEndsWith(string haystack, params string[] needles)
    {
        if (string.IsNullOrEmpty(haystack)) return false;
        foreach (var needle in needles)
            if (haystack.EndsWith(needle, StringComparison.Ordinal))
                return true;
        return false;
    }
}
