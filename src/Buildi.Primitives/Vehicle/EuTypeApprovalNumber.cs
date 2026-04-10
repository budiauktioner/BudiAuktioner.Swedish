using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// EU whole-vehicle type-approval number (<c>typgodkännandenummer</c>), e.g. <c>e9*2007/46*6364*09</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32018R0858">Regulation (EU) 2018/858</see> — EU type-approval framework</description></item>
/// <item><description><see href="https://unece.org/transport/vehicle-regulations">UNECE</see> — E-mark country codes</description></item>
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — typgodkännande</description></item>
/// </list>
/// </remarks>
public sealed class EuTypeApprovalNumber : IEquatable<EuTypeApprovalNumber>, IComparable<EuTypeApprovalNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("EU Type Approval Number", "Typgodkännandenummer", "✅", ["https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32018R0858", "https://unece.org/transport/vehicle-regulations", "https://www.transportstyrelsen.se/"]);

    private const int MaxCountryCode = 58;

    private static readonly Regex ApprovalPattern = new(
        @"^[eE](\d{1,2})\*([0-9/]+)\*(\d+)\*(\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<int, string> EMarkCountries = new()
    {
        [1] = "Germany",
        [2] = "France",
        [3] = "Italy",
        [4] = "Netherlands",
        [5] = "Sweden",
        [6] = "Belgium",
        [7] = "Hungary",
        [8] = "Czech Republic",
        [9] = "Spain",
        [10] = "Serbia",
        [11] = "United Kingdom",
        [12] = "Austria",
        [13] = "Luxembourg",
        [14] = "Switzerland",
        [16] = "Norway",
        [17] = "Finland",
        [18] = "Denmark",
        [19] = "Romania",
        [20] = "Poland",
        [21] = "Portugal",
        [22] = "Russia",
        [23] = "Greece",
        [24] = "Ireland",
        [25] = "Croatia",
        [26] = "Slovenia",
        [27] = "Slovakia",
        [28] = "Belarus",
        [29] = "Estonia",
        [31] = "Bosnia and Herzegovina",
        [32] = "Latvia",
        [34] = "Bulgaria",
        [36] = "Lithuania",
        [37] = "Turkey",
        [39] = "Azerbaijan",
        [40] = "North Macedonia",
        [42] = "EU",
        [43] = "Japan",
        [45] = "Australia",
        [46] = "Ukraine",
        [47] = "South Africa",
        [48] = "New Zealand",
        [49] = "Cyprus",
        [50] = "Malta",
        [51] = "South Korea",
        [52] = "Malaysia",
        [53] = "Thailand",
        [54] = "Indonesia",
        [56] = "Montenegro",
        [57] = "San Marino",
        [58] = "Tunisia",
    };

    /// <summary>Canonical form, e.g. <c>e9*2007/46*6364*09</c>.</summary>
    public string Value { get; }

    /// <summary>E-mark country code, e.g. <c>9</c> for Spain, <c>5</c> for Sweden.</summary>
    public int ApprovalCountryCode { get; }

    /// <summary>Country name for the E-mark code, or <see langword="null"/> if the code is unassigned.</summary>
    public string? ApprovalCountryName { get; }

    /// <summary>Directive or regulation reference, e.g. <c>2007/46</c> or <c>2018/858</c>.</summary>
    public string Directive { get; }

    /// <summary>Type approval number, e.g. <c>6364</c>.</summary>
    public string TypeNumber { get; }

    /// <summary>Extension/variant number, e.g. <c>09</c>.</summary>
    public string Extension { get; }

    private EuTypeApprovalNumber(string value, int approvalCountryCode, string? approvalCountryName,
        string directive, string typeNumber, string extension)
    {
        Value = value;
        ApprovalCountryCode = approvalCountryCode;
        ApprovalCountryName = approvalCountryName;
        Directive = directive;
        TypeNumber = typeNumber;
        Extension = extension;
    }

    /// <summary>
    /// Attempts to parse an EU type-approval number such as <c>e9*2007/46*6364*09</c>.
    /// </summary>
    public static bool TryParse(string? input, out EuTypeApprovalNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = input!.Trim();
        var match = ApprovalPattern.Match(trimmed);
        if (!match.Success) return false;

        var countryCode = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        if (countryCode < 1 || countryCode > MaxCountryCode) return false;

        var directive = match.Groups[2].Value;
        var typeNumber = match.Groups[3].Value;
        var extension = match.Groups[4].Value;

        EMarkCountries.TryGetValue(countryCode, out var countryName);

        var value = $"e{countryCode}*{directive}*{typeNumber}*{extension}";
        result = new EuTypeApprovalNumber(value, countryCode, countryName, directive, typeNumber, extension);
        return true;
    }

    /// <summary>
    /// Parses an EU type-approval number. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static EuTypeApprovalNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid EU type-approval number.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid EU type-approval number.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical type-approval number, e.g. <c>e9*2007/46*6364*09</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical type-approval number, e.g. <c>e9*2007/46*6364*09</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical type-approval number, e.g. <c>e9*2007/46*6364*09</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical type-approval number, e.g. <c>e9*2007/46*6364*09</c>.</summary>
    public override string ToString() => Value;

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\b[eE]\d{1,2}\*[0-9/]+\*\d+\*\d+\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for substrings that look like EU type-approval numbers
    /// (e.g. <c>e9*2007/46*6364*09</c>). The structured format is highly distinctive.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EuTypeApprovalNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EuTypeApprovalNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var approval)) continue;
            results.Add(new TextCandidate<EuTypeApprovalNumber>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(EuTypeApprovalNumber), TextCandidateCategory.Vehicle,
                approval!.ToNormalizedString(), approval.ToString(),
                approval.ToMaskedString(),
                TextMatchConfidence.High,
                approval));
        }
        return results;
    }

    public static bool operator ==(EuTypeApprovalNumber? a, EuTypeApprovalNumber? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(EuTypeApprovalNumber? a, EuTypeApprovalNumber? b) => !(a == b);

    public bool Equals(EuTypeApprovalNumber? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is EuTypeApprovalNumber other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(EuTypeApprovalNumber? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(EuTypeApprovalNumber left, EuTypeApprovalNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(EuTypeApprovalNumber left, EuTypeApprovalNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(EuTypeApprovalNumber left, EuTypeApprovalNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EuTypeApprovalNumber left, EuTypeApprovalNumber right) => left.CompareTo(right) >= 0;
}
