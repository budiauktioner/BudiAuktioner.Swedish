using Buildi.Primitives;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A phone country calling code (<c>landsnummer</c>) such as <c>46</c> for Sweden or <c>1</c> for the US.
/// The canonical form (<see cref="Value"/>) is the digits-only calling code (e.g. <c>46</c>).
/// Use <see cref="ToString"/> for the display form with <c>+</c> prefix (e.g. <c>+46</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.itu.int/rec/T-REC-E.164/">ITU-T E.164</see> — international phone number standard</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/List_of_country_calling_codes">Wikipedia — List of country calling codes</see></description></item>
/// </list>
/// </remarks>
public sealed class PhoneCallingCode : IEquatable<PhoneCallingCode>, IComparable<PhoneCallingCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Calling Code", "Landsnummer", "📞", ["https://www.itu.int/rec/T-REC-E.164/", "https://en.wikipedia.org/wiki/List_of_country_calling_codes"]);

    private static readonly Dictionary<string, string> CodeToCountry;
    private static readonly PhoneCallingCode[] AllCodes;

    /// <summary>
    /// The calling code digits without any prefix, e.g. <c>46</c> for Sweden.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code for the primary country associated with this calling code,
    /// e.g. <c>SE</c> for calling code <c>46</c>. For shared calling codes (e.g. <c>1</c>) the
    /// primary country is returned (<c>US</c> for <c>1</c>, <c>GB</c> for <c>44</c>, <c>RU</c> for <c>7</c>).
    /// Null if the calling code cannot be mapped to a known country.
    /// </summary>
    public string? CountryCode { get; }

    private PhoneCallingCode(string value, string? countryCode)
    {
        Value = value;
        CountryCode = countryCode;
    }

    internal static PhoneCallingCode FromResolvedDigits(string digits)
    {
        CodeToCountry.TryGetValue(digits, out var countryCode);
        return new PhoneCallingCode(digits, countryCode);
    }

    static PhoneCallingCode()
    {
        CodeToCountry = new(StringComparer.Ordinal)
        {
            ["1"] = "US",
            ["7"] = "RU",
            ["20"] = "EG",
            ["27"] = "ZA",
            ["30"] = "GR",
            ["31"] = "NL",
            ["32"] = "BE",
            ["33"] = "FR",
            ["34"] = "ES",
            ["36"] = "HU",
            ["39"] = "IT",
            ["40"] = "RO",
            ["41"] = "CH",
            ["43"] = "AT",
            ["44"] = "GB",
            ["45"] = "DK",
            ["46"] = "SE",
            ["47"] = "NO",
            ["48"] = "PL",
            ["49"] = "DE",
            ["51"] = "PE",
            ["52"] = "MX",
            ["53"] = "CU",
            ["54"] = "AR",
            ["55"] = "BR",
            ["56"] = "CL",
            ["57"] = "CO",
            ["58"] = "VE",
            ["60"] = "MY",
            ["61"] = "AU",
            ["62"] = "ID",
            ["63"] = "PH",
            ["64"] = "NZ",
            ["65"] = "SG",
            ["66"] = "TH",
            ["81"] = "JP",
            ["82"] = "KR",
            ["84"] = "VN",
            ["86"] = "CN",
            ["90"] = "TR",
            ["91"] = "IN",
            ["92"] = "PK",
            ["93"] = "AF",
            ["94"] = "LK",
            ["95"] = "MM",
            ["98"] = "IR",
            ["211"] = "SS",
            ["212"] = "MA",
            ["213"] = "DZ",
            ["216"] = "TN",
            ["218"] = "LY",
            ["220"] = "GM",
            ["221"] = "SN",
            ["222"] = "MR",
            ["223"] = "ML",
            ["224"] = "GN",
            ["225"] = "CI",
            ["226"] = "BF",
            ["227"] = "NE",
            ["228"] = "TG",
            ["229"] = "BJ",
            ["230"] = "MU",
            ["231"] = "LR",
            ["232"] = "SL",
            ["233"] = "GH",
            ["234"] = "NG",
            ["235"] = "TD",
            ["236"] = "CF",
            ["237"] = "CM",
            ["238"] = "CV",
            ["239"] = "ST",
            ["240"] = "GQ",
            ["241"] = "GA",
            ["242"] = "CG",
            ["243"] = "CD",
            ["244"] = "AO",
            ["245"] = "GW",
            ["248"] = "SC",
            ["249"] = "SD",
            ["250"] = "RW",
            ["251"] = "ET",
            ["252"] = "SO",
            ["253"] = "DJ",
            ["254"] = "KE",
            ["255"] = "TZ",
            ["256"] = "UG",
            ["257"] = "BI",
            ["258"] = "MZ",
            ["260"] = "ZM",
            ["261"] = "MG",
            ["263"] = "ZW",
            ["264"] = "NA",
            ["265"] = "MW",
            ["266"] = "LS",
            ["267"] = "BW",
            ["268"] = "SZ",
            ["269"] = "KM",
            ["291"] = "ER",
            ["297"] = "AW",
            ["298"] = "FO",
            ["299"] = "GL",
            ["350"] = "GI",
            ["351"] = "PT",
            ["352"] = "LU",
            ["353"] = "IE",
            ["354"] = "IS",
            ["355"] = "AL",
            ["356"] = "MT",
            ["357"] = "CY",
            ["358"] = "FI",
            ["359"] = "BG",
            ["370"] = "LT",
            ["371"] = "LV",
            ["372"] = "EE",
            ["373"] = "MD",
            ["374"] = "AM",
            ["375"] = "BY",
            ["376"] = "AD",
            ["377"] = "MC",
            ["378"] = "SM",
            ["379"] = "VA",
            ["380"] = "UA",
            ["381"] = "RS",
            ["382"] = "ME",
            ["383"] = "XK",
            ["385"] = "HR",
            ["386"] = "SI",
            ["387"] = "BA",
            ["389"] = "MK",
            ["420"] = "CZ",
            ["421"] = "SK",
            ["423"] = "LI",
            ["501"] = "BZ",
            ["502"] = "GT",
            ["503"] = "SV",
            ["504"] = "HN",
            ["505"] = "NI",
            ["506"] = "CR",
            ["507"] = "PA",
            ["509"] = "HT",
            ["591"] = "BO",
            ["592"] = "GY",
            ["593"] = "EC",
            ["595"] = "PY",
            ["597"] = "SR",
            ["598"] = "UY",
            ["599"] = "CW",
            ["670"] = "TL",
            ["673"] = "BN",
            ["674"] = "NR",
            ["675"] = "PG",
            ["676"] = "TO",
            ["677"] = "SB",
            ["678"] = "VU",
            ["679"] = "FJ",
            ["680"] = "PW",
            ["685"] = "WS",
            ["686"] = "KI",
            ["688"] = "TV",
            ["691"] = "FM",
            ["692"] = "MH",
            ["850"] = "KP",
            ["852"] = "HK",
            ["853"] = "MO",
            ["855"] = "KH",
            ["856"] = "LA",
            ["880"] = "BD",
            ["886"] = "TW",
            ["960"] = "MV",
            ["961"] = "LB",
            ["962"] = "JO",
            ["963"] = "SY",
            ["964"] = "IQ",
            ["965"] = "KW",
            ["966"] = "SA",
            ["967"] = "YE",
            ["968"] = "OM",
            ["970"] = "PS",
            ["971"] = "AE",
            ["972"] = "IL",
            ["973"] = "BH",
            ["974"] = "QA",
            ["975"] = "BT",
            ["976"] = "MN",
            ["977"] = "NP",
            ["992"] = "TJ",
            ["993"] = "TM",
            ["994"] = "AZ",
            ["995"] = "GE",
            ["996"] = "KG",
            ["998"] = "UZ",
        };

        AllCodes = CodeToCountry
            .Select(kvp => new PhoneCallingCode(kvp.Key, kvp.Value))
            .ToArray();
    }

    /// <summary>Sweden (+46).</summary>
    public static PhoneCallingCode Sweden { get; } = new("46", "SE");
    /// <summary>Norway (+47).</summary>
    public static PhoneCallingCode Norway { get; } = new("47", "NO");
    /// <summary>Finland (+358).</summary>
    public static PhoneCallingCode Finland { get; } = new("358", "FI");
    /// <summary>Denmark (+45).</summary>
    public static PhoneCallingCode Denmark { get; } = new("45", "DK");
    /// <summary>Germany (+49).</summary>
    public static PhoneCallingCode Germany { get; } = new("49", "DE");
    /// <summary>Poland (+48).</summary>
    public static PhoneCallingCode Poland { get; } = new("48", "PL");
    /// <summary>Estonia (+372).</summary>
    public static PhoneCallingCode Estonia { get; } = new("372", "EE");
    /// <summary>Lithuania (+370).</summary>
    public static PhoneCallingCode Lithuania { get; } = new("370", "LT");
    /// <summary>Romania (+40).</summary>
    public static PhoneCallingCode Romania { get; } = new("40", "RO");
    /// <summary>Bulgaria (+359).</summary>
    public static PhoneCallingCode Bulgaria { get; } = new("359", "BG");
    /// <summary>Latvia (+371).</summary>
    public static PhoneCallingCode Latvia { get; } = new("371", "LV");
    /// <summary>Czech Republic (+420).</summary>
    public static PhoneCallingCode CzechRepublic { get; } = new("420", "CZ");
    /// <summary>Spain (+34).</summary>
    public static PhoneCallingCode Spain { get; } = new("34", "ES");
    /// <summary>Netherlands (+31).</summary>
    public static PhoneCallingCode Netherlands { get; } = new("31", "NL");
    /// <summary>Greece (+30).</summary>
    public static PhoneCallingCode Greece { get; } = new("30", "GR");
    /// <summary>Italy (+39).</summary>
    public static PhoneCallingCode Italy { get; } = new("39", "IT");
    /// <summary>Slovenia (+386).</summary>
    public static PhoneCallingCode Slovenia { get; } = new("386", "SI");
    /// <summary>Croatia (+385).</summary>
    public static PhoneCallingCode Croatia { get; } = new("385", "HR");
    /// <summary>Portugal (+351).</summary>
    public static PhoneCallingCode Portugal { get; } = new("351", "PT");
    /// <summary>Hungary (+36).</summary>
    public static PhoneCallingCode Hungary { get; } = new("36", "HU");
    /// <summary>France (+33).</summary>
    public static PhoneCallingCode France { get; } = new("33", "FR");
    /// <summary>Slovakia (+421).</summary>
    public static PhoneCallingCode Slovakia { get; } = new("421", "SK");
    /// <summary>Belgium (+32).</summary>
    public static PhoneCallingCode Belgium { get; } = new("32", "BE");
    /// <summary>United Kingdom (+44).</summary>
    public static PhoneCallingCode UnitedKingdom { get; } = new("44", "GB");
    /// <summary>Austria (+43).</summary>
    public static PhoneCallingCode Austria { get; } = new("43", "AT");
    /// <summary>Cyprus (+357).</summary>
    public static PhoneCallingCode Cyprus { get; } = new("357", "CY");
    /// <summary>Iceland (+354).</summary>
    public static PhoneCallingCode Iceland { get; } = new("354", "IS");
    /// <summary>Switzerland (+41).</summary>
    public static PhoneCallingCode Switzerland { get; } = new("41", "CH");
    /// <summary>Ireland (+353).</summary>
    public static PhoneCallingCode Ireland { get; } = new("353", "IE");
    /// <summary>Luxembourg (+352).</summary>
    public static PhoneCallingCode Luxembourg { get; } = new("352", "LU");
    /// <summary>Malta (+356).</summary>
    public static PhoneCallingCode Malta { get; } = new("356", "MT");
    /// <summary>Liechtenstein (+423).</summary>
    public static PhoneCallingCode Liechtenstein { get; } = new("423", "LI");
    /// <summary>United States (+1).</summary>
    public static PhoneCallingCode UnitedStates { get; } = new("1", "US");
    /// <summary>Russia (+7).</summary>
    public static PhoneCallingCode Russia { get; } = new("7", "RU");

    /// <summary>
    /// All known phone calling codes.
    /// </summary>
    public static IReadOnlyList<PhoneCallingCode> All => AllCodes;

    public static bool TryParse(string? input, out PhoneCallingCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = ExtractDigits(input.Trim());
        if (digits is null) return false;
        if (!CodeToCountry.TryGetValue(digits, out var countryCode)) return false;

        result = new PhoneCallingCode(digits, countryCode);
        return true;
    }

    public static PhoneCallingCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown phone calling code.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the calling code with <c>+</c> prefix, e.g. <c>+46</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns the calling code as digits only, e.g. <c>46</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the calling code as digits only, e.g. <c>46</c>.
    /// </summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the calling code with <c>+</c> prefix, e.g. <c>+46</c>.
    /// </summary>
    public override string ToString() => "+" + Value;

    private static string? ExtractDigits(string input)
    {
        if (input.StartsWith('+'))
            return input[1..];

        if (input.StartsWith("00") && input.Length > 2)
            return input[2..];

        if (input.Length >= 1 && input.Length <= 3 && input.All(char.IsDigit))
            return input;

        return null;
    }

    public bool Equals(PhoneCallingCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PhoneCallingCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(PhoneCallingCode? a, PhoneCallingCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PhoneCallingCode? a, PhoneCallingCode? b) => !(a == b);
    public int CompareTo(PhoneCallingCode? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(PhoneCallingCode left, PhoneCallingCode right) => left.CompareTo(right) < 0;
    public static bool operator >(PhoneCallingCode left, PhoneCallingCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(PhoneCallingCode left, PhoneCallingCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PhoneCallingCode left, PhoneCallingCode right) => left.CompareTo(right) >= 0;
}
