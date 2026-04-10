using System.Text;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Extension methods for masking address components in display strings.
/// </summary>
public static class AddressMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked zip code, e.g. <c>114 53</c> → <c>*** **</c>.
    /// </summary>
    public static string ToMaskedString(this AddressZipCode zipCode)
    {
        var formatted = zipCode.Formatted;
        var masked = new char[formatted.Length];
        for (var i = 0; i < formatted.Length; i++)
            masked[i] = formatted[i] == ' ' ? ' ' : MaskChar;
        return new string(masked);
    }

    /// <summary>
    /// Returns a masked address preserving the street name, city, and country while masking
    /// house number, apartment number, and zip code.
    /// For example <c>Storgatan 12, 114 53 Stockholm</c> → <c>Storgatan **, *** ** Stockholm</c>
    /// and <c>Box 123, 114 53 Stockholm</c> → <c>Box ***, *** ** Stockholm</c>.
    /// </summary>
    public static string ToMaskedString(this Address address)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(address.CareOf))
            sb.Append($"c/o {MaskKeepFirst(address.CareOf, 1)}, ");

        if (address.IsPostBox && address.PostBox != null)
        {
            sb.Append("Box ");
            sb.Append(new string(MaskChar, address.PostBox.Length));
        }
        else
        {
            if (address.Street.StreetName != null)
                sb.Append(address.Street.StreetName);
            else
                sb.Append(address.Street.Street);

            if (address.Street.StreetNumber != null)
            {
                sb.Append(' ');
                sb.Append(new string(MaskChar, address.Street.StreetNumber.Length));
            }

            if (!string.IsNullOrWhiteSpace(address.ApartmentNumber))
            {
                sb.Append(" lgh ");
                sb.Append(new string(MaskChar, address.ApartmentNumber.Length));
            }
        }

        if (address.ZipCode != null)
        {
            sb.Append(", ");
            sb.Append(address.ZipCode.ToMaskedString());
        }

        if (address.City != null)
        {
            sb.Append(address.ZipCode != null ? " " : ", ");
            sb.Append(address.City.Value);
        }

        if (address.Country != null)
        {
            sb.Append(", ");
            sb.Append(address.Country.LocalizedName);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a masked Swedish zip code, e.g. <c>114 53</c> → <c>*** **</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishAddressZipCode zipCode) =>
        zipCode.ZipCode.ToMaskedString();

    /// <summary>
    /// Returns a masked Swedish address, preserving the street name and city while masking
    /// house number, apartment number, and zip code.
    /// For example <c>Storgatan 12, 114 53 Stockholm</c> → <c>Storgatan **, *** ** Stockholm</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishAddress address) =>
        address.Address.ToMaskedString();

    /// <summary>Masks a Polish zip code, e.g. <c>00-950</c> → <c>**-***</c>.</summary>
    public static string ToMaskedString(this PolishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Polish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this PolishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks an Estonian zip code, e.g. <c>10115</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this EstonianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks an Estonian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this EstonianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Finnish zip code, e.g. <c>00100</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this FinnishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Finnish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this FinnishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Lithuanian zip code, e.g. <c>LT-01001</c> → <c>**-*****</c>.</summary>
    public static string ToMaskedString(this LithuanianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Lithuanian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this LithuanianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Romanian zip code, e.g. <c>010011</c> → <c>******</c>.</summary>
    public static string ToMaskedString(this RomanianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Romanian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this RomanianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Danish zip code, e.g. <c>1050</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this DanishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Danish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this DanishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Norwegian zip code, e.g. <c>0150</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this NorwegianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Norwegian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this NorwegianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a German zip code, e.g. <c>10115</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this GermanAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a German address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this GermanAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Bulgarian zip code, e.g. <c>1000</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this BulgarianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Bulgarian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this BulgarianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Latvian zip code, e.g. <c>LV-1050</c> → <c>**-****</c>.</summary>
    public static string ToMaskedString(this LatvianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Latvian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this LatvianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Czech zip code, e.g. <c>110 00</c> → <c>*** **</c>.</summary>
    public static string ToMaskedString(this CzechAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Czech address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this CzechAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Spanish zip code, e.g. <c>28001</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this SpanishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Spanish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this SpanishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Dutch zip code, e.g. <c>1012 AB</c> → <c>**** **</c>.</summary>
    public static string ToMaskedString(this DutchAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Dutch address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this DutchAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Greek zip code, e.g. <c>106 74</c> → <c>*** **</c>.</summary>
    public static string ToMaskedString(this GreekAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Greek address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this GreekAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks an Italian zip code, e.g. <c>00186</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this ItalianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks an Italian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this ItalianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Slovenian zip code, e.g. <c>1000</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this SlovenianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Slovenian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this SlovenianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Croatian zip code, e.g. <c>10000</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this CroatianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Croatian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this CroatianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Portuguese zip code, e.g. <c>1100-148</c> → <c>****-***</c>.</summary>
    public static string ToMaskedString(this PortugueseAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Portuguese address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this PortugueseAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Hungarian zip code, e.g. <c>1055</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this HungarianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Hungarian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this HungarianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a French zip code, e.g. <c>75008</c> → <c>*****</c>.</summary>
    public static string ToMaskedString(this FrenchAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a French address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this FrenchAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Slovak zip code, e.g. <c>811 01</c> → <c>*** **</c>.</summary>
    public static string ToMaskedString(this SlovakAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Slovak address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this SlovakAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Belgian zip code, e.g. <c>1000</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this BelgianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Belgian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this BelgianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a British postcode, e.g. <c>SW1A 1AA</c> → <c>**** ***</c>.</summary>
    public static string ToMaskedString(this BritishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a British address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this BritishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks an Austrian zip code, e.g. <c>1010</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this AustrianAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks an Austrian address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this AustrianAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Cypriot zip code, e.g. <c>1060</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this CypriotAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Cypriot address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this CypriotAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks an Icelandic zip code, e.g. <c>101</c> → <c>***</c>.</summary>
    public static string ToMaskedString(this IcelandicAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks an Icelandic address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this IcelandicAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Swiss zip code, e.g. <c>3005</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this SwissAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Swiss address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this SwissAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks an Irish Eircode, e.g. <c>D02 XR20</c> → <c>*** ****</c>.</summary>
    public static string ToMaskedString(this IrishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks an Irish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this IrishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Luxembourgish zip code, e.g. <c>1648</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this LuxembourgishAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Luxembourgish address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this LuxembourgishAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Maltese zip code, e.g. <c>VLT 1535</c> → <c>*** ****</c>.</summary>
    public static string ToMaskedString(this MalteseAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Maltese address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this MalteseAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks a Liechtenstein zip code, e.g. <c>9490</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this LiechtensteinAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();
    /// <summary>Masks a Liechtenstein address, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this LiechtensteinAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks any country-specific address via the common interface, delegating to the underlying <see cref="Address"/>.</summary>
    public static string ToMaskedString(this ICountryAddress address) => address.Address.ToMaskedString();

    /// <summary>Masks any country-specific zip code via the common interface, delegating to the underlying <see cref="AddressZipCode"/>.</summary>
    public static string ToMaskedString(this ICountryAddressZipCode zipCode) => zipCode.ZipCode.ToMaskedString();

    /// <summary>
    /// Returns a masked street address preserving the street name but masking the house number,
    /// e.g. <c>Storgatan 12</c> → <c>Storgatan **</c>.
    /// </summary>
    public static string ToMaskedString(this AddressStreet street)
    {
        if (street.StreetName is null)
            return new string(MaskChar, street.Street.Length);

        if (street.StreetNumber is null)
            return street.StreetName;

        return $"{street.StreetName} {new string(MaskChar, street.StreetNumber.Length)}";
    }

    /// <summary>
    /// Returns a masked city name showing only the first character,
    /// e.g. <c>Stockholm</c> → <c>S********</c>.
    /// </summary>
    public static string ToMaskedString(this AddressCity city)
    {
        var value = city.Value;
        return value.Length <= 1
            ? new string(MaskChar, value.Length)
            : $"{value[0]}{new string(MaskChar, value.Length - 1)}";
    }

    private static string MaskKeepFirst(string value, int visibleChars)
    {
        if (value.Length <= visibleChars) return value;
        return value[..visibleChars] + new string(MaskChar, value.Length - visibleChars);
    }
}
