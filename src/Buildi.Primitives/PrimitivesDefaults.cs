using System.Globalization;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives;

/// <summary>
/// Configurable defaults that control locale-sensitive behavior across all primitive types.
/// When not explicitly set, values are auto-detected from the current thread's culture.
/// Set properties at application startup or override per-request as needed.
/// </summary>
/// <remarks>
/// <para>This class exposes two distinct culture properties that map to .NET's two culture concepts:</para>
/// <list type="bullet">
/// <item><description><see cref="Culture"/> (falls back to <see cref="CultureInfo.CurrentCulture"/>) —
/// governs formatting and text operations: number formatting, string casing (e.g. <c>TextInfo.ToUpper</c>).
/// Used by <c>AddressCity</c>, <c>AddressStreet</c>, <c>PersonNameNormalization</c>, <c>MoneyAmount</c>.</description></item>
/// <item><description><see cref="UICulture"/> (falls back to <see cref="CultureInfo.CurrentUICulture"/>) —
/// governs display language selection (Swedish vs English names).
/// Used by <c>Country.DisplayName</c>, <c>Currency.DisplayName</c>, <c>Address.ToDisplayString()</c>.</description></item>
/// </list>
/// </remarks>
public static class PrimitivesDefaults
{
    private static string? _countryAlpha2Code;
    private static CultureInfo? _culture;
    private static CultureInfo? _uiCulture;

    /// <summary>
    /// Default ISO 3166-1 alpha-2 country code used by types when no explicit country is specified.
    /// When not explicitly set, derived from <see cref="Culture"/> via <see cref="RegionInfo"/>
    /// (e.g. <c>sv-SE</c> → <c>SE</c>, <c>de-DE</c> → <c>DE</c>).
    /// Falls back to <c>SE</c> for invariant or region-less cultures.
    /// </summary>
    public static string CountryAlpha2Code
    {
        get => _countryAlpha2Code ?? DeriveCountryFromCulture();
        set => _countryAlpha2Code = value;
    }

    /// <summary>
    /// Culture for formatting and text operations (casing, number/date formatting).
    /// When not explicitly set, falls back to <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public static CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    /// <summary>
    /// Culture for display language selection (Swedish vs English names).
    /// When not explicitly set, falls back to <see cref="CultureInfo.CurrentUICulture"/>.
    /// When the two-letter language code is <c>sv</c>, Swedish display names are used; otherwise English.
    /// </summary>
    public static CultureInfo UICulture
    {
        get => _uiCulture ?? CultureInfo.CurrentUICulture;
        set => _uiCulture = value;
    }

    /// <summary>
    /// <see langword="true"/> when <see cref="UICulture"/> is Swedish (<c>sv</c> or <c>sv-*</c>).
    /// Types use this to select between <c>LocalizedName</c> and <c>EnglishName</c> for display.
    /// </summary>
    internal static bool UseLocalizedDisplayNames => UICulture.TwoLetterISOLanguageName == "sv";

    /// <summary>
    /// Default phone calling code, derived from <see cref="CountryAlpha2Code"/>.
    /// For example, when <see cref="CountryAlpha2Code"/> is <c>SE</c>, returns calling code <c>46</c>.
    /// Returns <see cref="PhoneCallingCode.Sweden"/> as fallback when the country cannot be resolved.
    /// </summary>
    public static PhoneCallingCode DefaultCallingCode
    {
        get
        {
            if (Country.TryParse(CountryAlpha2Code, out var country) && country is not null)
                return country.CallingCode;
            return PhoneCallingCode.Sweden;
        }
    }

    /// <summary>
    /// Resets all overrides. Properties revert to auto-detection from the current thread's cultures.
    /// </summary>
    public static void Reset()
    {
        _countryAlpha2Code = null;
        _culture = null;
        _uiCulture = null;
    }

    private static string DeriveCountryFromCulture()
    {
        try
        {
            var culture = _culture ?? CultureInfo.CurrentCulture;
            if (culture.Equals(CultureInfo.InvariantCulture) || string.IsNullOrEmpty(culture.Name))
                return "SE";

            var region = new RegionInfo(culture.Name);
            return region.TwoLetterISORegionName;
        }
        catch
        {
            return "SE";
        }
    }
}
