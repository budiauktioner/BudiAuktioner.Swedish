namespace Buildi.Primitives.Banking;

/// <summary>
/// Extension methods for masking sensitive banking information in display strings.
/// </summary>
public static class BankingMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked display string showing the clearing number but masking the account number,
    /// e.g. <c>5100-*******</c>.
    /// Set <paramref name="maskClearingNumber"/> to <see langword="true"/> to also mask the clearing number:
    /// <c>****-*******</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishBankAccount account, bool maskClearingNumber = false)
    {
        var clearing = maskClearingNumber
            ? new string(MaskChar, account.ClearingNumber.Length)
            : account.ClearingNumber;
        var maskedAccount = new string(MaskChar, account.AccountNumber.Length);
        return $"{clearing}-{maskedAccount}";
    }

    /// <summary>
    /// Returns a masked display string showing the country code and check digits but masking the account,
    /// e.g. <c>SE45 **** **** **** **** ****</c>.
    /// </summary>
    public static string ToMaskedString(this Iban iban)
    {
        var formatted = iban.Formatted;
        var firstSpace = formatted.IndexOf(' ');

        if (firstSpace < 0)
        {
            var prefix = formatted[..Math.Min(4, formatted.Length)];
            return prefix + new string(MaskChar, Math.Max(0, formatted.Length - 4));
        }

        var countryAndCheck = formatted[..firstSpace];
        var rest = formatted[(firstSpace + 1)..];

        var masked = new char[rest.Length];
        for (var i = 0; i < rest.Length; i++)
            masked[i] = rest[i] == ' ' ? ' ' : MaskChar;

        return $"{countryAndCheck} {new string(masked)}";
    }

    /// <summary>
    /// Returns a masked Bankgiro number showing either the suffix or the prefix,
    /// e.g. <c>****-6201</c> (default) or <c>5805-****</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishBankgiroNumber bankgiro, bool showLastDigits = true)
    {
        var formatted = bankgiro.Formatted;
        var dashIndex = formatted.IndexOf('-');
        if (dashIndex < 0) return new string(MaskChar, formatted.Length);

        if (showLastDigits)
        {
            var maskedPrefix = new string(MaskChar, dashIndex);
            return $"{maskedPrefix}-{formatted[(dashIndex + 1)..]}";
        }

        var maskedSuffix = new string(MaskChar, formatted.Length - dashIndex - 1);
        return $"{formatted[..dashIndex]}-{maskedSuffix}";
    }

    /// <summary>
    /// Returns a masked Plusgiro number showing the control digit by default,
    /// e.g. <c>******-9</c> or <c>123456-*</c> when <paramref name="showControlDigit"/>
    /// is <see langword="false"/>.
    /// </summary>
    public static string ToMaskedString(this SwedishPostgiroNumber postgiro, bool showControlDigit = true)
    {
        var formatted = postgiro.Formatted;
        var dashIndex = formatted.IndexOf('-');
        if (dashIndex < 0) return new string(MaskChar, formatted.Length);

        if (showControlDigit)
        {
            var maskedPrefix = new string(MaskChar, dashIndex);
            return $"{maskedPrefix}-{formatted[(dashIndex + 1)..]}";
        }

        var maskedSuffix = new string(MaskChar, formatted.Length - dashIndex - 1);
        return $"{formatted[..dashIndex]}-{maskedSuffix}";
    }

    /// <summary>
    /// Returns a masked BIC/SWIFT code showing the country code and masking the rest,
    /// e.g. <c>NDEASESS</c> → <c>****SE**</c>.
    /// </summary>
    public static string ToMaskedString(this Bic bic)
    {
        var code = bic.Code;
        var masked = new char[code.Length];
        for (var i = 0; i < code.Length; i++)
            masked[i] = (i >= 4 && i <= 5) ? code[i] : MaskChar;
        return new string(masked);
    }

    /// <summary>
    /// Returns a masked OCR reference number, e.g. <c>12345678901</c> → <c>***********</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishOcrReferenceNumber ocr) =>
        new string(MaskChar, ocr.Value.Length);

    /// <summary>
    /// Returns a masked clearing number showing the bank identifier (first digit) but masking the rest,
    /// e.g. <c>5001</c> → <c>5***</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishBankClearingNumber clearing)
    {
        var digits = clearing.Digits;
        return digits.Length <= 1
            ? new string(MaskChar, digits.Length)
            : $"{digits[0]}{new string(MaskChar, digits.Length - 1)}";
    }

    /// <summary>
    /// Returns a masked account holder name, e.g. <c>Anna Andersson</c> → <c>A*** A********</c>.
    /// Each word's first character is preserved; the rest is masked.
    /// </summary>
    public static string ToMaskedString(this SwedishBankAccountHolderName holderName)
    {
        var parts = holderName.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts.Select(p =>
            p.Length <= 1 ? new string(MaskChar, 1) : $"{p[0]}{new string(MaskChar, p.Length - 1)}"));
    }

}
