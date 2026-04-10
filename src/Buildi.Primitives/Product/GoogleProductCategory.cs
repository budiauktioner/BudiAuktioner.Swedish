using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// A Google product category path used to classify products in Google Shopping and Merchant Center.
/// Categories follow a hierarchical taxonomy expressed as <c>&gt;</c>-separated segments,
/// e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies &gt; Bird Supplies</c>.
/// The taxonomy is maintained by Google and updated periodically (current version: 2021-09-21).
/// This type validates and normalizes the path structure without verifying against the full taxonomy.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://support.google.com/merchants/answer/6324436">Google Merchant Center — google_product_category</see> — product data specification</description></item>
/// <item><description><see href="https://www.google.com/basepages/producttype/taxonomy.en-US.txt">Google Product Taxonomy</see> — full category list (en-US)</description></item>
/// <item><description><see href="https://support.google.com/merchants/answer/160081">Google Merchant Center — Supported product categories</see></description></item>
/// </list>
/// </remarks>
public sealed class GoogleProductCategory : IEquatable<GoogleProductCategory>, IComparable<GoogleProductCategory>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Google Product Category", "Google-produktkategori", "🏷️", ["https://support.google.com/merchants/answer/6324436", "https://www.google.com/basepages/producttype/taxonomy.en-US.txt", "https://support.google.com/merchants/answer/160081"]);

    private const int MaxInputLength = 1000;

    /// <summary>The full category path in canonical form, e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies &gt; Bird Supplies</c>.</summary>
    public string Path { get; }

    /// <summary>The individual category segments, e.g. <c>["Animals &amp; Pet Supplies", "Pet Supplies", "Bird Supplies"]</c>.</summary>
    public IReadOnlyList<string> Segments { get; }

    /// <summary>The number of hierarchy levels (1 = root category only).</summary>
    public int Depth { get; }

    /// <summary>The top-level category (first segment), e.g. <c>Animals &amp; Pet Supplies</c>.</summary>
    public string RootCategory { get; }

    /// <summary>The most specific category (last segment), e.g. <c>Bird Supplies</c>.</summary>
    public string LeafCategory { get; }

    private GoogleProductCategory(string path, string[] segments)
    {
        Path = path;
        Segments = segments;
        Depth = segments.Length;
        RootCategory = segments[0];
        LeafCategory = segments[^1];
    }

    public static bool TryParse(string? input, out GoogleProductCategory? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength) return false;

        var parts = input.Split('>');
        var segments = new string[parts.Length];

        for (var i = 0; i < parts.Length; i++)
        {
            var trimmed = parts[i].Trim();
            if (trimmed.Length == 0) return false;
            segments[i] = trimmed;
        }

        var path = string.Join(" > ", segments);

        result = new GoogleProductCategory(path, segments);
        return true;
    }

    public static GoogleProductCategory Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Google product category.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the category path in canonical form with <c> &gt; </c> separators,
    /// e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Path : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the category path in canonical form with <c> &gt; </c> separators,
    /// e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Path;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the category path in canonical form, e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies</c>.</summary>
    public string ToNormalizedString() => Path;

    /// <summary>Returns the category path in canonical form, e.g. <c>Animals &amp; Pet Supplies &gt; Pet Supplies</c>.</summary>
    public override string ToString() => Path;

    public bool Equals(GoogleProductCategory? other) => other is not null && Path == other.Path;
    public override bool Equals(object? obj) => obj is GoogleProductCategory other && Equals(other);
    public override int GetHashCode() => Path.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(GoogleProductCategory? a, GoogleProductCategory? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(GoogleProductCategory? a, GoogleProductCategory? b) => !(a == b);
    public int CompareTo(GoogleProductCategory? other)
    {
        if (other is null) return 1;
        var len = Math.Min(Segments.Count, other.Segments.Count);
        for (var i = 0; i < len; i++)
        {
            var c = string.Compare(Segments[i], other.Segments[i], StringComparison.Ordinal);
            if (c != 0) return c;
        }
        return Segments.Count.CompareTo(other.Segments.Count);
    }
    public static bool operator <(GoogleProductCategory left, GoogleProductCategory right) => left.CompareTo(right) < 0;
    public static bool operator >(GoogleProductCategory left, GoogleProductCategory right) => left.CompareTo(right) > 0;
    public static bool operator <=(GoogleProductCategory left, GoogleProductCategory right) => left.CompareTo(right) <= 0;
    public static bool operator >=(GoogleProductCategory left, GoogleProductCategory right) => left.CompareTo(right) >= 0;
}
