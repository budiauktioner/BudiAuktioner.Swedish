using Buildi.Primitives;

namespace Buildi.Primitives.Organization;

/// <summary>
/// An Entity Legal Form (ELF) code as defined by ISO 20275 identifies the legal form of an entity
/// (e.g. <c>XTIQ</c> for Aktiebolag in Sweden). The code is a 4-character alphanumeric identifier
/// maintained by GLEIF. This type validates the structure and provides descriptions for known
/// Swedish legal forms.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gleif.org/en/about-lei/code-lists/iso-20275-entity-legal-forms-code-list">GLEIF — ISO 20275 Entity Legal Forms Code List</see></description></item>
/// <item><description><see href="https://www.iso.org/standard/67462.html">ISO 20275</see> — Entity Legal Forms (ELF)</description></item>
/// </list>
/// </remarks>
public sealed class ElfCode : IEquatable<ElfCode>, IComparable<ElfCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Entity Legal Form Code", "Juridisk företagsform", "📋", ["https://www.gleif.org/en/about-lei/code-lists/iso-20275-entity-legal-forms-code-list", "https://www.iso.org/standard/67462.html"]);

    private const int CodeLength = 4;
    private const int MaxInputLength = 20;

    /// <summary>The 4-character ELF code in uppercase, for example <c>XTIQ</c>.</summary>
    public string Code { get; }

    /// <summary>The English description of the legal form, or <see langword="null"/> if the code is not in the known list.</summary>
    public string? EnglishName { get; }

    /// <summary>The Swedish description of the legal form, or <see langword="null"/> if the code is not in the known list.</summary>
    public string? LocalizedName { get; }

    /// <summary>Display name in the current display language. Returns the code itself when no description is available.</summary>
    public string DisplayName => (PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName) ?? Code;

    /// <summary>Whether this ELF code has a known description in the library.</summary>
    public bool IsKnown => EnglishName is not null;

    private ElfCode(string code, string? englishName, string? localizedName)
    {
        Code = code;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out ElfCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.KeepAsciiAlphanumericUppercase(input);
        if (cleaned.Length > MaxInputLength) return false;
        if (cleaned.Length != CodeLength) return false;

        var known = KnownCodes.GetValueOrDefault(cleaned);
        result = new ElfCode(cleaned, known?.EnglishName, known?.SwedishName);
        return true;
    }

    public static ElfCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid ELF code (must be exactly 4 alphanumeric characters).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name of the legal form, for example <c>Aktiebolag</c>.
    /// Returns the code itself for structurally valid but unknown codes.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r)
            ? r!.DisplayName
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 4-character ELF code in uppercase, for example <c>XTIQ</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the 4-character ELF code in uppercase, for example <c>XTIQ</c>.
    /// </summary>
    public string ToNormalizedString() => Code;

    /// <summary>
    /// Returns the display name of the legal form in the current display language,
    /// for example <c>Aktiebolag</c>. Returns the code for unknown codes.
    /// </summary>
    public override string ToString() => DisplayName;

    private record KnownElf(string EnglishName, string SwedishName);

    private static readonly Dictionary<string, KnownElf> KnownCodes = new(StringComparer.Ordinal)
    {
        ["XTIQ"] = new("Limited company", "Aktiebolag"),
        ["N2GY"] = new("General partnership", "Handelsbolag"),
        ["FR3V"] = new("Limited partnership", "Kommanditbolag"),
        ["V2YH"] = new("Economic association", "Ekonomisk förening"),
        ["WJEL"] = new("Sole proprietorship", "Enskild näringsidkare"),
        ["CLBQ"] = new("Foundation", "Stiftelse"),
        ["F85L"] = new("Non-profit association", "Ideell förening"),
        ["O9FH"] = new("Housing cooperative", "Bostadsrättsförening"),
        ["H0PO"] = new("Cooperative housing association", "Kooperativ hyresrättsförening"),
        ["2HBR"] = new("Government entity", "Statlig enhet"),
        ["EVKQ"] = new("Municipality", "Kommun"),
        ["R7GX"] = new("Region", "Region"),
        ["KQM9"] = new("Banking company", "Bankaktiebolag"),
        ["L5CF"] = new("Insurance company", "Försäkringsaktiebolag"),
        ["LRQE"] = new("Mutual insurance company", "Ömsesidigt försäkringsbolag"),
        ["J4GF"] = new("Branch of foreign company", "Filial till utländskt företag"),
        ["9GQP"] = new("European company (SE)", "Europabolag"),
    };

    public bool Equals(ElfCode? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is ElfCode other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(ElfCode? a, ElfCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ElfCode? a, ElfCode? b) => !(a == b);
    public int CompareTo(ElfCode? other) => other is null ? 1 : string.Compare(Code, other.Code, StringComparison.Ordinal);
    public static bool operator <(ElfCode left, ElfCode right) => left.CompareTo(right) < 0;
    public static bool operator >(ElfCode left, ElfCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(ElfCode left, ElfCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ElfCode left, ElfCode right) => left.CompareTo(right) >= 0;
}
