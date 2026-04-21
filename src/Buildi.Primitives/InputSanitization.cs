using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Buildi.Primitives;

/// <summary>
/// Shared input sanitization for text inputs across domain types.
/// Provides invisible-character stripping, whitespace normalization, and common
/// character-filtering helpers (digits-only, alphanumeric-only, collapse whitespace).
/// </summary>
/// <remarks>
/// Real-world inputs (e.g. from CRM/banking systems) commonly contain invisible
/// Unicode characters that break validation while being completely invisible in UIs
/// and logs. The most frequent offender is <c>U+202A</c> LEFT-TO-RIGHT EMBEDDING,
/// especially on values originating from RTL-language contexts (Arabic, Hebrew).
/// Other common contaminants include non-breaking spaces from web/PDF copy-paste,
/// null bytes from binary data, stray newlines from multi-line form fields,
/// and typographic (smart/curly) quotes from mobile keyboards and word processors
/// (e.g. <c>U+2019</c> RIGHT SINGLE QUOTATION MARK instead of ASCII apostrophe).
/// </remarks>
internal static class InputSanitization
{
    /// <summary>
    /// Strips invisible/non-printable characters, normalizes all whitespace variants
    /// to regular ASCII space (<c>U+0020</c>), normalizes typographic single quotes
    /// to ASCII apostrophe (<c>U+0027</c>), and normalizes typographic and guillemet
    /// double quotes to ASCII quotation mark (<c>U+0022</c>). The latter is required
    /// for Baltic/Slavic registry name conventions where the distinctive name is
    /// enclosed in double quotes (e.g. <c>SIA "EXAMPLE LV"</c>) and the source system
    /// may emit any of <c>"</c>, <c>"</c>, <c>„</c>, <c>‟</c>, <c>«</c>, <c>»</c>.
    /// </summary>
    internal static string SanitizeInput(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is '\u2018' or '\u2019' or '\u201A' or '\u201B')
            {
                sb.Append('\'');
                continue;
            }

            if (c is '\u201C' or '\u201D' or '\u201E' or '\u201F' or '\u00AB' or '\u00BB')
            {
                sb.Append('"');
                continue;
            }

            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.Format:
                case UnicodeCategory.OtherNotAssigned:
                    break;
                case UnicodeCategory.Control:
                    if (c is '\t' or '\n' or '\r')
                        sb.Append(' ');
                    break;
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    sb.Append(' ');
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    private static readonly Regex WhitespaceRunRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes the input, trims, and collapses whitespace runs to a single space.
    /// Returns <see cref="string.Empty"/> when <paramref name="input"/> is <see langword="null"/>.
    /// </summary>
    internal static string CollapseWhitespace(string? input)
    {
        if (input is null) return string.Empty;
        return WhitespaceRunRegex.Replace(SanitizeInput(input).Trim(), " ");
    }

    /// <summary>
    /// Keeps only ASCII digits (<c>0</c>–<c>9</c>), stripping everything else.
    /// </summary>
    internal static string KeepDigits(string input)
    {
        var buffer = new char[input.Length];
        var length = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c is >= '0' and <= '9')
                buffer[length++] = c;
        }
        return new string(buffer, 0, length);
    }

    /// <summary>
    /// Keeps only ASCII letters and digits, uppercasing letters.
    /// Strips whitespace, hyphens, and all other non-alphanumeric characters.
    /// </summary>
    internal static string KeepAsciiAlphanumericUppercase(string input)
    {
        var buffer = new char[input.Length];
        var length = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c is >= '0' and <= '9')
                buffer[length++] = c;
            else if (c is >= 'A' and <= 'Z')
                buffer[length++] = c;
            else if (c is >= 'a' and <= 'z')
                buffer[length++] = (char)(c - 32);
        }
        return new string(buffer, 0, length);
    }
}
