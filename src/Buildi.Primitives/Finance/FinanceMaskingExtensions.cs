namespace Buildi.Primitives.Finance;

/// <summary>
/// Extension methods for masking financial information in display strings.
/// </summary>
public static class FinanceMaskingExtensions
{
    private const char MaskChar = '*';
    private const string MaskedNumber = "***";

    /// <summary>
    /// Returns a masked money amount preserving the currency but hiding the value,
    /// e.g. <c>1 000,00 SEK</c> → <c>*** SEK</c>.
    /// </summary>
    public static string ToMaskedString(this MoneyAmount amount) =>
        $"{MaskedNumber} {amount.Currency.Code}";

    /// <summary>
    /// Returns a masked ISIN showing the country code but masking the NSIN and check digit,
    /// e.g. <c>SE0000108656</c> → <c>SE**********</c>.
    /// </summary>
    public static string ToMaskedString(this Isin isin) =>
        $"{isin.CountryCode}{new string(MaskChar, isin.Value.Length - 2)}";
}
