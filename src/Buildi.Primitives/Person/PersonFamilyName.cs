using Buildi.Primitives;

namespace Buildi.Primitives.Person;

/// <summary>
/// A person's family name (<c>efternamn</c>). Swedish naming law (namnlagen, SFS 2016:1013) defines
/// the family name as the surname a person bears, which may be acquired by birth, marriage, or
/// application. When all letters share the same case (all lower or all upper), normalization
/// auto-capitalizes each word; mixed-case input is preserved as-is.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html">Skatteverket — Namn</see></description></item>
/// <item><description><see href="https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/">Namnlagen (SFS 2016:1013)</see> — Swedish naming law</description></item>
/// </list>
/// </remarks>
public sealed class PersonFamilyName : IEquatable<PersonFamilyName>, IComparable<PersonFamilyName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Family Name", "Efternamn", "📛", ["https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html", "https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/"]);

    private const int MaxInputLength = 200;

    public string Value { get; }

    private PersonFamilyName(string value) => Value = value;

    public static bool TryParse(string? input, out PersonFamilyName? result)
    {
        result = null;
        var collapsed = PersonNameNormalization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(collapsed) || collapsed.Length > MaxInputLength) return false;

        var normalized = PersonNameNormalization.NormalizeCasing(collapsed);
        if (!PersonNameNormalization.ValidateNamePart(normalized)) return false;

        result = new PersonFamilyName(normalized);
        return true;
    }

    public static PersonFamilyName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid family name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the family name with normalized casing, for example <c>Andersson</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized family name, for example <c>Andersson</c>.
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

    /// <summary>Returns the normalized family name, for example <c>Andersson</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the normalized family name, for example <c>Andersson</c>.</summary>
    public override string ToString() => Value;

    public bool Equals(PersonFamilyName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PersonFamilyName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(PersonFamilyName? a, PersonFamilyName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PersonFamilyName? a, PersonFamilyName? b) => !(a == b);
    public int CompareTo(PersonFamilyName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(PersonFamilyName left, PersonFamilyName right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonFamilyName left, PersonFamilyName right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonFamilyName left, PersonFamilyName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonFamilyName left, PersonFamilyName right) => left.CompareTo(right) >= 0;
}
