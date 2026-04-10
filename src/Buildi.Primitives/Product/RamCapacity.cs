using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Product;

/// <summary>
/// RAM / memory capacity (<c>arbetsminne</c>).
/// Bare numbers are interpreted as gigabytes. Delegates to <see cref="DataSize"/> for unit parsing.
/// </summary>
public sealed class RamCapacity : IEquatable<RamCapacity>, IComparable<RamCapacity>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("RAM Capacity", "Arbetsminne", "💻", []);

    /// <summary>The underlying data size value.</summary>
    public DataSize Size { get; }

    /// <summary>Display form, e.g. <c>16 GB</c>.</summary>
    public string Value { get; }

    public decimal Bytes => Size.Bytes;
    public decimal Gigabytes => Size.Gigabytes;
    public decimal Megabytes => Size.Megabytes;

    private RamCapacity(DataSize size)
    {
        Size = size;
        Value = size.ToString();
    }

    private RamCapacity(DataSize size, string displayValue)
    {
        Size = size;
        Value = displayValue;
    }

    /// <summary>Creates a <see cref="RamCapacity"/> from a numeric value and data size unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static RamCapacity Create(decimal value, DataSizeUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "RAM capacity must be positive.");
        var size = DataSize.Create(value, unit);
        return new RamCapacity(size);
    }

    /// <summary>Creates a <see cref="RamCapacity"/> from a numeric value and data size unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static RamCapacity Create(int value, DataSizeUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates a <see cref="RamCapacity"/> from gigabytes, e.g. <c>FromGigabytes(16)</c>.</summary>
    public static RamCapacity FromGigabytes(decimal gb) => Create(gb, DataSizeUnit.Gigabyte);

    /// <summary>Creates a <see cref="RamCapacity"/> from gigabytes, e.g. <c>FromGigabytes(16)</c>.</summary>
    public static RamCapacity FromGigabytes(int gb) => Create(gb, DataSizeUnit.Gigabyte);

    /// <summary>Creates a <see cref="RamCapacity"/> from megabytes, e.g. <c>FromMegabytes(512)</c>.</summary>
    public static RamCapacity FromMegabytes(decimal mb) => Create(mb, DataSizeUnit.Megabyte);

    /// <summary>Creates a <see cref="RamCapacity"/> from megabytes, e.g. <c>FromMegabytes(512)</c>.</summary>
    public static RamCapacity FromMegabytes(int mb) => Create(mb, DataSizeUnit.Megabyte);

    public static bool TryParse(string? input, out RamCapacity? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var size = DataSize.FromGigabytes(bare);
            result = new RamCapacity(size, $"{FormatDecimal(bare)} GB");
            return true;
        }

        if (!DataSize.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Bytes <= 0) return false;

        result = new RamCapacity(parsed);
        return true;
    }

    public static RamCapacity Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid RAM capacity.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>16 GB</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, DataSizeUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.Size.ToString(unit ?? r.Size.OriginalUnit, decimals);
            return r.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Size.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString() => Size.ToNormalizedString();
    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>16 GB</c> instead of <c>16000000000 B</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => Size.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static RamCapacity operator +(RamCapacity a, RamCapacity b) => new(a.Size + b.Size);
    public static RamCapacity operator -(RamCapacity a, RamCapacity b) => new(a.Size - b.Size);
    public static RamCapacity operator *(RamCapacity a, decimal factor) => new(a.Size * factor);
    public static RamCapacity operator *(decimal factor, RamCapacity a) => new(a.Size * factor);
    public static RamCapacity operator /(RamCapacity a, decimal divisor) => new(a.Size / divisor);
    public static RamCapacity operator -(RamCapacity a) => new(-a.Size);

    public int CompareTo(RamCapacity? other) => other is null ? 1 : Size.Bytes.CompareTo(other.Size.Bytes);
    public bool Equals(RamCapacity? other) => other is not null && Size.Bytes == other.Size.Bytes;
    public override bool Equals(object? obj) => obj is RamCapacity other && Equals(other);
    public override int GetHashCode() => Size.Bytes.GetHashCode();
    public static bool operator ==(RamCapacity? a, RamCapacity? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(RamCapacity? a, RamCapacity? b) => !(a == b);
    public static bool operator <(RamCapacity a, RamCapacity b) => a.CompareTo(b) < 0;
    public static bool operator >(RamCapacity a, RamCapacity b) => a.CompareTo(b) > 0;
    public static bool operator <=(RamCapacity a, RamCapacity b) => a.CompareTo(b) <= 0;
    public static bool operator >=(RamCapacity a, RamCapacity b) => a.CompareTo(b) >= 0;
}
