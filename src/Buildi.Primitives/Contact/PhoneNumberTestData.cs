namespace Buildi.Primitives.Contact;

/// <summary>
/// PTS (Post- och telestyrelsen) reserves phone number ranges for use in fiction and testing — these numbers will never be assigned to real subscribers. <see cref="PhoneNumberTestData"/> enumerates all reserved numbers, and <see cref="PhoneNumber.IsSwedishTestPhoneNumber"/> checks if a parsed number is in a reserved range.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://pts.se/internet-och-telefoni/telefonnummer-och-adressering/telefonnummer-till-bocker-och-filmer/">PTS — Telefonnummer till böcker och filmer</see></description></item>
/// </list>
/// </remarks>
public static class PhoneNumberTestData
{
    /// <summary>Mobile: 070-174 06 05 through 070-174 06 99 (95 numbers).</summary>
    public static IEnumerable<string> Mobile => GenerateRange("07017406", 5, 99);

    /// <summary>Landline Gothenburg: 031-390 06 00 through 031-390 06 99 (100 numbers).</summary>
    public static IEnumerable<string> LandlineGothenburg => GenerateRange("03139006", 0, 99);

    /// <summary>Landline Malmö: 040-628 04 00 through 040-628 04 99 (100 numbers).</summary>
    public static IEnumerable<string> LandlineMalmo => GenerateRange("04062804", 0, 99);

    /// <summary>Landline Stockholm: 08-4650 04 00 through 08-4650 04 99 (100 numbers).</summary>
    public static IEnumerable<string> LandlineStockholm => GenerateRange("08465004", 0, 99);

    /// <summary>Landline Kiruna: 0980-31 92 00 through 0980-31 92 99 (100 numbers).</summary>
    public static IEnumerable<string> LandlineKiruna => GenerateRange("09803192", 0, 99);

    /// <summary>All reserved test phone numbers across all ranges (495 numbers).</summary>
    public static IEnumerable<string> All => Mobile
        .Concat(LandlineGothenburg)
        .Concat(LandlineMalmo)
        .Concat(LandlineStockholm)
        .Concat(LandlineKiruna);

    private static IEnumerable<string> GenerateRange(string prefix, int from, int to)
    {
        for (var i = from; i <= to; i++)
            yield return prefix + i.ToString("D2");
    }
}
