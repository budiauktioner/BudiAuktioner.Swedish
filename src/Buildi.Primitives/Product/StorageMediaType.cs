using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Storage media technology (<c>lagringsmedia</c>) used in computers, laptops, phones, and consumer
/// electronics, e.g. <c>SSD</c>, <c>HDD</c>, <c>NVMe</c>, <c>eMMC</c>, <c>UFS</c>, <c>Flash</c>.
/// </summary>
/// <remarks>
/// <para>Captures the physical storage media technology of a device, complementing
/// <see cref="StorageCapacity"/> (the size of that storage) and <see cref="RamCapacity"/>.
/// Each entry exposes its storage <see cref="Family"/> (HDD/SSD/Flash/Hybrid) and
/// <see cref="IsSolidState"/> flag. Recognises Swedish synonyms such as <c>Hårddisk</c> and
/// <c>Flashminne</c>, plus common interface aliases like <c>Solid State Drive</c>,
/// <c>Hard Disk Drive</c>, and <c>PCIe SSD</c>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Computer_data_storage">Wikipedia — Computer data storage</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/NVM_Express">Wikipedia — NVM Express</see></description></item>
/// </list>
/// </remarks>
public sealed class StorageMediaType : IEquatable<StorageMediaType>, IComparable<StorageMediaType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Storage Media Type",
        "Lagringsmedia",
        "💽",
        ["https://en.wikipedia.org/wiki/Computer_data_storage", "https://en.wikipedia.org/wiki/NVM_Express"]);

    private static readonly Lazy<Dictionary<string, StorageMediaType>> Lookup = new(BuildLookup);

    private readonly int _order;

    /// <summary>Canonical short identifier, e.g. <c>SSD</c>, <c>NVMe</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Solid-state drive</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>SSD-disk</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>
    /// Broader storage family, one of <c>HDD</c>, <c>SSD</c>, <c>Flash</c>, <c>Hybrid</c>.
    /// </summary>
    public string Family { get; }

    /// <summary>
    /// Returns <see langword="true"/> for non-mechanical (solid-state / flash-based) storage.
    /// </summary>
    public bool IsSolidState { get; }

    public static readonly StorageMediaType Hdd     = new("HDD",     "Hard disk drive",   "Hårddisk (HDD)",        "HDD",    isSolidState: false, 0);
    public static readonly StorageMediaType Sshd    = new("SSHD",    "Solid-state hybrid drive", "Hybriddisk (SSHD)", "Hybrid", isSolidState: false, 1);
    public static readonly StorageMediaType Ssd     = new("SSD",     "Solid-state drive", "SSD-disk",              "SSD",    isSolidState: true,  2);
    public static readonly StorageMediaType Nvme    = new("NVMe",    "NVMe SSD",          "NVMe SSD",              "SSD",    isSolidState: true,  3);
    public static readonly StorageMediaType EMmc    = new("eMMC",    "eMMC",              "eMMC",                  "Flash",  isSolidState: true,  4);
    public static readonly StorageMediaType Ufs     = new("UFS",     "Universal Flash Storage", "UFS",             "Flash",  isSolidState: true,  5);
    public static readonly StorageMediaType Flash   = new("Flash",   "Flash storage",     "Flashminne",            "Flash",  isSolidState: true,  6);
    public static readonly StorageMediaType Optane  = new("Optane",  "Intel Optane",      "Intel Optane",          "SSD",    isSolidState: true,  7);

    /// <summary>All predefined storage media types.</summary>
    public static IReadOnlyList<StorageMediaType> All { get; } =
    [
        Hdd, Sshd, Ssd, Nvme, EMmc, Ufs, Flash, Optane
    ];

    private StorageMediaType(string value, string englishName, string localizedName, string family, bool isSolidState, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        Family = family;
        IsSolidState = isSolidState;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a storage media type from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out StorageMediaType? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var v))
        {
            result = v;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Parses a storage media type. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static StorageMediaType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid storage media type.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized storage media type.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>SSD-disk</c> (Swedish) or
    /// <c>Solid-state drive</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical short identifier, e.g. <c>SSD</c>, <c>NVMe</c>, <c>eMMC</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical short identifier, e.g. <c>SSD</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/().]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, StorageMediaType> d, StorageMediaType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, StorageMediaType> BuildLookup()
    {
        var d = new Dictionary<string, StorageMediaType>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in All)
        {
            AddKey(d, s, s.Value);
            AddKey(d, s, s.EnglishName);
            AddKey(d, s, s.LocalizedName);
        }

        AddKey(d, Hdd, "Hard disk");
        AddKey(d, Hdd, "Hard drive");
        AddKey(d, Hdd, "Hårddisk");
        AddKey(d, Hdd, "Mekanisk hårddisk");
        AddKey(d, Hdd, "Mechanical drive");
        AddKey(d, Hdd, "Spinning disk");
        AddKey(d, Hdd, "Spinning drive");
        AddKey(d, Hdd, "Magnetic disk");
        AddKey(d, Hdd, "Magnetisk disk");
        AddKey(d, Hdd, "Rotational drive");

        AddKey(d, Sshd, "SSHD-disk");
        AddKey(d, Sshd, "Hybriddisk");
        AddKey(d, Sshd, "Hybrid drive");
        AddKey(d, Sshd, "Hybrid hard drive");
        AddKey(d, Sshd, "SSD/HDD hybrid");

        AddKey(d, Ssd, "Solid state");
        AddKey(d, Ssd, "Solid state drive");
        AddKey(d, Ssd, "SATA SSD");
        AddKey(d, Ssd, "M.2 SATA");
        AddKey(d, Ssd, "M.2 SATA SSD");

        AddKey(d, Nvme, "NVM Express");
        AddKey(d, Nvme, "NVMe SSD");
        AddKey(d, Nvme, "PCIe SSD");
        AddKey(d, Nvme, "PCI Express SSD");
        AddKey(d, Nvme, "M.2 NVMe");
        AddKey(d, Nvme, "M.2 NVMe SSD");
        AddKey(d, Nvme, "Gen3 NVMe");
        AddKey(d, Nvme, "Gen4 NVMe");
        AddKey(d, Nvme, "Gen5 NVMe");

        AddKey(d, EMmc, "Embedded MMC");
        AddKey(d, EMmc, "Embedded MultiMediaCard");
        AddKey(d, EMmc, "e-MMC");

        AddKey(d, Ufs, "Universal Flash Storage");
        AddKey(d, Ufs, "UFS 2.1");
        AddKey(d, Ufs, "UFS 3.0");
        AddKey(d, Ufs, "UFS 3.1");
        AddKey(d, Ufs, "UFS 4.0");

        AddKey(d, Flash, "Flash memory");
        AddKey(d, Flash, "NAND flash");
        AddKey(d, Flash, "Flash drive");
        AddKey(d, Flash, "Flash storage");
        AddKey(d, Flash, "Flashlagring");
        AddKey(d, Flash, "Flash-minne");

        AddKey(d, Optane, "Optane memory");
        AddKey(d, Optane, "3D XPoint");
        AddKey(d, Optane, "XPoint");

        return d;
    }

    public static bool operator ==(StorageMediaType? a, StorageMediaType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(StorageMediaType? a, StorageMediaType? b) => !(a == b);

    public bool Equals(StorageMediaType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is StorageMediaType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(StorageMediaType? other) =>
        other is null ? 1 : _order.CompareTo(other._order);

    public static bool operator <(StorageMediaType a, StorageMediaType b) => a.CompareTo(b) < 0;
    public static bool operator >(StorageMediaType a, StorageMediaType b) => a.CompareTo(b) > 0;
    public static bool operator <=(StorageMediaType a, StorageMediaType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(StorageMediaType a, StorageMediaType b) => a.CompareTo(b) >= 0;
}
