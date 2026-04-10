using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// A combined operating system name and version, e.g. <c>Windows 11</c>, <c>macOS 14.5</c>, <c>Ubuntu 22.04</c>.
/// </summary>
public sealed class OperatingSystemInfo : IEquatable<OperatingSystemInfo>, IComparable<OperatingSystemInfo>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Operating System", "Operativsystem", "💻", []);

    /// <summary>The operating system name.</summary>
    public OperatingSystemName Name { get; }

    /// <summary>The version, if present.</summary>
    public OperatingSystemVersion? Version { get; }

    /// <summary>Display string, e.g. <c>Windows 11</c>.</summary>
    public string Value { get; }

    private OperatingSystemInfo(OperatingSystemName name, OperatingSystemVersion? version)
    {
        Name = name;
        Version = version;
        Value = version is not null ? $"{name.Value} {version.Value}" : name.Value;
    }

    public static bool TryParse(string? input, out OperatingSystemInfo? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        // Try to split off trailing version: greedily match the longest OS name prefix
        // Strategy: try progressively shorter prefixes until we find a known OS name
        for (var i = trimmed.Length; i >= 1; i--)
        {
            var nameCandidate = trimmed[..i].TrimEnd();
            if (!OperatingSystemName.TryParse(nameCandidate, out var osName) || osName is null)
                continue;

            var rest = trimmed[i..].Trim();
            if (rest.Length == 0)
            {
                result = new OperatingSystemInfo(osName, null);
                return true;
            }

            if (OperatingSystemVersion.TryParse(rest, out var ver))
            {
                result = new OperatingSystemInfo(osName, ver);
                return true;
            }
        }

        return false;
    }

    public static OperatingSystemInfo Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid operating system.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the canonical display form, e.g. <c>Windows 11</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        var result = Format(input);
        if (result is not null) return result;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString() => Value;
    public override string ToString() => Value;

    public bool Equals(OperatingSystemInfo? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is OperatingSystemInfo other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(OperatingSystemInfo? a, OperatingSystemInfo? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(OperatingSystemInfo? a, OperatingSystemInfo? b) => !(a == b);
    public int CompareTo(OperatingSystemInfo? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(OperatingSystemInfo left, OperatingSystemInfo right) => left.CompareTo(right) < 0;
    public static bool operator >(OperatingSystemInfo left, OperatingSystemInfo right) => left.CompareTo(right) > 0;
    public static bool operator <=(OperatingSystemInfo left, OperatingSystemInfo right) => left.CompareTo(right) <= 0;
    public static bool operator >=(OperatingSystemInfo left, OperatingSystemInfo right) => left.CompareTo(right) >= 0;
}
