using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Product;

/// <summary>
/// Storage capacity for hard drives, SSDs, and USB drives (<c>lagringskapacitet</c>).
/// Bare numbers are interpreted as gigabytes. Delegates to <see cref="DataSize"/> for unit parsing.
/// </summary>
public sealed class StorageCapacity : IEquatable<StorageCapacity>, IComparable<StorageCapacity>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Storage Capacity", "Lagringskapacitet", "💾", []);

    /// <summary>The underlying data size value.</summary>
    public DataSize Size { get; }

    /// <summary>Display form, e.g. <c>512 GB</c>.</summary>
    public string Value { get; }

    public decimal Bytes => Size.Bytes;
    public decimal Gigabytes => Size.Gigabytes;
    public decimal Terabytes => Size.Terabytes;

    private StorageCapacity(DataSize size)
    {
        Size = size;
        Value = size.ToString();
    }

    private StorageCapacity(DataSize size, string displayValue)
    {
        Size = size;
        Value = displayValue;
    }

    /// <summary>Creates a <see cref="StorageCapacity"/> from a numeric value and data size unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static StorageCapacity Create(decimal value, DataSizeUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Storage capacity must be positive.");
        var size = DataSize.Create(value, unit);
        return new StorageCapacity(size);
    }

    /// <summary>Creates a <see cref="StorageCapacity"/> from a numeric value and data size unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static StorageCapacity Create(int value, DataSizeUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates a <see cref="StorageCapacity"/> from gigabytes, e.g. <c>FromGigabytes(512)</c>.</summary>
    public static StorageCapacity FromGigabytes(decimal gb) => Create(gb, DataSizeUnit.Gigabyte);

    /// <summary>Creates a <see cref="StorageCapacity"/> from gigabytes, e.g. <c>FromGigabytes(512)</c>.</summary>
    public static StorageCapacity FromGigabytes(int gb) => Create(gb, DataSizeUnit.Gigabyte);

    /// <summary>Creates a <see cref="StorageCapacity"/> from terabytes, e.g. <c>FromTerabytes(1)</c>.</summary>
    public static StorageCapacity FromTerabytes(decimal tb) => Create(tb, DataSizeUnit.Terabyte);

    /// <summary>Creates a <see cref="StorageCapacity"/> from terabytes, e.g. <c>FromTerabytes(1)</c>.</summary>
    public static StorageCapacity FromTerabytes(int tb) => Create(tb, DataSizeUnit.Terabyte);

    /// <summary>Creates a <see cref="StorageCapacity"/> from megabytes, e.g. <c>FromMegabytes(256)</c>.</summary>
    public static StorageCapacity FromMegabytes(decimal mb) => Create(mb, DataSizeUnit.Megabyte);

    /// <summary>Creates a <see cref="StorageCapacity"/> from megabytes, e.g. <c>FromMegabytes(256)</c>.</summary>
    public static StorageCapacity FromMegabytes(int mb) => Create(mb, DataSizeUnit.Megabyte);

    public static bool TryParse(string? input, out StorageCapacity? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var size = DataSize.FromGigabytes(bare);
            result = new StorageCapacity(size, $"{FormatDecimal(bare)} GB");
            return true;
        }

        if (!DataSize.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Bytes <= 0) return false;

        result = new StorageCapacity(parsed);
        return true;
    }

    public static StorageCapacity Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid storage capacity.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>512 GB</c>.</summary>
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

    /// <summary>Returns normalized form in bytes, e.g. <c>512000000000 B</c>.</summary>
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
    /// Returns the value formatted in the most human-readable unit, e.g. <c>512 GB</c> instead of <c>512000000000 B</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => Size.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static StorageCapacity operator +(StorageCapacity a, StorageCapacity b) => new(a.Size + b.Size);
    public static StorageCapacity operator -(StorageCapacity a, StorageCapacity b) => new(a.Size - b.Size);
    public static StorageCapacity operator *(StorageCapacity a, decimal factor) => new(a.Size * factor);
    public static StorageCapacity operator *(decimal factor, StorageCapacity a) => new(a.Size * factor);
    public static StorageCapacity operator /(StorageCapacity a, decimal divisor) => new(a.Size / divisor);
    public static StorageCapacity operator -(StorageCapacity a) => new(-a.Size);

    public int CompareTo(StorageCapacity? other) => other is null ? 1 : Size.Bytes.CompareTo(other.Size.Bytes);
    public bool Equals(StorageCapacity? other) => other is not null && Size.Bytes == other.Size.Bytes;
    public override bool Equals(object? obj) => obj is StorageCapacity other && Equals(other);
    public override int GetHashCode() => Size.Bytes.GetHashCode();
    public static bool operator ==(StorageCapacity? a, StorageCapacity? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(StorageCapacity? a, StorageCapacity? b) => !(a == b);
    public static bool operator <(StorageCapacity a, StorageCapacity b) => a.CompareTo(b) < 0;
    public static bool operator >(StorageCapacity a, StorageCapacity b) => a.CompareTo(b) > 0;
    public static bool operator <=(StorageCapacity a, StorageCapacity b) => a.CompareTo(b) <= 0;
    public static bool operator >=(StorageCapacity a, StorageCapacity b) => a.CompareTo(b) >= 0;
}
