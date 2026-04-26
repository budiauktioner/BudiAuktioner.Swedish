namespace Buildi.Primitives.Product;

/// <summary>
/// Extension methods for masking product identifiers in display strings.
/// </summary>
public static class ProductMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked GTIN-13 showing the GS1 prefix and masking the rest,
    /// e.g. <c>5901234123457</c> → <c>590**********</c>.
    /// </summary>
    public static string ToMaskedString(this Gtin13 gtin)
    {
        var prefix = gtin.Gs1Prefix;
        return $"{prefix}{new string(MaskChar, gtin.Digits.Length - prefix.Length)}";
    }

    /// <summary>
    /// Returns a masked GTIN-8, e.g. <c>96385074</c> → <c>********</c>.
    /// </summary>
    public static string ToMaskedString(this Gtin8 gtin) =>
        new string(MaskChar, gtin.Digits.Length);

    /// <summary>
    /// Returns a masked IP rating, e.g. <c>IP65</c> → <c>IP**</c>.
    /// </summary>
    public static string ToMaskedString(this IpRating ip) =>
        $"IP{MaskChar}{MaskChar}";

    /// <summary>
    /// Returns a masked GTIN-12, e.g. <c>614141000036</c> → <c>************</c>.
    /// </summary>
    public static string ToMaskedString(this Gtin12 gtin) =>
        new string(MaskChar, gtin.Digits.Length);

    /// <summary>
    /// Returns a masked GTIN-14, e.g. <c>10614141000415</c> → <c>**************</c>.
    /// </summary>
    public static string ToMaskedString(this Gtin14 gtin) =>
        new string(MaskChar, gtin.Digits.Length);

    /// <summary>
    /// Returns a masked electrical phase, e.g. <c>3-phase</c> → <c>*-phase</c>.
    /// </summary>
    public static string ToMaskedString(this ElectricalPhase ep) =>
        $"{new string(MaskChar, ep.PhaseCount.ToString().Length)}-phase";

    /// <summary>
    /// Returns a masked refrigerant designation showing the R prefix and masking the rest,
    /// e.g. <c>R-134a</c> → <c>R-****</c>.
    /// </summary>
    public static string ToMaskedString(this Refrigerant refrigerant)
    {
        var v = refrigerant.Value;
        if (v.StartsWith("R-", StringComparison.OrdinalIgnoreCase))
            return $"R-{new string(MaskChar, v.Length - 2)}";
        return $"R{new string(MaskChar, Math.Max(0, v.Length - 1))}";
    }

    /// <summary>
    /// Returns a masked aspect ratio, e.g. <c>16:9</c> → <c>**:*</c>.
    /// </summary>
    public static string ToMaskedString(this AspectRatio ratio) =>
        $"{new string(MaskChar, ratio.Width.ToString().Length)}:{new string(MaskChar, ratio.Height.ToString().Length)}";

    /// <summary>
    /// Returns a masked battery chemistry, e.g. <c>Li-ion</c> → <c>******</c>.
    /// </summary>
    public static string ToMaskedString(this BatteryChemistry chemistry) =>
        new(MaskChar, chemistry.Value.Length);
}
