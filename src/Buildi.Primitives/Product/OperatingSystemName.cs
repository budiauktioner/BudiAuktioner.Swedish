using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Canonical operating system name (<c>operativsystem</c>) resolved from common aliases.
/// </summary>
public enum OperatingSystemFamily
{
    Unknown = 0,
    Windows,
    MacOS,
    Linux,
    Android,
    IOS,
    IPadOS,
    ChromeOS,
    WatchOS,
    Other
}

/// <summary>
/// A recognized operating system name, e.g. <c>Windows</c>, <c>macOS</c>, <c>Ubuntu</c>.
/// </summary>
public sealed class OperatingSystemName : IEquatable<OperatingSystemName>, IComparable<OperatingSystemName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Operating System Name", "Operativsystemnamn", "💻", []);

    public string Value { get; }
    public OperatingSystemFamily Family { get; }

    private OperatingSystemName(string value, OperatingSystemFamily family)
    {
        Value = value;
        Family = family;
    }

    private static readonly Dictionary<string, OperatingSystemName> KnownSystems = BuildLookup();

    private static Dictionary<string, OperatingSystemName> BuildLookup()
    {
        var dict = new Dictionary<string, OperatingSystemName>(StringComparer.OrdinalIgnoreCase);
        void Add(string canonical, OperatingSystemFamily family, params string[] aliases)
        {
            var instance = new OperatingSystemName(canonical, family);
            dict[canonical] = instance;
            foreach (var a in aliases) dict.TryAdd(a, instance);
        }

        Add("Windows", OperatingSystemFamily.Windows, "win", "microsoft windows", "ms windows");
        Add("macOS", OperatingSystemFamily.MacOS, "macos", "mac os", "mac os x", "os x", "osx", "mac");
        Add("Linux", OperatingSystemFamily.Linux);
        Add("Ubuntu", OperatingSystemFamily.Linux, "ubuntu linux");
        Add("Debian", OperatingSystemFamily.Linux, "debian linux");
        Add("Fedora", OperatingSystemFamily.Linux, "fedora linux");
        Add("CentOS", OperatingSystemFamily.Linux, "centos linux");
        Add("Red Hat", OperatingSystemFamily.Linux, "rhel", "red hat enterprise linux", "redhat");
        Add("SUSE", OperatingSystemFamily.Linux, "suse linux");
        Add("openSUSE", OperatingSystemFamily.Linux, "opensuse linux");
        Add("Arch Linux", OperatingSystemFamily.Linux, "arch");
        Add("Linux Mint", OperatingSystemFamily.Linux, "mint");
        Add("Manjaro", OperatingSystemFamily.Linux, "manjaro linux");
        Add("Gentoo", OperatingSystemFamily.Linux, "gentoo linux");
        Add("Alpine", OperatingSystemFamily.Linux, "alpine linux");
        Add("Android", OperatingSystemFamily.Android);
        Add("iOS", OperatingSystemFamily.IOS, "iphone os");
        Add("iPadOS", OperatingSystemFamily.IPadOS, "ipad os");
        Add("ChromeOS", OperatingSystemFamily.ChromeOS, "chrome os", "chromeos");
        Add("watchOS", OperatingSystemFamily.WatchOS, "watch os");
        Add("tvOS", OperatingSystemFamily.Other, "tv os");
        Add("FreeBSD", OperatingSystemFamily.Other);
        Add("Unix", OperatingSystemFamily.Other);
        Add("Solaris", OperatingSystemFamily.Other);
        Add("HarmonyOS", OperatingSystemFamily.Other, "harmony os");
        return dict;
    }

    public static bool TryParse(string? input, out OperatingSystemName? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return KnownSystems.TryGetValue(InputSanitization.SanitizeInput(input!).Trim(), out result);
    }

    public static OperatingSystemName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown operating system name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the canonical OS name, e.g. <c>macOS</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the canonical name, e.g. <c>macOS</c>.</summary>
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

    public bool Equals(OperatingSystemName? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is OperatingSystemName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(OperatingSystemName? a, OperatingSystemName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(OperatingSystemName? a, OperatingSystemName? b) => !(a == b);
    public int CompareTo(OperatingSystemName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(OperatingSystemName left, OperatingSystemName right) => left.CompareTo(right) < 0;
    public static bool operator >(OperatingSystemName left, OperatingSystemName right) => left.CompareTo(right) > 0;
    public static bool operator <=(OperatingSystemName left, OperatingSystemName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(OperatingSystemName left, OperatingSystemName right) => left.CompareTo(right) >= 0;
}
