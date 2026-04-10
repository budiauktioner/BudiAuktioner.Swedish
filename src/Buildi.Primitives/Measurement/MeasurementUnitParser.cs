using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// Internal helper that splits measurement strings like <c>10 km</c>, <c>5.5cm</c>, <c>3,5 m</c>
/// into a numeric part and a unit suffix. Handles European/US decimal separators and thousands
/// separators (space, period, comma).
/// </summary>
internal static class MeasurementUnitParser
{
    private const int MaxInputLength = 100;

    private static readonly Regex InputPattern = new(
        @"^(?<sign>[+-])?\s*(?<number>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?<unit>.+)$",
        RegexOptions.Compiled);

    /// <summary>
    /// Tries to split <paramref name="input"/> into a decimal value and a unit suffix string.
    /// The unit suffix is trimmed and returned as-is (caller is responsible for unit lookup).
    /// </summary>
    public static bool TrySplit(string? input, out decimal value, out string unitSuffix)
    {
        value = 0;
        unitSuffix = "";

        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        var match = InputPattern.Match(trimmed);
        if (!match.Success) return false;

        var numberRaw = match.Groups["number"].Value;
        unitSuffix = match.Groups["unit"].Value.Trim();
        var isNegative = match.Groups["sign"].Value == "-";

        if (unitSuffix.Length == 0) return false;
        if (!TryParseNumber(numberRaw, out value)) return false;

        if (isNegative) value = -value;
        return true;
    }

    /// <summary>
    /// Tries to parse <paramref name="input"/> as a bare number without a unit suffix.
    /// </summary>
    public static bool TryParseNumberOnly(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = input.Trim();
        if (trimmed.Length > MaxInputLength) return false;

        var s = trimmed;
        var negative = false;
        if (s[0] == '-') { negative = true; s = s[1..].TrimStart(); }
        else if (s[0] == '+') { s = s[1..].TrimStart(); }

        if (s.Length == 0 || !s.All(c => char.IsDigit(c) || c is '.' or ',' or ' ' or '\u00A0'))
            return false;

        if (!TryParseNumber(s, out value)) return false;
        if (negative) value = -value;
        return true;
    }

    private static bool TryParseNumber(string raw, out decimal amount)
    {
        amount = 0;
        var s = raw.Trim();
        if (s.Length == 0) return false;

        var lastComma = s.LastIndexOf(',');
        var lastPeriod = s.LastIndexOf('.');

        string integerPart;
        var fracPart = "";

        if (lastComma > lastPeriod)
        {
            var afterComma = s[(lastComma + 1)..];
            if (afterComma.Length is > 0 and <= 10 && afterComma.All(char.IsDigit))
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
            var afterPeriod = s[(lastPeriod + 1)..];
            if (afterPeriod.Length is > 0 and <= 10 && afterPeriod.All(char.IsDigit))
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

        var cleanInteger = integerPart
            .Replace(" ", "")
            .Replace("\u00A0", "")
            .Replace(".", "")
            .Replace(",", "");

        if (cleanInteger.Length == 0 || !cleanInteger.All(char.IsDigit)) return false;

        var combined = fracPart.Length > 0 ? $"{cleanInteger}.{fracPart}" : cleanInteger;
        return decimal.TryParse(combined, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
    }
}
