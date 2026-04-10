using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Finance;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Geography;

/// <summary>
/// A country identified by its English name, Swedish name, local name (endonym), or ISO 3166-1 codes. Each country exposes its phone calling code, country-code top-level domain (ccTLD), continent, country-based European classifications (EU, EEA, Schengen, Nordics, SEPA), and currency metadata. Parsing accepts English names, Swedish names (for example <c>Tyskland</c>, <c>Frankrike</c>), local names (for example <c>Deutschland</c>), alpha-2/alpha-3 codes, and common aliases (for example <c>USA</c>, <c>UK</c>, <c>Holland</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/iso-3166-country-codes.html">ISO 3166-1</see> — country code standard followed by this type</description></item>
/// <item><description><see href="https://www.itu.int/rec/T-REC-E.164/">ITU-T E.164</see> — country calling codes</description></item>
/// <item><description><see href="https://www.iana.org/domains/root/db">IANA — ccTLD list</see></description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625 (coordinate location)</see> — geographic coordinates for each country and capital</description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P36">Wikidata P36 (capital)</see> — capital city data</description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P47">Wikidata P47 (shares border with)</see> — land borders between countries</description></item>
/// </list>
/// </remarks>
public sealed class Country : IEquatable<Country>, IComparable<Country>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Country", "Land", "🌍", ["https://www.iso.org/iso-3166-country-codes.html", "https://en.wikipedia.org/wiki/ISO_3166-1", "https://www.itu.int/rec/T-REC-E.164/", "https://www.iana.org/domains/root/db", "https://www.wikidata.org/wiki/Property:P625", "https://www.wikidata.org/wiki/Property:P36", "https://www.wikidata.org/wiki/Property:P47"]);

    private const int MaxInputLength = 100;

    private static readonly Dictionary<string, Country> ByAlpha2;
    private static readonly Dictionary<string, Country> ByAlpha3;
    private static readonly Dictionary<string, Country> ByName;
    private static readonly Dictionary<string, Country> ByCallingCode;
    private static readonly Country[] AllCountries;
    private static readonly Regex ScanPattern;

    private static readonly HashSet<string> EuropeanUnionCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
        "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE"
    };

    private static readonly HashSet<string> EeaCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
        "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        "IS", "LI", "NO"
    };

    private static readonly HashSet<string> SchengenCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "AT", "BE", "BG", "HR", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IT",
        "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        "IS", "LI", "NO", "CH"
    };

    private static readonly HashSet<string> NordicCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "DK", "FI", "IS", "NO", "SE"
    };

    private static readonly HashSet<string> SepaCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
        "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        "IS", "LI", "NO",
        "AL", "AD", "MD", "MC", "ME", "MK", "SM", "RS", "CH", "GB", "VA",
        "GG", "IM", "JE"
    };

    private static readonly Dictionary<string, string[]> CurrencyOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AX"] = ["EUR"],
        ["FO"] = ["DKK"],
        ["GG"] = ["GBP"],
        ["IM"] = ["GBP"],
        ["JE"] = ["GBP"],
        ["MC"] = ["EUR"],
        ["ME"] = ["EUR"],
        ["SM"] = ["EUR"],
        ["VA"] = ["EUR"],
        ["XK"] = ["EUR"],
    };

    /// <summary>
    /// Geographic coordinates (latitude, longitude) for each country, sourced from
    /// <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    private static readonly Dictionary<string, (double Lat, double Lon)> Coordinates = new(StringComparer.Ordinal)
    {
        ["AF"] = (33.0, 65.0),
        ["AL"] = (41.0, 20.0),
        ["DZ"] = (28.0, 3.0),
        ["AS"] = (-14.27, -170.7),
        ["AD"] = (42.5, 1.5),
        ["AO"] = (-12.5, 18.5),
        ["AG"] = (17.05, -61.8),
        ["AR"] = (-34.0, -64.0),
        ["AM"] = (40.0, 45.0),
        ["AW"] = (12.5, -69.97),
        ["AU"] = (-27.0, 133.0),
        ["AT"] = (47.33, 13.33),
        ["AZ"] = (40.5, 47.5),
        ["BS"] = (24.25, -76.0),
        ["BH"] = (26.0, 50.55),
        ["BD"] = (24.0, 90.0),
        ["BB"] = (13.17, -59.53),
        ["BY"] = (53.0, 28.0),
        ["BE"] = (50.83, 4.0),
        ["BZ"] = (17.25, -88.75),
        ["BJ"] = (9.5, 2.25),
        ["BM"] = (32.33, -64.75),
        ["BT"] = (27.5, 90.5),
        ["BO"] = (-17.0, -65.0),
        ["BA"] = (44.0, 18.0),
        ["BW"] = (-22.0, 24.0),
        ["BR"] = (-10.0, -55.0),
        ["BN"] = (4.5, 114.67),
        ["BG"] = (43.0, 25.0),
        ["BF"] = (13.0, -2.0),
        ["BI"] = (-3.5, 30.0),
        ["CV"] = (16.0, -24.0),
        ["KH"] = (13.0, 105.0),
        ["CM"] = (6.0, 12.0),
        ["CA"] = (60.0, -95.0),
        ["CF"] = (7.0, 21.0),
        ["TD"] = (15.0, 19.0),
        ["CL"] = (-30.0, -71.0),
        ["CN"] = (35.0, 105.0),
        ["CO"] = (4.0, -72.0),
        ["KM"] = (-12.17, 44.25),
        ["CG"] = (-1.0, 15.0),
        ["CD"] = (0.0, 25.0),
        ["CR"] = (10.0, -84.0),
        ["CI"] = (8.0, -5.0),
        ["HR"] = (45.17, 15.5),
        ["CU"] = (21.5, -80.0),
        ["CW"] = (12.17, -68.98),
        ["CY"] = (35.0, 33.0),
        ["CZ"] = (49.75, 15.5),
        ["DK"] = (56.0, 10.0),
        ["DJ"] = (11.5, 43.0),
        ["DM"] = (15.42, -61.33),
        ["DO"] = (19.0, -70.67),
        ["EC"] = (-2.0, -77.5),
        ["EG"] = (27.0, 30.0),
        ["SV"] = (13.83, -88.92),
        ["GQ"] = (2.0, 10.0),
        ["ER"] = (15.0, 39.0),
        ["EE"] = (59.0, 26.0),
        ["SZ"] = (-26.5, 31.5),
        ["ET"] = (8.0, 38.0),
        ["FJ"] = (-18.0, 175.0),
        ["FI"] = (64.0, 26.0),
        ["FR"] = (46.0, 2.0),
        ["GA"] = (-1.0, 11.75),
        ["GM"] = (13.47, -16.57),
        ["GE"] = (42.0, 43.5),
        ["DE"] = (51.0, 9.0),
        ["GH"] = (8.0, -2.0),
        ["GI"] = (36.13, -5.35),
        ["GR"] = (39.0, 22.0),
        ["GL"] = (72.0, -40.0),
        ["GD"] = (12.12, -61.67),
        ["GU"] = (13.47, 144.78),
        ["GT"] = (15.5, -90.25),
        ["GG"] = (49.45, -2.54),
        ["GN"] = (11.0, -10.0),
        ["GW"] = (12.0, -15.0),
        ["GY"] = (5.0, -59.0),
        ["HT"] = (19.0, -72.42),
        ["HN"] = (15.0, -86.5),
        ["HK"] = (22.25, 114.17),
        ["HU"] = (47.0, 20.0),
        ["IS"] = (65.0, -18.0),
        ["IN"] = (20.0, 77.0),
        ["ID"] = (-5.0, 120.0),
        ["IR"] = (32.0, 53.0),
        ["IQ"] = (33.0, 44.0),
        ["IE"] = (53.0, -8.0),
        ["IM"] = (54.23, -4.55),
        ["IL"] = (31.5, 34.75),
        ["IT"] = (42.83, 12.83),
        ["JM"] = (18.25, -77.5),
        ["JP"] = (36.0, 138.0),
        ["JE"] = (49.21, -2.13),
        ["JO"] = (31.0, 36.0),
        ["KZ"] = (48.0, 68.0),
        ["KE"] = (1.0, 38.0),
        ["KI"] = (1.42, 173.0),
        ["KP"] = (40.0, 127.0),
        ["KR"] = (37.0, 127.5),
        ["KW"] = (29.34, 47.66),
        ["KG"] = (41.0, 75.0),
        ["LA"] = (18.0, 105.0),
        ["LV"] = (57.0, 25.0),
        ["LB"] = (33.83, 35.83),
        ["LS"] = (-29.5, 28.5),
        ["LR"] = (6.5, -9.5),
        ["LY"] = (25.0, 17.0),
        ["LI"] = (47.17, 9.53),
        ["LT"] = (56.0, 24.0),
        ["LU"] = (49.75, 6.17),
        ["MO"] = (22.17, 113.55),
        ["MG"] = (-20.0, 47.0),
        ["MW"] = (-13.5, 34.0),
        ["MY"] = (2.5, 112.5),
        ["MV"] = (3.25, 73.0),
        ["ML"] = (17.0, -4.0),
        ["MT"] = (35.83, 14.58),
        ["MH"] = (9.0, 168.0),
        ["MR"] = (20.0, -12.0),
        ["MU"] = (-20.28, 57.55),
        ["MX"] = (23.0, -102.0),
        ["FM"] = (6.92, 158.25),
        ["MD"] = (47.0, 29.0),
        ["MC"] = (43.73, 7.42),
        ["MN"] = (46.0, 105.0),
        ["ME"] = (42.0, 19.0),
        ["MA"] = (32.0, -5.0),
        ["MZ"] = (-18.25, 35.0),
        ["MM"] = (22.0, 98.0),
        ["NA"] = (-22.0, 17.0),
        ["NR"] = (-0.53, 166.92),
        ["NP"] = (28.0, 84.0),
        ["NL"] = (52.5, 5.75),
        ["NZ"] = (-41.0, 174.0),
        ["NI"] = (13.0, -85.0),
        ["NE"] = (16.0, 8.0),
        ["NG"] = (10.0, 8.0),
        ["MK"] = (41.83, 22.0),
        ["NO"] = (62.0, 10.0),
        ["OM"] = (21.0, 57.0),
        ["PK"] = (30.0, 70.0),
        ["PW"] = (7.5, 134.5),
        ["PS"] = (32.0, 35.25),
        ["PA"] = (9.0, -80.0),
        ["PG"] = (-6.0, 147.0),
        ["PY"] = (-23.0, -58.0),
        ["PE"] = (-10.0, -76.0),
        ["PH"] = (13.0, 122.0),
        ["PL"] = (52.0, 20.0),
        ["PT"] = (39.5, -8.0),
        ["PR"] = (18.25, -66.5),
        ["QA"] = (25.5, 51.25),
        ["RO"] = (46.0, 25.0),
        ["RU"] = (60.0, 100.0),
        ["RW"] = (-2.0, 30.0),
        ["KN"] = (17.33, -62.75),
        ["LC"] = (13.88, -61.13),
        ["VC"] = (13.25, -61.2),
        ["WS"] = (-13.58, -172.33),
        ["SM"] = (43.77, 12.42),
        ["ST"] = (1.0, 7.0),
        ["SA"] = (25.0, 45.0),
        ["SN"] = (14.0, -14.0),
        ["RS"] = (44.0, 21.0),
        ["SC"] = (-4.58, 55.67),
        ["SL"] = (8.5, -11.5),
        ["SG"] = (1.37, 103.8),
        ["SK"] = (48.67, 19.5),
        ["SI"] = (46.12, 14.82),
        ["SB"] = (-8.0, 159.0),
        ["SO"] = (10.0, 49.0),
        ["ZA"] = (-29.0, 24.0),
        ["SS"] = (8.0, 30.0),
        ["ES"] = (40.0, -4.0),
        ["LK"] = (7.0, 81.0),
        ["SD"] = (15.0, 30.0),
        ["SR"] = (4.0, -56.0),
        ["SE"] = (62.0, 15.0),
        ["CH"] = (47.0, 8.0),
        ["SY"] = (35.0, 38.0),
        ["TW"] = (23.5, 121.0),
        ["TJ"] = (39.0, 71.0),
        ["TZ"] = (-6.0, 35.0),
        ["TH"] = (15.0, 100.0),
        ["TL"] = (-8.55, 125.73),
        ["TG"] = (8.0, 1.17),
        ["TO"] = (-20.0, -175.0),
        ["TT"] = (11.0, -61.0),
        ["TN"] = (34.0, 9.0),
        ["TR"] = (39.0, 35.0),
        ["TM"] = (40.0, 60.0),
        ["TV"] = (-8.0, 178.0),
        ["UG"] = (1.0, 32.0),
        ["UA"] = (49.0, 32.0),
        ["AE"] = (24.0, 54.0),
        ["GB"] = (54.0, -2.0),
        ["US"] = (38.0, -97.0),
        ["UY"] = (-33.0, -56.0),
        ["UZ"] = (41.0, 64.0),
        ["VU"] = (-16.0, 167.0),
        ["VA"] = (41.9, 12.45),
        ["VE"] = (8.0, -66.0),
        ["VN"] = (16.0, 106.0),
        ["YE"] = (15.0, 48.0),
        ["ZM"] = (-15.0, 30.0),
        ["ZW"] = (-20.0, 30.0),
        ["AX"] = (60.12, 19.9),
        ["FO"] = (62.0, -7.0),
        ["XK"] = (42.67, 21.17),
    };

    private sealed record CapitalData(string En, string Sv, string Native, double Lat, double Lon);

    /// <summary>
    /// Capital city data for each country, sourced from
    /// <see href="https://www.wikidata.org/wiki/Property:P36">Wikidata P36</see> (capital) and
    /// <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see> (coordinates).
    /// </summary>
    private static readonly Dictionary<string, CapitalData> Capitals = new(StringComparer.Ordinal)
    {
        ["AF"] = new("Kabul", "Kabul", "کابل", 34.53, 69.17),
        ["AL"] = new("Tirana", "Tirana", "Tiranë", 41.33, 19.82),
        ["DZ"] = new("Algiers", "Alger", "الجزائر", 36.75, 3.04),
        ["AD"] = new("Andorra la Vella", "Andorra la Vella", "Andorra la Vella", 42.51, 1.52),
        ["AO"] = new("Luanda", "Luanda", "Luanda", -8.84, 13.23),
        ["AG"] = new("St. John's", "Saint John's", "St. John's", 17.12, -61.85),
        ["AR"] = new("Buenos Aires", "Buenos Aires", "Buenos Aires", -34.61, -58.38),
        ["AM"] = new("Yerevan", "Jerevan", "Երևան", 40.18, 44.51),
        ["AU"] = new("Canberra", "Canberra", "Canberra", -35.28, 149.13),
        ["AT"] = new("Vienna", "Wien", "Wien", 48.21, 16.37),
        ["AZ"] = new("Baku", "Baku", "Bakı", 40.41, 49.87),
        ["BS"] = new("Nassau", "Nassau", "Nassau", 25.06, -77.34),
        ["BH"] = new("Manama", "Manama", "المنامة", 26.23, 50.59),
        ["BD"] = new("Dhaka", "Dhaka", "ঢাকা", 23.71, 90.41),
        ["BB"] = new("Bridgetown", "Bridgetown", "Bridgetown", 13.1, -59.62),
        ["BY"] = new("Minsk", "Minsk", "Мінск", 53.9, 27.57),
        ["BE"] = new("Brussels", "Bryssel", "Bruxelles", 50.85, 4.35),
        ["BZ"] = new("Belmopan", "Belmopan", "Belmopan", 17.25, -88.77),
        ["BJ"] = new("Porto-Novo", "Porto-Novo", "Porto-Novo", 6.5, 2.6),
        ["BT"] = new("Thimphu", "Thimphu", "ཐིམ་ཕུ", 27.47, 89.64),
        ["BO"] = new("Sucre", "Sucre", "Sucre", -19.04, -65.26),
        ["BA"] = new("Sarajevo", "Sarajevo", "Sarajevo", 43.86, 18.41),
        ["BW"] = new("Gaborone", "Gaborone", "Gaborone", -24.65, 25.91),
        ["BR"] = new("Brasília", "Brasília", "Brasília", -15.79, -47.88),
        ["BN"] = new("Bandar Seri Begawan", "Bandar Seri Begawan", "Bandar Seri Begawan", 4.94, 114.95),
        ["BG"] = new("Sofia", "Sofia", "София", 42.7, 23.32),
        ["BF"] = new("Ouagadougou", "Ouagadougou", "Ouagadougou", 12.37, -1.52),
        ["BI"] = new("Gitega", "Gitega", "Gitega", -3.43, 29.93),
        ["CV"] = new("Praia", "Praia", "Praia", 14.92, -23.51),
        ["KH"] = new("Phnom Penh", "Phnom Penh", "ភ្នំពេញ", 11.56, 104.93),
        ["CM"] = new("Yaoundé", "Yaoundé", "Yaoundé", 3.87, 11.52),
        ["CA"] = new("Ottawa", "Ottawa", "Ottawa", 45.42, -75.7),
        ["CF"] = new("Bangui", "Bangui", "Bangui", 4.37, 18.59),
        ["TD"] = new("N'Djamena", "N'Djamena", "N'Djamena", 12.11, 15.05),
        ["CL"] = new("Santiago", "Santiago", "Santiago", -33.45, -70.67),
        ["CN"] = new("Beijing", "Peking", "北京", 39.91, 116.39),
        ["CO"] = new("Bogotá", "Bogotá", "Bogotá", 4.71, -74.07),
        ["KM"] = new("Moroni", "Moroni", "Moroni", -11.7, 43.26),
        ["CG"] = new("Brazzaville", "Brazzaville", "Brazzaville", -4.27, 15.28),
        ["CD"] = new("Kinshasa", "Kinshasa", "Kinshasa", -4.32, 15.31),
        ["CR"] = new("San José", "San José", "San José", 9.93, -84.08),
        ["CI"] = new("Yamoussoukro", "Yamoussoukro", "Yamoussoukro", 6.82, -5.28),
        ["HR"] = new("Zagreb", "Zagreb", "Zagreb", 45.81, 15.98),
        ["CU"] = new("Havana", "Havanna", "La Habana", 23.13, -82.38),
        ["CY"] = new("Nicosia", "Nicosia", "Λευκωσία", 35.17, 33.37),
        ["CZ"] = new("Prague", "Prag", "Praha", 50.09, 14.42),
        ["DK"] = new("Copenhagen", "Köpenhamn", "København", 55.68, 12.57),
        ["DJ"] = new("Djibouti", "Djibouti", "Djibouti", 11.59, 43.15),
        ["DM"] = new("Roseau", "Roseau", "Roseau", 15.3, -61.39),
        ["DO"] = new("Santo Domingo", "Santo Domingo", "Santo Domingo", 18.47, -69.9),
        ["EC"] = new("Quito", "Quito", "Quito", -0.18, -78.47),
        ["EG"] = new("Cairo", "Kairo", "القاهرة", 30.04, 31.24),
        ["SV"] = new("San Salvador", "San Salvador", "San Salvador", 13.69, -89.19),
        ["GQ"] = new("Malabo", "Malabo", "Malabo", 3.75, 8.78),
        ["ER"] = new("Asmara", "Asmara", "ኣስመራ", 15.33, 38.93),
        ["EE"] = new("Tallinn", "Tallinn", "Tallinn", 59.44, 24.75),
        ["SZ"] = new("Mbabane", "Mbabane", "Mbabane", -26.32, 31.13),
        ["ET"] = new("Addis Ababa", "Addis Abeba", "አዲስ አበባ", 9.02, 38.75),
        ["FJ"] = new("Suva", "Suva", "Suva", -18.14, 178.44),
        ["FI"] = new("Helsinki", "Helsingfors", "Helsinki", 60.17, 24.94),
        ["FR"] = new("Paris", "Paris", "Paris", 48.86, 2.35),
        ["GA"] = new("Libreville", "Libreville", "Libreville", 0.39, 9.45),
        ["GM"] = new("Banjul", "Banjul", "Banjul", 13.45, -16.58),
        ["GE"] = new("Tbilisi", "Tbilisi", "თბილისი", 41.69, 44.8),
        ["DE"] = new("Berlin", "Berlin", "Berlin", 52.52, 13.41),
        ["GH"] = new("Accra", "Accra", "Accra", 5.56, -0.19),
        ["GR"] = new("Athens", "Aten", "Αθήνα", 37.98, 23.73),
        ["GD"] = new("St. George's", "Saint George's", "St. George's", 12.05, -61.75),
        ["GT"] = new("Guatemala City", "Guatemala City", "Ciudad de Guatemala", 14.63, -90.51),
        ["GN"] = new("Conakry", "Conakry", "Conakry", 9.54, -13.68),
        ["GW"] = new("Bissau", "Bissau", "Bissau", 11.86, -15.6),
        ["GY"] = new("Georgetown", "Georgetown", "Georgetown", 6.8, -58.16),
        ["HT"] = new("Port-au-Prince", "Port-au-Prince", "Pòtoprens", 18.54, -72.34),
        ["HN"] = new("Tegucigalpa", "Tegucigalpa", "Tegucigalpa", 14.1, -87.22),
        ["HK"] = new("Hong Kong", "Hongkong", "香港", 22.28, 114.16),
        ["HU"] = new("Budapest", "Budapest", "Budapest", 47.5, 19.04),
        ["IS"] = new("Reykjavik", "Reykjavik", "Reykjavík", 64.14, -21.9),
        ["IN"] = new("New Delhi", "New Delhi", "नई दिल्ली", 28.61, 77.21),
        ["ID"] = new("Jakarta", "Jakarta", "Jakarta", -6.21, 106.85),
        ["IR"] = new("Tehran", "Teheran", "تهران", 35.69, 51.39),
        ["IQ"] = new("Baghdad", "Bagdad", "بغداد", 33.34, 44.4),
        ["IE"] = new("Dublin", "Dublin", "Baile Átha Cliath", 53.35, -6.26),
        ["IL"] = new("Jerusalem", "Jerusalem", "ירושלים", 31.77, 35.23),
        ["IT"] = new("Rome", "Rom", "Roma", 41.9, 12.5),
        ["JM"] = new("Kingston", "Kingston", "Kingston", 18.0, -76.79),
        ["JP"] = new("Tokyo", "Tokyo", "東京", 35.68, 139.69),
        ["JO"] = new("Amman", "Amman", "عمّان", 31.95, 35.93),
        ["KZ"] = new("Astana", "Astana", "Астана", 51.17, 71.43),
        ["KE"] = new("Nairobi", "Nairobi", "Nairobi", -1.29, 36.82),
        ["KI"] = new("Tarawa", "Tarawa", "Tarawa", 1.45, 173.0),
        ["KP"] = new("Pyongyang", "Pyongyang", "평양", 39.02, 125.75),
        ["KR"] = new("Seoul", "Seoul", "서울", 37.57, 126.98),
        ["KW"] = new("Kuwait City", "Kuwait City", "مدينة الكويت", 29.37, 47.98),
        ["KG"] = new("Bishkek", "Bisjkek", "Бишкек", 42.87, 74.59),
        ["LA"] = new("Vientiane", "Vientiane", "ວຽງຈັນ", 17.97, 102.63),
        ["LV"] = new("Riga", "Riga", "Rīga", 56.95, 24.11),
        ["LB"] = new("Beirut", "Beirut", "بيروت", 33.89, 35.5),
        ["LS"] = new("Maseru", "Maseru", "Maseru", -29.31, 27.48),
        ["LR"] = new("Monrovia", "Monrovia", "Monrovia", 6.3, -10.8),
        ["LY"] = new("Tripoli", "Tripoli", "طرابلس", 32.9, 13.18),
        ["LI"] = new("Vaduz", "Vaduz", "Vaduz", 47.14, 9.52),
        ["LT"] = new("Vilnius", "Vilnius", "Vilnius", 54.69, 25.28),
        ["LU"] = new("Luxembourg City", "Luxemburg", "Lëtzebuerg", 49.61, 6.13),
        ["MO"] = new("Macao", "Macao", "澳門", 22.2, 113.54),
        ["MG"] = new("Antananarivo", "Antananarivo", "Antananarivo", -18.91, 47.53),
        ["MW"] = new("Lilongwe", "Lilongwe", "Lilongwe", -13.97, 33.79),
        ["MY"] = new("Kuala Lumpur", "Kuala Lumpur", "Kuala Lumpur", 3.14, 101.69),
        ["MV"] = new("Malé", "Malé", "މާލެ", 4.18, 73.51),
        ["ML"] = new("Bamako", "Bamako", "Bamako", 12.65, -8.0),
        ["MT"] = new("Valletta", "Valletta", "Valletta", 35.9, 14.51),
        ["MH"] = new("Majuro", "Majuro", "Majuro", 7.09, 171.38),
        ["MR"] = new("Nouakchott", "Nouakchott", "نواكشوط", 18.09, -15.98),
        ["MU"] = new("Port Louis", "Port Louis", "Port Louis", -20.16, 57.5),
        ["MX"] = new("Mexico City", "Mexico City", "Ciudad de México", 19.43, -99.13),
        ["FM"] = new("Palikir", "Palikir", "Palikir", 6.92, 158.16),
        ["MD"] = new("Chișinău", "Chișinău", "Chișinău", 47.01, 28.86),
        ["MC"] = new("Monaco", "Monaco", "Monaco", 43.73, 7.42),
        ["MN"] = new("Ulaanbaatar", "Ulaanbaatar", "Улаанбаатар", 47.91, 106.91),
        ["ME"] = new("Podgorica", "Podgorica", "Podgorica", 42.44, 19.26),
        ["MA"] = new("Rabat", "Rabat", "الرباط", 34.01, -6.84),
        ["MZ"] = new("Maputo", "Maputo", "Maputo", -25.97, 32.57),
        ["MM"] = new("Naypyidaw", "Naypyidaw", "နေပြည်တော်", 19.76, 96.07),
        ["NA"] = new("Windhoek", "Windhoek", "Windhoek", -22.57, 17.08),
        ["NR"] = new("Yaren", "Yaren", "Yaren", -0.55, 166.93),
        ["NP"] = new("Kathmandu", "Kathmandu", "काठमाडौं", 27.7, 85.32),
        ["NL"] = new("Amsterdam", "Amsterdam", "Amsterdam", 52.37, 4.9),
        ["NZ"] = new("Wellington", "Wellington", "Wellington", -41.29, 174.78),
        ["NI"] = new("Managua", "Managua", "Managua", 12.15, -86.27),
        ["NE"] = new("Niamey", "Niamey", "Niamey", 13.51, 2.11),
        ["NG"] = new("Abuja", "Abuja", "Abuja", 9.06, 7.49),
        ["MK"] = new("Skopje", "Skopje", "Скопје", 42.0, 21.43),
        ["NO"] = new("Oslo", "Oslo", "Oslo", 59.91, 10.75),
        ["OM"] = new("Muscat", "Muskat", "مسقط", 23.59, 58.54),
        ["PK"] = new("Islamabad", "Islamabad", "اسلام آباد", 33.69, 73.04),
        ["PW"] = new("Ngerulmud", "Ngerulmud", "Ngerulmud", 7.5, 134.62),
        ["PS"] = new("Ramallah", "Ramallah", "رام الله", 31.9, 35.2),
        ["PA"] = new("Panama City", "Panama City", "Ciudad de Panamá", 8.98, -79.52),
        ["PG"] = new("Port Moresby", "Port Moresby", "Port Moresby", -9.48, 147.15),
        ["PY"] = new("Asunción", "Asunción", "Asunción", -25.26, -57.58),
        ["PE"] = new("Lima", "Lima", "Lima", -12.05, -77.04),
        ["PH"] = new("Manila", "Manila", "Maynila", 14.6, 120.98),
        ["PL"] = new("Warsaw", "Warszawa", "Warszawa", 52.23, 21.01),
        ["PT"] = new("Lisbon", "Lissabon", "Lisboa", 38.72, -9.14),
        ["PR"] = new("San Juan", "San Juan", "San Juan", 18.47, -66.11),
        ["QA"] = new("Doha", "Doha", "الدوحة", 25.29, 51.53),
        ["RO"] = new("Bucharest", "Bukarest", "București", 44.43, 26.1),
        ["RU"] = new("Moscow", "Moskva", "Москва", 55.76, 37.62),
        ["RW"] = new("Kigali", "Kigali", "Kigali", -1.94, 30.06),
        ["KN"] = new("Basseterre", "Basseterre", "Basseterre", 17.3, -62.72),
        ["LC"] = new("Castries", "Castries", "Castries", 14.01, -61.0),
        ["VC"] = new("Kingstown", "Kingstown", "Kingstown", 13.15, -61.22),
        ["WS"] = new("Apia", "Apia", "Apia", -13.83, -171.76),
        ["SM"] = new("San Marino", "San Marino", "San Marino", 43.94, 12.45),
        ["ST"] = new("São Tomé", "São Tomé", "São Tomé", 0.34, 6.73),
        ["SA"] = new("Riyadh", "Riyadh", "الرياض", 24.69, 46.72),
        ["SN"] = new("Dakar", "Dakar", "Dakar", 14.69, -17.44),
        ["RS"] = new("Belgrade", "Belgrad", "Београд", 44.82, 20.46),
        ["SC"] = new("Victoria", "Victoria", "Victoria", -4.62, 55.45),
        ["SL"] = new("Freetown", "Freetown", "Freetown", 8.48, -13.23),
        ["SG"] = new("Singapore", "Singapore", "Singapore", 1.29, 103.85),
        ["SK"] = new("Bratislava", "Bratislava", "Bratislava", 48.14, 17.11),
        ["SI"] = new("Ljubljana", "Ljubljana", "Ljubljana", 46.05, 14.51),
        ["SB"] = new("Honiara", "Honiara", "Honiara", -9.43, 159.96),
        ["SO"] = new("Mogadishu", "Mogadishu", "Muqdisho", 2.05, 45.32),
        ["ZA"] = new("Pretoria", "Pretoria", "Pretoria", -25.75, 28.19),
        ["SS"] = new("Juba", "Juba", "Juba", 4.85, 31.58),
        ["ES"] = new("Madrid", "Madrid", "Madrid", 40.42, -3.7),
        ["LK"] = new("Sri Jayawardenepura Kotte", "Sri Jayawardenepura Kotte", "ශ්‍රී ජයවර්ධනපුර කෝට්ටේ", 6.89, 79.92),
        ["SD"] = new("Khartoum", "Khartoum", "الخرطوم", 15.59, 32.53),
        ["SR"] = new("Paramaribo", "Paramaribo", "Paramaribo", 5.83, -55.17),
        ["SE"] = new("Stockholm", "Stockholm", "Stockholm", 59.33, 18.07),
        ["CH"] = new("Bern", "Bern", "Bern", 46.95, 7.45),
        ["SY"] = new("Damascus", "Damaskus", "دمشق", 33.51, 36.29),
        ["TW"] = new("Taipei", "Taipei", "臺北", 25.03, 121.57),
        ["TJ"] = new("Dushanbe", "Dusjanbe", "Душанбе", 38.56, 68.77),
        ["TZ"] = new("Dodoma", "Dodoma", "Dodoma", -6.16, 35.75),
        ["TH"] = new("Bangkok", "Bangkok", "กรุงเทพมหานคร", 13.76, 100.5),
        ["TL"] = new("Dili", "Dili", "Dili", -8.56, 125.57),
        ["TG"] = new("Lomé", "Lomé", "Lomé", 6.14, 1.21),
        ["TO"] = new("Nukuʻalofa", "Nukuʻalofa", "Nukuʻalofa", -21.21, -175.2),
        ["TT"] = new("Port of Spain", "Port of Spain", "Port of Spain", 10.66, -61.51),
        ["TN"] = new("Tunis", "Tunis", "تونس", 36.81, 10.17),
        ["TR"] = new("Ankara", "Ankara", "Ankara", 39.93, 32.86),
        ["TM"] = new("Ashgabat", "Asjchabad", "Aşgabat", 37.95, 58.38),
        ["TV"] = new("Funafuti", "Funafuti", "Funafuti", -8.52, 179.2),
        ["UG"] = new("Kampala", "Kampala", "Kampala", 0.31, 32.58),
        ["UA"] = new("Kyiv", "Kiev", "Київ", 50.45, 30.52),
        ["AE"] = new("Abu Dhabi", "Abu Dhabi", "أبو ظبي", 24.45, 54.65),
        ["GB"] = new("London", "London", "London", 51.51, -0.13),
        ["US"] = new("Washington, D.C.", "Washington, D.C.", "Washington, D.C.", 38.9, -77.04),
        ["UY"] = new("Montevideo", "Montevideo", "Montevideo", -34.88, -56.19),
        ["UZ"] = new("Tashkent", "Tasjkent", "Toshkent", 41.3, 69.28),
        ["VU"] = new("Port Vila", "Port Vila", "Port Vila", -17.74, 168.31),
        ["VA"] = new("Vatican City", "Vatikanstaden", "Città del Vaticano", 41.9, 12.45),
        ["VE"] = new("Caracas", "Caracas", "Caracas", 10.49, -66.9),
        ["VN"] = new("Hanoi", "Hanoi", "Hà Nội", 21.03, 105.85),
        ["YE"] = new("Sana'a", "Sana'a", "صنعاء", 15.35, 44.21),
        ["ZM"] = new("Lusaka", "Lusaka", "Lusaka", -15.39, 28.32),
        ["ZW"] = new("Harare", "Harare", "Harare", -17.83, 31.05),
        ["AX"] = new("Mariehamn", "Mariehamn", "Mariehamn", 60.1, 19.94),
        ["FO"] = new("Tórshavn", "Torshamn", "Tórshavn", 62.01, -6.77),
        ["XK"] = new("Pristina", "Pristina", "Prishtinë", 42.66, 21.17),
        ["GI"] = new("Gibraltar", "Gibraltar", "Gibraltar", 36.14, -5.35),
        ["GL"] = new("Nuuk", "Nuuk", "Nuuk", 64.17, -51.74),
        ["GG"] = new("St. Peter Port", "Saint Peter Port", "St. Peter Port", 49.46, -2.54),
        ["IM"] = new("Douglas", "Douglas", "Douglas", 54.15, -4.48),
        ["JE"] = new("St. Helier", "Saint Helier", "St. Helier", 49.19, -2.1),
    };

    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string NativeName { get; }
    public string Alpha2Code { get; }
    public string Alpha3Code { get; }
    public int NumericCode { get; }

    /// <summary>Phone country calling code (e.g. <c>+46</c> for Sweden, <c>+1</c> for US/Canada).</summary>
    public PhoneCallingCode CallingCode { get; }

    /// <summary>Country-code top-level domain including the dot (e.g. ".se" for Sweden).</summary>
    public string TopLevelDomain { get; }

    public Continent Continent { get; }

    /// <summary>Country name in the current display language, for example <c>Tyskland</c> or <c>Germany</c> depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>True if the country is one of the current European Union member states.</summary>
    public bool IsInEuropeanUnion => EuropeanUnionCountries.Contains(Alpha2Code);

    /// <summary>True if the country is in the European Economic Area (EU plus Iceland, Liechtenstein, and Norway).</summary>
    public bool IsInEea => EeaCountries.Contains(Alpha2Code);

    /// <summary>True if the country participates in the Schengen area.</summary>
    public bool IsInSchengen => SchengenCountries.Contains(Alpha2Code);

    /// <summary>True for the five Nordic countries: Denmark, Finland, Iceland, Norway, and Sweden.</summary>
    public bool IsInNordics => NordicCountries.Contains(Alpha2Code);

    /// <summary>True if the country's primary currency is the euro (<c>EUR</c>).</summary>
    public bool UsesEuro => CurrencyCodes.Contains("EUR");

    /// <summary>True if the country is in the SEPA geographical scope used for euro payments.</summary>
    public bool IsInSepa => SepaCountries.Contains(Alpha2Code);

    /// <summary>Primary ISO 4217 currency code when known, for example <c>SEK</c> or <c>EUR</c>.</summary>
    public string? CurrencyCode { get; }

    /// <summary>
    /// Known ISO 4217 currency codes for the country. This is typically a single-item list for the current
    /// country-based model, for example <c>["SEK"]</c> or <c>["EUR"]</c>.
    /// </summary>
    public IReadOnlyList<string> CurrencyCodes { get; }

    /// <summary>
    /// The primary legal-tender currency, or <see langword="null"/> when unknown. For most countries
    /// this is the sole currency. When a country uses multiple currencies (e.g. Cuba with CUP and CUC,
    /// or Panama with PAB and USD), this returns the first one; use <see cref="Currencies"/> for the full list.
    /// </summary>
    public Currency? Currency { get; }

    /// <summary>
    /// All recognized currencies for this country. For the vast majority of countries this list has a single
    /// entry identical to <see cref="Currency"/>. Countries with multiple legal tenders (e.g. Cuba, Panama,
    /// Namibia) will have more than one entry. Only includes currencies present in <see cref="Currency.All"/>.
    /// </summary>
    public IReadOnlyList<Currency> Currencies { get; }

    /// <summary>
    /// The primary official language of the country, or <see langword="null"/> when unknown.
    /// For countries with multiple official languages (e.g. Belgium, Switzerland), this is the most
    /// widely spoken one; use <see cref="OfficialLanguages"/> for the full list.
    /// </summary>
    public Language? PrimaryLanguage { get; }

    /// <summary>
    /// Official languages of the country. For most countries this is a single-item list identical
    /// to <see cref="PrimaryLanguage"/>. Countries with multiple official languages (e.g. Belgium
    /// with Dutch, French, and German) will have more than one entry.
    /// </summary>
    public IReadOnlyList<Language> OfficialLanguages { get; }

    /// <summary>
    /// The approximate geographic coordinate (WGS 84) of the country, e.g. <c>62°N, 15°E</c> for Sweden.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public GeoCoordinate Coordinate { get; }

    /// <summary>The approximate latitude (WGS 84) of the country, e.g. <c>62.0</c> for Sweden.</summary>
    public double Latitude => Coordinate.Latitude;

    /// <summary>The approximate longitude (WGS 84) of the country, e.g. <c>15.0</c> for Sweden.</summary>
    public double Longitude => Coordinate.Longitude;

    /// <summary>
    /// The capital city of the country, or <see langword="null"/> for territories without a capital.
    /// Includes English, Swedish, and native names plus geographic coordinates.
    /// </summary>
    public CountryCapital? Capital { get; }

    public string Value => Alpha2Code;
    public string Formatted => EnglishName;

    private Country(string alpha2, string alpha3, int numeric, string englishName, string localizedName, string localName, string callingCode, string tld, Continent continent)
    {
        Alpha2Code = alpha2;
        Alpha3Code = alpha3;
        NumericCode = numeric;
        EnglishName = englishName;
        LocalizedName = localizedName;
        NativeName = localName;
        CallingCode = PhoneCallingCode.Parse(callingCode);
        TopLevelDomain = tld;
        Continent = continent;
        CurrencyCodes = ResolveCurrencyCodes(alpha2);
        CurrencyCode = CurrencyCodes.Count > 0 ? CurrencyCodes[0] : null;
        Currencies = CurrencyCodes
            .Select(c => Currency.TryParse(c, out var cur) ? cur : null)
            .Where(c => c != null)
            .ToList()!;
        Currency = Currencies.Count > 0 ? Currencies[0] : null;
        var langCodes = ResolveLanguageCodes(alpha2);
        OfficialLanguages = langCodes
            .Select(c => Language.TryParse(c, out var lang) ? lang : null)
            .Where(l => l != null)
            .ToList()!;
        PrimaryLanguage = OfficialLanguages.Count > 0 ? OfficialLanguages[0] : null;

        var (lat, lon) = Coordinates.GetValueOrDefault(alpha2);
        Coordinate = GeoCoordinate.Create(lat, lon);

        if (Capitals.TryGetValue(alpha2, out var cap))
            Capital = new CountryCapital(cap.En, cap.Sv, cap.Native, GeoCoordinate.Create(cap.Lat, cap.Lon));
    }

    private static Country C(string a2, string a3, int n, string en, string sv, string local, string cc, string tld, Continent cont)
        => new(a2, a3, n, en, sv, local, cc, tld, cont);

    static Country()
    {
        AllCountries =
        [
            // A
            C("AF", "AFG", 4, "Afghanistan", "Afghanistan", "افغانستان", "93", ".af", Continent.Asia),
            C("AL", "ALB", 8, "Albania", "Albanien", "Shqipëria", "355", ".al", Continent.Europe),
            C("DZ", "DZA", 12, "Algeria", "Algeriet", "الجزائر", "213", ".dz", Continent.Africa),
            C("AS", "ASM", 16, "American Samoa", "Amerikanska Samoa", "American Samoa", "1", ".as", Continent.Oceania),
            C("AD", "AND", 20, "Andorra", "Andorra", "Andorra", "376", ".ad", Continent.Europe),
            C("AO", "AGO", 24, "Angola", "Angola", "Angola", "244", ".ao", Continent.Africa),
            C("AG", "ATG", 28, "Antigua and Barbuda", "Antigua och Barbuda", "Antigua and Barbuda", "1", ".ag", Continent.NorthAmerica),
            C("AR", "ARG", 32, "Argentina", "Argentina", "Argentina", "54", ".ar", Continent.SouthAmerica),
            C("AM", "ARM", 51, "Armenia", "Armenien", "Հայաստան", "374", ".am", Continent.Asia),
            C("AW", "ABW", 533, "Aruba", "Aruba", "Aruba", "297", ".aw", Continent.NorthAmerica),
            C("AU", "AUS", 36, "Australia", "Australien", "Australia", "61", ".au", Continent.Oceania),
            C("AT", "AUT", 40, "Austria", "Österrike", "Österreich", "43", ".at", Continent.Europe),
            C("AZ", "AZE", 31, "Azerbaijan", "Azerbajdzjan", "Azərbaycan", "994", ".az", Continent.Asia),

            // B
            C("BS", "BHS", 44, "Bahamas", "Bahamas", "Bahamas", "1", ".bs", Continent.NorthAmerica),
            C("BH", "BHR", 48, "Bahrain", "Bahrain", "البحرين", "973", ".bh", Continent.Asia),
            C("BD", "BGD", 50, "Bangladesh", "Bangladesh", "বাংলাদেশ", "880", ".bd", Continent.Asia),
            C("BB", "BRB", 52, "Barbados", "Barbados", "Barbados", "1", ".bb", Continent.NorthAmerica),
            C("BY", "BLR", 112, "Belarus", "Belarus", "Беларусь", "375", ".by", Continent.Europe),
            C("BE", "BEL", 56, "Belgium", "Belgien", "België", "32", ".be", Continent.Europe),
            C("BZ", "BLZ", 84, "Belize", "Belize", "Belize", "501", ".bz", Continent.NorthAmerica),
            C("BJ", "BEN", 204, "Benin", "Benin", "Bénin", "229", ".bj", Continent.Africa),
            C("BM", "BMU", 60, "Bermuda", "Bermuda", "Bermuda", "1", ".bm", Continent.NorthAmerica),
            C("BT", "BTN", 64, "Bhutan", "Bhutan", "འབྲུག", "975", ".bt", Continent.Asia),
            C("BO", "BOL", 68, "Bolivia", "Bolivia", "Bolivia", "591", ".bo", Continent.SouthAmerica),
            C("BA", "BIH", 70, "Bosnia and Herzegovina", "Bosnien och Hercegovina", "Bosna i Hercegovina", "387", ".ba", Continent.Europe),
            C("BW", "BWA", 72, "Botswana", "Botswana", "Botswana", "267", ".bw", Continent.Africa),
            C("BR", "BRA", 76, "Brazil", "Brasilien", "Brasil", "55", ".br", Continent.SouthAmerica),
            C("BN", "BRN", 96, "Brunei", "Brunei", "Brunei", "673", ".bn", Continent.Asia),
            C("BG", "BGR", 100, "Bulgaria", "Bulgarien", "България", "359", ".bg", Continent.Europe),
            C("BF", "BFA", 854, "Burkina Faso", "Burkina Faso", "Burkina Faso", "226", ".bf", Continent.Africa),
            C("BI", "BDI", 108, "Burundi", "Burundi", "Burundi", "257", ".bi", Continent.Africa),

            // C
            C("CV", "CPV", 132, "Cabo Verde", "Kap Verde", "Cabo Verde", "238", ".cv", Continent.Africa),
            C("KH", "KHM", 116, "Cambodia", "Kambodja", "កម្ពុជា", "855", ".kh", Continent.Asia),
            C("CM", "CMR", 120, "Cameroon", "Kamerun", "Cameroun", "237", ".cm", Continent.Africa),
            C("CA", "CAN", 124, "Canada", "Kanada", "Canada", "1", ".ca", Continent.NorthAmerica),
            C("CF", "CAF", 140, "Central African Republic", "Centralafrikanska republiken", "République centrafricaine", "236", ".cf", Continent.Africa),
            C("TD", "TCD", 148, "Chad", "Tchad", "Tchad", "235", ".td", Continent.Africa),
            C("CL", "CHL", 152, "Chile", "Chile", "Chile", "56", ".cl", Continent.SouthAmerica),
            C("CN", "CHN", 156, "China", "Kina", "中国", "86", ".cn", Continent.Asia),
            C("CO", "COL", 170, "Colombia", "Colombia", "Colombia", "57", ".co", Continent.SouthAmerica),
            C("KM", "COM", 174, "Comoros", "Komorerna", "Komori", "269", ".km", Continent.Africa),
            C("CG", "COG", 178, "Congo", "Kongo", "Congo", "242", ".cg", Continent.Africa),
            C("CD", "COD", 180, "Congo (Democratic Republic)", "Demokratiska republiken Kongo", "République démocratique du Congo", "243", ".cd", Continent.Africa),
            C("CR", "CRI", 188, "Costa Rica", "Costa Rica", "Costa Rica", "506", ".cr", Continent.NorthAmerica),
            C("CI", "CIV", 384, "Côte d'Ivoire", "Elfenbenskusten", "Côte d'Ivoire", "225", ".ci", Continent.Africa),
            C("HR", "HRV", 191, "Croatia", "Kroatien", "Hrvatska", "385", ".hr", Continent.Europe),
            C("CU", "CUB", 192, "Cuba", "Kuba", "Cuba", "53", ".cu", Continent.NorthAmerica),
            C("CW", "CUW", 531, "Curaçao", "Curaçao", "Curaçao", "599", ".cw", Continent.NorthAmerica),
            C("CY", "CYP", 196, "Cyprus", "Cypern", "Κύπρος", "357", ".cy", Continent.Europe),
            C("CZ", "CZE", 203, "Czechia", "Tjeckien", "Česko", "420", ".cz", Continent.Europe),

            // D
            C("DK", "DNK", 208, "Denmark", "Danmark", "Danmark", "45", ".dk", Continent.Europe),
            C("DJ", "DJI", 262, "Djibouti", "Djibouti", "Djibouti", "253", ".dj", Continent.Africa),
            C("DM", "DMA", 212, "Dominica", "Dominica", "Dominica", "1", ".dm", Continent.NorthAmerica),
            C("DO", "DOM", 214, "Dominican Republic", "Dominikanska republiken", "República Dominicana", "1", ".do", Continent.NorthAmerica),

            // E
            C("EC", "ECU", 218, "Ecuador", "Ecuador", "Ecuador", "593", ".ec", Continent.SouthAmerica),
            C("EG", "EGY", 818, "Egypt", "Egypten", "مصر", "20", ".eg", Continent.Africa),
            C("SV", "SLV", 222, "El Salvador", "El Salvador", "El Salvador", "503", ".sv", Continent.NorthAmerica),
            C("GQ", "GNQ", 226, "Equatorial Guinea", "Ekvatorialguinea", "Guinea Ecuatorial", "240", ".gq", Continent.Africa),
            C("ER", "ERI", 232, "Eritrea", "Eritrea", "ኤርትራ", "291", ".er", Continent.Africa),
            C("EE", "EST", 233, "Estonia", "Estland", "Eesti", "372", ".ee", Continent.Europe),
            C("SZ", "SWZ", 748, "Eswatini", "Eswatini", "eSwatini", "268", ".sz", Continent.Africa),
            C("ET", "ETH", 231, "Ethiopia", "Etiopien", "ኢትዮጵያ", "251", ".et", Continent.Africa),

            // F
            C("FJ", "FJI", 242, "Fiji", "Fiji", "Fiji", "679", ".fj", Continent.Oceania),
            C("FI", "FIN", 246, "Finland", "Finland", "Suomi", "358", ".fi", Continent.Europe),
            C("FR", "FRA", 250, "France", "Frankrike", "France", "33", ".fr", Continent.Europe),

            // G
            C("GA", "GAB", 266, "Gabon", "Gabon", "Gabon", "241", ".ga", Continent.Africa),
            C("GM", "GMB", 270, "Gambia", "Gambia", "Gambia", "220", ".gm", Continent.Africa),
            C("GE", "GEO", 268, "Georgia", "Georgien", "საქართველო", "995", ".ge", Continent.Asia),
            C("DE", "DEU", 276, "Germany", "Tyskland", "Deutschland", "49", ".de", Continent.Europe),
            C("GH", "GHA", 288, "Ghana", "Ghana", "Ghana", "233", ".gh", Continent.Africa),
            C("GI", "GIB", 292, "Gibraltar", "Gibraltar", "Gibraltar", "350", ".gi", Continent.Europe),
            C("GR", "GRC", 300, "Greece", "Grekland", "Ελλάδα", "30", ".gr", Continent.Europe),
            C("GL", "GRL", 304, "Greenland", "Grönland", "Kalaallit Nunaat", "299", ".gl", Continent.NorthAmerica),
            C("GD", "GRD", 308, "Grenada", "Grenada", "Grenada", "1", ".gd", Continent.NorthAmerica),
            C("GU", "GUM", 316, "Guam", "Guam", "Guam", "1", ".gu", Continent.Oceania),
            C("GT", "GTM", 320, "Guatemala", "Guatemala", "Guatemala", "502", ".gt", Continent.NorthAmerica),
            C("GG", "GGY", 831, "Guernsey", "Guernsey", "Guernsey", "44", ".gg", Continent.Europe),
            C("GN", "GIN", 324, "Guinea", "Guinea", "Guinée", "224", ".gn", Continent.Africa),
            C("GW", "GNB", 624, "Guinea-Bissau", "Guinea-Bissau", "Guiné-Bissau", "245", ".gw", Continent.Africa),
            C("GY", "GUY", 328, "Guyana", "Guyana", "Guyana", "592", ".gy", Continent.SouthAmerica),

            // H
            C("HT", "HTI", 332, "Haiti", "Haiti", "Haïti", "509", ".ht", Continent.NorthAmerica),
            C("HN", "HND", 340, "Honduras", "Honduras", "Honduras", "504", ".hn", Continent.NorthAmerica),
            C("HK", "HKG", 344, "Hong Kong", "Hongkong", "香港", "852", ".hk", Continent.Asia),
            C("HU", "HUN", 348, "Hungary", "Ungern", "Magyarország", "36", ".hu", Continent.Europe),

            // I
            C("IS", "ISL", 352, "Iceland", "Island", "Ísland", "354", ".is", Continent.Europe),
            C("IN", "IND", 356, "India", "Indien", "भारत", "91", ".in", Continent.Asia),
            C("ID", "IDN", 360, "Indonesia", "Indonesien", "Indonesia", "62", ".id", Continent.Asia),
            C("IR", "IRN", 364, "Iran", "Iran", "ایران", "98", ".ir", Continent.Asia),
            C("IQ", "IRQ", 368, "Iraq", "Irak", "العراق", "964", ".iq", Continent.Asia),
            C("IE", "IRL", 372, "Ireland", "Irland", "Éire", "353", ".ie", Continent.Europe),
            C("IM", "IMN", 833, "Isle of Man", "Isle of Man", "Isle of Man", "44", ".im", Continent.Europe),
            C("IL", "ISR", 376, "Israel", "Israel", "ישראל", "972", ".il", Continent.Asia),
            C("IT", "ITA", 380, "Italy", "Italien", "Italia", "39", ".it", Continent.Europe),

            // J
            C("JM", "JAM", 388, "Jamaica", "Jamaica", "Jamaica", "1", ".jm", Continent.NorthAmerica),
            C("JP", "JPN", 392, "Japan", "Japan", "日本", "81", ".jp", Continent.Asia),
            C("JE", "JEY", 832, "Jersey", "Jersey", "Jersey", "44", ".je", Continent.Europe),
            C("JO", "JOR", 400, "Jordan", "Jordanien", "الأردن", "962", ".jo", Continent.Asia),

            // K
            C("KZ", "KAZ", 398, "Kazakhstan", "Kazakstan", "Қазақстан", "7", ".kz", Continent.Asia),
            C("KE", "KEN", 404, "Kenya", "Kenya", "Kenya", "254", ".ke", Continent.Africa),
            C("KI", "KIR", 296, "Kiribati", "Kiribati", "Kiribati", "686", ".ki", Continent.Oceania),
            C("KP", "PRK", 408, "North Korea", "Nordkorea", "조선", "850", ".kp", Continent.Asia),
            C("KR", "KOR", 410, "South Korea", "Sydkorea", "대한민국", "82", ".kr", Continent.Asia),
            C("KW", "KWT", 414, "Kuwait", "Kuwait", "الكويت", "965", ".kw", Continent.Asia),
            C("KG", "KGZ", 417, "Kyrgyzstan", "Kirgizistan", "Кыргызстан", "996", ".kg", Continent.Asia),

            // L
            C("LA", "LAO", 418, "Laos", "Laos", "ລາວ", "856", ".la", Continent.Asia),
            C("LV", "LVA", 428, "Latvia", "Lettland", "Latvija", "371", ".lv", Continent.Europe),
            C("LB", "LBN", 422, "Lebanon", "Libanon", "لبنان", "961", ".lb", Continent.Asia),
            C("LS", "LSO", 426, "Lesotho", "Lesotho", "Lesotho", "266", ".ls", Continent.Africa),
            C("LR", "LBR", 430, "Liberia", "Liberia", "Liberia", "231", ".lr", Continent.Africa),
            C("LY", "LBY", 434, "Libya", "Libyen", "ليبيا", "218", ".ly", Continent.Africa),
            C("LI", "LIE", 438, "Liechtenstein", "Liechtenstein", "Liechtenstein", "423", ".li", Continent.Europe),
            C("LT", "LTU", 440, "Lithuania", "Litauen", "Lietuva", "370", ".lt", Continent.Europe),
            C("LU", "LUX", 442, "Luxembourg", "Luxemburg", "Lëtzebuerg", "352", ".lu", Continent.Europe),

            // M
            C("MO", "MAC", 446, "Macao", "Macao", "澳門", "853", ".mo", Continent.Asia),
            C("MG", "MDG", 450, "Madagascar", "Madagaskar", "Madagasikara", "261", ".mg", Continent.Africa),
            C("MW", "MWI", 454, "Malawi", "Malawi", "Malawi", "265", ".mw", Continent.Africa),
            C("MY", "MYS", 458, "Malaysia", "Malaysia", "Malaysia", "60", ".my", Continent.Asia),
            C("MV", "MDV", 462, "Maldives", "Maldiverna", "ދިވެހިރާއްޖެ", "960", ".mv", Continent.Asia),
            C("ML", "MLI", 466, "Mali", "Mali", "Mali", "223", ".ml", Continent.Africa),
            C("MT", "MLT", 470, "Malta", "Malta", "Malta", "356", ".mt", Continent.Europe),
            C("MH", "MHL", 584, "Marshall Islands", "Marshallöarna", "Marshall Islands", "692", ".mh", Continent.Oceania),
            C("MR", "MRT", 478, "Mauritania", "Mauretanien", "موريتانيا", "222", ".mr", Continent.Africa),
            C("MU", "MUS", 480, "Mauritius", "Mauritius", "Maurice", "230", ".mu", Continent.Africa),
            C("MX", "MEX", 484, "Mexico", "Mexiko", "México", "52", ".mx", Continent.NorthAmerica),
            C("FM", "FSM", 583, "Micronesia", "Mikronesien", "Micronesia", "691", ".fm", Continent.Oceania),
            C("MD", "MDA", 498, "Moldova", "Moldavien", "Moldova", "373", ".md", Continent.Europe),
            C("MC", "MCO", 492, "Monaco", "Monaco", "Monaco", "377", ".mc", Continent.Europe),
            C("MN", "MNG", 496, "Mongolia", "Mongoliet", "Монгол", "976", ".mn", Continent.Asia),
            C("ME", "MNE", 499, "Montenegro", "Montenegro", "Crna Gora", "382", ".me", Continent.Europe),
            C("MA", "MAR", 504, "Morocco", "Marocko", "المغرب", "212", ".ma", Continent.Africa),
            C("MZ", "MOZ", 508, "Mozambique", "Moçambique", "Moçambique", "258", ".mz", Continent.Africa),
            C("MM", "MMR", 104, "Myanmar", "Myanmar", "မြန်မာ", "95", ".mm", Continent.Asia),

            // N
            C("NA", "NAM", 516, "Namibia", "Namibia", "Namibia", "264", ".na", Continent.Africa),
            C("NR", "NRU", 520, "Nauru", "Nauru", "Nauru", "674", ".nr", Continent.Oceania),
            C("NP", "NPL", 524, "Nepal", "Nepal", "नेपाल", "977", ".np", Continent.Asia),
            C("NL", "NLD", 528, "Netherlands", "Nederländerna", "Nederland", "31", ".nl", Continent.Europe),
            C("NZ", "NZL", 554, "New Zealand", "Nya Zeeland", "Aotearoa", "64", ".nz", Continent.Oceania),
            C("NI", "NIC", 558, "Nicaragua", "Nicaragua", "Nicaragua", "505", ".ni", Continent.NorthAmerica),
            C("NE", "NER", 562, "Niger", "Niger", "Niger", "227", ".ne", Continent.Africa),
            C("NG", "NGA", 566, "Nigeria", "Nigeria", "Nigeria", "234", ".ng", Continent.Africa),
            C("MK", "MKD", 807, "North Macedonia", "Nordmakedonien", "Северна Македонија", "389", ".mk", Continent.Europe),
            C("NO", "NOR", 578, "Norway", "Norge", "Norge", "47", ".no", Continent.Europe),

            // O
            C("OM", "OMN", 512, "Oman", "Oman", "عمان", "968", ".om", Continent.Asia),

            // P
            C("PK", "PAK", 586, "Pakistan", "Pakistan", "پاکستان", "92", ".pk", Continent.Asia),
            C("PW", "PLW", 585, "Palau", "Palau", "Palau", "680", ".pw", Continent.Oceania),
            C("PS", "PSE", 275, "Palestine", "Palestina", "فلسطين", "970", ".ps", Continent.Asia),
            C("PA", "PAN", 591, "Panama", "Panama", "Panamá", "507", ".pa", Continent.NorthAmerica),
            C("PG", "PNG", 598, "Papua New Guinea", "Papua Nya Guinea", "Papua Niugini", "675", ".pg", Continent.Oceania),
            C("PY", "PRY", 600, "Paraguay", "Paraguay", "Paraguay", "595", ".py", Continent.SouthAmerica),
            C("PE", "PER", 604, "Peru", "Peru", "Perú", "51", ".pe", Continent.SouthAmerica),
            C("PH", "PHL", 608, "Philippines", "Filippinerna", "Pilipinas", "63", ".ph", Continent.Asia),
            C("PL", "POL", 616, "Poland", "Polen", "Polska", "48", ".pl", Continent.Europe),
            C("PT", "PRT", 620, "Portugal", "Portugal", "Portugal", "351", ".pt", Continent.Europe),
            C("PR", "PRI", 630, "Puerto Rico", "Puerto Rico", "Puerto Rico", "1", ".pr", Continent.NorthAmerica),

            // Q
            C("QA", "QAT", 634, "Qatar", "Qatar", "قطر", "974", ".qa", Continent.Asia),

            // R
            C("RO", "ROU", 642, "Romania", "Rumänien", "România", "40", ".ro", Continent.Europe),
            C("RU", "RUS", 643, "Russia", "Ryssland", "Россия", "7", ".ru", Continent.Europe),
            C("RW", "RWA", 646, "Rwanda", "Rwanda", "Rwanda", "250", ".rw", Continent.Africa),

            // S
            C("KN", "KNA", 659, "Saint Kitts and Nevis", "Saint Kitts och Nevis", "Saint Kitts and Nevis", "1", ".kn", Continent.NorthAmerica),
            C("LC", "LCA", 662, "Saint Lucia", "Saint Lucia", "Saint Lucia", "1", ".lc", Continent.NorthAmerica),
            C("VC", "VCT", 670, "Saint Vincent and the Grenadines", "Saint Vincent och Grenadinerna", "Saint Vincent and the Grenadines", "1", ".vc", Continent.NorthAmerica),
            C("WS", "WSM", 882, "Samoa", "Samoa", "Sāmoa", "685", ".ws", Continent.Oceania),
            C("SM", "SMR", 674, "San Marino", "San Marino", "San Marino", "378", ".sm", Continent.Europe),
            C("ST", "STP", 678, "São Tomé and Príncipe", "São Tomé och Príncipe", "São Tomé e Príncipe", "239", ".st", Continent.Africa),
            C("SA", "SAU", 682, "Saudi Arabia", "Saudiarabien", "السعودية", "966", ".sa", Continent.Asia),
            C("SN", "SEN", 686, "Senegal", "Senegal", "Sénégal", "221", ".sn", Continent.Africa),
            C("RS", "SRB", 688, "Serbia", "Serbien", "Србија", "381", ".rs", Continent.Europe),
            C("SC", "SYC", 690, "Seychelles", "Seychellerna", "Seychelles", "248", ".sc", Continent.Africa),
            C("SL", "SLE", 694, "Sierra Leone", "Sierra Leone", "Sierra Leone", "232", ".sl", Continent.Africa),
            C("SG", "SGP", 702, "Singapore", "Singapore", "Singapore", "65", ".sg", Continent.Asia),
            C("SK", "SVK", 703, "Slovakia", "Slovakien", "Slovensko", "421", ".sk", Continent.Europe),
            C("SI", "SVN", 705, "Slovenia", "Slovenien", "Slovenija", "386", ".si", Continent.Europe),
            C("SB", "SLB", 90, "Solomon Islands", "Salomonöarna", "Solomon Islands", "677", ".sb", Continent.Oceania),
            C("SO", "SOM", 706, "Somalia", "Somalia", "Soomaaliya", "252", ".so", Continent.Africa),
            C("ZA", "ZAF", 710, "South Africa", "Sydafrika", "Suid-Afrika", "27", ".za", Continent.Africa),
            C("SS", "SSD", 728, "South Sudan", "Sydsudan", "South Sudan", "211", ".ss", Continent.Africa),
            C("ES", "ESP", 724, "Spain", "Spanien", "España", "34", ".es", Continent.Europe),
            C("LK", "LKA", 144, "Sri Lanka", "Sri Lanka", "ශ්‍රී ලංකාව", "94", ".lk", Continent.Asia),
            C("SD", "SDN", 729, "Sudan", "Sudan", "السودان", "249", ".sd", Continent.Africa),
            C("SR", "SUR", 740, "Suriname", "Surinam", "Suriname", "597", ".sr", Continent.SouthAmerica),
            C("SE", "SWE", 752, "Sweden", "Sverige", "Sverige", "46", ".se", Continent.Europe),
            C("CH", "CHE", 756, "Switzerland", "Schweiz", "Schweiz", "41", ".ch", Continent.Europe),
            C("SY", "SYR", 760, "Syria", "Syrien", "سوريا", "963", ".sy", Continent.Asia),

            // T
            C("TW", "TWN", 158, "Taiwan", "Taiwan", "臺灣", "886", ".tw", Continent.Asia),
            C("TJ", "TJK", 762, "Tajikistan", "Tadzjikistan", "Тоҷикистон", "992", ".tj", Continent.Asia),
            C("TZ", "TZA", 834, "Tanzania", "Tanzania", "Tanzania", "255", ".tz", Continent.Africa),
            C("TH", "THA", 764, "Thailand", "Thailand", "ประเทศไทย", "66", ".th", Continent.Asia),
            C("TL", "TLS", 626, "Timor-Leste", "Östtimor", "Timor-Leste", "670", ".tl", Continent.Asia),
            C("TG", "TGO", 768, "Togo", "Togo", "Togo", "228", ".tg", Continent.Africa),
            C("TO", "TON", 776, "Tonga", "Tonga", "Tonga", "676", ".to", Continent.Oceania),
            C("TT", "TTO", 780, "Trinidad and Tobago", "Trinidad och Tobago", "Trinidad and Tobago", "1", ".tt", Continent.NorthAmerica),
            C("TN", "TUN", 788, "Tunisia", "Tunisien", "تونس", "216", ".tn", Continent.Africa),
            C("TR", "TUR", 792, "Turkey", "Turkiet", "Türkiye", "90", ".tr", Continent.Asia),
            C("TM", "TKM", 795, "Turkmenistan", "Turkmenistan", "Türkmenistan", "993", ".tm", Continent.Asia),
            C("TV", "TUV", 798, "Tuvalu", "Tuvalu", "Tuvalu", "688", ".tv", Continent.Oceania),

            // U
            C("UG", "UGA", 800, "Uganda", "Uganda", "Uganda", "256", ".ug", Continent.Africa),
            C("UA", "UKR", 804, "Ukraine", "Ukraina", "Україна", "380", ".ua", Continent.Europe),
            C("AE", "ARE", 784, "United Arab Emirates", "Förenade Arabemiraten", "الإمارات", "971", ".ae", Continent.Asia),
            C("GB", "GBR", 826, "United Kingdom", "Storbritannien", "United Kingdom", "44", ".uk", Continent.Europe),
            C("US", "USA", 840, "United States", "Förenta staterna", "United States", "1", ".us", Continent.NorthAmerica),
            C("UY", "URY", 858, "Uruguay", "Uruguay", "Uruguay", "598", ".uy", Continent.SouthAmerica),
            C("UZ", "UZB", 860, "Uzbekistan", "Uzbekistan", "Oʻzbekiston", "998", ".uz", Continent.Asia),

            // V
            C("VU", "VUT", 548, "Vanuatu", "Vanuatu", "Vanuatu", "678", ".vu", Continent.Oceania),
            C("VA", "VAT", 336, "Vatican City", "Vatikanstaten", "Città del Vaticano", "379", ".va", Continent.Europe),
            C("VE", "VEN", 862, "Venezuela", "Venezuela", "Venezuela", "58", ".ve", Continent.SouthAmerica),
            C("VN", "VNM", 704, "Vietnam", "Vietnam", "Việt Nam", "84", ".vn", Continent.Asia),

            // Y
            C("YE", "YEM", 887, "Yemen", "Jemen", "اليمن", "967", ".ye", Continent.Asia),

            // Z
            C("ZM", "ZMB", 894, "Zambia", "Zambia", "Zambia", "260", ".zm", Continent.Africa),
            C("ZW", "ZWE", 716, "Zimbabwe", "Zimbabwe", "Zimbabwe", "263", ".zw", Continent.Africa),

            // Territories
            C("AX", "ALA", 248, "Åland Islands", "Åland", "Åland", "358", ".ax", Continent.Europe),
            C("FO", "FRO", 234, "Faroe Islands", "Färöarna", "Føroyar", "298", ".fo", Continent.Europe),
            C("XK", "XKX", 0, "Kosovo", "Kosovo", "Kosova", "383", ".xk", Continent.Europe),
        ];

        ByAlpha2 = new(StringComparer.OrdinalIgnoreCase);
        ByAlpha3 = new(StringComparer.OrdinalIgnoreCase);
        ByName = new(StringComparer.OrdinalIgnoreCase);

        foreach (var c in AllCountries)
        {
            ByAlpha2[c.Alpha2Code] = c;
            ByAlpha3[c.Alpha3Code] = c;
            ByName[c.EnglishName] = c;

            if (!ByName.ContainsKey(c.LocalizedName))
                ByName[c.LocalizedName] = c;

            if (!string.Equals(c.NativeName, c.EnglishName, StringComparison.OrdinalIgnoreCase)
                && !ByName.ContainsKey(c.NativeName))
                ByName[c.NativeName] = c;
        }

        // Common aliases
        ByName["USA"] = ByAlpha2["US"];
        ByName["UK"] = ByAlpha2["GB"];
        ByName["Holland"] = ByAlpha2["NL"];
        ByName["Czech Republic"] = ByAlpha2["CZ"];
        ByName["Ivory Coast"] = ByAlpha2["CI"];
        ByName["Burma"] = ByAlpha2["MM"];
        ByName["Swaziland"] = ByAlpha2["SZ"];
        ByName["Persia"] = ByAlpha2["IR"];
        ByName["Great Britain"] = ByAlpha2["GB"];
        ByName["England"] = ByAlpha2["GB"];
        ByName["UAE"] = ByAlpha2["AE"];
        ByName["Suisse"] = ByAlpha2["CH"];
        ByName["Svizzera"] = ByAlpha2["CH"];
        ByName["Belgique"] = ByAlpha2["BE"];
        ByName["Noreg"] = ByAlpha2["NO"];
        ByName["Vitryssland"] = ByAlpha2["BY"];

        ByCallingCode = new(StringComparer.Ordinal);
        foreach (var c in AllCountries)
            ByCallingCode.TryAdd(c.CallingCode.Value, c);

        ByCallingCode["1"] = ByAlpha2["US"];
        ByCallingCode["44"] = ByAlpha2["GB"];
        ByCallingCode["7"] = ByAlpha2["RU"];

        var names = ByName.Keys
            .Where(n => n.Length >= 4)
            .OrderByDescending(n => n.Length)
            .Select(Regex.Escape);
        ScanPattern = new Regex(
            @"\b(?:" + string.Join('|', names) + @")\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    /// <summary>Sweden (SE).</summary>
    public static Country Sweden => ByAlpha2["SE"];
    /// <summary>Norway (NO).</summary>
    public static Country Norway => ByAlpha2["NO"];
    /// <summary>Finland (FI).</summary>
    public static Country Finland => ByAlpha2["FI"];
    /// <summary>Denmark (DK).</summary>
    public static Country Denmark => ByAlpha2["DK"];
    /// <summary>Germany (DE).</summary>
    public static Country Germany => ByAlpha2["DE"];
    /// <summary>Poland (PL).</summary>
    public static Country Poland => ByAlpha2["PL"];
    /// <summary>Estonia (EE).</summary>
    public static Country Estonia => ByAlpha2["EE"];
    /// <summary>Lithuania (LT).</summary>
    public static Country Lithuania => ByAlpha2["LT"];
    /// <summary>Romania (RO).</summary>
    public static Country Romania => ByAlpha2["RO"];
    /// <summary>Bulgaria (BG).</summary>
    public static Country Bulgaria => ByAlpha2["BG"];
    /// <summary>Latvia (LV).</summary>
    public static Country Latvia => ByAlpha2["LV"];
    /// <summary>Czech Republic (CZ).</summary>
    public static Country CzechRepublic => ByAlpha2["CZ"];
    /// <summary>Spain (ES).</summary>
    public static Country Spain => ByAlpha2["ES"];
    /// <summary>Netherlands (NL).</summary>
    public static Country Netherlands => ByAlpha2["NL"];
    /// <summary>Greece (GR).</summary>
    public static Country Greece => ByAlpha2["GR"];
    /// <summary>Italy (IT).</summary>
    public static Country Italy => ByAlpha2["IT"];
    /// <summary>Slovenia (SI).</summary>
    public static Country Slovenia => ByAlpha2["SI"];
    /// <summary>Croatia (HR).</summary>
    public static Country Croatia => ByAlpha2["HR"];
    /// <summary>Portugal (PT).</summary>
    public static Country Portugal => ByAlpha2["PT"];
    /// <summary>Hungary (HU).</summary>
    public static Country Hungary => ByAlpha2["HU"];
    /// <summary>France (FR).</summary>
    public static Country France => ByAlpha2["FR"];
    /// <summary>Slovakia (SK).</summary>
    public static Country Slovakia => ByAlpha2["SK"];
    /// <summary>Belgium (BE).</summary>
    public static Country Belgium => ByAlpha2["BE"];
    /// <summary>United Kingdom (GB).</summary>
    public static Country UnitedKingdom => ByAlpha2["GB"];
    /// <summary>Austria (AT).</summary>
    public static Country Austria => ByAlpha2["AT"];
    /// <summary>Cyprus (CY).</summary>
    public static Country Cyprus => ByAlpha2["CY"];
    /// <summary>Iceland (IS).</summary>
    public static Country Iceland => ByAlpha2["IS"];
    /// <summary>Switzerland (CH).</summary>
    public static Country Switzerland => ByAlpha2["CH"];
    /// <summary>Ireland (IE).</summary>
    public static Country Ireland => ByAlpha2["IE"];
    /// <summary>Luxembourg (LU).</summary>
    public static Country Luxembourg => ByAlpha2["LU"];
    /// <summary>Malta (MT).</summary>
    public static Country Malta => ByAlpha2["MT"];
    /// <summary>Liechtenstein (LI).</summary>
    public static Country Liechtenstein => ByAlpha2["LI"];

    /// <summary>
    /// All known countries.
    /// </summary>
    public static IReadOnlyList<Country> All => AllCountries;

    /// <summary>
    /// Returns all countries within <paramref name="radiusKm"/> kilometers of this country,
    /// ordered by distance (nearest first). The current country is never included.
    /// Distance is calculated using the Haversine formula on the Wikidata coordinates.
    /// </summary>
    /// <example>
    /// <code>
    /// var sweden = Country.Sweden;
    /// var nearby = sweden.FindNeighbors(500); // all within 500 km
    /// </code>
    /// </example>
    public IReadOnlyList<CountryDistance> FindNeighbors(double radiusKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radiusKm);

        var results = new List<CountryDistance>();
        foreach (var other in AllCountries)
        {
            if (ReferenceEquals(this, other)) continue;
            var dist = GeoCoordinate.Distance(Coordinate, other.Coordinate);
            if ((double)dist.Kilometers <= radiusKm)
                results.Add(new CountryDistance(other, dist));
        }
        results.Sort((a, b) => a.Distance.Kilometers.CompareTo(b.Distance.Kilometers));
        return results;
    }

    /// <summary>
    /// Returns all countries within <paramref name="radiusKm"/> kilometers of the given
    /// <paramref name="coordinate"/>, ordered by distance (nearest first).
    /// </summary>
    public static IReadOnlyList<CountryDistance> FindNeighbors(GeoCoordinate coordinate, double radiusKm)
    {
        ArgumentNullException.ThrowIfNull(coordinate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radiusKm);

        var results = new List<CountryDistance>();
        foreach (var c in AllCountries)
        {
            var dist = GeoCoordinate.Distance(coordinate, c.Coordinate);
            if ((double)dist.Kilometers <= radiusKm)
                results.Add(new CountryDistance(c, dist));
        }
        results.Sort((a, b) => a.Distance.Kilometers.CompareTo(b.Distance.Kilometers));
        return results;
    }

    /// <summary>
    /// Looks up a country by its phone calling code (e.g. <c>"46"</c> for Sweden, <c>"1"</c> for US).
    /// For shared calling codes the primary country is returned (US for 1, UK for 44, Russia for 7).
    /// </summary>
    public static bool TryFindByCallingCode(string? callingCode, out Country? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(callingCode)) return false;
        return ByCallingCode.TryGetValue(callingCode.Trim(), out result);
    }

    /// <summary>
    /// Looks up a country by its <see cref="PhoneCallingCode"/>.
    /// For shared calling codes the primary country is returned (US for +1, UK for +44, Russia for +7).
    /// </summary>
    public static bool TryFindByCallingCode(PhoneCallingCode? callingCode, out Country? result)
    {
        result = null;
        if (callingCode is null) return false;
        return ByCallingCode.TryGetValue(callingCode.Value, out result);
    }

    /// <summary>
    /// Scans unstructured text for country names in Swedish, English, and local forms.
    /// Matches names of 4+ characters at word boundaries. Short codes (alpha-2/alpha-3) are
    /// excluded to avoid false positives. Results use <see cref="TextMatchConfidence.Low"/>
    /// since country names are common words that may appear in non-geographic contexts.
    /// No guarantee is made that a match represents a country reference in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Country>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Country>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var country)) continue;
            results.Add(new TextCandidate<Country>(
                match.Index,
                match.Length,
                match.Value,
                nameof(Country),
                TextCandidateCategory.Geography,
                country!.ToNormalizedString(),
                country.ToString(),
                country.LocalizedName,
                TextMatchConfidence.Low,
                country));
        }
        return results;
    }

    public static bool TryParse(string? input, out Country? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        if (ByAlpha2.TryGetValue(trimmed, out result)) return true;
        if (ByAlpha3.TryGetValue(trimmed, out result)) return true;
        if (ByName.TryGetValue(trimmed, out result)) return true;

        if (trimmed.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
        {
            var withoutArticle = trimmed[4..];
            if (withoutArticle.Length > 0 && ByName.TryGetValue(withoutArticle, out result)) return true;
        }

        return false;
    }

    public static Country Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown country.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the country in the current display language, for example <c>Tyskland</c> or <c>Germany</c>
    /// depending on <see cref="PrimitivesDefaults.UICulture"/>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.DisplayName : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized ISO 3166-1 alpha-2 country code, for example <c>SE</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Alpha2Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized ISO 3166-1 alpha-2 country code, for example <c>SE</c>.
    /// </summary>
    public string ToNormalizedString() => Alpha2Code;
    /// <summary>
    /// Returns the country in the current display language (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>
    /// is true, otherwise English), for example <c>Tyskland</c> or <c>Germany</c>.
    /// </summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>
    /// Returns the country as its English display name, for example <c>Sweden</c>.
    /// </summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>
    /// Returns the country in its own native language (endonym), for example <c>Deutschland</c> for Germany,
    /// <c>日本</c> for Japan, <c>Sverige</c> for Sweden.
    /// </summary>
    public string ToNativeString() => NativeName;

    /// <summary>
    /// Returns the country in the current display language, for example <c>Tyskland</c> or <c>Germany</c>.
    /// </summary>
    public override string ToString() => DisplayName;

    private static string[] ResolveCurrencyCodes(string alpha2)
    {
        if (CurrencyOverrides.TryGetValue(alpha2, out var overridden))
            return overridden;

        try
        {
            var region = new RegionInfo(alpha2);
            var isoCurrencySymbol = region.ISOCurrencySymbol;
            if (!string.IsNullOrWhiteSpace(isoCurrencySymbol))
                return [isoCurrencySymbol];
        }
        catch (ArgumentException)
        {
            // Not all entries are recognized by RegionInfo (for example Kosovo).
        }

        return [];
    }

    private static string[] ResolveLanguageCodes(string alpha2)
        => LanguageMapping.TryGetValue(alpha2, out var codes) ? codes : [];

    private static readonly Dictionary<string, string[]> LanguageMapping = new(StringComparer.Ordinal)
    {
        ["AF"] = ["ps", "fa"],    // Afghanistan — Pashto, Persian/Dari
        ["AL"] = ["sq"],          // Albania
        ["DZ"] = ["ar"],          // Algeria
        ["AD"] = ["ca"],          // Andorra
        ["AO"] = ["pt"],          // Angola
        ["AG"] = ["en"],          // Antigua and Barbuda
        ["AR"] = ["es"],          // Argentina
        ["AM"] = ["hy"],          // Armenia
        ["AU"] = ["en"],          // Australia
        ["AT"] = ["de"],          // Austria
        ["AZ"] = ["az"],          // Azerbaijan
        ["BS"] = ["en"],          // Bahamas
        ["BH"] = ["ar"],          // Bahrain
        ["BD"] = ["bn"],          // Bangladesh
        ["BB"] = ["en"],          // Barbados
        ["BY"] = ["be", "ru"],    // Belarus
        ["BE"] = ["nl", "fr", "de"], // Belgium
        ["BZ"] = ["en"],          // Belize
        ["BJ"] = ["fr"],          // Benin
        ["BT"] = ["dz"],          // Bhutan
        ["BO"] = ["es", "qu", "ay"], // Bolivia
        ["BA"] = ["bs", "hr", "sr"], // Bosnia and Herzegovina
        ["BW"] = ["en", "tn"],    // Botswana
        ["BR"] = ["pt"],          // Brazil
        ["BN"] = ["ms"],          // Brunei
        ["BG"] = ["bg"],          // Bulgaria
        ["BF"] = ["fr"],          // Burkina Faso
        ["BI"] = ["rn", "fr"],    // Burundi
        ["CV"] = ["pt"],          // Cabo Verde
        ["KH"] = ["km"],          // Cambodia
        ["CM"] = ["fr", "en"],    // Cameroon
        ["CA"] = ["en", "fr"],    // Canada
        ["CF"] = ["fr", "sg"],    // Central African Republic
        ["TD"] = ["fr", "ar"],    // Chad
        ["CL"] = ["es"],          // Chile
        ["CN"] = ["zh"],          // China
        ["CO"] = ["es"],          // Colombia
        ["KM"] = ["ar", "fr"],    // Comoros
        ["CG"] = ["fr"],          // Congo
        ["CD"] = ["fr"],          // Congo (Democratic Republic)
        ["CR"] = ["es"],          // Costa Rica
        ["CI"] = ["fr"],          // Côte d'Ivoire
        ["HR"] = ["hr"],          // Croatia
        ["CU"] = ["es"],          // Cuba
        ["CY"] = ["el", "tr"],    // Cyprus
        ["CZ"] = ["cs"],          // Czechia
        ["DK"] = ["da"],          // Denmark
        ["DJ"] = ["fr", "ar"],    // Djibouti
        ["DM"] = ["en"],          // Dominica
        ["DO"] = ["es"],          // Dominican Republic
        ["EC"] = ["es"],          // Ecuador
        ["EG"] = ["ar"],          // Egypt
        ["SV"] = ["es"],          // El Salvador
        ["GQ"] = ["es", "fr", "pt"], // Equatorial Guinea
        ["ER"] = ["ti", "ar", "en"], // Eritrea
        ["EE"] = ["et"],          // Estonia
        ["SZ"] = ["en", "ss"],    // Eswatini
        ["ET"] = ["am"],          // Ethiopia
        ["FJ"] = ["en", "fj"],    // Fiji
        ["FI"] = ["fi", "sv"],    // Finland
        ["FR"] = ["fr"],          // France
        ["GA"] = ["fr"],          // Gabon
        ["GM"] = ["en"],          // Gambia
        ["GE"] = ["ka"],          // Georgia
        ["DE"] = ["de"],          // Germany
        ["GH"] = ["en"],          // Ghana
        ["GR"] = ["el"],          // Greece
        ["GD"] = ["en"],          // Grenada
        ["GT"] = ["es"],          // Guatemala
        ["GN"] = ["fr"],          // Guinea
        ["GW"] = ["pt"],          // Guinea-Bissau
        ["GY"] = ["en"],          // Guyana
        ["HT"] = ["ht", "fr"],    // Haiti
        ["HN"] = ["es"],          // Honduras
        ["HU"] = ["hu"],          // Hungary
        ["IS"] = ["is"],          // Iceland
        ["IN"] = ["hi", "en"],    // India
        ["ID"] = ["id"],          // Indonesia
        ["IR"] = ["fa"],          // Iran
        ["IQ"] = ["ar", "ku"],    // Iraq
        ["IE"] = ["en", "ga"],    // Ireland
        ["IL"] = ["he", "ar"],    // Israel
        ["IT"] = ["it"],          // Italy
        ["JM"] = ["en"],          // Jamaica
        ["JP"] = ["ja"],          // Japan
        ["JO"] = ["ar"],          // Jordan
        ["KZ"] = ["kk", "ru"],    // Kazakhstan
        ["KE"] = ["en", "sw"],    // Kenya
        ["KI"] = ["en"],          // Kiribati
        ["KP"] = ["ko"],          // North Korea
        ["KR"] = ["ko"],          // South Korea
        ["KW"] = ["ar"],          // Kuwait
        ["KG"] = ["ky", "ru"],    // Kyrgyzstan
        ["LA"] = ["lo"],          // Laos
        ["LV"] = ["lv"],          // Latvia
        ["LB"] = ["ar", "fr"],    // Lebanon
        ["LS"] = ["en", "st"],    // Lesotho
        ["LR"] = ["en"],          // Liberia
        ["LY"] = ["ar"],          // Libya
        ["LI"] = ["de"],          // Liechtenstein
        ["LT"] = ["lt"],          // Lithuania
        ["LU"] = ["lb", "fr", "de"], // Luxembourg
        ["MG"] = ["mg", "fr"],    // Madagascar
        ["MW"] = ["en", "ny"],    // Malawi
        ["MY"] = ["ms"],          // Malaysia
        ["MV"] = ["dv"],          // Maldives
        ["ML"] = ["fr"],          // Mali
        ["MT"] = ["mt", "en"],    // Malta
        ["MH"] = ["en", "mh"],    // Marshall Islands
        ["MR"] = ["ar"],          // Mauritania
        ["MU"] = ["en"],          // Mauritius
        ["MX"] = ["es"],          // Mexico
        ["MD"] = ["ro"],          // Moldova
        ["MC"] = ["fr"],          // Monaco
        ["MN"] = ["mn"],          // Mongolia
        ["ME"] = ["sr"],          // Montenegro
        ["MA"] = ["ar"],          // Morocco
        ["MZ"] = ["pt"],          // Mozambique
        ["MM"] = ["my"],          // Myanmar
        ["NA"] = ["en"],          // Namibia
        ["NR"] = ["en", "na"],    // Nauru
        ["NP"] = ["ne"],          // Nepal
        ["NL"] = ["nl"],          // Netherlands
        ["NZ"] = ["en", "mi"],    // New Zealand
        ["NI"] = ["es"],          // Nicaragua
        ["NE"] = ["fr"],          // Niger
        ["NG"] = ["en"],          // Nigeria
        ["MK"] = ["mk"],          // North Macedonia
        ["NO"] = ["no", "nb", "nn"], // Norway
        ["OM"] = ["ar"],          // Oman
        ["PK"] = ["ur", "en"],    // Pakistan
        ["PW"] = ["en"],          // Palau
        ["PA"] = ["es"],          // Panama
        ["PG"] = ["en"],          // Papua New Guinea
        ["PY"] = ["es", "gn"],    // Paraguay
        ["PE"] = ["es", "qu"],    // Peru
        ["PH"] = ["tl", "en"],    // Philippines
        ["PL"] = ["pl"],          // Poland
        ["PT"] = ["pt"],          // Portugal
        ["QA"] = ["ar"],          // Qatar
        ["RO"] = ["ro"],          // Romania
        ["RU"] = ["ru"],          // Russia
        ["RW"] = ["rw", "fr", "en"], // Rwanda
        ["KN"] = ["en"],          // Saint Kitts and Nevis
        ["LC"] = ["en"],          // Saint Lucia
        ["VC"] = ["en"],          // Saint Vincent and the Grenadines
        ["WS"] = ["sm", "en"],    // Samoa
        ["SM"] = ["it"],          // San Marino
        ["ST"] = ["pt"],          // São Tomé and Príncipe
        ["SA"] = ["ar"],          // Saudi Arabia
        ["SN"] = ["fr"],          // Senegal
        ["RS"] = ["sr"],          // Serbia
        ["SC"] = ["en", "fr"],    // Seychelles
        ["SL"] = ["en"],          // Sierra Leone
        ["SG"] = ["en", "ms", "zh", "ta"], // Singapore
        ["SK"] = ["sk"],          // Slovakia
        ["SI"] = ["sl"],          // Slovenia
        ["SB"] = ["en"],          // Solomon Islands
        ["SO"] = ["so", "ar"],    // Somalia
        ["ZA"] = ["en", "af", "zu"], // South Africa
        ["SS"] = ["en"],          // South Sudan
        ["ES"] = ["es"],          // Spain
        ["LK"] = ["si", "ta"],    // Sri Lanka
        ["SD"] = ["ar", "en"],    // Sudan
        ["SR"] = ["nl"],          // Suriname
        ["SE"] = ["sv"],          // Sweden
        ["CH"] = ["de", "fr", "it", "rm"], // Switzerland
        ["SY"] = ["ar"],          // Syria
        ["TW"] = ["zh"],          // Taiwan
        ["TJ"] = ["tg"],          // Tajikistan
        ["TZ"] = ["sw", "en"],    // Tanzania
        ["TH"] = ["th"],          // Thailand
        ["TL"] = ["pt"],          // Timor-Leste
        ["TG"] = ["fr"],          // Togo
        ["TO"] = ["en", "to"],    // Tonga
        ["TT"] = ["en"],          // Trinidad and Tobago
        ["TN"] = ["ar"],          // Tunisia
        ["TR"] = ["tr"],          // Turkey
        ["TM"] = ["tk"],          // Turkmenistan
        ["TV"] = ["en"],          // Tuvalu
        ["UG"] = ["en", "sw"],    // Uganda
        ["UA"] = ["uk"],          // Ukraine
        ["AE"] = ["ar"],          // United Arab Emirates
        ["GB"] = ["en"],          // United Kingdom
        ["US"] = ["en"],          // United States
        ["UY"] = ["es"],          // Uruguay
        ["UZ"] = ["uz"],          // Uzbekistan
        ["VU"] = ["en", "fr", "bi"], // Vanuatu
        ["VA"] = ["it", "la"],    // Vatican City
        ["VE"] = ["es"],          // Venezuela
        ["VN"] = ["vi"],          // Vietnam
        ["YE"] = ["ar"],          // Yemen
        ["ZM"] = ["en"],          // Zambia
        ["ZW"] = ["en", "sn", "nd"], // Zimbabwe

        // Territories and special entries
        ["AS"] = ["en", "sm"],    // American Samoa
        ["AW"] = ["nl"],          // Aruba
        ["BM"] = ["en"],          // Bermuda
        ["CW"] = ["nl"],          // Curaçao
        ["FO"] = ["fo", "da"],    // Faroe Islands
        ["GI"] = ["en"],          // Gibraltar
        ["GL"] = ["kl", "da"],    // Greenland
        ["GG"] = ["en"],          // Guernsey
        ["GU"] = ["en"],          // Guam
        ["HK"] = ["zh", "en"],    // Hong Kong
        ["IM"] = ["en"],          // Isle of Man
        ["JE"] = ["en"],          // Jersey
        ["XK"] = ["sq", "sr"],    // Kosovo
        ["MO"] = ["zh", "pt"],    // Macau
        ["PR"] = ["es", "en"],    // Puerto Rico
        ["VI"] = ["en"],          // U.S. Virgin Islands
    };

    public bool Equals(Country? other) => other is not null && Alpha2Code == other.Alpha2Code;
    public override bool Equals(object? obj) => obj is Country other && Equals(other);
    public override int GetHashCode() => Alpha2Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Country? a, Country? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Country? a, Country? b) => !(a == b);
    public int CompareTo(Country? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Country left, Country right) => left.CompareTo(right) < 0;
    public static bool operator >(Country left, Country right) => left.CompareTo(right) > 0;
    public static bool operator <=(Country left, Country right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Country left, Country right) => left.CompareTo(right) >= 0;
}

/// <summary>A neighboring country with its distance from a reference point.</summary>
/// <param name="Country">The neighboring country.</param>
/// <param name="Distance">The distance from the reference country or coordinate.</param>
public sealed record CountryDistance(Country Country, Length Distance);
