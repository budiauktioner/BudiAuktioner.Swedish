namespace Buildi.Primitives.Property;

/// <summary>
/// Extension methods for masking property designation information in display strings.
/// </summary>
public static class PropertyMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked property designation preserving the name but hiding the register number,
    /// e.g. <c>Stockholm Söder 75:2</c> → <c>Stockholm Söder **:*</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishPropertyDesignation designation)
    {
        var block = new string(MaskChar, designation.BlockNumber.ToString().Length);
        var unit = new string(MaskChar, designation.UnitNumber.ToString().Length);
        return $"{designation.DesignationName} {block}:{unit}";
    }
}
