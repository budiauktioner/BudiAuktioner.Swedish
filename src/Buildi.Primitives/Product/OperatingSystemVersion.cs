using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// An operating system version string, e.g. <c>11</c>, <c>14.5</c>, <c>10.15.7</c>, <c>22.04</c>.
/// </summary>
public sealed class OperatingSystemVersion : IEquatable<OperatingSystemVersion>, IComparable<OperatingSystemVersion>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Operating System Version", "Operativsystemversion", "💻", []);

    private static readonly Regex VersionPattern = new(
        @"^\s*v?(?<ver>\d+(?:\.\d+){0,3})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>The version string, e.g. <c>14.5</c>.</summary>
    public string Value { get; }
    public int Major { get; }
    public int? Minor { get; }
    public int? Patch { get; }
    public int? Build { get; }

    private OperatingSystemVersion(string value, int major, int? minor, int? patch, int? build)
    {
        Value = value;
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
    }

    public static bool TryParse(string? input, out OperatingSystemVersion? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = VersionPattern.Match(InputSanitization.SanitizeInput(input!));
        if (!match.Success) return false;

        var parts = match.Groups["ver"].Value.Split('.');
        if (!int.TryParse(parts[0], out var major)) return false;
        int? minor = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : null;
        int? patch = parts.Length > 2 && int.TryParse(parts[2], out var p) ? p : null;
        int? build = parts.Length > 3 && int.TryParse(parts[3], out var b) ? b : null;

        result = new OperatingSystemVersion(match.Groups["ver"].Value, major, minor, patch, build);
        return true;
    }

    public static OperatingSystemVersion Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid OS version.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the canonical version string, e.g. <c>14.5</c>.</summary>
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

    public int CompareTo(OperatingSystemVersion? other)
    {
        if (other is null) return 1;
        var c = Major.CompareTo(other.Major);
        if (c != 0) return c;
        c = (Minor ?? 0).CompareTo(other.Minor ?? 0);
        if (c != 0) return c;
        c = (Patch ?? 0).CompareTo(other.Patch ?? 0);
        if (c != 0) return c;
        return (Build ?? 0).CompareTo(other.Build ?? 0);
    }

    public bool Equals(OperatingSystemVersion? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is OperatingSystemVersion other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(OperatingSystemVersion? a, OperatingSystemVersion? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(OperatingSystemVersion? a, OperatingSystemVersion? b) => !(a == b);
    public static bool operator <(OperatingSystemVersion a, OperatingSystemVersion b) => a.CompareTo(b) < 0;
    public static bool operator >(OperatingSystemVersion a, OperatingSystemVersion b) => a.CompareTo(b) > 0;
    public static bool operator <=(OperatingSystemVersion a, OperatingSystemVersion b) => a.CompareTo(b) <= 0;
    public static bool operator >=(OperatingSystemVersion a, OperatingSystemVersion b) => a.CompareTo(b) >= 0;
}
