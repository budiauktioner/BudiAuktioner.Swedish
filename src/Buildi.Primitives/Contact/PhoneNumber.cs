using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A phone number (<c>telefonnummer</c>) normalized to international digit format with a <c>00</c> prefix
/// (e.g. <c>0046701740633</c>). Swedish numbers are identified by country code 46 and regulated by
/// PTS (Post- och telestyrelsen). Both local Swedish formats and international numbers are accepted.
/// Use <see cref="ToE164String"/> (or its alias <see cref="ToInternationalString"/>) for the
/// E.164 format with <c>+</c> prefix (e.g. <c>+46701740633</c>).
/// When parsing numbers without an explicit country prefix, the default calling code is <c>46</c> (Sweden);
/// use the <c>defaultCallingCode</c> parameter to override this for other countries.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://pts.se/">PTS</see> — Post- och telestyrelsen (Swedish Post and Telecom Authority)</description></item>
/// <item><description><see href="https://www.itu.int/rec/T-REC-E.164/">ITU-T E.164</see> — international phone number standard</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/E.164">Wikipedia — E.164</see></description></item>
/// </list>
/// <para>For test/fictional phone numbers reserved by PTS, see <see cref="PhoneNumberTestData"/> and <see cref="IsSwedishTestPhoneNumber"/>.</para>
/// </remarks>
public sealed class PhoneNumber : IEquatable<PhoneNumber>, IComparable<PhoneNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Phone Number", "Telefonnummer", "📞", ["https://pts.se/", "https://www.itu.int/rec/T-REC-E.164/", "https://en.wikipedia.org/wiki/E.164"]);

    private const int MaxInputLength = 50;

    private static readonly string[] UriPrefixes = ["callto:", "tel:", "sms:", "call:"];

    private static readonly HashSet<string> TwoDigitCountryCodes =
    [
        "20", "27",
        "30", "31", "32", "33", "34", "36", "39",
        "40", "41", "43", "44", "45", "46", "47", "48", "49",
        "51", "52", "53", "54", "55", "56", "57", "58",
        "60", "61", "62", "63", "64", "65", "66",
        "81", "82", "84", "86",
        "90", "91", "92", "93", "94", "95", "98"
    ];

    /// <summary>
    /// Normalized digits: "00" + country code + subscriber number (e.g. "0046701740633").
    /// </summary>
    public string Digits { get; }

    /// <summary>
    /// Country calling code, e.g. <c>+46</c> for Sweden, <c>+44</c> for UK, <c>+1</c> for US.
    /// </summary>
    public PhoneCallingCode CountryCallingCode { get; }

    /// <summary>
    /// The country associated with the calling code, or <see langword="null"/> if the calling code
    /// cannot be mapped to a known country. For shared calling codes (e.g. +1) the primary country
    /// is returned (US for +1, UK for +44, Russia for +7).
    /// </summary>
    public Country? Country { get; }

    /// <summary>
    /// True if the number has Swedish country code (46).
    /// </summary>
    public bool IsSwedish { get; }

    /// <summary>
    /// True if this is a Swedish mobile number (local part starts with 7).
    /// Always false for non-Swedish numbers.
    /// </summary>
    public bool IsMobile { get; }

    /// <summary>
    /// True if this is a PTS-reserved Swedish test/fictional phone number.
    /// </summary>
    /// <seealso cref="PhoneNumberTestData"/>
    public bool IsSwedishTestPhoneNumber
    {
        get
        {
            if (!IsSwedish) return false;
            var local = Digits.Substring(4);
            return IsInPtsRange(local, "7017406", 5, 99)
                || IsInPtsRange(local, "3139006", 0, 99)
                || IsInPtsRange(local, "4062804", 0, 99)
                || IsInPtsRange(local, "8465004", 0, 99)
                || IsInPtsRange(local, "9803192", 0, 99);
        }
    }

    /// <summary>
    /// Human-readable display form. Swedish numbers use local format, others use spaced international format.
    /// </summary>
    public string Formatted => IsSwedish ? ToLocalString() : ToDisplayInternationalString();

    private PhoneNumber(string digits, bool isSwedish, bool isMobile)
    {
        Digits = digits;
        IsSwedish = isSwedish;
        IsMobile = isMobile;
        CountryCallingCode = PhoneCallingCode.FromResolvedDigits(ResolveCallingCode(digits));
        Geography.Country.TryFindByCallingCode(CountryCallingCode, out var country);
        Country = country;
    }

    /// <summary>
    /// Parses a phone number. Numbers without an explicit country prefix (<c>+</c> or <c>00</c>)
    /// are assumed to use the default calling code from <see cref="PrimitivesDefaults.DefaultCallingCode"/>.
    /// </summary>
    public static bool TryParse(string? input, out PhoneNumber? result)
        => TryParse(input, PrimitivesDefaults.DefaultCallingCode.Value, out result);

    /// <summary>
    /// Parses a phone number. Numbers without an explicit country prefix (<c>+</c> or <c>00</c>)
    /// are assumed to belong to the country identified by <paramref name="defaultCallingCode"/>
    /// (e.g. <c>"46"</c> for Sweden, <c>"47"</c> for Norway, <c>"1"</c> for US).
    /// </summary>
    public static bool TryParse(string? input, string defaultCallingCode, out PhoneNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (string.IsNullOrWhiteSpace(defaultCallingCode)) return false;

        var cleaned = InputSanitization.SanitizeInput(input!).Trim();
        if (cleaned.Length > MaxInputLength) return false;

        foreach (var prefix in UriPrefixes)
        {
            if (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[prefix.Length..].TrimStart();
                break;
            }
        }

        var hasPlus = cleaned.StartsWith('+');
        var withOAs0 = cleaned.Replace("O", "0");
        var digitsOnly = Regex.Replace(withOAs0, @"[^\d]", "");

        if (string.IsNullOrEmpty(digitsOnly)) return false;

        string normalized;

        if (hasPlus)
        {
            if (digitsOnly.Length < 7 || digitsOnly.Length > 15) return false;
            normalized = "00" + digitsOnly;
        }
        else if (digitsOnly.StartsWith("00"))
        {
            var countryAndNumber = digitsOnly.Substring(2);
            if (countryAndNumber.Length < 7 || countryAndNumber.Length > 15) return false;
            normalized = digitsOnly;
        }
        else if (digitsOnly.StartsWith(defaultCallingCode) && digitsOnly.Length >= defaultCallingCode.Length + 7)
        {
            var localNumber = digitsOnly.Substring(defaultCallingCode.Length).TrimStart('0');
            normalized = "00" + defaultCallingCode + localNumber;
        }
        else if (digitsOnly.StartsWith("0"))
        {
            var localNumber = digitsOnly.TrimStart('0');
            var totalE164Length = defaultCallingCode.Length + localNumber.Length;
            if (totalE164Length < 7 || totalE164Length > 15) return false;
            normalized = "00" + defaultCallingCode + localNumber;
        }
        else if (defaultCallingCode == "46" && digitsOnly.Length == 9 && digitsOnly.StartsWith("7"))
        {
            normalized = "0046" + digitsOnly;
        }
        else
        {
            return false;
        }

        var e164Part = normalized[2..];
        if (e164Part.Length < 7 || e164Part.Length > 15) return false;

        var isSwedish = e164Part.StartsWith("46");
        if (isSwedish)
        {
            var localPart = e164Part[2..];
            if (localPart.Length < 7 || localPart.Length > 9) return false;
        }

        var isMobile = isSwedish && e164Part[2..].StartsWith("7");
        result = new PhoneNumber(normalized, isSwedish, isMobile);
        return true;
    }

    /// <summary>
    /// Parses a phone number. Numbers without an explicit country prefix (<c>+</c> or <c>00</c>)
    /// are assumed to belong to <paramref name="defaultCallingCode"/>.
    /// </summary>
    public static bool TryParse(string? input, PhoneCallingCode defaultCallingCode, out PhoneNumber? result)
        => TryParse(input, defaultCallingCode.Value, out result);

    /// <summary>
    /// Parses a phone number. Numbers without an explicit country prefix (<c>+</c> or <c>00</c>)
    /// are assumed to belong to <paramref name="defaultCountry"/>.
    /// </summary>
    public static bool TryParse(string? input, Country defaultCountry, out PhoneNumber? result)
        => TryParse(input, defaultCountry.CallingCode, out result);

    /// <summary>
    /// Parses a phone number using the default calling code from <see cref="PrimitivesDefaults.DefaultCallingCode"/>.
    /// </summary>
    public static PhoneNumber Parse(string input)
        => Parse(input, PrimitivesDefaults.DefaultCallingCode.Value);

    /// <summary>
    /// Parses a phone number using the specified <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix.
    /// </summary>
    public static PhoneNumber Parse(string input, string defaultCallingCode)
    {
        if (!TryParse(input, defaultCallingCode, out var result))
            throw new ArgumentException("Invalid phone number.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Parses a phone number using <paramref name="defaultCallingCode"/> for numbers without an explicit country prefix.
    /// </summary>
    public static PhoneNumber Parse(string input, PhoneCallingCode defaultCallingCode)
        => Parse(input, defaultCallingCode.Value);

    /// <summary>
    /// Parses a phone number using <paramref name="defaultCountry"/> for numbers without an explicit country prefix.
    /// </summary>
    public static PhoneNumber Parse(string input, Country defaultCountry)
        => Parse(input, defaultCountry.CallingCode);

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid phone number, using Swedish country code as default.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid phone number, using the specified
    /// <paramref name="defaultCallingCode"/> for numbers without an explicit country prefix.
    /// </summary>
    public static bool IsValid(string? input, string defaultCallingCode) => TryParse(input, defaultCallingCode, out _);

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid phone number, using <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix.
    /// </summary>
    public static bool IsValid(string? input, PhoneCallingCode defaultCallingCode) => TryParse(input, defaultCallingCode.Value, out _);

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid phone number, using <paramref name="defaultCountry"/>
    /// for numbers without an explicit country prefix.
    /// </summary>
    public static bool IsValid(string? input, Country defaultCountry) => TryParse(input, defaultCountry.CallingCode, out _);

    /// <summary>
    /// Returns the phone number in display format, for example <c>070-174 06 33</c> for Swedish numbers
    /// or <c>+44 20 7946 0958</c> for non-Swedish numbers. Uses Swedish country code as default.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => Format(input, PrimitivesDefaults.DefaultCallingCode.Value, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns the phone number in display format using the specified <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix. Numbers matching the default calling code are
    /// shown in local format (e.g. <c>070-174 06 33</c>), others in international format (e.g. <c>+44207946 0958</c>).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, string defaultCallingCode, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (!TryParse(input, defaultCallingCode, out var r))
            return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

        return r!.CountryCallingCode.Value == defaultCallingCode ? r.ToLocalString() : r.ToDisplayInternationalString();
    }

    /// <summary>
    /// Returns the phone number in display format using <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix. Numbers matching the default calling code are
    /// shown in local format, others in international format.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, PhoneCallingCode defaultCallingCode, bool fallbackToTrimmedInputWhenInvalid = false)
        => Format(input, defaultCallingCode.Value, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns the phone number in display format using <paramref name="defaultCountry"/>
    /// for numbers without an explicit country prefix. Numbers belonging to the default country are
    /// shown in local format without the country code, others in international format.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, Country defaultCountry, bool fallbackToTrimmedInputWhenInvalid = false)
        => Format(input, defaultCountry.CallingCode, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns the phone number as digits with the international dialing prefix <c>00</c>,
    /// for example <c>0046701740633</c>. Uses Swedish country code as default.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => Normalize(input, PrimitivesDefaults.DefaultCallingCode.Value, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns the phone number as digits with the international dialing prefix <c>00</c>,
    /// for example <c>0046701740633</c>, using the specified <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, string defaultCallingCode, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, defaultCallingCode, out var r)) return r!.Digits;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns the phone number as digits with the international dialing prefix <c>00</c>,
    /// for example <c>0046701740633</c>, using <paramref name="defaultCallingCode"/>
    /// for numbers without an explicit country prefix.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, PhoneCallingCode defaultCallingCode, bool fallbackToTrimmedInputWhenInvalid = false)
        => Normalize(input, defaultCallingCode.Value, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns the phone number as digits with the international dialing prefix <c>00</c>,
    /// for example <c>0046701740633</c>, using <paramref name="defaultCountry"/>
    /// for numbers without an explicit country prefix.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, Country defaultCountry, bool fallbackToTrimmedInputWhenInvalid = false)
        => Normalize(input, defaultCountry.CallingCode, fallbackToTrimmedInputWhenInvalid);

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the phone number as digits with the international dialing prefix <c>00</c>,
    /// for example <c>0046701740633</c>.
    /// </summary>
    public string ToNormalizedString() => Digits;

    /// <summary>
    /// Local format without country code, e.g. <c>070-174 06 33</c> for Swedish numbers
    /// or <c>0211234567</c> for others. Swedish numbers use area-code–aware formatting.
    /// </summary>
    public string ToLocalString()
    {
        var subscriberDigits = Digits[(2 + CountryCallingCode.Value.Length)..];

        if (IsSwedish)
            return FormatSwedishLocal("0" + subscriberDigits);

        return "0" + subscriberDigits;
    }

    /// <summary>
    /// Returns local format (without country code) when the number belongs to
    /// <paramref name="defaultCallingCode"/>, e.g. <c>070-174 06 33</c> for a Swedish number
    /// when <paramref name="defaultCallingCode"/> is <c>+46</c>. Otherwise returns spaced international format
    /// with <c>+</c> prefix.
    /// </summary>
    public string ToLocalString(PhoneCallingCode defaultCallingCode)
        => CountryCallingCode.Value == defaultCallingCode.Value ? ToLocalString() : ToDisplayInternationalString();

    /// <summary>
    /// Returns local format (without country code) when the number belongs to
    /// <paramref name="defaultCountry"/>, e.g. <c>070-174 06 33</c> for a Swedish number
    /// when <paramref name="defaultCountry"/> is Sweden. Otherwise returns spaced international format
    /// with <c>+</c> prefix.
    /// </summary>
    public string ToLocalString(Country defaultCountry)
        => ToLocalString(defaultCountry.CallingCode);

    /// <summary>
    /// Returns the number in E.164 format with <c>+</c> prefix, e.g. <c>+46701740633</c>.
    /// This is the ITU-T standard representation used by most telephony APIs.
    /// Equivalent to <see cref="ToInternationalString"/>.
    /// </summary>
    public string ToE164String() => "+" + Digits[2..];

    /// <summary>
    /// Returns the number in international format with <c>+</c> prefix, e.g. <c>+46701740633</c>.
    /// This is a convenience alias for <see cref="ToE164String"/> — both return the same E.164 representation.
    /// </summary>
    public string ToInternationalString() => ToE164String();

    private string ToDisplayInternationalString()
    {
        var subscriberDigits = Digits[(2 + CountryCallingCode.Value.Length)..];
        return "+" + CountryCallingCode.Value + " " + FormatInternationalSubscriberDigits(CountryCallingCode.Value, subscriberDigits);
    }

    /// <summary>
    /// Returns the phone number in display format, for example <c>+46 70 123 45 67</c>.
    /// </summary>
    public override string ToString() => Formatted;

    private static string ResolveCallingCode(string digits)
    {
        var e164 = digits[2..];
        if (e164.Length == 0) return string.Empty;
        if (e164[0] is '1' or '7') return e164[..1];
        if (e164.Length >= 2 && TwoDigitCountryCodes.Contains(e164[..2])) return e164[..2];
        return e164.Length >= 3 ? e164[..3] : e164;
    }

    private static bool IsInPtsRange(string localDigits, string prefix, int from, int to)
    {
        if (!localDigits.StartsWith(prefix)) return false;
        var suffix = localDigits.Substring(prefix.Length);
        return suffix.Length == 2 && int.TryParse(suffix, out var n) && n >= from && n <= to;
    }

    private static string FormatInternationalSubscriberDigits(string callingCode, string digits)
    {
        if (callingCode == "1" && digits.Length == 10)
            return $"{digits[..3]} {digits.Substring(3, 3)} {digits.Substring(6, 4)}";

        return digits.Length switch
        {
            7 => $"{digits[..3]} {digits.Substring(3, 2)} {digits.Substring(5, 2)}",
            8 => $"{digits[..2]} {digits.Substring(2, 2)} {digits.Substring(4, 2)} {digits.Substring(6, 2)}",
            9 => $"{digits[..2]} {digits.Substring(2, 3)} {digits.Substring(5, 2)} {digits.Substring(7, 2)}",
            10 => $"{digits[..2]} {digits.Substring(2, 4)} {digits.Substring(6, 4)}",
            11 => $"{digits[..3]} {digits.Substring(3, 4)} {digits.Substring(7, 4)}",
            _ => digits
        };
    }

    private static readonly Regex ScanPattern = new(
        @"(?:(?:\+|00)\d{1,3}[\s\-]\(?\d{1,4}\)?[\s\-]?\d[\d\s\-]{5,12}\d)|(?:(?:\+|00)\d{7,15})|(?:0\d{1,3}[\s\-]?\d[\d\s\-]{5,10}\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential phone numbers (international or Swedish local format).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real phone number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<PhoneNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<PhoneNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var phone)) continue;
            results.Add(new TextCandidate<PhoneNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(PhoneNumber),
                TextCandidateCategory.Contact,
                phone!.ToNormalizedString(),
                phone.ToString(),
                phone.ToMaskedString(),
                TextMatchConfidence.Medium,
                phone));
        }
        return results;
    }

    private static string FormatSwedishLocal(string number)
    {
        var digits = number.TrimStart('0');

        if (digits.Length == 9)
            return $"0{digits.Substring(0, 3)}-{digits.Substring(3, 2)} {digits.Substring(5, 2)} {digits.Substring(7, 2)}";

        if (digits.Length == 7)
        {
            var areaCode = digits.Substring(0, 1);
            var rest = digits.Substring(1);
            return $"0{areaCode}-{rest.Substring(0, 2)} {rest.Substring(2, 2)} {rest.Substring(4, 2)}";
        }

        if (digits.Length == 8)
        {
            var areaCode = digits.Substring(0, 1);
            var rest = digits.Substring(1);
            return $"0{areaCode}-{rest.Substring(0, 2)} {rest.Substring(2, 2)} {rest.Substring(4, 3)}";
        }

        return number;
    }

    public bool Equals(PhoneNumber? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(PhoneNumber? a, PhoneNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PhoneNumber? a, PhoneNumber? b) => !(a == b);
    public int CompareTo(PhoneNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(PhoneNumber left, PhoneNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(PhoneNumber left, PhoneNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(PhoneNumber left, PhoneNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PhoneNumber left, PhoneNumber right) => left.CompareTo(right) >= 0;
}
