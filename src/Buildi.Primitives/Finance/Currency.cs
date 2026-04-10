using System.Globalization;
using Buildi.Primitives;

namespace Buildi.Primitives.Finance;

/// <summary>
/// An ISO 4217 currency identified by its three-letter code. Each currency exposes its English name,
/// Swedish name, symbol, and the number of minor-unit decimal places. Parsing accepts ISO codes
/// (e.g. "SEK", "EUR") and common display names (e.g. "Swedish krona", "Euro").
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/iso-4217-currency-codes.html">ISO 4217</see> — currency codes standard</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/ISO_4217">Wikipedia — ISO 4217</see></description></item>
/// </list>
/// </remarks>
public sealed class Currency : IEquatable<Currency>, IComparable<Currency>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Currency", "Valuta", "💱", ["https://www.iso.org/iso-4217-currency-codes.html", "https://en.wikipedia.org/wiki/ISO_4217"]);

    private static readonly Lazy<Dictionary<string, Currency>> ByCode = new(BuildCodeIndex);
    private static readonly Lazy<Dictionary<string, Currency>> ByName = new(BuildNameIndex);
    private static readonly Lazy<Dictionary<string, Currency>> BySymbol = new(BuildSymbolIndex);

    public string Code { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }

    /// <summary>Currency name in the current display language, for example <c>Svensk krona</c> or <c>Swedish krona</c> depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Currency name in the primary country's own language, for example <c>Svensk krona</c> for SEK (Swedish), <c>United States dollar</c> for USD (English). Falls back to <see cref="EnglishName"/> when no country association exists.</summary>
    public string NativeName => ResolveNativeName();

    private Currency(string code, string englishName, string localizedName, string symbol, int decimalPlaces)
    {
        Code = code;
        EnglishName = englishName;
        LocalizedName = localizedName;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    private static Currency C(string code, string en, string sv, string sym, int dec = 2)
        => new(code, en, sv, sym, dec);

    private static readonly Currency[] KnownCurrencies =
    [
        C("SEK", "Swedish krona", "Svensk krona", "kr"),
        C("NOK", "Norwegian krone", "Norsk krona", "kr"),
        C("DKK", "Danish krone", "Dansk krona", "kr"),
        C("ISK", "Icelandic króna", "Isländsk krona", "kr", 0),
        C("EUR", "Euro", "Euro", "€"),
        C("USD", "United States dollar", "Amerikansk dollar", "$"),
        C("GBP", "Pound sterling", "Brittiskt pund", "£"),
        C("CHF", "Swiss franc", "Schweizisk franc", "CHF"),
        C("JPY", "Japanese yen", "Japansk yen", "¥", 0),
        C("CNY", "Chinese yuan", "Kinesisk yuan", "¥"),
        C("CAD", "Canadian dollar", "Kanadensisk dollar", "CA$"),
        C("AUD", "Australian dollar", "Australisk dollar", "A$"),
        C("NZD", "New Zealand dollar", "Nyzeeländsk dollar", "NZ$"),
        C("PLN", "Polish złoty", "Polsk zloty", "zł"),
        C("CZK", "Czech koruna", "Tjeckisk koruna", "Kč"),
        C("HUF", "Hungarian forint", "Ungersk forint", "Ft"),
        C("RON", "Romanian leu", "Rumänsk leu", "lei"),
        C("BGN", "Bulgarian lev", "Bulgarisk lev", "лв"),
        C("HRK", "Croatian kuna", "Kroatisk kuna", "kn"),
        C("TRY", "Turkish lira", "Turkisk lira", "₺"),
        C("RUB", "Russian ruble", "Rysk rubel", "₽"),
        C("INR", "Indian rupee", "Indisk rupie", "₹"),
        C("BRL", "Brazilian real", "Brasiliansk real", "R$"),
        C("ZAR", "South African rand", "Sydafrikansk rand", "R"),
        C("MXN", "Mexican peso", "Mexikansk peso", "MX$"),
        C("SGD", "Singapore dollar", "Singaporiansk dollar", "S$"),
        C("HKD", "Hong Kong dollar", "Hongkongdollar", "HK$"),
        C("KRW", "South Korean won", "Sydkoreansk won", "₩", 0),
        C("THB", "Thai baht", "Thailändsk baht", "฿"),
        C("TWD", "New Taiwan dollar", "Taiwanesisk dollar", "NT$"),
        C("ILS", "Israeli new shekel", "Israelisk shekel", "₪"),
        C("AED", "United Arab Emirates dirham", "Emiratisk dirham", "د.إ"),
        C("SAR", "Saudi riyal", "Saudisk riyal", "﷼"),
        C("PHP", "Philippine peso", "Filippinsk peso", "₱"),
        C("IDR", "Indonesian rupiah", "Indonesisk rupiah", "Rp"),
        C("MYR", "Malaysian ringgit", "Malaysisk ringgit", "RM"),
        C("VND", "Vietnamese đồng", "Vietnamesisk dong", "₫", 0),
        C("UAH", "Ukrainian hryvnia", "Ukrainsk hryvnia", "₴"),
        C("EGP", "Egyptian pound", "Egyptiskt pund", "E£"),
        C("NGN", "Nigerian naira", "Nigeriansk naira", "₦"),
        C("KES", "Kenyan shilling", "Kenyansk shilling", "KSh"),
        C("COP", "Colombian peso", "Colombiansk peso", "CO$"),
        C("ARS", "Argentine peso", "Argentinsk peso", "AR$"),
        C("CLP", "Chilean peso", "Chilensk peso", "CL$", 0),
        C("PEN", "Peruvian sol", "Peruansk sol", "S/."),
    ];

    /// <summary>Swedish krona (SEK).</summary>
    public static Currency SEK => ByCode.Value["SEK"];
    /// <summary>Euro (EUR).</summary>
    public static Currency EUR => ByCode.Value["EUR"];
    /// <summary>United States dollar (USD).</summary>
    public static Currency USD => ByCode.Value["USD"];
    /// <summary>Norwegian krone (NOK).</summary>
    public static Currency NOK => ByCode.Value["NOK"];
    /// <summary>Danish krone (DKK).</summary>
    public static Currency DKK => ByCode.Value["DKK"];
    /// <summary>Pound sterling (GBP).</summary>
    public static Currency GBP => ByCode.Value["GBP"];

    public static IReadOnlyList<Currency> All { get; } = KnownCurrencies;

    public static bool TryParse(string? input, out Currency? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (ByCode.Value.TryGetValue(trimmed.ToUpperInvariant(), out var byCode))
        {
            result = byCode;
            return true;
        }

        if (ByName.Value.TryGetValue(trimmed, out var byName))
        {
            result = byName;
            return true;
        }

        return false;
    }

    public static Currency Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown currency.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the currency in the current display language, for example <c>Svensk krona</c> or <c>Swedish krona</c>
    /// depending on <see cref="PrimitivesDefaults.UICulture"/>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.DisplayName : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the ISO 4217 three-letter currency code, for example <c>SEK</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the ISO 4217 three-letter currency code, for example <c>SEK</c>.
    /// </summary>
    public string ToNormalizedString() => Code;

    /// <summary>
    /// Returns the currency in the current display language (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>
    /// is true, otherwise English), for example <c>Svensk krona</c> or <c>Swedish krona</c>.
    /// </summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>
    /// Returns the currency's English display name, for example <c>Swedish krona</c>.
    /// </summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>
    /// Returns the currency name in the primary country's own language, for example <c>Svensk krona</c> for SEK.
    /// </summary>
    public string ToNativeString() => NativeName;

    /// <summary>
    /// Returns the currency in the current display language, for example <c>Svensk krona</c> or <c>Swedish krona</c>.
    /// </summary>
    public override string ToString() => DisplayName;

    internal static bool TryParseSymbol(string? symbol, out Currency? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(symbol)) return false;
        return BySymbol.Value.TryGetValue(symbol.Trim(), out result);
    }

    private static Dictionary<string, Currency> BuildCodeIndex()
    {
        var dict = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in KnownCurrencies)
            dict[c.Code] = c;
        return dict;
    }

    private static Dictionary<string, Currency> BuildNameIndex()
    {
        var dict = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in KnownCurrencies)
        {
            dict[c.EnglishName] = c;
            dict[c.LocalizedName] = c;
        }
        return dict;
    }

    private static Dictionary<string, Currency> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in KnownCurrencies)
        {
            // Only keep the first currency per symbol to avoid ambiguity;
            // "kr" maps to SEK since it appears first in the list.
            if (!dict.ContainsKey(c.Symbol))
                dict[c.Symbol] = c;
        }
        return dict;
    }

    private string ResolveNativeName()
    {
        var countries = Geography.Country.All;
        var country = countries.FirstOrDefault(c =>
            c.CurrencyCode != null &&
            c.CurrencyCode.Equals(Code, StringComparison.OrdinalIgnoreCase));

        if (country is null)
            return EnglishName;

        return country.NativeName switch
        {
            var ln when ln.Equals(country.LocalizedName, StringComparison.Ordinal) => LocalizedName,
            var ln when ln.Equals(country.EnglishName, StringComparison.Ordinal) => EnglishName,
            _ => LocalizedName
        };
    }

    public bool Equals(Currency? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is Currency other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Currency? a, Currency? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Currency? a, Currency? b) => !(a == b);
    public int CompareTo(Currency? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Currency left, Currency right) => left.CompareTo(right) < 0;
    public static bool operator >(Currency left, Currency right) => left.CompareTo(right) > 0;
    public static bool operator <=(Currency left, Currency right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Currency left, Currency right) => left.CompareTo(right) >= 0;
}
