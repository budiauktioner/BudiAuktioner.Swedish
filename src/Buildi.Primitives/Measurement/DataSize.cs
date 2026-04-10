using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A digital data size stored internally in bytes. Supports decimal (SI) and binary (IEC) units
/// (e.g. <c>10 MB</c>, <c>1 GiB</c>, <c>512 KB</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/publications/si-brochure">BIPM SI Brochure</see> — SI prefixes for decimal multiples</description></item>
/// <item><description><see href="https://www.iec.ch/standardsonline/iec-standard/iec-60027-2">IEC 60027-2</see> — binary prefixes (kibi, mebi, …)</description></item>
/// </list>
/// </remarks>
public sealed class DataSize : IComparable<DataSize>, IEquatable<DataSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Data Size", "Datastorlek", "💾", ["https://www.bipm.org/en/publications/si-brochure", "https://www.iec.ch/standardsonline/iec-standard/iec-60027-2"]);

    private readonly decimal _bytes;
    private readonly DataSizeUnit _originalUnit;

    private DataSize(decimal bytes, DataSizeUnit originalUnit)
    {
        _bytes = bytes;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in bytes, e.g. <c>1024</c> for 1 KiB.</summary>
    public decimal Bytes => _bytes;
    /// <summary>The value in kilobytes (1000 B), e.g. <c>1</c> for 1000 B.</summary>
    public decimal Kilobytes => _bytes / DataSizeUnit.Kilobyte.ToBaseUnitFactor;
    /// <summary>The value in megabytes (1000² B).</summary>
    public decimal Megabytes => _bytes / DataSizeUnit.Megabyte.ToBaseUnitFactor;
    /// <summary>The value in gigabytes (1000³ B).</summary>
    public decimal Gigabytes => _bytes / DataSizeUnit.Gigabyte.ToBaseUnitFactor;
    /// <summary>The value in terabytes (1000⁴ B).</summary>
    public decimal Terabytes => _bytes / DataSizeUnit.Terabyte.ToBaseUnitFactor;
    /// <summary>The value in petabytes (1000⁵ B).</summary>
    public decimal Petabytes => _bytes / DataSizeUnit.Petabyte.ToBaseUnitFactor;
    /// <summary>The value in exabytes (1000⁶ B).</summary>
    public decimal Exabytes => _bytes / DataSizeUnit.Exabyte.ToBaseUnitFactor;
    /// <summary>The value in kibibytes (1024 B).</summary>
    public decimal Kibibytes => _bytes / DataSizeUnit.Kibibyte.ToBaseUnitFactor;
    /// <summary>The value in mebibytes (1024² B).</summary>
    public decimal Mebibytes => _bytes / DataSizeUnit.Mebibyte.ToBaseUnitFactor;
    /// <summary>The value in gibibytes (1024³ B).</summary>
    public decimal Gibibytes => _bytes / DataSizeUnit.Gibibyte.ToBaseUnitFactor;
    /// <summary>The value in tebibytes (1024⁴ B).</summary>
    public decimal Tebibytes => _bytes / DataSizeUnit.Tebibyte.ToBaseUnitFactor;
    /// <summary>The value in pebibytes (1024⁵ B).</summary>
    public decimal Pebibytes => _bytes / DataSizeUnit.Pebibyte.ToBaseUnitFactor;
    /// <summary>The value in exbibytes (1024⁶ B).</summary>
    public decimal Exbibytes => _bytes / DataSizeUnit.Exbibyte.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public DataSizeUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(DataSizeUnit unit) => _bytes / unit.ToBaseUnitFactor;

    public static DataSize FromBytes(decimal b) => new(b, DataSizeUnit.Byte);
    public static DataSize FromKilobytes(decimal kb) => new(kb * DataSizeUnit.Kilobyte.ToBaseUnitFactor, DataSizeUnit.Kilobyte);
    public static DataSize FromMegabytes(decimal mb) => new(mb * DataSizeUnit.Megabyte.ToBaseUnitFactor, DataSizeUnit.Megabyte);
    public static DataSize FromGigabytes(decimal gb) => new(gb * DataSizeUnit.Gigabyte.ToBaseUnitFactor, DataSizeUnit.Gigabyte);
    public static DataSize FromTerabytes(decimal tb) => new(tb * DataSizeUnit.Terabyte.ToBaseUnitFactor, DataSizeUnit.Terabyte);
    public static DataSize FromPetabytes(decimal pb) => new(pb * DataSizeUnit.Petabyte.ToBaseUnitFactor, DataSizeUnit.Petabyte);
    public static DataSize FromExabytes(decimal eb) => new(eb * DataSizeUnit.Exabyte.ToBaseUnitFactor, DataSizeUnit.Exabyte);
    public static DataSize FromKibibytes(decimal kib) => new(kib * DataSizeUnit.Kibibyte.ToBaseUnitFactor, DataSizeUnit.Kibibyte);
    public static DataSize FromMebibytes(decimal mib) => new(mib * DataSizeUnit.Mebibyte.ToBaseUnitFactor, DataSizeUnit.Mebibyte);
    public static DataSize FromGibibytes(decimal gib) => new(gib * DataSizeUnit.Gibibyte.ToBaseUnitFactor, DataSizeUnit.Gibibyte);
    public static DataSize FromTebibytes(decimal tib) => new(tib * DataSizeUnit.Tebibyte.ToBaseUnitFactor, DataSizeUnit.Tebibyte);
    public static DataSize FromPebibytes(decimal pib) => new(pib * DataSizeUnit.Pebibyte.ToBaseUnitFactor, DataSizeUnit.Pebibyte);
    public static DataSize FromExbibytes(decimal eib) => new(eib * DataSizeUnit.Exbibyte.ToBaseUnitFactor, DataSizeUnit.Exbibyte);

    /// <summary>Creates a <see cref="DataSize"/> from a value and unit.</summary>
    public static DataSize Create(decimal value, DataSizeUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out DataSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!DataSizeUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new DataSize(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static DataSize Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid data size.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string preserving the input unit, e.g. <c>10 MB</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, DataSizeUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.ToString(unit ?? r.OriginalUnit, decimals);
            return r.ToString();
        }
        if (fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input))
            return input.Trim();
        return null;
    }

    /// <summary>
    /// Returns the value in bytes as an invariant string, e.g. <c>10000 B</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the value in bytes with invariant formatting, e.g. <c>1024 B</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_bytes);
        return $"{formatted} {DataSizeUnit.Byte.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>1 KiB</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1000 KB</c>.
    /// </summary>
    public string ToString(DataSizeUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable unit for this value, e.g. GB for 512,000,000,000 bytes.
    /// </summary>
    public DataSizeUnit NaturalUnit => DataSizeUnit.GetNatural(_bytes);

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>512 GB</c> instead of <c>512000000000 B</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => ToString(NaturalUnit, decimals);

    private static string FormatDecimal(decimal value, int? decimals = null)
    {
        if (decimals is not null)
            value = Math.Round(value, decimals.Value, MidpointRounding.AwayFromZero);
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static DataSize operator +(DataSize a, DataSize b) => new(a._bytes + b._bytes, a._originalUnit);
    public static DataSize operator -(DataSize a, DataSize b) => new(a._bytes - b._bytes, a._originalUnit);
    public static DataSize operator *(DataSize a, decimal factor) => new(a._bytes * factor, a._originalUnit);
    public static DataSize operator *(decimal factor, DataSize a) => new(a._bytes * factor, a._originalUnit);
    public static DataSize operator /(DataSize a, decimal divisor) => new(a._bytes / divisor, a._originalUnit);
    public static DataSize operator -(DataSize a) => new(-a._bytes, a._originalUnit);

    public static bool operator ==(DataSize? a, DataSize? b) => a?._bytes == b?._bytes;
    public static bool operator !=(DataSize? a, DataSize? b) => !(a == b);
    public static bool operator <(DataSize a, DataSize b) => a._bytes < b._bytes;
    public static bool operator >(DataSize a, DataSize b) => a._bytes > b._bytes;
    public static bool operator <=(DataSize a, DataSize b) => a._bytes <= b._bytes;
    public static bool operator >=(DataSize a, DataSize b) => a._bytes >= b._bytes;

    public int CompareTo(DataSize? other) => other is null ? 1 : _bytes.CompareTo(other._bytes);
    public bool Equals(DataSize? other) => other is not null && _bytes == other._bytes;
    public override bool Equals(object? obj) => obj is DataSize other && Equals(other);
    public override int GetHashCode() => _bytes.GetHashCode();

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:exbibytes|exbibyte|exabytes|exabyte|pebibytes|pebibyte|petabytes|petabyte|tebibytes|tebibyte|terabytes|terabyte|gibibytes|gibibyte|gigabytes|gigabyte|mebibytes|mebibyte|megabytes|megabyte|kibibytes|kibibyte|kilobytes|kilobyte|EiB|PiB|TiB|GiB|MiB|KiB|EB|PB|TB|GB|MB|KB|bytes|byte|B)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like data size values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<DataSize>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<DataSize>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var size)) continue;
            results.Add(new TextCandidate<DataSize>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(DataSize), TextCandidateCategory.Measurement,
                size!.ToNormalizedString(), size.ToString(),
                size.ToMaskedString(),
                TextMatchConfidence.Medium,
                size));
        }
        return results;
    }
}
