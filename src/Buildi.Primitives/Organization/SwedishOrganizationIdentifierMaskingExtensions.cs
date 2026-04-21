namespace Buildi.Primitives.Organization;

/// <summary>
/// Extension methods for masking organization identifiers that do not already have masking in
/// <see cref="OrganizationMaskingExtensions"/> (LEI and D-U-N-S).
/// </summary>
public static class SwedishOrganizationIdentifierMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked LEI showing the LOU prefix and masking the rest,
    /// e.g. <c>5493001KJTIIGC8Y1R12</c> → <c>5493****************</c>.
    /// </summary>
    public static string ToMaskedString(this LeiCode lei)
    {
        var prefix = lei.LouPrefix;
        return $"{prefix}{new string(MaskChar, lei.Value.Length - prefix.Length)}";
    }

    /// <summary>
    /// Returns a masked D-U-N-S number, e.g. <c>123456789</c> → <c>*********</c>.
    /// D-U-N-S numbers have no structural prefix worth preserving, so all digits are masked.
    /// </summary>
    public static string ToMaskedString(this DunsNumber duns) =>
        new string(MaskChar, duns.Digits.Length);

    /// <summary>
    /// Returns a masked organization name showing only the first character of each word,
    /// e.g. <c>Volvo Cars AB</c> → <c>V**** C*** AB</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishOrganizationName orgName)
    {
        var parts = orgName.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts.Select(p =>
            p.Length <= 1 ? new string(MaskChar, 1) : $"{p[0]}{new string(MaskChar, p.Length - 1)}"));
    }

    /// <summary>
    /// Returns a masked EU organization name showing only the first character of each word,
    /// with the structural separators (space, <c>|</c>, <c>"</c>) preserved verbatim.
    /// Examples:
    /// <list type="bullet">
    /// <item><description><c>SIA "Example LV"</c> → <c>S** "E****** L*"</c></description></item>
    /// <item><description><c>Volvo AB||Volvo Cars</c> → <c>V**** A*||V**** C***</c></description></item>
    /// </list>
    /// </summary>
    public static string ToMaskedString(this EuOrganizationName orgName)
    {
        var sb = new System.Text.StringBuilder(orgName.Value.Length);
        var word = new System.Text.StringBuilder();
        foreach (var c in orgName.Value)
        {
            if (c == ' ' || c == '|' || c == '"')
            {
                AppendMaskedWord(sb, word);
                sb.Append(c);
            }
            else
            {
                word.Append(c);
            }
        }
        AppendMaskedWord(sb, word);
        return sb.ToString();
    }

    private static void AppendMaskedWord(System.Text.StringBuilder sb, System.Text.StringBuilder word)
    {
        if (word.Length == 0) return;
        sb.Append(word[0]);
        if (word.Length > 1)
            sb.Append(MaskChar, word.Length - 1);
        word.Clear();
    }
}
