using System.Globalization;
using Buildi.Primitives;

namespace Buildi.Primitives.Finance;

/// <summary>
/// A monetary amount composed of a decimal value and a <see cref="Currency"/>. Parsing handles
/// a wide range of everyday formats including prefix/suffix currency codes or symbols, thousands separators
/// (space, period, comma), and various decimal separators.
/// </summary>
/// <remarks>
/// <para>Supported input patterns (examples):</para>
/// <list type="bullet">
/// <item><description><c>1 000 SEK</c>, <c>1000 SEK</c>, <c>SEK 1000</c></description></item>
/// <item><description><c>1000 kr</c>, <c>kr 1000</c>, <c>1 000,50 kr</c></description></item>
/// <item><description><c>USD500</c>, <c>$500</c>, <c>€1 000,00</c></description></item>
/// <item><description><c>1.000,50 SEK</c> (European thousands/decimal), <c>1,000.50 USD</c> (US thousands/decimal)</description></item>
/// </list>
/// <para>When the currency cannot be determined from the input string, supply a fallback currency
/// to <c>TryParse(string?, Currency?, out MoneyAmount?)</c>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/iso-4217-currency-codes.html">ISO 4217</see> — currency codes standard</description></item>
/// </list>
/// </remarks>
public sealed class MoneyAmount : IEquatable<MoneyAmount>, IComparable<MoneyAmount>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Money Amount", "Belopp", "💰", ["https://www.iso.org/iso-4217-currency-codes.html"]);

    private const int MaxInputLength = 100;

    public decimal Amount { get; }
    public Currency Currency { get; }

    private MoneyAmount(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a <see cref="MoneyAmount"/> from an explicit amount and currency.
    /// </summary>
    public static MoneyAmount Create(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        if (!HasSupportedPrecision(amount, currency))
            throw new ArgumentOutOfRangeException(nameof(amount), $"Currency {currency.Code} supports at most {currency.DecimalPlaces} decimal places.");

        return new(amount, currency);
    }

    /// <summary>
    /// Attempts to parse a string like <c>1 000 SEK</c>, <c>$500</c>, or <c>1000 kr</c> into a
    /// <see cref="MoneyAmount"/>. When the string does not contain a recognizable currency indicator,
    /// <paramref name="fallbackCurrency"/> is used. For ambiguous currency symbols such as <c>kr</c>,
    /// Swedish krona is the default, but <paramref name="fallbackCurrency"/> can override that default
    /// when it uses the same symbol. Returns <see langword="false"/> when parsing fails.
    /// </summary>
    public static bool TryParse(string? input, Currency? fallbackCurrency, out MoneyAmount? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;
        if (!SplitCurrencyAndNumber(trimmed, fallbackCurrency, out var currency, out var numberPart))
            return false;

        if (currency == null) return false;
        if (!TryParseNumber(numberPart, currency, out var amount)) return false;

        result = new MoneyAmount(amount, currency);
        return true;
    }

    /// <summary>
    /// Attempts to parse using no fallback currency; the currency must be present in the string.
    /// </summary>
    public static bool TryParse(string? input, out MoneyAmount? result)
        => TryParse(input, null, out result);

    public static MoneyAmount Parse(string input, Currency? fallbackCurrency = null)
    {
        if (!TryParse(input, fallbackCurrency, out var result))
            throw new ArgumentException("Invalid money amount.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    public static bool IsValid(string? input, Currency? fallbackCurrency) => TryParse(input, fallbackCurrency, out _);

    /// <summary>
    /// Returns a formatted string like <c>1 000,00 SEK</c> using Swedish numeric conventions (space as
    /// thousands separator, comma as decimal separator).
    /// </summary>
    public static string? Format(string? input, Currency? fallbackCurrency = null, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, fallbackCurrency, out var result) && result is not null)
            return result.ToString();

        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns the amount as a plain decimal string with the ISO code, for example <c>1000.50 SEK</c>.
    /// </summary>
    public static string? Normalize(string? input, Currency? fallbackCurrency = null, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, fallbackCurrency, out var result) && result is not null)
            return result.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the amount as a plain decimal string (invariant culture) with the ISO code,
    /// for example <c>1000.50 SEK</c>.
    /// </summary>
    public string ToNormalizedString() => $"{Amount.ToString(CultureInfo.InvariantCulture)} {Currency.Code}";

    /// <summary>
    /// Returns the amount formatted using the default culture's numeric conventions and the ISO code,
    /// for example <c>1 000,00 SEK</c> when culture is <c>sv-SE</c>.
    /// </summary>
    public override string ToString()
    {
        var formatted = Amount.ToString($"N{Currency.DecimalPlaces}", PrimitivesDefaults.Culture);
        return $"{formatted} {Currency.Code}";
    }

    private static bool SplitCurrencyAndNumber(
        string input, Currency? fallback,
        out Currency? currency, out string numberPart)
    {
        currency = null;
        numberPart = input;

        // Try ISO code or symbol as prefix
        if (TryMatchPrefix(input, fallback, out currency, out numberPart))
            return true;

        // Try ISO code or symbol as suffix
        if (TryMatchSuffix(input, fallback, out currency, out numberPart))
            return true;

        // No currency found in string — use fallback
        currency = fallback;
        numberPart = input;
        return currency != null;
    }

    private static bool TryMatchPrefix(string input, Currency? fallback, out Currency? currency, out string numberPart)
    {
        currency = null;
        numberPart = input;

        // 3-letter code prefix: "SEK1000", "SEK 1000"
        if (input.Length >= 4 && char.IsLetter(input[0]) && char.IsLetter(input[1]) && char.IsLetter(input[2]))
        {
            var code = input[..3];
            var rest = input[3..].TrimStart();
            if (rest.Length > 0 && Currency.TryParse(code, out currency))
            {
                numberPart = rest;
                return true;
            }
        }

        // Symbol prefix: "$500", "€1000", "kr1000", "kr 1000"
        if (TryResolveSymbolPrefix(input, fallback, out currency, out numberPart))
            return true;

        return false;
    }

    private static bool TryMatchSuffix(string input, Currency? fallback, out Currency? currency, out string numberPart)
    {
        currency = null;
        numberPart = input;

        // 3-letter code suffix: "1000SEK", "1000 SEK"
        if (input.Length >= 4 && char.IsLetter(input[^1]) && char.IsLetter(input[^2]) && char.IsLetter(input[^3]))
        {
            var code = input[^3..];
            var rest = input[..^3].TrimEnd();
            if (rest.Length > 0 && Currency.TryParse(code, out currency))
            {
                numberPart = rest;
                return true;
            }
        }

        // Symbol suffix: "1000 kr", "1000kr", "1000 €"
        if (TryResolveSymbolSuffix(input, fallback, out currency, out numberPart))
            return true;

        return false;
    }

    private static bool TryResolveSymbolPrefix(string input, Currency? fallback, out Currency? currency, out string numberPart)
    {
        currency = null;
        numberPart = input;

        foreach (var symbol in Currency.All
                     .Select(c => c.Symbol)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderByDescending(s => s.Length))
        {
            if (!input.StartsWith(symbol, StringComparison.OrdinalIgnoreCase))
                continue;

            var rest = input[symbol.Length..].TrimStart();
            if (rest.Length == 0)
                return false;

            currency = ResolveCurrencyForSymbol(symbol, fallback);
            numberPart = rest;
            return true;
        }

        return false;
    }

    private static bool TryResolveSymbolSuffix(string input, Currency? fallback, out Currency? currency, out string numberPart)
    {
        currency = null;
        numberPart = input;

        foreach (var symbol in Currency.All
                     .Select(c => c.Symbol)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderByDescending(s => s.Length))
        {
            if (!input.EndsWith(symbol, StringComparison.OrdinalIgnoreCase))
                continue;

            var rest = input[..^symbol.Length].TrimEnd();
            if (rest.Length == 0)
                return false;

            currency = ResolveCurrencyForSymbol(symbol, fallback);
            numberPart = rest;
            return true;
        }

        return false;
    }

    private static Currency ResolveCurrencyForSymbol(string symbol, Currency? fallback)
    {
        if (fallback != null && string.Equals(fallback.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
            return fallback;

        return Currency.All.First(c => string.Equals(c.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasSupportedPrecision(decimal amount, Currency currency)
    {
        var scale = (decimal.GetBits(decimal.Abs(amount))[3] >> 16) & 0xFF;
        return scale <= currency.DecimalPlaces;
    }

    private static bool TryParseNumber(string raw, Currency currency, out decimal amount)
    {
        amount = 0;

        var s = raw.Trim();
        if (s.Length == 0) return false;

        var negative = false;
        if (s[0] == '-') { negative = true; s = s[1..].TrimStart(); }
        else if (s[0] == '+') { s = s[1..].TrimStart(); }

        if (s.Length == 0) return false;

        // Determine thousands vs decimal separator heuristic:
        // "1.000,50" => European (period = thousands, comma = decimal)
        // "1,000.50" => US/UK (comma = thousands, period = decimal)
        // "1 000,50" => Swedish (space = thousands, comma = decimal)
        // "1000.5"   => plain decimal point

        var lastComma = s.LastIndexOf(',');
        var lastPeriod = s.LastIndexOf('.');

        string integerPart;
        string fracPart = "";

        if (lastComma > lastPeriod)
        {
            // Comma is the decimal separator (European/Swedish style)
            var afterComma = s[(lastComma + 1)..];
            if (afterComma.Length <= 2 && afterComma.All(char.IsDigit))
            {
                fracPart = afterComma;
                integerPart = s[..lastComma];
            }
            else
            {
                integerPart = s;
            }
        }
        else if (lastPeriod > lastComma)
        {
            // Period is the decimal separator (US/UK or plain)
            var afterPeriod = s[(lastPeriod + 1)..];
            if (afterPeriod.Length <= 2 && afterPeriod.All(char.IsDigit))
            {
                fracPart = afterPeriod;
                integerPart = s[..lastPeriod];
            }
            else
            {
                integerPart = s;
            }
        }
        else
        {
            integerPart = s;
        }

        // Strip thousands separators (spaces, periods, commas used as thousands)
        var cleanInteger = integerPart.Replace(" ", "").Replace("\u00A0", "").Replace(".", "").Replace(",", "");
        if (cleanInteger.Length == 0 || !cleanInteger.All(char.IsDigit)) return false;

        var combined = fracPart.Length > 0 ? $"{cleanInteger}.{fracPart}" : cleanInteger;
        if (!decimal.TryParse(combined, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
            return false;

        if (fracPart.Length > currency.DecimalPlaces || !HasSupportedPrecision(amount, currency))
            return false;

        if (negative) amount = -amount;
        return true;
    }

    /// <summary>
    /// Converts this amount to <paramref name="targetCurrency"/> using the given <paramref name="exchangeRate"/>.
    /// The rate is applied as a multiplier (e.g., 100 SEK × 0.095 = 9.50 USD).
    /// The result is rounded to the target currency's decimal places using banker's rounding.
    /// </summary>
    public MoneyAmount ConvertTo(Currency targetCurrency, decimal exchangeRate)
    {
        ArgumentNullException.ThrowIfNull(targetCurrency);
        if (exchangeRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be positive.");

        var converted = Math.Round(Amount * exchangeRate, targetCurrency.DecimalPlaces, MidpointRounding.ToEven);
        return new MoneyAmount(converted, targetCurrency);
    }

    /// <summary>
    /// Converts this amount to <paramref name="targetCurrency"/> by looking up the exchange rate
    /// in <paramref name="rates"/>.
    /// </summary>
    public MoneyAmount ConvertTo(Currency targetCurrency, IExchangeRates rates)
    {
        ArgumentNullException.ThrowIfNull(targetCurrency);
        ArgumentNullException.ThrowIfNull(rates);

        var rate = rates.GetRate(Currency, targetCurrency);
        return ConvertTo(targetCurrency, rate);
    }

    public static MoneyAmount operator +(MoneyAmount a, MoneyAmount b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot add {a.Currency.Code} and {b.Currency.Code}.");
        return new MoneyAmount(a.Amount + b.Amount, a.Currency);
    }

    public static MoneyAmount operator -(MoneyAmount a, MoneyAmount b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot subtract {b.Currency.Code} from {a.Currency.Code}.");
        return new MoneyAmount(a.Amount - b.Amount, a.Currency);
    }

    public static MoneyAmount operator *(MoneyAmount a, decimal factor) => new(a.Amount * factor, a.Currency);
    public static MoneyAmount operator *(decimal factor, MoneyAmount a) => new(a.Amount * factor, a.Currency);
    public static MoneyAmount operator /(MoneyAmount a, decimal divisor) => new(a.Amount / divisor, a.Currency);
    public static MoneyAmount operator -(MoneyAmount a) => new(-a.Amount, a.Currency);

    public bool Equals(MoneyAmount? other) => other is not null && Amount == other.Amount && Currency.Code == other.Currency.Code;
    public override bool Equals(object? obj) => obj is MoneyAmount other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency.Code);
    public static bool operator ==(MoneyAmount? a, MoneyAmount? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(MoneyAmount? a, MoneyAmount? b) => !(a == b);
    public int CompareTo(MoneyAmount? other)
    {
        if (other is null) return 1;
        var c = string.Compare(Currency.Code, other.Currency.Code, StringComparison.Ordinal);
        return c != 0 ? c : Amount.CompareTo(other.Amount);
    }
    public static bool operator <(MoneyAmount left, MoneyAmount right) => left.CompareTo(right) < 0;
    public static bool operator >(MoneyAmount left, MoneyAmount right) => left.CompareTo(right) > 0;
    public static bool operator <=(MoneyAmount left, MoneyAmount right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MoneyAmount left, MoneyAmount right) => left.CompareTo(right) >= 0;
}
