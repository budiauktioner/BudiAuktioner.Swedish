namespace Buildi.Primitives.Geography;

/// <summary>
/// The capital city of a country (<c>huvudstad</c>), with names in English, Swedish, and the country's
/// native language, plus its geographic coordinates.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P36">Wikidata P36 (capital)</see> — capital city of each country</description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625 (coordinate location)</see> — geographic coordinates for each capital</description></item>
/// </list>
/// </remarks>
public sealed class CountryCapital
{
    /// <summary>Capital city name in English, e.g. <c>Stockholm</c>, <c>Berlin</c>, <c>Tokyo</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Capital city name in Swedish, e.g. <c>Stockholm</c>, <c>Berlin</c>, <c>Tokyo</c> (or <c>Köpenhamn</c> for Copenhagen).</summary>
    public string LocalizedName { get; }

    /// <summary>Capital city name in the country's native language (endonym), e.g. <c>東京</c> for Tokyo, <c>Αθήνα</c> for Athens.</summary>
    public string NativeName { get; }

    /// <summary>
    /// Geographic coordinates (WGS 84) of the capital city, e.g. <c>59.3293°N, 18.0686°E</c> for Stockholm.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public GeoCoordinate Coordinate { get; }

    /// <summary>Capital city name in the current display language, e.g. <c>Köpenhamn</c> or <c>Copenhagen</c>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    internal CountryCapital(string englishName, string localizedName, string nativeName, GeoCoordinate coordinate)
    {
        EnglishName = englishName;
        LocalizedName = localizedName;
        NativeName = nativeName;
        Coordinate = coordinate;
    }

    /// <summary>Returns the capital city name in the current display language.</summary>
    public override string ToString() => DisplayName;
}
