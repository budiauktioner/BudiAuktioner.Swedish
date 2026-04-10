namespace Buildi.Primitives.Banking;

/// <summary>
/// Extension methods that generate lookup URLs for banking-related types.
/// </summary>
public static class BankingExternalLinkExtensions
{
    /// <summary>
    /// Returns the Bankgirot search URL for this Bankgiro number,
    /// e.g. <c>https://www.bankgirot.se/sok-bankgironummer/?bgnr=235-9321</c>.
    /// </summary>
    public static Uri GetBankgirotUrl(this SwedishBankgiroNumber bg)
        => new($"https://www.bankgirot.se/sok-bankgironummer/?bgnr={bg.Formatted}");
}
