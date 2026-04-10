using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Generic shoe size (<c>skostorlek</c>) that wraps either <see cref="AdultShoeSize"/> or <see cref="ChildShoeSize"/>, chosen by parsing heuristics.
/// </summary>
/// <remarks>
/// <para>Explicit <c>adult</c>/<c>vuxen</c> or <c>child</c>/<c>barn</c> prefixes force the corresponding parser. US sizes with a <c>C</c> or <c>Y</c> suffix are parsed as children’s first; otherwise adult is tried before child (so EU 35–39 without a prefix defaults to adult when valid).</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/83106.html">ISO 19407:2023</see> — Footwear sizing — Conversion of sizing systems</description></item>
/// <item><description><see href="https://www.iso.org/standard/71594.html">ISO 9407:2019</see> — Footwear sizing — Mondopoint system</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Shoe_size">Wikipedia — Shoe size</see> — regional systems</description></item>
/// </list>
/// </remarks>
public sealed class ShoeSize : IEquatable<ShoeSize>, IComparable<ShoeSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Shoe Size", "Skostorlek", "👟", ["https://www.iso.org/standard/83106.html", "https://www.iso.org/standard/71594.html", "https://en.wikipedia.org/wiki/Shoe_size"]);

    private static readonly Regex RxAdultPrefix = new(
        @"^\s*(adult|vuxen)\s+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxChildPrefix = new(
        @"^\s*(child|barn)\s+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUsChildHint = new(
        @"\bUS\s+[0-9]+(?:[.,][0-9]+)?\s*[CY]\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly AdultShoeSize? _adult;
    private readonly ChildShoeSize? _child;

    /// <summary>Returns <see langword="true"/> when the value is an <see cref="AdultShoeSize"/>.</summary>
    public bool IsAdult => _adult is not null;

    /// <summary>Returns <see langword="true"/> when the value is a <see cref="ChildShoeSize"/>.</summary>
    public bool IsChild => _child is not null;

    /// <summary>Display form, e.g. <c>EU 42</c> or <c>EU 28</c>.</summary>
    public string Value => _adult?.Value ?? _child!.Value;

    private ShoeSize(AdultShoeSize? adult, ChildShoeSize? child)
    {
        if (adult is null == (child is null))
            throw new ArgumentException("Exactly one of adult or child must be non-null.", nameof(adult));

        _adult = adult;
        _child = child;
    }

    /// <summary>Returns the adult size, or <see langword="null"/> when <see cref="IsChild"/>.</summary>
    public AdultShoeSize? AsAdult() => _adult;

    /// <summary>Returns the child size, or <see langword="null"/> when <see cref="IsAdult"/>.</summary>
    public ChildShoeSize? AsChild() => _child;

    public static bool TryParse(string? input, out ShoeSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();

        var adultPref = RxAdultPrefix.Match(s);
        if (adultPref.Success)
        {
            var remainder = s[adultPref.Length..].Trim();
            if (AdultShoeSize.TryParse(remainder, out var a) && a is not null)
            {
                result = new ShoeSize(a, null);
                return true;
            }

            return false;
        }

        var childPref = RxChildPrefix.Match(s);
        if (childPref.Success)
        {
            var remainder = s[childPref.Length..].Trim();
            if (ChildShoeSize.TryParse(remainder, out var c) && c is not null)
            {
                result = new ShoeSize(null, c);
                return true;
            }

            return false;
        }

        if (RxUsChildHint.IsMatch(s))
        {
            if (ChildShoeSize.TryParse(s, out var cUs) && cUs is not null)
            {
                result = new ShoeSize(null, cUs);
                return true;
            }

            if (AdultShoeSize.TryParse(s, out var aUs) && aUs is not null)
            {
                result = new ShoeSize(aUs, null);
                return true;
            }

            return false;
        }

        if (AdultShoeSize.TryParse(s, out var adult) && adult is not null)
        {
            result = new ShoeSize(adult, null);
            return true;
        }

        if (ChildShoeSize.TryParse(s, out var child) && child is not null)
        {
            result = new ShoeSize(null, child);
            return true;
        }

        return false;
    }

    public static ShoeSize Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid shoe size.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns canonical EU display, e.g. <c>EU 42</c> or <c>EU 28</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns canonical <c>EU {size}</c>, e.g. <c>EU 42</c> — same as <see cref="Format(string?, bool)"/> for this wrapper.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns <c>EU 42</c> or <c>EU 28</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns <c>EU 42</c> or <c>EU 28</c>.</summary>
    public override string ToString() => Value;

    public static bool operator ==(ShoeSize? a, ShoeSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ShoeSize? a, ShoeSize? b) => !(a == b);
    public int CompareTo(ShoeSize? other)
    {
        if (other is null) return 1;
        if (IsChild && other.IsChild) return _child!.CompareTo(other._child);
        if (IsAdult && other.IsAdult) return _adult!.CompareTo(other._adult);
        return IsChild ? -1 : 1;
    }
    public static bool operator <(ShoeSize left, ShoeSize right) => left.CompareTo(right) < 0;
    public static bool operator >(ShoeSize left, ShoeSize right) => left.CompareTo(right) > 0;
    public static bool operator <=(ShoeSize left, ShoeSize right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ShoeSize left, ShoeSize right) => left.CompareTo(right) >= 0;

    public bool Equals(ShoeSize? other)
    {
        if (other is null) return false;
        if (IsAdult && other.IsAdult) return AsAdult()!.Equals(other.AsAdult());
        if (IsChild && other.IsChild) return AsChild()!.Equals(other.AsChild());
        return false;
    }
    public override bool Equals(object? obj) => obj is ShoeSize other && Equals(other);
    public override int GetHashCode() => ToNormalizedString().GetHashCode();
}
