using Buildi.Primitives;

namespace Buildi.Primitives.Person;

/// <summary>
/// A person's given names (<c>förnamn</c>). Swedish naming law (namnlagen, SFS 2016:1013) defines
/// given names as one or more names chosen at birth or later. One of the given names may be designated
/// as the preferred name (<c>tilltalsnamn</c>) — the name the person goes by in everyday use.
/// When all letters share the same case (all lower or all upper), normalization auto-capitalizes
/// each word; mixed-case input is preserved as-is.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html">Skatteverket — Namn</see></description></item>
/// <item><description><see href="https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/">Namnlagen (SFS 2016:1013)</see> — Swedish naming law</description></item>
/// </list>
/// </remarks>
public sealed class PersonGivenName : IEquatable<PersonGivenName>, IComparable<PersonGivenName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Given Name", "Förnamn", "✏️", ["https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html", "https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/"]);

    private const int MaxInputLength = 200;

    /// <summary>All given names as a single normalized string, e.g. <c>Anna Maria</c>.</summary>
    public string Value { get; }

    /// <summary>Individual given names, e.g. <c>["Anna", "Maria"]</c>.</summary>
    public IReadOnlyList<string> Names { get; }

    /// <summary>
    /// The preferred name (<c>tilltalsnamn</c>), or <see langword="null"/> if not explicitly set.
    /// Must be one of the values in <see cref="Names"/> when specified.
    /// </summary>
    public string? PreferredName { get; }

    private PersonGivenName(string value, IReadOnlyList<string> names, string? preferredName)
    {
        Value = value;
        Names = names;
        PreferredName = preferredName;
    }

    public static bool TryParse(string? input, out PersonGivenName? result)
        => TryParse(input, preferredName: null, out result);

    /// <summary>
    /// Parses given names and explicitly sets the preferred name (<c>tilltalsnamn</c>).
    /// The <paramref name="preferredName"/> must match one of the parsed given names (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, string? preferredName, out PersonGivenName? result)
    {
        result = null;
        var collapsed = PersonNameNormalization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(collapsed) || collapsed.Length > MaxInputLength) return false;

        var normalized = PersonNameNormalization.NormalizeCasing(collapsed);
        if (!PersonNameNormalization.ValidateGivenNames(normalized)) return false;

        var names = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string? resolvedPreferred = null;
        if (!string.IsNullOrWhiteSpace(preferredName))
        {
            var normalizedPreferred = PersonNameNormalization.NormalizeCasing(
                PersonNameNormalization.CollapseWhitespace(preferredName));
            resolvedPreferred = names.FirstOrDefault(n =>
                string.Equals(n, normalizedPreferred, StringComparison.OrdinalIgnoreCase));
            if (resolvedPreferred == null) return false;
        }

        result = new PersonGivenName(normalized, names, resolvedPreferred);
        return true;
    }

    public static PersonGivenName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid given name(s).", nameof(input));
        return result!;
    }

    /// <summary>
    /// Parses given names and explicitly sets the preferred name (<c>tilltalsnamn</c>).
    /// </summary>
    public static PersonGivenName Parse(string input, string preferredName)
    {
        if (!TryParse(input, preferredName, out var result))
            throw new ArgumentException("Invalid given name(s) or preferred name not found.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Returns a new instance with the specified preferred name (<c>tilltalsnamn</c>).
    /// The name must match one of the existing given names (case-insensitive).
    /// </summary>
    public PersonGivenName WithPreferredName(string preferredName)
    {
        var resolved = Names.FirstOrDefault(n =>
            string.Equals(n, preferredName, StringComparison.OrdinalIgnoreCase));
        if (resolved == null)
            throw new ArgumentException("Preferred name must be one of the given names.", nameof(preferredName));
        return new PersonGivenName(Value, Names, resolved);
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the given names with normalized casing, for example <c>Anna Maria</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized given names, for example <c>Anna Maria</c>.
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

    /// <summary>Returns the normalized given names, for example <c>Anna Maria</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the normalized given names, for example <c>Anna Maria</c>.</summary>
    public override string ToString() => Value;

    public bool Equals(PersonGivenName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PersonGivenName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(PersonGivenName? a, PersonGivenName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PersonGivenName? a, PersonGivenName? b) => !(a == b);
    public int CompareTo(PersonGivenName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(PersonGivenName left, PersonGivenName right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonGivenName left, PersonGivenName right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonGivenName left, PersonGivenName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonGivenName left, PersonGivenName right) => left.CompareTo(right) >= 0;
}
