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
}
