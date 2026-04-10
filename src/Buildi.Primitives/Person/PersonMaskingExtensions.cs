namespace Buildi.Primitives.Person;

/// <summary>
/// Extension methods for masking sensitive personal information in display strings.
/// </summary>
public static class PersonMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked display string, e.g. <c>990807-****</c>.
    /// When <paramref name="maskBirthDate"/> is <see langword="true"/>, the birth date is also masked: <c>******-****</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishPersonalIdentityNumber pin, bool maskBirthDate = false)
    {
        var formatted = pin.Formatted;
        var sep = formatted[6];

        if (maskBirthDate)
            return $"{new string(MaskChar, 6)}{sep}{new string(MaskChar, 4)}";

        return $"{formatted[..6]}{sep}{new string(MaskChar, 4)}";
    }

    /// <summary>
    /// Returns a masked display string, e.g. <c>680164-****</c>.
    /// When <paramref name="maskBirthDate"/> is <see langword="true"/>, the birth date part is also masked: <c>******-****</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishCoordinationNumber cn, bool maskBirthDate = false)
    {
        var formatted = cn.Formatted;
        var sep = formatted[6];

        if (maskBirthDate)
            return $"{new string(MaskChar, 6)}{sep}{new string(MaskChar, 4)}";

        return $"{formatted[..6]}{sep}{new string(MaskChar, 4)}";
    }

    private const string NameMask = "***";

    /// <summary>
    /// Returns a masked given name, e.g. <c>Anna Maria</c> → <c>*** ***</c>.
    /// When <paramref name="useInitials"/> is <see langword="true"/>, returns initials instead:
    /// <c>A. M.</c>.
    /// </summary>
    public static string ToMaskedString(this PersonGivenName givenName, bool useInitials = false)
    {
        return useInitials
            ? string.Join(" ", givenName.Names.Select(n => $"{n[0]}."))
            : string.Join(" ", givenName.Names.Select(_ => NameMask));
    }

    /// <summary>
    /// Returns a masked family name, e.g. <c>Andersson</c> → <c>***</c>.
    /// When <paramref name="useInitials"/> is <see langword="true"/>, returns the initial instead:
    /// <c>A.</c>.
    /// </summary>
    public static string ToMaskedString(this PersonFamilyName familyName, bool useInitials = false)
    {
        return useInitials
            ? $"{familyName.Value[0]}."
            : NameMask;
    }

    /// <summary>
    /// Returns a masked full name, e.g. <c>Anna Maria Andersson</c> → <c>*** *** ***</c>.
    /// When <paramref name="useInitials"/> is <see langword="true"/>, returns initials instead:
    /// <c>A. M. A.</c>.
    /// When <paramref name="showGivenName"/> is <see langword="true"/>, given names are shown
    /// with the family name masked: <c>Anna Maria ***</c> (or <c>Anna Maria A.</c> with initials).
    /// </summary>
    public static string ToMaskedString(this PersonFullName fullName,
        bool useInitials = false,
        bool showGivenName = false)
    {
        var familyMasked = fullName.FamilyName.ToMaskedString(useInitials);

        if (showGivenName)
            return $"{fullName.GivenName.Value} {familyMasked}";

        return $"{fullName.GivenName.ToMaskedString(useInitials)} {familyMasked}";
    }

    /// <summary>
    /// Returns a masked age string, e.g. <c>25 år</c> → <c>** år</c>
    /// or <c>8 månader</c> → <c>* månader</c>.
    /// </summary>
    public static string ToMaskedString(this PersonAge age)
    {
        return age.Years >= 1
            ? $"{new string(MaskChar, age.Years.ToString().Length)} år"
            : $"{new string(MaskChar, age.TotalMonths.ToString().Length)} månader";
    }
}
