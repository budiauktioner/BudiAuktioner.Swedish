using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Banking;

public enum SwedishBankingNumberType
{
    Unknown = 0,
    SwedishBankgiroNumber = 1,
    SwedishPostgiroNumber = 2,
    SwedishOcrReferenceNumber = 3,
    Iban = 4,
    SwedishBankAccount = 5,
    Bic = 6,
}

/// <summary>
/// Parsed banking identifier result from the unified banking parser. The parser classifies arbitrary input as Bankgiro, Plusgiro, OCR reference, BIC, IBAN, or Swedish bank account and returns the matching typed model together with a normalized value.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankgirot.se/">Bankgirot</see> — Swedish payment infrastructure and reference formats</description></item>
/// <item><description><see href="https://www.iso.org/standard/81090.html">ISO 13616</see> — IBAN standard</description></item>
/// </list>
/// </remarks>
public sealed class SwedishBankingNumber
{
    public SwedishBankingNumberType Type { get; init; }
    public string NormalizedValue { get; init; } = string.Empty;

    public SwedishBankgiroNumber? SwedishBankgiroNumber { get; init; }
    public SwedishPostgiroNumber? SwedishPostgiroNumber { get; init; }
    public SwedishOcrReferenceNumber? SwedishOcrReferenceNumber { get; init; }
    public Iban? Iban { get; init; }
    public SwedishBankAccount? SwedishBankAccount { get; init; }
    public Bic? Bic { get; init; }
}

/// <summary>
/// A unified parser for common banking identifiers used in Swedish payment flows. It attempts to
/// classify an arbitrary input as Bankgiro, Plusgiro, OCR reference, BIC, IBAN, or Swedish bank account,
/// and returns the matching typed model together with a normalized value. Standalone clearing numbers
/// (4–5 digits) are not classified because they are too short and ambiguous — use
/// <see cref="SwedishBankClearingNumber.TryParse"/> directly when the input is known to be a clearing number.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankgirot.se/">Bankgirot</see> — Swedish payment infrastructure and reference formats</description></item>
/// <item><description><see href="https://www.iso.org/standard/81090.html">ISO 13616</see> — IBAN standard</description></item>
/// </list>
/// </remarks>
public static class SwedishBankingNumberParser
{
    private const int MaxInputLength = 50;

    private static readonly Regex BankgiroPattern = new(@"^\s*\d{3,4}\s*-\s*\d{4}\s*$", RegexOptions.Compiled);
    private static readonly Regex PlusgiroPattern = new(@"^\s*\d{1,7}\s*-\s*\d\s*$", RegexOptions.Compiled);

    private static readonly (string Prefix, SwedishBankingNumberType Type)[] TypePrefixes =
    [
        ("Bankgiro ", SwedishBankingNumberType.SwedishBankgiroNumber),
        ("Plusgiro ", SwedishBankingNumberType.SwedishPostgiroNumber),
        ("Postgiro ", SwedishBankingNumberType.SwedishPostgiroNumber),
        ("BG ", SwedishBankingNumberType.SwedishBankgiroNumber),
        ("PG ", SwedishBankingNumberType.SwedishPostgiroNumber),
    ];

    public static bool TryParse(string? input, out SwedishBankingNumber result)
    {
        result = CreateUnknownResult();

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = InputSanitization.SanitizeInput(input).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        // 0. Explicit type prefix ("BG ", "PG ", "Bankgiro ", "Plusgiro ", "Postgiro ") —
        //    strip the prefix and only try the indicated type.
        var (hintedType, payload) = DetectTypePrefix(trimmed);
        if (hintedType == SwedishBankingNumberType.SwedishBankgiroNumber)
        {
            if (SwedishBankgiroNumber.TryParse(payload, out var prefixedBankgiro))
            {
                result = new SwedishBankingNumber
                {
                    Type = SwedishBankingNumberType.SwedishBankgiroNumber,
                    NormalizedValue = prefixedBankgiro!.Formatted,
                    SwedishBankgiroNumber = prefixedBankgiro
                };
                return true;
            }
            return false;
        }
        if (hintedType == SwedishBankingNumberType.SwedishPostgiroNumber)
        {
            if (SwedishPostgiroNumber.TryParse(payload, out var prefixedPlusgiro))
            {
                result = new SwedishBankingNumber
                {
                    Type = SwedishBankingNumberType.SwedishPostgiroNumber,
                    NormalizedValue = prefixedPlusgiro!.Formatted,
                    SwedishPostgiroNumber = prefixedPlusgiro
                };
                return true;
            }
            return false;
        }

        // 1. IBAN is the clearest distinctive format due to country letters and length/check digit rules.
        if (ContainsLetters(trimmed) && Iban.TryParse(trimmed, out var iban))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.Iban,
                NormalizedValue = iban!.Value,
                Iban = iban
            };
            return true;
        }

        // 2. BIC/SWIFT comes before domestic numeric identifiers.
        if (ContainsLetters(trimmed) && Bic.TryParse(trimmed, out var bic))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.Bic,
                NormalizedValue = bic!.Code,
                Bic = bic
            };
            return true;
        }

        // 3. Respect explicit hyphenated formats where possible.
        if (BankgiroPattern.IsMatch(trimmed) && SwedishBankgiroNumber.TryParse(trimmed, out var explicitBankgiro))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishBankgiroNumber,
                NormalizedValue = explicitBankgiro!.Formatted,
                SwedishBankgiroNumber = explicitBankgiro
            };
            return true;
        }

        if (PlusgiroPattern.IsMatch(trimmed) && SwedishPostgiroNumber.TryParse(trimmed, out var explicitPlusgiro))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishPostgiroNumber,
                NormalizedValue = explicitPlusgiro!.Formatted,
                SwedishPostgiroNumber = explicitPlusgiro
            };
            return true;
        }

        // 4. Longer Swedish domestic account numbers are more specific than OCR.
        if (SwedishBankAccount.TryParse(trimmed, out var bankAccount))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishBankAccount,
                NormalizedValue = bankAccount!.ToNormalizedString(),
                SwedishBankAccount = bankAccount
            };
            return true;
        }

        // 5. Bankgiro before Plusgiro for ambiguous digit-only 7-8 digit inputs.
        if (SwedishBankgiroNumber.TryParse(trimmed, out var bankgiro))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishBankgiroNumber,
                NormalizedValue = bankgiro!.Formatted,
                SwedishBankgiroNumber = bankgiro
            };
            return true;
        }

        if (SwedishPostgiroNumber.TryParse(trimmed, out var plusgiro))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishPostgiroNumber,
                NormalizedValue = plusgiro!.Formatted,
                SwedishPostgiroNumber = plusgiro
            };
            return true;
        }

        // 6. OCR is deliberately last because it overlaps with several other numeric identifiers.
        if (SwedishOcrReferenceNumber.TryParse(trimmed, out var ocr))
        {
            result = new SwedishBankingNumber
            {
                Type = SwedishBankingNumberType.SwedishOcrReferenceNumber,
                NormalizedValue = ocr!.Value,
                SwedishOcrReferenceNumber = ocr
            };
            return true;
        }

        return false;
    }

    public static SwedishBankingNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Could not parse input as a known banking number.", nameof(input));

        return result;
    }

    private static SwedishBankingNumber CreateUnknownResult()
        => new()
        {
            Type = SwedishBankingNumberType.Unknown,
            NormalizedValue = string.Empty
        };

    private static (SwedishBankingNumberType Type, string Payload) DetectTypePrefix(string input)
    {
        foreach (var (prefix, type) in TypePrefixes)
        {
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var payload = input.Substring(prefix.Length).Trim();
                if (payload.Length > 0)
                    return (type, payload);
            }
        }
        return (SwedishBankingNumberType.Unknown, input);
    }

    private static bool ContainsLetters(string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsLetter(input[i]))
                return true;
        }

        return false;
    }
}
