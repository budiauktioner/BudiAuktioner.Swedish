using Buildi.Primitives;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Banking;

/// <summary>
/// The type of bank account holder: natural person or legal entity.
/// </summary>
public enum BankAccountHolderType
{
    Unknown = 0,
    Person,
    Organization
}

/// <summary>
/// A Swedish bank account holder name (<c>kontoinnehavare</c>).
/// Holder names in Swedish banking systems (Bankgirot, SUS) are often in ALL CAPS.
/// Delegates validation to <see cref="PersonFullName"/> and
/// <see cref="Organization.SwedishOrganizationName"/> under the hood, and uses
/// <see cref="Organization.SwedishOrganizationName.HasOrganizationIndicators"/> to detect
/// whether the holder is a person or an organization.
/// </summary>
/// <remarks>
/// <para>Detection uses suffixes such as <c>AB</c>, <c>HB</c>, <c>KB</c>, <c>BRF</c>
/// and keywords like <c>kommun</c>, <c>stiftelse</c>, <c>förening</c>.</para>
/// </remarks>
public sealed class SwedishBankAccountHolderName : IEquatable<SwedishBankAccountHolderName>, IComparable<SwedishBankAccountHolderName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Account Holder Name", "Kontoinnehavare", "👤", []);

    /// <summary>The holder name with whitespace collapsed but original casing preserved, e.g. <c>VOLVO AB</c> or <c>Anna Andersson</c>.</summary>
    public string Value { get; }

    /// <summary>Whether the holder is detected as a person or organization.</summary>
    public BankAccountHolderType HolderType { get; }

    /// <summary>
    /// The parsed person name when <see cref="HolderType"/> is <see cref="BankAccountHolderType.Person"/>.
    /// <see langword="null"/> when the holder is an organization.
    /// </summary>
    public PersonFullName? PersonName { get; }

    /// <summary>
    /// The parsed organization name when <see cref="HolderType"/> is <see cref="BankAccountHolderType.Organization"/>.
    /// <see langword="null"/> when the holder is a person.
    /// </summary>
    public Organization.SwedishOrganizationName? OrganizationName { get; }

    private SwedishBankAccountHolderName(
        string value,
        BankAccountHolderType holderType,
        PersonFullName? personName,
        Organization.SwedishOrganizationName? organizationName)
    {
        Value = value;
        HolderType = holderType;
        PersonName = personName;
        OrganizationName = organizationName;
    }

    public static bool TryParse(string? input, out SwedishBankAccountHolderName? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var collapsed = InputSanitization.CollapseWhitespace(input);
        if (collapsed.Length < 2) return false;
        if (!collapsed.Any(char.IsLetter)) return false;

        if (Organization.SwedishOrganizationName.TryParse(collapsed, out var orgName) && orgName!.HasOrganizationIndicators)
        {
            result = new SwedishBankAccountHolderName(
                collapsed, BankAccountHolderType.Organization, null, orgName);
            return true;
        }

        if (PersonFullName.TryParse(collapsed, out var personName) && personName is not null)
        {
            result = new SwedishBankAccountHolderName(
                collapsed, BankAccountHolderType.Person, personName, null);
            return true;
        }

        if (orgName is not null)
        {
            result = new SwedishBankAccountHolderName(
                collapsed, BankAccountHolderType.Organization, null, orgName);
            return true;
        }

        return false;
    }

    public static SwedishBankAccountHolderName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid bank account holder name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the holder name with whitespace collapsed and original casing preserved.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the holder name with whitespace collapsed and original casing preserved.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        var result = Format(input);
        if (result is not null) return result;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the holder name with whitespace collapsed and original casing preserved.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the holder name with whitespace collapsed and original casing preserved.</summary>
    public override string ToString() => Value;

    public bool Equals(SwedishBankAccountHolderName? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is SwedishBankAccountHolderName other && Equals(other);
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    public static bool operator ==(SwedishBankAccountHolderName? a, SwedishBankAccountHolderName? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishBankAccountHolderName? a, SwedishBankAccountHolderName? b) => !(a == b);
    public int CompareTo(SwedishBankAccountHolderName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishBankAccountHolderName left, SwedishBankAccountHolderName right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishBankAccountHolderName left, SwedishBankAccountHolderName right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishBankAccountHolderName left, SwedishBankAccountHolderName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishBankAccountHolderName left, SwedishBankAccountHolderName right) => left.CompareTo(right) >= 0;
}
