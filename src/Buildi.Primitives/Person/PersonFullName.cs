using Buildi.Primitives;

namespace Buildi.Primitives.Person;

/// <summary>
/// A person's full name (<c>fullständigt namn</c>), composed of given names (<c>förnamn</c>) and
/// a family name (<c>efternamn</c>). Can be constructed from already-parsed parts via
/// <see cref="Create(PersonGivenName, PersonFamilyName)"/> or parsed from free text via
/// <c>TryParse</c>. When parsing free text the last token becomes the family name and
/// all preceding tokens become given names. Each part applies the same casing rule: uniform case
/// (all lower or all upper) is auto-capitalized; mixed case is preserved.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html">Skatteverket — Namn</see></description></item>
/// <item><description><see href="https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/">Namnlagen (SFS 2016:1013)</see> — Swedish naming law</description></item>
/// </list>
/// </remarks>
public sealed class PersonFullName : IEquatable<PersonFullName>, IComparable<PersonFullName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Full Name", "Fullständigt namn", "📇", ["https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html", "https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/"]);

    private const int MaxInputLength = 500;

    public PersonGivenName GivenName { get; }
    public PersonFamilyName FamilyName { get; }
    public string Value { get; }

    /// <summary>
    /// Convenience accessor for <see cref="PersonGivenName.PreferredName"/> (<c>tilltalsnamn</c>).
    /// </summary>
    public string? PreferredName => GivenName.PreferredName;

    private PersonFullName(PersonGivenName givenName, PersonFamilyName familyName, string value)
    {
        GivenName = givenName;
        FamilyName = familyName;
        Value = value;
    }

    /// <summary>
    /// Creates a full name from already-parsed given and family name parts.
    /// </summary>
    public static PersonFullName Create(PersonGivenName givenName, PersonFamilyName familyName)
    {
        var value = $"{givenName.Value} {familyName.Value}";
        return new PersonFullName(givenName, familyName, value);
    }

    /// <summary>
    /// Parses a free-text full name. All tokens except the last become given names (<c>förnamn</c>);
    /// the last token becomes the family name (<c>efternamn</c>). Each part is normalized independently.
    /// </summary>
    public static bool TryParse(string? input, out PersonFullName? result)
    {
        result = null;
        var collapsed = PersonNameNormalization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(collapsed) || collapsed.Length > MaxInputLength) return false;

        var tokens = collapsed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2) return false;

        var givenPart = string.Join(" ", tokens[..^1]);
        if (!PersonGivenName.TryParse(givenPart, out var givenName)) return false;
        if (!PersonFamilyName.TryParse(tokens[^1], out var familyName)) return false;

        var value = $"{givenName!.Value} {familyName!.Value}";
        result = new PersonFullName(givenName!, familyName!, value);
        return true;
    }

    /// <summary>
    /// Parses a free-text full name and explicitly sets the preferred name (<c>tilltalsnamn</c>).
    /// The <paramref name="preferredName"/> must match one of the parsed given names (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, string preferredName, out PersonFullName? result)
    {
        result = null;
        var collapsed = PersonNameNormalization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(collapsed) || collapsed.Length > MaxInputLength) return false;

        var tokens = collapsed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2) return false;

        var givenPart = string.Join(" ", tokens[..^1]);
        if (!PersonGivenName.TryParse(givenPart, preferredName, out var givenName)) return false;
        if (!PersonFamilyName.TryParse(tokens[^1], out var familyName)) return false;

        var value = $"{givenName!.Value} {familyName!.Value}";
        result = new PersonFullName(givenName!, familyName!, value);
        return true;
    }

    public static PersonFullName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid full name. Must contain at least given name(s) and a family name.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Returns a new instance with the specified preferred name (<c>tilltalsnamn</c>).
    /// The name must match one of the given names (case-insensitive).
    /// </summary>
    public PersonFullName WithPreferredName(string preferredName)
    {
        var givenWithPreferred = GivenName.WithPreferredName(preferredName);
        return new PersonFullName(givenWithPreferred, FamilyName, Value);
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the full name with each part normalized, for example <c>Anna Maria Andersson</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized full name, for example <c>Anna Maria Andersson</c>.
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

    /// <summary>Returns the normalized full name, for example <c>Anna Maria Andersson</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the normalized full name, for example <c>Anna Maria Andersson</c>.</summary>
    public override string ToString() => Value;

    public bool Equals(PersonFullName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PersonFullName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(PersonFullName? a, PersonFullName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PersonFullName? a, PersonFullName? b) => !(a == b);
    public int CompareTo(PersonFullName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(PersonFullName left, PersonFullName right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonFullName left, PersonFullName right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonFullName left, PersonFullName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonFullName left, PersonFullName right) => left.CompareTo(right) >= 0;
}
