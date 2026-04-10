using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Generic clothing size (<c>klädstorlek</c>) that wraps either <see cref="AdultClothingSize"/> or
/// <see cref="ChildClothingSize"/>, auto-detecting from the input or optional <c>adult</c>/<c>vuxen</c> /
/// <c>child</c>/<c>barn</c> prefixes.
/// </summary>
/// <remarks>
/// <para>Letter sizes (e.g. <c>M</c>) and adult EU/US/UK ranges are resolved as adult; child height
/// centimetres (e.g. <c>128</c>, <c>EU 128</c>) as child when adult parsing does not apply.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/EN_13402">EN 13402</see> — European standard for size designation of clothes</description></item>
/// <item><description><see cref="AdultClothingSize"/> and <see cref="ChildClothingSize"/> — see those types for full references</description></item>
/// </list>
/// </remarks>
public sealed class ClothingSize : IEquatable<ClothingSize>, IComparable<ClothingSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Clothing Size", "Klädstorlek", "👕", ["https://en.wikipedia.org/wiki/EN_13402"]);

    private readonly AdultClothingSize? _adult;
    private readonly ChildClothingSize? _child;

    private ClothingSize(AdultClothingSize? adult, ChildClothingSize? child)
    {
        if (adult is null == (child is null))
            throw new ArgumentException("Exactly one of adult or child must be non-null.", nameof(adult));

        _adult = adult;
        _child = child;
    }

    /// <summary>Returns <see langword="true"/> when this instance wraps an <see cref="AdultClothingSize"/>.</summary>
    public bool IsAdult => _adult is not null;

    /// <summary>Returns <see langword="true"/> when this instance wraps a <see cref="ChildClothingSize"/>.</summary>
    public bool IsChild => _child is not null;

    /// <summary>Display form: same as underlying adult <see cref="AdultClothingSize.Value"/> or child <see cref="ChildClothingSize.ToString"/> (e.g. <c>L</c> or <c>EU 128</c>).</summary>
    public string Value => _adult?.Value ?? _child!.ToString();

    /// <summary>Returns the adult size when <see cref="IsAdult"/>; otherwise <see langword="null"/>.</summary>
    public AdultClothingSize? AsAdult() => _adult;

    /// <summary>Returns the child size when <see cref="IsChild"/>; otherwise <see langword="null"/>.</summary>
    public ChildClothingSize? AsChild() => _child;

    public static bool TryParse(string? input, out ClothingSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();
        if (s.Length == 0) return false;

        if (TryStripAdultPrefix(s, out var adultBody))
        {
            if (AdultClothingSize.TryParse(adultBody, out var adult) && adult is not null)
            {
                result = new ClothingSize(adult, null);
                return true;
            }

            return false;
        }

        if (TryStripChildPrefix(s, out var childBody))
        {
            if (ChildClothingSize.TryParse(childBody, out var child) && child is not null)
            {
                result = new ClothingSize(null, child);
                return true;
            }

            return false;
        }

        if (AdultClothingSize.TryParse(s, out var a) && a is not null)
        {
            result = new ClothingSize(a, null);
            return true;
        }

        if (ChildClothingSize.TryParse(s, out var c) && c is not null)
        {
            result = new ClothingSize(null, c);
            return true;
        }

        return false;
    }

    public static ClothingSize Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid clothing size.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the underlying formatted display (e.g. <c>L</c> or <c>EU 128</c>). Returns <see langword="null"/> when invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var s = input.Trim();

        if (TryStripAdultPrefix(s, out var adultRem))
            return AdultClothingSize.Format(adultRem, fallbackToTrimmedInputWhenInvalid);

        if (TryStripChildPrefix(s, out var childRem))
            return ChildClothingSize.Format(childRem, fallbackToTrimmedInputWhenInvalid);

        var adultFormatted = AdultClothingSize.Format(s, fallbackToTrimmedInputWhenInvalid);
        if (adultFormatted is not null)
            return adultFormatted;

        return ChildClothingSize.Format(s, fallbackToTrimmedInputWhenInvalid);
    }

    /// <summary>Returns canonical EU form for the resolved kind, e.g. <c>EU 40</c> or <c>EU 128</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var s = input.Trim();

        if (TryStripAdultPrefix(s, out var adultRem))
            return AdultClothingSize.Normalize(adultRem);

        if (TryStripChildPrefix(s, out var childRem))
            return ChildClothingSize.Normalize(childRem);

        var adultNorm = AdultClothingSize.Normalize(s);
        if (adultNorm is not null)
            return adultNorm;

        var childNorm = ChildClothingSize.Normalize(s);
        if (childNorm is not null) return childNorm;
        if (!fallbackToTrimmedInputWhenInvalid) return null;
        return s.Length > 0 ? s : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/> for the resolved kind.</summary>
    public static bool IsNormalized(string? input)
    {
        if (!TryParse(input, out var cs) || cs is null) return false;

        var trimmed = input!.Trim();
        return cs.IsAdult
            ? AdultClothingSize.IsNormalized(trimmed)
            : ChildClothingSize.IsNormalized(trimmed);
    }

    /// <summary>Returns canonical EU form, e.g. <c>EU 40</c> or <c>EU 128</c>.</summary>
    public string ToNormalizedString() => _adult?.ToNormalizedString() ?? _child!.ToNormalizedString();

    /// <summary>Returns the display form (same as <see cref="Value"/>).</summary>
    public override string ToString() => Value;

    private static bool TryStripAdultPrefix(string s, out string remainder)
    {
        if (TryStripPrefix(s, "adult", out remainder) || TryStripPrefix(s, "vuxen", out remainder))
            return true;

        remainder = string.Empty;
        return false;
    }

    private static bool TryStripChildPrefix(string s, out string remainder)
    {
        if (TryStripPrefix(s, "child", out remainder) || TryStripPrefix(s, "barn", out remainder))
            return true;

        remainder = string.Empty;
        return false;
    }

    /// <summary>
    /// Strips <paramref name="prefix"/> when it appears as a whole word at the start (case-insensitive),
    /// followed by end-of-string or whitespace before the rest.
    /// </summary>
    private static bool TryStripPrefix(string s, string prefix, out string remainder)
    {
        remainder = string.Empty;
        if (s.Length < prefix.Length || !s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (s.Length == prefix.Length)
        {
            remainder = string.Empty;
            return true;
        }

        if (!char.IsWhiteSpace(s[prefix.Length]))
            return false;

        remainder = s.AsSpan(prefix.Length).TrimStart().ToString();
        return true;
    }

    public static bool operator ==(ClothingSize? a, ClothingSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ClothingSize? a, ClothingSize? b) => !(a == b);
    public int CompareTo(ClothingSize? other)
    {
        if (other is null) return 1;
        if (IsChild && other.IsChild) return _child!.CompareTo(other._child);
        if (IsAdult && other.IsAdult) return _adult!.CompareTo(other._adult);
        return IsChild ? -1 : 1;
    }
    public static bool operator <(ClothingSize left, ClothingSize right) => left.CompareTo(right) < 0;
    public static bool operator >(ClothingSize left, ClothingSize right) => left.CompareTo(right) > 0;
    public static bool operator <=(ClothingSize left, ClothingSize right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ClothingSize left, ClothingSize right) => left.CompareTo(right) >= 0;

    public bool Equals(ClothingSize? other)
    {
        if (other is null) return false;
        if (IsAdult && other.IsAdult) return AsAdult()!.Equals(other.AsAdult());
        if (IsChild && other.IsChild) return AsChild()!.Equals(other.AsChild());
        return false;
    }
    public override bool Equals(object? obj) => obj is ClothingSize other && Equals(other);
    public override int GetHashCode() => ToNormalizedString().GetHashCode();
}
