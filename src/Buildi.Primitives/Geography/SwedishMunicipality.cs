using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Geography;

/// <summary>
/// A Swedish municipality (<c>kommun</c>) is one of SCB's official municipality divisions. This type lets you work with both the official 4-digit municipality code and the municipality name, and also exposes the related county.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/">SCB - Counties and municipalities in numerical order</see></description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625 (coordinate location)</see> — geographic coordinates for each municipality via SPARQL query on <see href="https://www.wikidata.org/wiki/Property:P525">P525 (Swedish municipality code)</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishMunicipality : IEquatable<SwedishMunicipality>, IComparable<SwedishMunicipality>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Municipality", "Kommun", "🏘️", ["https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/"]);

    private const int MaxInputLength = 100;
    private const int MinScanNameLength = 4;

    private static readonly Dictionary<string, string> EnglishNameOverrides = new(StringComparer.Ordinal)
    {
        ["1480"] = "Gothenburg",
    };

    /// <summary>
    /// Geographic coordinates (latitude, longitude) for each municipality, sourced from
    /// <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    private static readonly Dictionary<string, (double Lat, double Lon)> Coordinates = new(StringComparer.Ordinal)
    {
        ["0114"] = (59.516667, 17.9), ["0115"] = (59.583333, 18.2), ["0117"] = (59.5, 18.45), ["0120"] = (59.316667, 18.566667), ["0123"] = (59.433333, 17.833333),
        ["0125"] = (59.35, 17.683333), ["0126"] = (59.233333, 17.983333), ["0127"] = (59.2, 17.833333), ["0128"] = (59.233333, 17.683333), ["0136"] = (59.166667, 18.133333),
        ["0138"] = (59.233333, 18.216667), ["0139"] = (59.533333, 17.666667), ["0140"] = (59.183333, 17.4), ["0160"] = (59.4439, 18.06872), ["0162"] = (59.4, 18.083333),
        ["0163"] = (59.45, 17.916667), ["0180"] = (59.3275, 18.054719), ["0181"] = (59.1, 17.516667), ["0182"] = (59.316667, 18.166667), ["0183"] = (59.363333, 17.965556),
        ["0184"] = (59.366667, 18.016667), ["0186"] = (59.36303, 18.15096), ["0187"] = (59.4, 18.283333), ["0188"] = (59.766667, 18.7), ["0191"] = (59.633333, 17.883333),
        ["0192"] = (58.933333, 17.883333), ["0305"] = (59.567778, 17.529722), ["0319"] = (60.624444, 17.416111), ["0330"] = (59.716667, 17.8), ["0331"] = (59.933333, 16.883333),
        ["0360"] = (60.344444, 17.513056), ["0380"] = (59.866667, 17.633333), ["0381"] = (59.635556, 17.076389), ["0382"] = (60.258889, 18.373611), ["0428"] = (59.033333, 15.866667),
        ["0461"] = (59.0475, 17.312222), ["0480"] = (58.752778, 17.008611), ["0481"] = (58.666667, 17.116667), ["0482"] = (59.058333, 16.588889), ["0483"] = (58.995556, 16.206111),
        ["0484"] = (59.370556, 16.512778), ["0486"] = (59.376944, 17.035278), ["0488"] = (58.893889, 17.552222), ["0509"] = (58.230833, 14.652778), ["0512"] = (57.827222, 15.273889),
        ["0513"] = (57.989167, 15.627222), ["0560"] = (58.196389, 15.051944), ["0561"] = (58.201667, 15.998056), ["0562"] = (58.709167, 15.786944), ["0563"] = (58.205556, 16.601944),
        ["0580"] = (58.4, 15.616667), ["0581"] = (58.591944, 16.185556), ["0582"] = (58.480278, 16.3225), ["0583"] = (58.536389, 15.0375), ["0584"] = (58.448333, 14.889722),
        ["0586"] = (58.325, 15.130556), ["0604"] = (57.833333, 14.8), ["0617"] = (57.366667, 13.733333), ["0642"] = (57.916667, 13.883333), ["0643"] = (57.916667, 14.066667),
        ["0662"] = (57.3, 13.533333), ["0665"] = (57.5, 14.116667), ["0680"] = (57.783333, 14.2), ["0682"] = (57.65, 14.683333), ["0683"] = (57.183333, 14.033333),
        ["0684"] = (57.4, 14.666667), ["0685"] = (57.433333, 15.066667), ["0686"] = (57.666667, 14.966667), ["0687"] = (58.033333, 14.966667), ["0760"] = (57.166667, 15.333333),
        ["0761"] = (56.75, 15.266667), ["0763"] = (56.533333, 14.983333), ["0764"] = (56.9, 14.55), ["0765"] = (56.55, 14.133333), ["0767"] = (56.466667, 13.6),
        ["0780"] = (56.883333, 14.8), ["0781"] = (56.833333, 13.933333), ["0821"] = (57.166667, 16.033333), ["0834"] = (56.4, 16.0), ["0840"] = (56.516667, 16.383333),
        ["0860"] = (57.483333, 15.833333), ["0861"] = (57.033333, 16.45), ["0862"] = (56.616667, 15.55), ["0880"] = (56.666667, 16.366667), ["0881"] = (56.733333, 15.9),
        ["0882"] = (57.266667, 16.45), ["0883"] = (57.75, 16.633333), ["0884"] = (57.666667, 15.85), ["0885"] = (56.878056, 16.661667), ["0980"] = (57.615278, 18.280556),
        ["1060"] = (56.266667, 14.533333), ["1080"] = (56.183333, 15.65), ["1081"] = (56.2, 15.283333), ["1082"] = (56.166667, 14.85), ["1083"] = (56.05, 14.583333),
        ["1214"] = (55.916667, 13.116667), ["1230"] = (55.633333, 13.2), ["1231"] = (55.633333, 13.066667), ["1233"] = (55.466667, 13.016667), ["1256"] = (56.25, 14.066667),
        ["1257"] = (56.283333, 13.283333), ["1260"] = (56.083333, 12.916667), ["1261"] = (55.8, 13.116667), ["1262"] = (55.666667, 13.083333), ["1263"] = (55.5, 13.233333),
        ["1264"] = (55.483333, 13.5), ["1265"] = (55.633333, 13.7), ["1266"] = (55.85, 13.65), ["1267"] = (55.933333, 13.55), ["1270"] = (55.55, 13.95),
        ["1272"] = (56.05, 14.466667), ["1273"] = (56.366667, 13.983333), ["1275"] = (56.133333, 13.383333), ["1276"] = (56.133333, 13.133333), ["1277"] = (56.133333, 12.95),
        ["1278"] = (56.416667, 12.866667), ["1280"] = (55.565, 13.018611), ["1281"] = (55.7, 13.2), ["1282"] = (55.866667, 12.85), ["1283"] = (56.05, 12.716667),
        ["1284"] = (56.2, 12.566667), ["1285"] = (55.833333, 13.3), ["1286"] = (55.416667, 13.833333), ["1287"] = (55.366667, 13.166667), ["1290"] = (56.033333, 14.15),
        ["1291"] = (55.55, 14.35), ["1292"] = (56.25, 12.85), ["1293"] = (56.166667, 13.766667), ["1315"] = (57.0, 13.25), ["1380"] = (56.666667, 12.85),
        ["1381"] = (56.516667, 13.033333), ["1382"] = (56.916667, 12.5), ["1383"] = (57.1, 12.25), ["1384"] = (57.483333, 12.066667), ["1401"] = (57.666667, 12.116667),
        ["1402"] = (57.733333, 12.1), ["1407"] = (57.711111, 11.647222), ["1415"] = (58.083333, 11.816667), ["1419"] = (58.0, 11.55), ["1421"] = (58.233333, 11.683333),
        ["1427"] = (58.366667, 11.25), ["1430"] = (58.483333, 11.683333), ["1435"] = (58.716667, 11.316667), ["1438"] = (58.916667, 11.916667), ["1439"] = (58.566667, 11.983333),
        ["1440"] = (57.925, 12.086944), ["1441"] = (57.766667, 12.3), ["1442"] = (58.033333, 12.8), ["1443"] = (57.666667, 12.566667), ["1444"] = (58.333333, 12.666667),
        ["1445"] = (58.183333, 12.716667), ["1446"] = (58.533333, 14.516667), ["1447"] = (58.983333, 14.116667), ["1452"] = (57.483333, 13.35), ["1460"] = (59.033333, 12.216667),
        ["1461"] = (58.7, 12.466667), ["1462"] = (58.133333, 12.133333), ["1463"] = (57.516667, 12.683333), ["1465"] = (57.5, 13.116667), ["1466"] = (58.083333, 13.033333),
        ["1470"] = (58.266667, 12.95), ["1471"] = (58.533333, 13.483333), ["1472"] = (58.416667, 14.166667), ["1473"] = (58.716667, 14.133333), ["1480"] = (57.7, 11.933333),
        ["1481"] = (57.654167, 12.019167), ["1482"] = (57.866667, 11.966667), ["1484"] = (58.283333, 11.433333), ["1485"] = (58.35, 11.916667), ["1486"] = (58.933333, 11.166667),
        ["1487"] = (58.383333, 12.316667), ["1488"] = (58.283333, 12.283333), ["1489"] = (57.933333, 12.533333), ["1490"] = (57.716667, 12.933333), ["1491"] = (57.791667, 13.412778),
        ["1492"] = (59.05, 12.7), ["1493"] = (58.7, 13.816667), ["1494"] = (58.5, 13.183333), ["1495"] = (58.383333, 13.433333), ["1496"] = (58.383333, 13.85),
        ["1497"] = (58.3, 14.283333), ["1498"] = (58.183333, 13.95), ["1499"] = (58.166667, 13.55), ["1715"] = (59.5, 13.316389), ["1730"] = (59.883333, 12.283333),
        ["1737"] = (60.133333, 13.0), ["1760"] = (59.533333, 14.266667), ["1761"] = (59.333333, 13.433333), ["1762"] = (59.833333, 13.533333), ["1763"] = (59.533333, 13.466667),
        ["1764"] = (59.351667, 13.111389), ["1765"] = (59.383333, 12.133333), ["1766"] = (59.833333, 13.133333), ["1780"] = (59.383333, 13.533333), ["1781"] = (59.3, 14.116667),
        ["1782"] = (59.716667, 14.166667), ["1783"] = (60.033333, 13.65), ["1784"] = (59.65, 12.583333), ["1785"] = (59.133333, 12.933333), ["1814"] = (59.166667, 14.866667),
        ["1860"] = (58.988611, 14.6175), ["1861"] = (59.066667, 15.116667), ["1862"] = (59.233333, 14.433333), ["1863"] = (59.766667, 14.516667), ["1864"] = (59.866667, 14.983333),
        ["1880"] = (59.273889, 15.213333), ["1881"] = (59.116667, 15.133333), ["1882"] = (58.883333, 14.9), ["1883"] = (59.325, 14.525), ["1884"] = (59.516667, 15.033333),
        ["1885"] = (59.583333, 15.25), ["1904"] = (59.833333, 15.683333), ["1907"] = (59.716667, 16.216667), ["1960"] = (59.416667, 16.083333), ["1961"] = (59.616667, 16.25),
        ["1962"] = (60.083333, 15.95), ["1980"] = (59.616667, 16.533333), ["1981"] = (59.920417, 16.606111), ["1982"] = (60.0, 15.816667), ["1983"] = (59.513056, 15.998611),
        ["1984"] = (59.4, 15.833333), ["2021"] = (60.516667, 14.216667), ["2023"] = (60.683333, 13.733333), ["2026"] = (60.55, 15.133333), ["2029"] = (60.733333, 15.0),
        ["2031"] = (60.883333, 15.133333), ["2034"] = (61.116667, 14.616667), ["2039"] = (61.233333, 14.033333), ["2061"] = (60.133333, 15.416667), ["2062"] = (61.016667, 14.533333),
        ["2080"] = (60.6, 15.633333), ["2081"] = (60.482778, 15.436389), ["2082"] = (60.35, 15.75), ["2083"] = (60.283333, 15.983333), ["2084"] = (60.145556, 16.168333),
        ["2085"] = (60.133333, 15.183333), ["2101"] = (60.883333, 16.716667), ["2104"] = (60.55, 16.283333), ["2121"] = (61.383333, 15.816667), ["2132"] = (61.983333, 17.066667),
        ["2161"] = (61.833333, 16.083333), ["2180"] = (60.666667, 17.166667), ["2181"] = (60.616667, 16.783333), ["2182"] = (61.3, 17.083333), ["2183"] = (61.35, 16.4),
        ["2184"] = (61.733333, 17.116667), ["2260"] = (62.516667, 15.616667), ["2262"] = (62.5, 17.333333), ["2280"] = (62.633333, 17.933333), ["2281"] = (62.4, 17.316667),
        ["2282"] = (62.932167, 17.777167), ["2283"] = (63.166667, 17.266667), ["2284"] = (63.283333, 18.733333), ["2303"] = (63.1, 16.35), ["2305"] = (62.75, 15.416667),
        ["2309"] = (63.316667, 14.5), ["2313"] = (63.85, 15.583333), ["2321"] = (63.35, 13.466667), ["2326"] = (62.766667, 14.45), ["2361"] = (62.033333, 14.35),
        ["2380"] = (63.183333, 14.666667), ["2401"] = (63.566667, 19.5), ["2403"] = (63.916667, 19.216667), ["2404"] = (64.2, 19.716667), ["2409"] = (64.192014, 20.849513),
        ["2417"] = (64.916667, 19.483333), ["2418"] = (65.183333, 18.75), ["2421"] = (65.1, 17.1), ["2422"] = (65.533333, 17.533333), ["2425"] = (64.266667, 16.416667),
        ["2460"] = (63.916667, 19.75), ["2462"] = (64.616667, 16.65), ["2463"] = (64.15, 17.35), ["2480"] = (63.833333, 20.25), ["2481"] = (64.6, 18.666667),
        ["2482"] = (64.75, 20.95), ["2505"] = (65.583333, 19.116667), ["2506"] = (66.033333, 17.95), ["2510"] = (66.616667, 19.833333), ["2513"] = (66.35, 22.883333),
        ["2514"] = (65.85, 23.166667), ["2518"] = (66.383333, 23.666667), ["2521"] = (67.183333, 23.366667), ["2523"] = (67.133333, 20.65), ["2560"] = (65.683333, 21.016667),
        ["2580"] = (65.584444, 22.153889), ["2581"] = (65.333333, 21.5), ["2582"] = (65.833333, 21.716667), ["2583"] = (65.833333, 24.133333), ["2584"] = (67.854722, 20.222778),
    };

    private static readonly Dictionary<string, string> Municipalities = new(StringComparer.Ordinal)
    {
        ["0114"] = "Upplands Väsby", ["0115"] = "Vallentuna", ["0117"] = "Österåker", ["0120"] = "Värmdö", ["0123"] = "Järfälla",
        ["0125"] = "Ekerö", ["0126"] = "Huddinge", ["0127"] = "Botkyrka", ["0128"] = "Salem", ["0136"] = "Haninge",
        ["0138"] = "Tyresö", ["0139"] = "Upplands-Bro", ["0140"] = "Nykvarn", ["0160"] = "Täby", ["0162"] = "Danderyd",
        ["0163"] = "Sollentuna", ["0180"] = "Stockholm", ["0181"] = "Södertälje", ["0182"] = "Nacka", ["0183"] = "Sundbyberg",
        ["0184"] = "Solna", ["0186"] = "Lidingö", ["0187"] = "Vaxholm", ["0188"] = "Norrtälje", ["0191"] = "Sigtuna",
        ["0192"] = "Nynäshamn", ["0305"] = "Håbo", ["0319"] = "Älvkarleby", ["0330"] = "Knivsta", ["0331"] = "Heby",
        ["0360"] = "Tierp", ["0380"] = "Uppsala", ["0381"] = "Enköping", ["0382"] = "Östhammar", ["0428"] = "Vingåker",
        ["0461"] = "Gnesta", ["0480"] = "Nyköping", ["0481"] = "Oxelösund", ["0482"] = "Flen", ["0483"] = "Katrineholm",
        ["0484"] = "Eskilstuna", ["0486"] = "Strängnäs", ["0488"] = "Trosa", ["0509"] = "Ödeshög", ["0512"] = "Ydre",
        ["0513"] = "Kinda", ["0560"] = "Boxholm", ["0561"] = "Åtvidaberg", ["0562"] = "Finspång", ["0563"] = "Valdemarsvik",
        ["0580"] = "Linköping", ["0581"] = "Norrköping", ["0582"] = "Söderköping", ["0583"] = "Motala", ["0584"] = "Vadstena",
        ["0586"] = "Mjölby", ["0604"] = "Aneby", ["0617"] = "Gnosjö", ["0642"] = "Mullsjö", ["0643"] = "Habo",
        ["0662"] = "Gislaved", ["0665"] = "Vaggeryd", ["0680"] = "Jönköping", ["0682"] = "Nässjö", ["0683"] = "Värnamo",
        ["0684"] = "Sävsjö", ["0685"] = "Vetlanda", ["0686"] = "Eksjö", ["0687"] = "Tranås", ["0760"] = "Uppvidinge",
        ["0761"] = "Lessebo", ["0763"] = "Tingsryd", ["0764"] = "Alvesta", ["0765"] = "Älmhult", ["0767"] = "Markaryd",
        ["0780"] = "Växjö", ["0781"] = "Ljungby", ["0821"] = "Högsby", ["0834"] = "Torsås", ["0840"] = "Mörbylånga",
        ["0860"] = "Hultsfred", ["0861"] = "Mönsterås", ["0862"] = "Emmaboda", ["0880"] = "Kalmar", ["0881"] = "Nybro",
        ["0882"] = "Oskarshamn", ["0883"] = "Västervik", ["0884"] = "Vimmerby", ["0885"] = "Borgholm", ["0980"] = "Gotland",
        ["1060"] = "Olofström", ["1080"] = "Karlskrona", ["1081"] = "Ronneby", ["1082"] = "Karlshamn", ["1083"] = "Sölvesborg",
        ["1214"] = "Svalöv", ["1230"] = "Staffanstorp", ["1231"] = "Burlöv", ["1233"] = "Vellinge", ["1256"] = "Östra Göinge",
        ["1257"] = "Örkelljunga", ["1260"] = "Bjuv", ["1261"] = "Kävlinge", ["1262"] = "Lomma", ["1263"] = "Svedala",
        ["1264"] = "Skurup", ["1265"] = "Sjöbo", ["1266"] = "Hörby", ["1267"] = "Höör", ["1270"] = "Tomelilla",
        ["1272"] = "Bromölla", ["1273"] = "Osby", ["1275"] = "Perstorp", ["1276"] = "Klippan", ["1277"] = "Åstorp",
        ["1278"] = "Båstad", ["1280"] = "Malmö", ["1281"] = "Lund", ["1282"] = "Landskrona", ["1283"] = "Helsingborg",
        ["1284"] = "Höganäs", ["1285"] = "Eslöv", ["1286"] = "Ystad", ["1287"] = "Trelleborg", ["1290"] = "Kristianstad",
        ["1291"] = "Simrishamn", ["1292"] = "Ängelholm", ["1293"] = "Hässleholm", ["1315"] = "Hylte", ["1380"] = "Halmstad",
        ["1381"] = "Laholm", ["1382"] = "Falkenberg", ["1383"] = "Varberg", ["1384"] = "Kungsbacka", ["1401"] = "Härryda",
        ["1402"] = "Partille", ["1407"] = "Öckerö", ["1415"] = "Stenungsund", ["1419"] = "Tjörn", ["1421"] = "Orust",
        ["1427"] = "Sotenäs", ["1430"] = "Munkedal", ["1435"] = "Tanum", ["1438"] = "Dals-Ed", ["1439"] = "Färgelanda",
        ["1440"] = "Ale", ["1441"] = "Lerum", ["1442"] = "Vårgårda", ["1443"] = "Bollebygd", ["1444"] = "Grästorp",
        ["1445"] = "Essunga", ["1446"] = "Karlsborg", ["1447"] = "Gullspång", ["1452"] = "Tranemo", ["1460"] = "Bengtsfors",
        ["1461"] = "Mellerud", ["1462"] = "Lilla Edet", ["1463"] = "Mark", ["1465"] = "Svenljunga", ["1466"] = "Herrljunga",
        ["1470"] = "Vara", ["1471"] = "Götene", ["1472"] = "Tibro", ["1473"] = "Töreboda", ["1480"] = "Göteborg",
        ["1481"] = "Mölndal", ["1482"] = "Kungälv", ["1484"] = "Lysekil", ["1485"] = "Uddevalla", ["1486"] = "Strömstad",
        ["1487"] = "Vänersborg", ["1488"] = "Trollhättan", ["1489"] = "Alingsås", ["1490"] = "Borås", ["1491"] = "Ulricehamn",
        ["1492"] = "Åmål", ["1493"] = "Mariestad", ["1494"] = "Lidköping", ["1495"] = "Skara", ["1496"] = "Skövde",
        ["1497"] = "Hjo", ["1498"] = "Tidaholm", ["1499"] = "Falköping", ["1715"] = "Kil", ["1730"] = "Eda",
        ["1737"] = "Torsby", ["1760"] = "Storfors", ["1761"] = "Hammarö", ["1762"] = "Munkfors", ["1763"] = "Forshaga",
        ["1764"] = "Grums", ["1765"] = "Årjäng", ["1766"] = "Sunne", ["1780"] = "Karlstad", ["1781"] = "Kristinehamn",
        ["1782"] = "Filipstad", ["1783"] = "Hagfors", ["1784"] = "Arvika", ["1785"] = "Säffle", ["1814"] = "Lekeberg",
        ["1860"] = "Laxå", ["1861"] = "Hallsberg", ["1862"] = "Degerfors", ["1863"] = "Hällefors", ["1864"] = "Ljusnarsberg",
        ["1880"] = "Örebro", ["1881"] = "Kumla", ["1882"] = "Askersund", ["1883"] = "Karlskoga", ["1884"] = "Nora",
        ["1885"] = "Lindesberg", ["1904"] = "Skinnskatteberg", ["1907"] = "Surahammar", ["1960"] = "Kungsör", ["1961"] = "Hallstahammar",
        ["1962"] = "Norberg", ["1980"] = "Västerås", ["1981"] = "Sala", ["1982"] = "Fagersta", ["1983"] = "Köping",
        ["1984"] = "Arboga", ["2021"] = "Vansbro", ["2023"] = "Malung-Sälen", ["2026"] = "Gagnef", ["2029"] = "Leksand",
        ["2031"] = "Rättvik", ["2034"] = "Orsa", ["2039"] = "Älvdalen", ["2061"] = "Smedjebacken", ["2062"] = "Mora",
        ["2080"] = "Falun", ["2081"] = "Borlänge", ["2082"] = "Säter", ["2083"] = "Hedemora", ["2084"] = "Avesta",
        ["2085"] = "Ludvika", ["2101"] = "Ockelbo", ["2104"] = "Hofors", ["2121"] = "Ovanåker", ["2132"] = "Nordanstig",
        ["2161"] = "Ljusdal", ["2180"] = "Gävle", ["2181"] = "Sandviken", ["2182"] = "Söderhamn", ["2183"] = "Bollnäs",
        ["2184"] = "Hudiksvall", ["2260"] = "Ånge", ["2262"] = "Timrå", ["2280"] = "Härnösand", ["2281"] = "Sundsvall",
        ["2282"] = "Kramfors", ["2283"] = "Sollefteå", ["2284"] = "Örnsköldsvik", ["2303"] = "Ragunda", ["2305"] = "Bräcke",
        ["2309"] = "Krokom", ["2313"] = "Strömsund", ["2321"] = "Åre", ["2326"] = "Berg", ["2361"] = "Härjedalen",
        ["2380"] = "Östersund", ["2401"] = "Nordmaling", ["2403"] = "Bjurholm", ["2404"] = "Vindeln", ["2409"] = "Robertsfors",
        ["2417"] = "Norsjö", ["2418"] = "Malå", ["2421"] = "Storuman", ["2422"] = "Sorsele", ["2425"] = "Dorotea",
        ["2460"] = "Vännäs", ["2462"] = "Vilhelmina", ["2463"] = "Åsele", ["2480"] = "Umeå", ["2481"] = "Lycksele",
        ["2482"] = "Skellefteå", ["2505"] = "Arvidsjaur", ["2506"] = "Arjeplog", ["2510"] = "Jokkmokk", ["2513"] = "Överkalix",
        ["2514"] = "Kalix", ["2518"] = "Övertorneå", ["2521"] = "Pajala", ["2523"] = "Gällivare", ["2560"] = "Älvsbyn",
        ["2580"] = "Luleå", ["2581"] = "Piteå", ["2582"] = "Boden", ["2583"] = "Haparanda", ["2584"] = "Kiruna",
    };

    private static readonly Dictionary<string, SwedishMunicipality> ByCode = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SwedishMunicipality> ByName = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Regex ScanPattern;

    /// <summary>
    /// Returns all 290 Swedish municipalities, ordered by municipality code.
    /// </summary>
    public static IReadOnlyList<SwedishMunicipality> All { get; private set; } = [];

    public string Code { get; }
    public string LocalizedName { get; }

    /// <summary>
    /// The English name when it differs from the Swedish name (e.g. <c>Gothenburg</c> for Göteborg),
    /// otherwise the same as <see cref="LocalizedName"/>.
    /// </summary>
    public string EnglishName { get; }

    public SwedishCounty County { get; }
    public string CountyCode => County.Code;

    /// <summary>
    /// The approximate geographic coordinate (WGS 84) of the municipality, e.g. <c>59.3275°N, 18.054719°E</c> for Stockholm.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public GeoCoordinate Coordinate { get; }

    /// <summary>
    /// The approximate latitude (WGS 84) of the municipality, e.g. <c>59.3275</c> for Stockholm.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public double Latitude => Coordinate.Latitude;

    /// <summary>
    /// The approximate longitude (WGS 84) of the municipality, e.g. <c>18.054719</c> for Stockholm.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public double Longitude => Coordinate.Longitude;

    static SwedishMunicipality()
    {
        foreach (var (code, name) in Municipalities)
        {
            if (!global::Buildi.Primitives.Geography.SwedishCounty.TryParse(code[..2], out var county))
                continue;

            var englishName = EnglishNameOverrides.GetValueOrDefault(code, name);
            var (lat, lon) = Coordinates.GetValueOrDefault(code);
            var coordinate = GeoCoordinate.Create(lat, lon);
            var municipality = new SwedishMunicipality(code, name, englishName, county!, coordinate);
            ByCode[code] = municipality;
            ByName[municipality.LocalizedName] = municipality;

            if (!string.Equals(englishName, name, StringComparison.OrdinalIgnoreCase))
                ByName[englishName] = municipality;

            AddGenitive(ByName, municipality.LocalizedName, municipality);
            if (!string.Equals(englishName, name, StringComparison.OrdinalIgnoreCase))
                AddGenitive(ByName, englishName, municipality);
        }

        All = [.. ByCode.Values.OrderBy(m => m.Code)];

        var names = ByName.Keys
            .Where(n => n.Length >= MinScanNameLength)
            .OrderByDescending(n => n.Length)
            .Select(Regex.Escape);
        ScanPattern = new Regex(
            @"\b(?:" + string.Join('|', names) + @")\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    private SwedishMunicipality(string code, string localizedName, string englishName, SwedishCounty county, GeoCoordinate coordinate)
    {
        Code = code;
        LocalizedName = localizedName;
        EnglishName = englishName;
        County = county;
        Coordinate = coordinate;
    }

    private static void AddGenitive(Dictionary<string, SwedishMunicipality> dict, string name, SwedishMunicipality municipality)
    {
        if (name.EndsWith('s') || name.EndsWith('S')) return;
        var genitive = name + "s";
        dict.TryAdd(genitive, municipality);
    }

    /// <summary>
    /// Searches <paramref name="text"/> for substrings that match known Swedish municipality names
    /// (4+ characters). Results use <see cref="TextMatchConfidence.Low"/> since municipality names
    /// may appear in non-geographic contexts. No guarantee is made that a match represents a
    /// municipality reference in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishMunicipality>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishMunicipality>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var municipality)) continue;
            results.Add(new TextCandidate<SwedishMunicipality>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishMunicipality),
                TextCandidateCategory.Geography,
                municipality!.ToNormalizedString(),
                municipality.ToString(),
                municipality.LocalizedName,
                TextMatchConfidence.Low,
                municipality));
        }
        return results;
    }

    public static bool TryParse(string? input, out SwedishMunicipality? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        var digits = InputSanitization.KeepDigits(trimmed);
        if (digits.Length == 4 && ByCode.TryGetValue(digits, out result))
            return true;

        return ByName.TryGetValue(trimmed, out result);
    }

    public static SwedishMunicipality Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish municipality.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Swedish municipality name, for example <c>Stockholm</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.LocalizedName : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;

    /// <summary>
    /// Returns the normalized municipality code as 4 digits, for example <c>0180</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
    /// <summary>
    /// Returns the normalized municipality code as 4 digits, for example <c>0180</c>.
    /// </summary>
    public string ToNormalizedString() => Code;
    /// <summary>Municipality name in the current display language, for example <c>Stockholm</c> or <c>Gothenburg</c> depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>
    /// Returns the municipality in the current display language (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>
    /// is true, otherwise English), for example <c>Göteborg</c> or <c>Gothenburg</c>.
    /// </summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>
    /// Returns the English municipality name when it differs from the Swedish name
    /// (e.g. <c>Gothenburg</c> for Göteborg), otherwise the Swedish name.
    /// </summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>
    /// Returns the municipality name in Swedish (the local language), for example <c>Göteborg</c>.
    /// </summary>
    public string ToLocalString() => LocalizedName;

    /// <summary>
    /// Returns the municipality in the current display language, for example <c>Stockholm</c>.
    /// </summary>
    public override string ToString() => DisplayName;

    /// <summary>
    /// Returns all municipalities within <paramref name="radiusKm"/> kilometers of this municipality,
    /// ordered by distance (nearest first). The current municipality is never included.
    /// Distance is calculated using the Haversine formula on the Wikidata coordinates.
    /// </summary>
    /// <example>
    /// <code>
    /// var stockholm = SwedishMunicipality.Parse("Stockholm");
    /// var nearby = stockholm.FindNeighbors(50); // all within 50 km
    /// </code>
    /// </example>
    public IReadOnlyList<MunicipalityDistance> FindNeighbors(double radiusKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radiusKm);

        var results = new List<MunicipalityDistance>();
        foreach (var other in All)
        {
            if (ReferenceEquals(this, other)) continue;
            var dist = GeoCoordinate.Distance(Coordinate, other.Coordinate);
            if ((double)dist.Kilometers <= radiusKm)
                results.Add(new MunicipalityDistance(other, dist));
        }
        results.Sort((a, b) => a.Distance.Kilometers.CompareTo(b.Distance.Kilometers));
        return results;
    }

    /// <summary>
    /// Calculates the distance between two municipalities as a <see cref="Length"/>,
    /// e.g. <c>398.2 km</c>.
    /// </summary>
    public static Length Distance(SwedishMunicipality a, SwedishMunicipality b) =>
        GeoCoordinate.Distance(a.Coordinate, b.Coordinate);

    /// <summary>
    /// Calculates the distance from a municipality to a geographic coordinate as a <see cref="Length"/>.
    /// </summary>
    public static Length Distance(SwedishMunicipality municipality, GeoCoordinate coordinate) =>
        GeoCoordinate.Distance(municipality.Coordinate, coordinate);

    /// <summary>
    /// Calculates the distance from this municipality to the given <paramref name="coordinate"/>
    /// as a <see cref="Length"/>.
    /// </summary>
    public Length DistanceTo(GeoCoordinate coordinate) =>
        GeoCoordinate.Distance(Coordinate, coordinate);

    /// <summary>
    /// Calculates the distance from this municipality to <paramref name="other"/> as a <see cref="Length"/>.
    /// </summary>
    public Length DistanceTo(SwedishMunicipality other) =>
        GeoCoordinate.Distance(Coordinate, other.Coordinate);

    /// <summary>
    /// Returns all municipalities within <paramref name="radiusKm"/> kilometers of the given
    /// <paramref name="coordinate"/>, ordered by distance (nearest first).
    /// </summary>
    public static IReadOnlyList<MunicipalityDistance> FindNeighbors(GeoCoordinate coordinate, double radiusKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radiusKm);

        var results = new List<MunicipalityDistance>();
        foreach (var municipality in All)
        {
            var dist = GeoCoordinate.Distance(coordinate, municipality.Coordinate);
            if ((double)dist.Kilometers <= radiusKm)
                results.Add(new MunicipalityDistance(municipality, dist));
        }
        results.Sort((a, b) => a.Distance.Kilometers.CompareTo(b.Distance.Kilometers));
        return results;
    }

    public bool Equals(SwedishMunicipality? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is SwedishMunicipality other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishMunicipality? a, SwedishMunicipality? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishMunicipality? a, SwedishMunicipality? b) => !(a == b);
    public int CompareTo(SwedishMunicipality? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishMunicipality left, SwedishMunicipality right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishMunicipality left, SwedishMunicipality right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishMunicipality left, SwedishMunicipality right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishMunicipality left, SwedishMunicipality right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// A municipality paired with its distance from a reference point.
/// </summary>
/// <param name="Municipality">The neighboring municipality.</param>
/// <param name="Distance">The distance, calculated using the Haversine formula.</param>
public sealed record MunicipalityDistance(SwedishMunicipality Municipality, Length Distance);
