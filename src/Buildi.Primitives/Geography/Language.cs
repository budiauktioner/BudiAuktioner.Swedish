using Buildi.Primitives;

namespace Buildi.Primitives.Geography;

/// <summary>
/// Text direction for a writing system.
/// </summary>
public enum TextDirection
{
    /// <summary>Left-to-right (e.g. Latin, Cyrillic, Greek scripts).</summary>
    LeftToRight = 0,

    /// <summary>Right-to-left (e.g. Arabic, Hebrew scripts).</summary>
    RightToLeft = 1
}

/// <summary>
/// Primary writing script / writing system used by a language.
/// </summary>
public enum WritingScript
{
    /// <summary>Unknown or unclassified script.</summary>
    Unknown = 0,
    Latin,
    Cyrillic,
    Arabic,
    Greek,
    Hebrew,
    Devanagari,
    Bengali,
    Gurmukhi,
    Gujarati,
    Oriya,
    Tamil,
    Telugu,
    Kannada,
    Malayalam,
    Sinhala,
    Thai,
    Lao,
    Tibetan,
    Myanmar,
    Georgian,
    Hangul,
    Ethiopic,
    Cherokee,
    Canadian,
    Khmer,
    Mongolian,
    Han,
    Armenian,
    Thaana
}

/// <summary>
/// A language (<c>språk</c>) identified by its ISO 639-1 two-letter code, ISO 639-2/T three-letter code,
/// English name, Swedish name, or native name (endonym).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.loc.gov/standards/iso639-2/php/code_list.php">ISO 639-2 — Library of Congress</see> — code list with English names</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes">Wikipedia — ISO 639-1 codes</see> — codes, names, native names, scripts</description></item>
/// <item><description><see href="https://iso639-3.sil.org/">SIL International — ISO 639-3</see> — language code tables</description></item>
/// </list>
/// </remarks>
public sealed class Language : IEquatable<Language>, IComparable<Language>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Language",
        "Språk",
        "🗣️",
        [
            "https://www.loc.gov/standards/iso639-2/php/code_list.php",
            "https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes",
            "https://iso639-3.sil.org/"
        ]);

    private const int MaxInputLength = 100;

    private static readonly Dictionary<string, Language> ByAlpha2;
    private static readonly Dictionary<string, Language> ByAlpha3;
    private static readonly Dictionary<string, Language> ByName;
    private static readonly Language[] AllLanguages;

    /// <summary>ISO 639-1 two-letter code (lowercased), e.g. <c>sv</c>.</summary>
    public string Alpha2Code { get; }

    /// <summary>ISO 639-2/T three-letter terminological code (lowercased), e.g. <c>swe</c>.</summary>
    public string Alpha3Code { get; }

    /// <summary>English name, e.g. <c>Swedish</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Swedish name, e.g. <c>Svenska</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Native name (endonym) — the language's name in that language, e.g. <c>Deutsch</c> for German.</summary>
    public string NativeName { get; }

    /// <summary>Primary writing script, e.g. <see cref="WritingScript.Latin"/>.</summary>
    public WritingScript Script { get; }

    /// <summary>Text direction, e.g. <see cref="TextDirection.LeftToRight"/>.</summary>
    public TextDirection Direction { get; }

    /// <summary>Display name in the current UI language, e.g. <c>Svenska</c> or <c>Swedish</c> depending on <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Canonical value (ISO 639-1 code), e.g. <c>sv</c>.</summary>
    public string Value => Alpha2Code;

    private Language(string alpha2, string alpha3, string englishName, string localizedName, string nativeName, WritingScript script, TextDirection direction)
    {
        Alpha2Code = alpha2;
        Alpha3Code = alpha3;
        EnglishName = englishName;
        LocalizedName = localizedName;
        NativeName = nativeName;
        Script = script;
        Direction = direction;
    }

    private static Language L(string a2, string a3, string en, string sv, string native, WritingScript script, TextDirection dir = TextDirection.LeftToRight)
        => new(a2, a3, en, sv, native, script, dir);

    static Language()
    {
        AllLanguages =
        [
            L("aa", "aar", "Afar", "Afar", "Afaraf", WritingScript.Latin),
            L("ab", "abk", "Abkhazian", "Abchaziska", "Аҧсуа", WritingScript.Cyrillic),
            L("af", "afr", "Afrikaans", "Afrikaans", "Afrikaans", WritingScript.Latin),
            L("ak", "aka", "Akan", "Akan", "Akan", WritingScript.Latin),
            L("am", "amh", "Amharic", "Amhariska", "አማርኛ", WritingScript.Ethiopic),
            L("an", "arg", "Aragonese", "Aragonesiska", "Aragonés", WritingScript.Latin),
            L("ar", "ara", "Arabic", "Arabiska", "العربية", WritingScript.Arabic, TextDirection.RightToLeft),
            L("as", "asm", "Assamese", "Assamesiska", "অসমীয়া", WritingScript.Bengali),
            L("av", "ava", "Avaric", "Avariska", "Авар", WritingScript.Cyrillic),
            L("ay", "aym", "Aymara", "Aymara", "Aymar", WritingScript.Latin),
            L("az", "aze", "Azerbaijani", "Azerbajdzjanska", "Azərbaycan", WritingScript.Latin),

            L("ba", "bak", "Bashkir", "Basjkiriska", "Башҡорт", WritingScript.Cyrillic),
            L("be", "bel", "Belarusian", "Vitryska", "Беларуская", WritingScript.Cyrillic),
            L("bg", "bul", "Bulgarian", "Bulgariska", "Български", WritingScript.Cyrillic),
            L("bh", "bih", "Bihari", "Bihari", "भोजपुरी", WritingScript.Devanagari),
            L("bi", "bis", "Bislama", "Bislama", "Bislama", WritingScript.Latin),
            L("bm", "bam", "Bambara", "Bambara", "Bamanankan", WritingScript.Latin),
            L("bn", "ben", "Bengali", "Bengali", "বাংলা", WritingScript.Bengali),
            L("bo", "bod", "Tibetan", "Tibetanska", "བོད་ཡིག", WritingScript.Tibetan),
            L("br", "bre", "Breton", "Bretonska", "Brezhoneg", WritingScript.Latin),
            L("bs", "bos", "Bosnian", "Bosniska", "Bosanski", WritingScript.Latin),

            L("ca", "cat", "Catalan", "Katalanska", "Català", WritingScript.Latin),
            L("ce", "che", "Chechen", "Tjetjenska", "Нохчийн", WritingScript.Cyrillic),
            L("ch", "cha", "Chamorro", "Chamorro", "Chamoru", WritingScript.Latin),
            L("co", "cos", "Corsican", "Korsikanska", "Corsu", WritingScript.Latin),
            L("cr", "cre", "Cree", "Cree", "ᓀᐦᐃᔭᐍᐏᐣ", WritingScript.Canadian),
            L("cs", "ces", "Czech", "Tjeckiska", "Čeština", WritingScript.Latin),
            L("cu", "chu", "Church Slavonic", "Kyrkoslaviska", "Словѣньскъ", WritingScript.Cyrillic),
            L("cv", "chv", "Chuvash", "Tjuvasjiska", "Чӑваш", WritingScript.Cyrillic),
            L("cy", "cym", "Welsh", "Walesiska", "Cymraeg", WritingScript.Latin),

            L("da", "dan", "Danish", "Danska", "Dansk", WritingScript.Latin),
            L("de", "deu", "German", "Tyska", "Deutsch", WritingScript.Latin),
            L("dv", "div", "Divehi", "Divehi", "ދިވެހި", WritingScript.Thaana, TextDirection.RightToLeft),
            L("dz", "dzo", "Dzongkha", "Dzongkha", "རྫོང་ཁ", WritingScript.Tibetan),

            L("ee", "ewe", "Ewe", "Ewe", "Eʋegbe", WritingScript.Latin),
            L("el", "ell", "Greek", "Grekiska", "Ελληνικά", WritingScript.Greek),
            L("en", "eng", "English", "Engelska", "English", WritingScript.Latin),
            L("eo", "epo", "Esperanto", "Esperanto", "Esperanto", WritingScript.Latin),
            L("es", "spa", "Spanish", "Spanska", "Español", WritingScript.Latin),
            L("et", "est", "Estonian", "Estniska", "Eesti", WritingScript.Latin),
            L("eu", "eus", "Basque", "Baskiska", "Euskara", WritingScript.Latin),

            L("fa", "fas", "Persian", "Persiska", "فارسی", WritingScript.Arabic, TextDirection.RightToLeft),
            L("ff", "ful", "Fulah", "Fulani", "Fulfulde", WritingScript.Latin),
            L("fi", "fin", "Finnish", "Finska", "Suomi", WritingScript.Latin),
            L("fj", "fij", "Fijian", "Fijianska", "Vosa Vakaviti", WritingScript.Latin),
            L("fo", "fao", "Faroese", "Färöiska", "Føroyskt", WritingScript.Latin),
            L("fr", "fra", "French", "Franska", "Français", WritingScript.Latin),
            L("fy", "fry", "Western Frisian", "Frisiska", "Frysk", WritingScript.Latin),

            L("ga", "gle", "Irish", "Iriska", "Gaeilge", WritingScript.Latin),
            L("gd", "gla", "Scottish Gaelic", "Skotsk gaeliska", "Gàidhlig", WritingScript.Latin),
            L("gl", "glg", "Galician", "Galiciska", "Galego", WritingScript.Latin),
            L("gn", "grn", "Guarani", "Guaraní", "Avañe'ẽ", WritingScript.Latin),
            L("gu", "guj", "Gujarati", "Gujarati", "ગુજરાતી", WritingScript.Gujarati),
            L("gv", "glv", "Manx", "Manx", "Gaelg", WritingScript.Latin),

            L("ha", "hau", "Hausa", "Hausa", "Hausa", WritingScript.Latin),
            L("he", "heb", "Hebrew", "Hebreiska", "עברית", WritingScript.Hebrew, TextDirection.RightToLeft),
            L("hi", "hin", "Hindi", "Hindi", "हिन्दी", WritingScript.Devanagari),
            L("ho", "hmo", "Hiri Motu", "Hiri Motu", "Hiri Motu", WritingScript.Latin),
            L("hr", "hrv", "Croatian", "Kroatiska", "Hrvatski", WritingScript.Latin),
            L("ht", "hat", "Haitian Creole", "Haitisk kreol", "Kreyòl Ayisyen", WritingScript.Latin),
            L("hu", "hun", "Hungarian", "Ungerska", "Magyar", WritingScript.Latin),
            L("hy", "hye", "Armenian", "Armeniska", "Հայերեն", WritingScript.Armenian),

            L("ia", "ina", "Interlingua", "Interlingua", "Interlingua", WritingScript.Latin),
            L("id", "ind", "Indonesian", "Indonesiska", "Bahasa Indonesia", WritingScript.Latin),
            L("ie", "ile", "Interlingue", "Interlingue", "Interlingue", WritingScript.Latin),
            L("ig", "ibo", "Igbo", "Igbo", "Igbo", WritingScript.Latin),
            L("ii", "iii", "Sichuan Yi", "Yi", "ꆈꌠ꒡", WritingScript.Unknown),
            L("ik", "ipk", "Inupiaq", "Inupiaq", "Iñupiaq", WritingScript.Latin),
            L("io", "ido", "Ido", "Ido", "Ido", WritingScript.Latin),
            L("is", "isl", "Icelandic", "Isländska", "Íslenska", WritingScript.Latin),
            L("it", "ita", "Italian", "Italienska", "Italiano", WritingScript.Latin),
            L("iu", "iku", "Inuktitut", "Inuktitut", "ᐃᓄᒃᑎᑐᑦ", WritingScript.Canadian),

            L("ja", "jpn", "Japanese", "Japanska", "日本語", WritingScript.Han),
            L("jv", "jav", "Javanese", "Javanesiska", "Basa Jawa", WritingScript.Latin),

            L("ka", "kat", "Georgian", "Georgiska", "ქართული", WritingScript.Georgian),
            L("kg", "kon", "Kongo", "Kikongo", "KiKongo", WritingScript.Latin),
            L("ki", "kik", "Kikuyu", "Kikuyu", "Gĩkũyũ", WritingScript.Latin),
            L("kj", "kua", "Kuanyama", "Kuanyama", "Kuanyama", WritingScript.Latin),
            L("kk", "kaz", "Kazakh", "Kazakiska", "Қазақша", WritingScript.Cyrillic),
            L("kl", "kal", "Kalaallisut", "Grönländska", "Kalaallisut", WritingScript.Latin),
            L("km", "khm", "Khmer", "Khmer", "ភាសាខ្មែរ", WritingScript.Khmer),
            L("kn", "kan", "Kannada", "Kannada", "ಕನ್ನಡ", WritingScript.Kannada),
            L("ko", "kor", "Korean", "Koreanska", "한국어", WritingScript.Hangul),
            L("kr", "kau", "Kanuri", "Kanuri", "Kanuri", WritingScript.Latin),
            L("ks", "kas", "Kashmiri", "Kashmiri", "कश्मीरी", WritingScript.Arabic, TextDirection.RightToLeft),
            L("ku", "kur", "Kurdish", "Kurdiska", "Kurdî", WritingScript.Latin),
            L("kv", "kom", "Komi", "Komi", "Коми", WritingScript.Cyrillic),
            L("kw", "cor", "Cornish", "Korniska", "Kernewek", WritingScript.Latin),
            L("ky", "kir", "Kyrgyz", "Kirgiziska", "Кыргызча", WritingScript.Cyrillic),

            L("la", "lat", "Latin", "Latin", "Latina", WritingScript.Latin),
            L("lb", "ltz", "Luxembourgish", "Luxemburgiska", "Lëtzebuergesch", WritingScript.Latin),
            L("lg", "lug", "Ganda", "Luganda", "Luganda", WritingScript.Latin),
            L("li", "lim", "Limburgish", "Limburgiska", "Limburgs", WritingScript.Latin),
            L("ln", "lin", "Lingala", "Lingala", "Lingála", WritingScript.Latin),
            L("lo", "lao", "Lao", "Laotiska", "ພາສາລາວ", WritingScript.Lao),
            L("lt", "lit", "Lithuanian", "Litauiska", "Lietuvių", WritingScript.Latin),
            L("lu", "lub", "Luba-Katanga", "Luba-Katanga", "Kiluba", WritingScript.Latin),
            L("lv", "lav", "Latvian", "Lettiska", "Latviešu", WritingScript.Latin),

            L("mg", "mlg", "Malagasy", "Malagassiska", "Malagasy", WritingScript.Latin),
            L("mh", "mah", "Marshallese", "Marshallesiska", "Kajin M̧ajeļ", WritingScript.Latin),
            L("mi", "mri", "Maori", "Maori", "Te Reo Māori", WritingScript.Latin),
            L("mk", "mkd", "Macedonian", "Makedonska", "Македонски", WritingScript.Cyrillic),
            L("ml", "mal", "Malayalam", "Malayalam", "മലയാളം", WritingScript.Malayalam),
            L("mn", "mon", "Mongolian", "Mongoliska", "Монгол", WritingScript.Cyrillic),
            L("mr", "mar", "Marathi", "Marathi", "मराठी", WritingScript.Devanagari),
            L("ms", "msa", "Malay", "Malajiska", "Bahasa Melayu", WritingScript.Latin),
            L("mt", "mlt", "Maltese", "Maltesiska", "Malti", WritingScript.Latin),
            L("my", "mya", "Burmese", "Burmesiska", "မြန်မာစာ", WritingScript.Myanmar),

            L("na", "nau", "Nauru", "Nauru", "Dorerin Naoero", WritingScript.Latin),
            L("nb", "nob", "Norwegian Bokmål", "Norska (bokmål)", "Norsk bokmål", WritingScript.Latin),
            L("nd", "nde", "Northern Ndebele", "Nordndebele", "isiNdebele", WritingScript.Latin),
            L("ne", "nep", "Nepali", "Nepalesiska", "नेपाली", WritingScript.Devanagari),
            L("ng", "ndo", "Ndonga", "Ndonga", "Owambo", WritingScript.Latin),
            L("nl", "nld", "Dutch", "Nederländska", "Nederlands", WritingScript.Latin),
            L("nn", "nno", "Norwegian Nynorsk", "Norska (nynorsk)", "Norsk nynorsk", WritingScript.Latin),
            L("no", "nor", "Norwegian", "Norska", "Norsk", WritingScript.Latin),
            L("nr", "nbl", "Southern Ndebele", "Sydndebele", "isiNdebele", WritingScript.Latin),
            L("nv", "nav", "Navajo", "Navajo", "Diné Bizaad", WritingScript.Latin),
            L("ny", "nya", "Chichewa", "Chichewa", "Chichewa", WritingScript.Latin),

            L("oc", "oci", "Occitan", "Occitanska", "Occitan", WritingScript.Latin),
            L("oj", "oji", "Ojibwa", "Ojibwa", "ᐊᓂᔑᓈᐯᒧᐎᓐ", WritingScript.Canadian),
            L("om", "orm", "Oromo", "Oromo", "Afaan Oromoo", WritingScript.Latin),
            L("or", "ori", "Oriya", "Oriya", "ଓଡ଼ିଆ", WritingScript.Oriya),
            L("os", "oss", "Ossetian", "Ossetiska", "Ирон", WritingScript.Cyrillic),

            L("pa", "pan", "Punjabi", "Punjabi", "ਪੰਜਾਬੀ", WritingScript.Gurmukhi),
            L("pi", "pli", "Pali", "Pali", "पालि", WritingScript.Devanagari),
            L("pl", "pol", "Polish", "Polska", "Polski", WritingScript.Latin),
            L("ps", "pus", "Pashto", "Pashto", "پښتو", WritingScript.Arabic, TextDirection.RightToLeft),
            L("pt", "por", "Portuguese", "Portugisiska", "Português", WritingScript.Latin),

            L("qu", "que", "Quechua", "Quechua", "Runa Simi", WritingScript.Latin),

            L("rm", "roh", "Romansh", "Rätoromanska", "Rumantsch", WritingScript.Latin),
            L("rn", "run", "Rundi", "Kirundi", "Ikirundi", WritingScript.Latin),
            L("ro", "ron", "Romanian", "Rumänska", "Română", WritingScript.Latin),
            L("ru", "rus", "Russian", "Ryska", "Русский", WritingScript.Cyrillic),
            L("rw", "kin", "Kinyarwanda", "Kinyarwanda", "Ikinyarwanda", WritingScript.Latin),

            L("sa", "san", "Sanskrit", "Sanskrit", "संस्कृतम्", WritingScript.Devanagari),
            L("sc", "srd", "Sardinian", "Sardiska", "Sardu", WritingScript.Latin),
            L("sd", "snd", "Sindhi", "Sindhi", "سنڌي", WritingScript.Arabic, TextDirection.RightToLeft),
            L("se", "sme", "Northern Sami", "Nordsamiska", "Davvisámegiella", WritingScript.Latin),
            L("sg", "sag", "Sango", "Sango", "Yângâ tî Sängö", WritingScript.Latin),
            L("si", "sin", "Sinhala", "Singalesiska", "සිංහල", WritingScript.Sinhala),
            L("sk", "slk", "Slovak", "Slovakiska", "Slovenčina", WritingScript.Latin),
            L("sl", "slv", "Slovenian", "Slovenska", "Slovenščina", WritingScript.Latin),
            L("sm", "smo", "Samoan", "Samoanska", "Gagana Sāmoa", WritingScript.Latin),
            L("sn", "sna", "Shona", "Shona", "ChiShona", WritingScript.Latin),
            L("so", "som", "Somali", "Somaliska", "Soomaali", WritingScript.Latin),
            L("sq", "sqi", "Albanian", "Albanska", "Shqip", WritingScript.Latin),
            L("sr", "srp", "Serbian", "Serbiska", "Српски", WritingScript.Cyrillic),
            L("ss", "ssw", "Swati", "Swati", "SiSwati", WritingScript.Latin),
            L("st", "sot", "Southern Sotho", "Sotho", "Sesotho", WritingScript.Latin),
            L("su", "sun", "Sundanese", "Sundanesiska", "Basa Sunda", WritingScript.Latin),
            L("sv", "swe", "Swedish", "Svenska", "Svenska", WritingScript.Latin),
            L("sw", "swa", "Swahili", "Swahili", "Kiswahili", WritingScript.Latin),

            L("ta", "tam", "Tamil", "Tamil", "தமிழ்", WritingScript.Tamil),
            L("te", "tel", "Telugu", "Telugu", "తెలుగు", WritingScript.Telugu),
            L("tg", "tgk", "Tajik", "Tadzjikiska", "Тоҷикӣ", WritingScript.Cyrillic),
            L("th", "tha", "Thai", "Thailändska", "ไทย", WritingScript.Thai),
            L("ti", "tir", "Tigrinya", "Tigrinja", "ትግርኛ", WritingScript.Ethiopic),
            L("tk", "tuk", "Turkmen", "Turkmeniska", "Türkmen", WritingScript.Latin),
            L("tl", "tgl", "Tagalog", "Tagalog", "Tagalog", WritingScript.Latin),
            L("tn", "tsn", "Tswana", "Tswana", "Setswana", WritingScript.Latin),
            L("to", "ton", "Tongan", "Tonganska", "Lea Faka-Tonga", WritingScript.Latin),
            L("tr", "tur", "Turkish", "Turkiska", "Türkçe", WritingScript.Latin),
            L("ts", "tso", "Tsonga", "Tsonga", "Xitsonga", WritingScript.Latin),
            L("tt", "tat", "Tatar", "Tatariska", "Татар", WritingScript.Cyrillic),
            L("tw", "twi", "Twi", "Twi", "Twi", WritingScript.Latin),
            L("ty", "tah", "Tahitian", "Tahitiska", "Reo Tahiti", WritingScript.Latin),

            L("ug", "uig", "Uyghur", "Uiguriska", "ئۇيغۇرچە", WritingScript.Arabic, TextDirection.RightToLeft),
            L("uk", "ukr", "Ukrainian", "Ukrainska", "Українська", WritingScript.Cyrillic),
            L("ur", "urd", "Urdu", "Urdu", "اردو", WritingScript.Arabic, TextDirection.RightToLeft),
            L("uz", "uzb", "Uzbek", "Uzbekiska", "Oʻzbek", WritingScript.Latin),

            L("ve", "ven", "Venda", "Venda", "Tshivenḓa", WritingScript.Latin),
            L("vi", "vie", "Vietnamese", "Vietnamesiska", "Tiếng Việt", WritingScript.Latin),
            L("vo", "vol", "Volapük", "Volapük", "Volapük", WritingScript.Latin),

            L("wa", "wln", "Walloon", "Vallonska", "Walon", WritingScript.Latin),
            L("wo", "wol", "Wolof", "Wolof", "Wollof", WritingScript.Latin),

            L("xh", "xho", "Xhosa", "Xhosa", "isiXhosa", WritingScript.Latin),

            L("yi", "yid", "Yiddish", "Jiddisch", "ייִדיש", WritingScript.Hebrew, TextDirection.RightToLeft),
            L("yo", "yor", "Yoruba", "Yoruba", "Yorùbá", WritingScript.Latin),

            L("za", "zha", "Zhuang", "Zhuang", "Saɯ cueŋƅ", WritingScript.Latin),
            L("zh", "zho", "Chinese", "Kinesiska", "中文", WritingScript.Han),
            L("zu", "zul", "Zulu", "Zulu", "isiZulu", WritingScript.Latin),
        ];

        ByAlpha2 = new(StringComparer.OrdinalIgnoreCase);
        ByAlpha3 = new(StringComparer.OrdinalIgnoreCase);
        ByName = new(StringComparer.OrdinalIgnoreCase);

        foreach (var lang in AllLanguages)
        {
            ByAlpha2[lang.Alpha2Code] = lang;
            ByAlpha3[lang.Alpha3Code] = lang;
            ByName[lang.EnglishName] = lang;

            if (!ByName.ContainsKey(lang.LocalizedName))
                ByName[lang.LocalizedName] = lang;

            if (!string.Equals(lang.NativeName, lang.EnglishName, StringComparison.OrdinalIgnoreCase)
                && !ByName.ContainsKey(lang.NativeName))
                ByName[lang.NativeName] = lang;
        }
    }

    /// <summary>All recognized ISO 639-1 languages.</summary>
    public static IReadOnlyList<Language> All => AllLanguages;

    // --- Static named properties for commonly used languages ---

    public static Language Afrikaans => ByAlpha2["af"];
    public static Language Albanian => ByAlpha2["sq"];
    public static Language Amharic => ByAlpha2["am"];
    public static Language Arabic => ByAlpha2["ar"];
    public static Language Armenian => ByAlpha2["hy"];
    public static Language Azerbaijani => ByAlpha2["az"];
    public static Language Basque => ByAlpha2["eu"];
    public static Language Belarusian => ByAlpha2["be"];
    public static Language Bengali => ByAlpha2["bn"];
    public static Language Bosnian => ByAlpha2["bs"];
    public static Language Bulgarian => ByAlpha2["bg"];
    public static Language Burmese => ByAlpha2["my"];
    public static Language Catalan => ByAlpha2["ca"];
    public static Language Chinese => ByAlpha2["zh"];
    public static Language Croatian => ByAlpha2["hr"];
    public static Language Czech => ByAlpha2["cs"];
    public static Language Danish => ByAlpha2["da"];
    public static Language Dutch => ByAlpha2["nl"];
    public static Language English => ByAlpha2["en"];
    public static Language Estonian => ByAlpha2["et"];
    public static Language Faroese => ByAlpha2["fo"];
    public static Language Finnish => ByAlpha2["fi"];
    public static Language French => ByAlpha2["fr"];
    public static Language Galician => ByAlpha2["gl"];
    public static Language Georgian => ByAlpha2["ka"];
    public static Language German => ByAlpha2["de"];
    public static Language Greek => ByAlpha2["el"];
    public static Language Gujarati => ByAlpha2["gu"];
    public static Language Hebrew => ByAlpha2["he"];
    public static Language Hindi => ByAlpha2["hi"];
    public static Language Hungarian => ByAlpha2["hu"];
    public static Language Icelandic => ByAlpha2["is"];
    public static Language Indonesian => ByAlpha2["id"];
    public static Language Irish => ByAlpha2["ga"];
    public static Language Italian => ByAlpha2["it"];
    public static Language Japanese => ByAlpha2["ja"];
    public static Language Kannada => ByAlpha2["kn"];
    public static Language Kazakh => ByAlpha2["kk"];
    public static Language Khmer => ByAlpha2["km"];
    public static Language Korean => ByAlpha2["ko"];
    public static Language Kurdish => ByAlpha2["ku"];
    public static Language Kyrgyz => ByAlpha2["ky"];
    public static Language Lao => ByAlpha2["lo"];
    public static Language Latin => ByAlpha2["la"];
    public static Language Latvian => ByAlpha2["lv"];
    public static Language Lithuanian => ByAlpha2["lt"];
    public static Language Luxembourgish => ByAlpha2["lb"];
    public static Language Macedonian => ByAlpha2["mk"];
    public static Language Malay => ByAlpha2["ms"];
    public static Language Malayalam => ByAlpha2["ml"];
    public static Language Maltese => ByAlpha2["mt"];
    public static Language Maori => ByAlpha2["mi"];
    public static Language Marathi => ByAlpha2["mr"];
    public static Language Mongolian => ByAlpha2["mn"];
    public static Language Nepali => ByAlpha2["ne"];
    public static Language NorthernSami => ByAlpha2["se"];
    public static Language Norwegian => ByAlpha2["no"];
    public static Language NorwegianBokmal => ByAlpha2["nb"];
    public static Language NorwegianNynorsk => ByAlpha2["nn"];
    public static Language Pashto => ByAlpha2["ps"];
    public static Language Persian => ByAlpha2["fa"];
    public static Language Polish => ByAlpha2["pl"];
    public static Language Portuguese => ByAlpha2["pt"];
    public static Language Punjabi => ByAlpha2["pa"];
    public static Language Quechua => ByAlpha2["qu"];
    public static Language Romanian => ByAlpha2["ro"];
    public static Language Romansh => ByAlpha2["rm"];
    public static Language Russian => ByAlpha2["ru"];
    public static Language Serbian => ByAlpha2["sr"];
    public static Language Sinhala => ByAlpha2["si"];
    public static Language Slovak => ByAlpha2["sk"];
    public static Language Slovenian => ByAlpha2["sl"];
    public static Language Somali => ByAlpha2["so"];
    public static Language Spanish => ByAlpha2["es"];
    public static Language Swahili => ByAlpha2["sw"];
    public static Language Swedish => ByAlpha2["sv"];
    public static Language Tagalog => ByAlpha2["tl"];
    public static Language Tamil => ByAlpha2["ta"];
    public static Language Telugu => ByAlpha2["te"];
    public static Language Thai => ByAlpha2["th"];
    public static Language Tibetan => ByAlpha2["bo"];
    public static Language Tigrinya => ByAlpha2["ti"];
    public static Language Turkish => ByAlpha2["tr"];
    public static Language Ukrainian => ByAlpha2["uk"];
    public static Language Urdu => ByAlpha2["ur"];
    public static Language Uzbek => ByAlpha2["uz"];
    public static Language Vietnamese => ByAlpha2["vi"];
    public static Language Welsh => ByAlpha2["cy"];
    public static Language Yiddish => ByAlpha2["yi"];
    public static Language Zulu => ByAlpha2["zu"];

    public static bool TryParse(string? input, out Language? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        if (ByAlpha2.TryGetValue(trimmed, out result)) return true;
        if (ByAlpha3.TryGetValue(trimmed, out result)) return true;
        if (ByName.TryGetValue(trimmed, out result)) return true;

        return false;
    }

    public static Language Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown language.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the language in the current display language, e.g. <c>Svenska</c> or <c>Swedish</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r) ? r!.DisplayName
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
            : null;

    /// <summary>
    /// Returns the normalized ISO 639-1 two-letter code, e.g. <c>sv</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Alpha2Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the ISO 639-1 two-letter code, e.g. <c>sv</c>.</summary>
    public string ToNormalizedString() => Alpha2Code;

    /// <summary>
    /// Returns the language in the current display language (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>
    /// is true, otherwise English), for example <c>Svenska</c> or <c>Swedish</c>.
    /// </summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>
    /// Returns the language as its English name, for example <c>Swedish</c>.
    /// </summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>
    /// Returns the language in its own native form (endonym), for example <c>Deutsch</c> for German,
    /// <c>日本語</c> for Japanese, <c>Svenska</c> for Swedish.
    /// </summary>
    public string ToNativeString() => NativeName;

    /// <summary>
    /// Returns the language in the current display language, for example <c>Svenska</c> or <c>Swedish</c>.
    /// </summary>
    public override string ToString() => DisplayName;

    public bool Equals(Language? other) => other is not null && Alpha2Code == other.Alpha2Code;
    public override bool Equals(object? obj) => obj is Language other && Equals(other);
    public override int GetHashCode() => Alpha2Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Language? a, Language? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Language? a, Language? b) => !(a == b);
    public int CompareTo(Language? other) => other is null ? 1 : string.Compare(Alpha2Code, other.Alpha2Code, StringComparison.Ordinal);
    public static bool operator <(Language left, Language right) => left.CompareTo(right) < 0;
    public static bool operator >(Language left, Language right) => left.CompareTo(right) > 0;
    public static bool operator <=(Language left, Language right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Language left, Language right) => left.CompareTo(right) >= 0;
}
