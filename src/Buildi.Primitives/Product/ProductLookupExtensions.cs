namespace Buildi.Primitives.Product;

/// <summary>
/// Extension methods that produce public lookup URLs for product identifiers.
/// </summary>
public static class ProductLookupExtensions
{
    /// <summary>
    /// Returns a BarcodeLookup.com URL, e.g.
    /// <c>https://www.barcodelookup.com/5901234123457</c>.
    /// </summary>
    public static string ToBarcodeLookupUrl(this Gtin13 gtin) =>
        $"https://www.barcodelookup.com/{gtin.ToNormalizedString()}";

    /// <summary>
    /// Returns a BarcodeLookup.com URL, e.g.
    /// <c>https://www.barcodelookup.com/96385074</c>.
    /// </summary>
    public static string ToBarcodeLookupUrl(this Gtin8 gtin) =>
        $"https://www.barcodelookup.com/{gtin.ToNormalizedString()}";
}
