using System.Text;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Organization;

/// <summary>
/// The registered name of a legal entity from any European jurisdiction. Use this type when
/// ingesting company names from cross-border sources such as EU VAT lookups (VIES), EORI
/// customs lookups, GEMI exports, or commercial business-data brokers — situations where the
/// strict Swedish naming rules of <see cref="SwedishOrganizationName"/> do not apply.
/// </summary>
/// <remarks>
/// <para>The allowed character set is broader than <see cref="SwedishOrganizationName"/> to
/// accommodate two real-world conventions:</para>
/// <list type="bullet">
/// <item><description>The pipe character (<c>|</c>) is permitted because some upstream
/// registries flatten the legal name and the trade/brand name into a single string using
/// <c>|</c> or <c>||</c> as a separator (e.g. Greek GEMI exports). When such a separator is
/// present, the parts are exposed via <see cref="LegalName"/> and <see cref="TradeName"/>.
/// </description></item>
/// <item><description>The double-quote character (<c>"</c>) is permitted because Baltic and
/// Slavic registries (Latvia, Lithuania, Estonia, Poland, Russia, Ukraine, Bulgaria) legally
/// enclose the distinctive name in double quotes with the legal form sitting outside, e.g.
/// <c>SIA "Example LV"</c>, <c>UAB "Example LT"</c>. Typographic and guillemet variants
/// (<c>“ ” „ ‟ « »</c>) are normalized to ASCII <c>"</c> by <c>InputSanitization</c> before
/// validation.</description></item>
/// </list>
/// <para>This type performs validation and split-name extraction only; it does not infer a
/// jurisdiction-specific organization type. For Swedish-specific inference (Aktiebolag,
/// Handelsbolag, Bostadsrättsförening, government agencies, …) use
/// <see cref="SwedishOrganizationName"/>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://ec.europa.eu/taxation_customs/vies/">VIES</see> — EU VAT validation</description></item>
/// <item><description><see href="https://www.businessregisters.eu/">European Business Registry Association</see></description></item>
/// </list>
/// </remarks>
public sealed class EuOrganizationName : IEquatable<EuOrganizationName>, IComparable<EuOrganizationName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "EU Organization Name",
        "Europeiskt organisationsnamn",
        "🇪🇺",
        ["https://ec.europa.eu/taxation_customs/vies/", "https://www.businessregisters.eu/"]);

    private const int MaxInputLength = 300;

    // Allowed: Unicode letters, digits, whitespace, and the punctuation - ' & . , / : ( ) +
    // plus | (combined LEGAL||TRADE separator) and " (Baltic/Slavic distinctive-name quoting).
    private static readonly Regex CompanyNamePattern = new(@"^[\p{L}\d\s\-\'\&\.,/:()\+\|""]+$", RegexOptions.Compiled);

    private static readonly Regex PipeSeparatorRegex = new(@"\s*\|+\s*", RegexOptions.Compiled);

    /// <summary>The whitespace-collapsed name as provided. Casing is preserved as entered.</summary>
    public string Value { get; }

    /// <summary>
    /// The legal/registered portion of the name. When the input contains a pipe-separated
    /// combined form like <c>LEGAL||TRADE</c> or <c>LEGAL | TRADE</c>, this returns the part
    /// before the first pipe run. Otherwise this equals <see cref="Value"/>.
    /// </summary>
    public string LegalName { get; }

    /// <summary>
    /// The trade/brand portion when the input is a pipe-separated combined form;
    /// <see langword="null"/> otherwise. Multi-segment forms (<c>A||B||C</c>) are joined
    /// with <c> | </c>.
    /// </summary>
    public string? TradeName { get; }

    /// <summary>
    /// <see langword="true"/> when the input was a pipe-separated combined form and a
    /// non-empty trade name was extracted. Equivalent to <c>TradeName is not null</c>.
    /// </summary>
    public bool HasTradeName => TradeName is not null;

    private EuOrganizationName(string value, string legalName, string? tradeName)
    {
        Value = value;
        LegalName = legalName;
        TradeName = tradeName;
    }

    public static bool TryParse(string? input, out EuOrganizationName? result)
    {
        result = null;
        var normalized = InputSanitization.CollapseWhitespace(input);
        if (normalized.Length > MaxInputLength) return false;
        if (!Validate(normalized)) return false;

        var (legalName, tradeName) = SplitLegalAndTrade(normalized);
        result = new EuOrganizationName(normalized, legalName, tradeName);
        return true;
    }

    public static EuOrganizationName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid EU organization name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the whitespace-collapsed organization name, for example <c>SIA "Example LV"</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty. When
    /// <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns
    /// the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r)
            ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical normalized form (whitespace-collapsed). Returns <see langword="null"/>
    /// when the input is invalid.
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

    /// <summary>
    /// Splits a combined <c>LEGAL||TRADE</c> or <c>LEGAL | TRADE</c> name into its parts.
    /// Recognizes any run of one or more <c>|</c> characters (with optional surrounding
    /// whitespace) as a separator. Multi-segment forms like <c>A||B||C</c> yield
    /// <paramref name="tradeName"/> = <c>"B | C"</c>. When no usable separator is found,
    /// <paramref name="legalName"/> equals the trimmed/normalized input and
    /// <paramref name="tradeName"/> is <see langword="null"/>. Returns <see langword="false"/>
    /// only for null/empty input.
    /// </summary>
    public static bool TrySplitLegalAndTrade(string? input, out string legalName, out string? tradeName)
    {
        legalName = string.Empty;
        tradeName = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var (legal, trade) = SplitLegalAndTrade(InputSanitization.CollapseWhitespace(input));
        legalName = legal;
        tradeName = trade;
        return true;
    }

    /// <summary>Returns the canonical normalized form, for example <c>SIA "Example LV"</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical normalized form, for example <c>SIA "Example LV"</c>.</summary>
    public override string ToString() => Value;

    // --- Internal / private ---

    private static (string Legal, string? Trade) SplitLegalAndTrade(string collapsed)
    {
        if (collapsed.IndexOf('|') < 0) return (collapsed, null);

        var parts = PipeSeparatorRegex.Split(collapsed);
        var nonEmpty = new List<string>(parts.Length);
        foreach (var p in parts)
        {
            if (!string.IsNullOrEmpty(p)) nonEmpty.Add(p);
        }
        if (nonEmpty.Count < 2) return (collapsed, null);

        var legal = nonEmpty[0];
        var trade = string.Join(" | ", nonEmpty.Skip(1));
        return (legal, trade);
    }

    private static bool Validate(string? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value)) return false;
        if (value.Any(char.IsControl)) return false;
        if (value.Length < 2 || value.Length > 200) return false;
        return CompanyNamePattern.IsMatch(value);
    }

    public bool Equals(EuOrganizationName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EuOrganizationName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(EuOrganizationName? a, EuOrganizationName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EuOrganizationName? a, EuOrganizationName? b) => !(a == b);
    public int CompareTo(EuOrganizationName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(EuOrganizationName left, EuOrganizationName right) => left.CompareTo(right) < 0;
    public static bool operator >(EuOrganizationName left, EuOrganizationName right) => left.CompareTo(right) > 0;
    public static bool operator <=(EuOrganizationName left, EuOrganizationName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EuOrganizationName left, EuOrganizationName right) => left.CompareTo(right) >= 0;
}
