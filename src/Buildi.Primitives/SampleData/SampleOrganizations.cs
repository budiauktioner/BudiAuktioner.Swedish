using BankingTypes = Buildi.Primitives.Banking;
using ContactTypes = Buildi.Primitives.Contact;
using GeoTypes = Buildi.Primitives.Geography;
using OrgTypes = Buildi.Primitives.Organization;
using PropTypes = Buildi.Primitives.Property;
using WebTypes = Buildi.Primitives.Web;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.SampleData;

/// <summary>
/// Catalog of publicly known Swedish organizations with parsed sample data.
/// See <c>TEST_AND_SAMPLE_DATA.md</c> for strategy, rules, and source links.
/// </summary>
public static class SampleOrganizations
{
    // ---------------------------------------------------------------------------
    // Aktiebolag (AB)
    // ---------------------------------------------------------------------------

    public static SampleOrganization BudiAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Budi AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("559246-0421"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE559246042101"),
        DunsNumber = OrgTypes.DunsNumber.Parse("350827673"),
        SniCode = OrgTypes.SwedishSniCode.Parse("47917"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Linta Gårdsväg 5A"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("168 74"),
        AddressCity = ContactTypes.AddressCity.Parse("Bromma"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Bankgiro = BankingTypes.SwedishBankgiroNumber.Parse("235-9321"),
        Sources =
        [
            "Provided directly by the organization (package author)",
            "https://www.allabolag.se/5592460421",
        ],
    };

    public static SampleOrganization SystembolagetAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Systembolaget AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556059-9473"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556059947301"),
        SniCode = OrgTypes.SwedishSniCode.Parse("47250"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46850330000"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Kungsträdgårdsgatan 14"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("103 84"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.systembolaget.se/kontakta-oss/",
            "https://www.allabolag.se/5560599473/systembolaget-aktiebolag",
        ],
    };

    public static SampleOrganization SJAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("SJ AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556196-1599"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556196159901"),
        SniCode = OrgTypes.SwedishSniCode.Parse("49100"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46107516000"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Vasagatan 10"),
        PostalAddressBox = "Box 105 50",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("111 20"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.sj.se/kundservice/kontakt",
            "https://www.allabolag.se/5561961599",
        ],
    };

    public static SampleOrganization PostNordSverigeAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("PostNord Sverige AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556711-5695"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556711569501"),
        SniCode = OrgTypes.SwedishSniCode.Parse("53100"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Terminalvägen 24"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("171 73"),
        AddressCity = ContactTypes.AddressCity.Parse("Solna"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Solna"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.postnord.se/foretag/forberedelser/fakturering",
            "https://www.allabolag.se/5567115695",
        ],
    };

    public static SampleOrganization VattenfallAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Vattenfall AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556036-2138"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556036213801"),
        LeiCode = OrgTypes.LeiCode.Parse("549300T5RZ1HA5HZ3109"),
        SniCode = OrgTypes.SwedishSniCode.Parse("35120"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+4687396000"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Evenemangsgatan 13"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("169 79"),
        AddressCity = ContactTypes.AddressCity.Parse("Solna"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Solna"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Bankgiro = BankingTypes.SwedishBankgiroNumber.Parse("5110-8348"),
        Plusgiro = BankingTypes.SwedishPostgiroNumber.Parse("4131300-8"),
        Iban = BankingTypes.Iban.Parse("SE7495000099604203849767"),
        Bic = BankingTypes.Bic.Parse("NDEASESS"),
        Sources =
        [
            "https://www.vattenfalleldistribution.se/abonnemang-och-avgifter/faktura-och-betalning/betalningsuppgifter/",
            "https://www.vattenfall.se/foretag/kundservice/fakturaguide/",
            "https://lei.bloomberg.com/leis/view/549300T5RZ1HA5HZ3109",
            "https://www.allabolag.se/5560362138",
        ],
    };

    public static SampleOrganization SamhallAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Samhall AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556448-1397"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556448139701"),
        SniCode = OrgTypes.SwedishSniCode.Parse("81210"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+4620572572"),
        EmailAddress = WebTypes.EmailAddress.Parse("kontakt@samhall.se"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Hammarbybacken 31"),
        PostalAddressBox = "Box 391, 737 26 Fagersta",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("120 30"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://samhall.se/kontakt/",
            "https://samhall.se/kontakt/e-faktura-leverantorer/",
            "https://www.allabolag.se/5564481397",
        ],
    };

    public static SampleOrganization LKAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Luossavaara-Kiirunavaara AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556001-5835"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556001583501"),
        LeiCode = OrgTypes.LeiCode.Parse("549300ONBUTV20237K19"),
        SniCode = OrgTypes.SwedishSniCode.Parse("07100"),
        EmailAddress = WebTypes.EmailAddress.Parse("redovisning@lkab.com"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Varvsgatan 45"),
        PostalAddressBox = "Box 952, 971 28 Luleå",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("972 33"),
        AddressCity = ContactTypes.AddressCity.Parse("Luleå"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Luleå"),
        County = GeoTypes.SwedishCounty.Parse("25"),
        Sources =
        [
            "https://lkab.com/kontakta-oss/",
            "https://lkab.com/kontakta-oss/fakturering/",
            "https://www.lei-lookup.com/record/549300ONBUTV20237K19/",
            "https://www.allabolag.se/5560015835",
        ],
    };

    public static SampleOrganization SverigesTelevisionAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Sveriges Television AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556033-4285"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556033428501"),
        Sources =
        [
            "https://b2b.svt.se/program/fakturering.html",
            "https://www.allabolag.se/5560334285",
        ],
    };

    public static SampleOrganization SverigesRadioAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Sveriges Radio AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556419-3232"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556419323201"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Oxenstiernsgatan 20"),
        PostalAddressBox = "Box 105 10",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("105 10"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.sverigesradio.se/artikel/information-for-kunder-och-leverantorer",
            "https://www.sverigesradio.se/artikel/8912268",
        ],
    };

    public static SampleOrganization TeliaSverigeAB { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Telia Sverige AB"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("556430-0142"),
        EuVatNumber = OrgTypes.EuVatNumber.Parse("SE556430014201"),
        SniCode = OrgTypes.SwedishSniCode.Parse("61100"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Stjärntorget 1"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("169 79"),
        AddressCity = ContactTypes.AddressCity.Parse("Solna"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Solna"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Bankgiro = BankingTypes.SwedishBankgiroNumber.Parse("5117-7913"),
        Sources =
        [
            "https://www.telia.se/foretag/support/faktura",
            "https://www.telia.se/support/faktura-och-betalning/guider/kom-igang-med-autogiro",
            "https://www.allabolag.se/5564300142",
        ],
    };

    // ---------------------------------------------------------------------------
    // Statliga myndigheter
    // ---------------------------------------------------------------------------

    public static SampleOrganization Skatteverket { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Skatteverket"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-5448"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771567567"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Lindhagensgatan 76"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("112 18"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Bankgiro = BankingTypes.SwedishBankgiroNumber.Parse("5050-1055"),
        Sources =
        [
            "https://www.skatteverket.se/kontakt",
            "https://www.allabolag.se/2021005448",
        ],
    };

    public static SampleOrganization Forsakringskassan { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Försäkringskassan"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-5521"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771524524"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Klara Västra Kyrkogata 11"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("103 51"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.forsakringskassan.se/kontakta-oss",
            "https://www.allabolag.se/2021005521",
        ],
    };

    public static SampleOrganization Arbetsformedlingen { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Arbetsförmedlingen"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-2114"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771416416"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Hälsingegatan 38"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("113 99"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://arbetsformedlingen.se/kontakta-oss",
            "https://www.allabolag.se/2021002114",
        ],
    };

    public static SampleOrganization Bolagsverket { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Bolagsverket"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-5000"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771670670"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Stuvarvägen 21"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("851 81"),
        AddressCity = ContactTypes.AddressCity.Parse("Sundsvall"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Sundsvall"),
        County = GeoTypes.SwedishCounty.Parse("22"),
        Sources =
        [
            "https://bolagsverket.se/om-oss/kontakta-oss",
            "https://www.allabolag.se/2021005000",
        ],
    };

    public static SampleOrganization Trafikverket { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Trafikverket"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-6297"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771921921"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Röda vägen 1"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("781 89"),
        AddressCity = ContactTypes.AddressCity.Parse("Borlänge"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Borlänge"),
        County = GeoTypes.SwedishCounty.Parse("20"),
        Sources =
        [
            "https://www.trafikverket.se/kontakta-oss/",
            "https://www.allabolag.se/2021006297",
        ],
    };

    public static SampleOrganization Lantmateriet { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Lantmäteriet"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-4888"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771636363"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Lantmäterigatan 2C"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("801 82"),
        AddressCity = ContactTypes.AddressCity.Parse("Gävle"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Gävle"),
        County = GeoTypes.SwedishCounty.Parse("21"),
        Sources =
        [
            "https://www.lantmateriet.se/sv/om-lantmateriet/kontakta-oss/",
            "https://www.allabolag.se/2021004888",
        ],
    };

    public static SampleOrganization Kronofogden { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Kronofogden"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-2809"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771737300"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Esplanaden 2A"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("172 67"),
        AddressCity = ContactTypes.AddressCity.Parse("Sundbyberg"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Sundbyberg"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.kronofogden.se/om-kronofogden/kontakta-oss",
            "https://www.allabolag.se/2021002809",
        ],
    };

    public static SampleOrganization Transportstyrelsen { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Transportstyrelsen"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-6099"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771503503"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Olai Kyrkogata 35"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("601 73"),
        AddressCity = ContactTypes.AddressCity.Parse("Norrköping"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Norrköping"),
        County = GeoTypes.SwedishCounty.Parse("05"),
        Sources =
        [
            "https://www.transportstyrelsen.se/sv/kontakta-oss/",
            "https://www.allabolag.se/2021006099",
        ],
    };

    public static SampleOrganization Migrationsverket { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Migrationsverket"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-2163"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771235235"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Slottsgatan 82"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("602 22"),
        AddressCity = ContactTypes.AddressCity.Parse("Norrköping"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Norrköping"),
        County = GeoTypes.SwedishCounty.Parse("05"),
        Sources =
        [
            "https://www.migrationsverket.se/Kontakta-oss.html",
            "https://www.allabolag.se/2021002163",
        ],
    };

    public static SampleOrganization Tullverket { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Tullverket"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("202100-0969"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771520520"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("103 13"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.tullverket.se/omoss/kontaktaoss",
            "https://www.allabolag.se/2021000969",
        ],
    };

    // ---------------------------------------------------------------------------
    // Non-AB entities (kommuner, ekonomiska föreningar, etc.)
    // ---------------------------------------------------------------------------

    public static SampleOrganization StockholmsKommun { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Stockholms kommun"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("212000-0142"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46850829000"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Ragnar Östbergs Plan 1"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("105 35"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.stockholm.se/",
            "https://start.stockholm/kontakta-oss/",
            "https://www.allabolag.se/2120000142",
        ],
    };

    public static SampleOrganization SKR { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Sveriges Kommuner och Regioner"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("222000-0315"),
        SniCode = OrgTypes.SwedishSniCode.Parse("94112"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Hornsgatan 20"),
        PostalAddressBox = "Box 17175, 104 62 Stockholm",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("118 82"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://skr.se/omskr/kontaktaskr.8317.html",
        ],
    };

    public static SampleOrganization SvenskaKyrkan { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Trossamfundet Svenska kyrkan"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("252002-6135"),
        SniCode = OrgTypes.SwedishSniCode.Parse("94910"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+4618169500"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Sysslomansgatan 4"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("751 70"),
        AddressCity = ContactTypes.AddressCity.Parse("Uppsala"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Uppsala"),
        County = GeoTypes.SwedishCounty.Parse("03"),
        PropertyDesignation = PropTypes.SwedishPropertyDesignation.Parse("Uppsala Fjärdingen 22:1"),
        Bankgiro = BankingTypes.SwedishBankgiroNumber.Parse("900-1223"),
        Plusgiro = BankingTypes.SwedishPostgiroNumber.Parse("900122-3"),
        Sources =
        [
            "https://svenskakyrkan.se/kyrkokansliet",
            "https://www.insamlingskontroll.se/organisationer/trossamfundet-svenska-kyrkan/",
            "https://bebyggelseregistret.raa.se/bbr2/byggnad/visaHistorik.raa?byggnadId=21400000356388&page=historik",
            "https://www.allabolag.se/foretag/trossamfundet-svenska-kyrkan-med-firma-trossamfundet-svenska-kyrkan/-/f%C3%B6reningar/15OCWLZI63IGG",
        ],
    };

    public static SampleOrganization Folksam { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Folksam ömsesidig sakförsäkring"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("502006-1619"),
        LeiCode = OrgTypes.LeiCode.Parse("5493003384H0SVUD4J19"),
        SniCode = OrgTypes.SwedishSniCode.Parse("65120"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46771950950"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Bohusgatan 14"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("106 60"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.folksam.se/om-oss/om-folksam/det-har-ar-vi/allman-information-om-folksam",
            "https://www.folksam.se/om-oss/om-folksam/det-har-ar-vi/kontakta-oss",
            "https://lei.bloomberg.com/leis/view/5493003384H0SVUD4J19",
            "https://www.allabolag.se/5020061619",
        ],
    };

    public static SampleOrganization Lantmannen { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Lantmännen ek för"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("769605-2856"),
        SniCode = OrgTypes.SwedishSniCode.Parse("46210"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Sankt Göransgatan 160 A"),
        AddressZipCode = ContactTypes.AddressZipCode.Parse("112 17"),
        AddressCity = ContactTypes.AddressCity.Parse("Stockholm"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Stockholm"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://www.lantmannen.com/about-lantmannen/contact/lantmannen-supplier-portal/contact-and-support/",
            "https://www.allabolag.se/7696052856/verksamhet",
        ],
    };

    public static SampleOrganization KooperativaForbundet { get; } = new()
    {
        Name = OrgTypes.SwedishOrganizationName.Parse("Kooperativa Förbundet ekonomisk förening"),
        OrganizationNumber = OrgTypes.SwedishOrganizationNumber.Parse("702001-1693"),
        SniCode = OrgTypes.SwedishSniCode.Parse("70100"),
        PhoneNumber = ContactTypes.PhoneNumber.Parse("+46107400000"),
        AddressStreet = ContactTypes.AddressStreet.Parse("Englundavägen 4"),
        PostalAddressBox = "Box 171 88 Solna",
        AddressZipCode = ContactTypes.AddressZipCode.Parse("171 41"),
        AddressCity = ContactTypes.AddressCity.Parse("Solna"),
        Country = GeoTypes.Country.Parse("SE"),
        Municipality = GeoTypes.SwedishMunicipality.Parse("Solna"),
        County = GeoTypes.SwedishCounty.Parse("01"),
        Sources =
        [
            "https://kf.se/kontakt/",
            "https://www.allabolag.se/7020011693",
        ],
    };

    /// <summary>
    /// All sample organizations in the catalog.
    /// </summary>
    public static IReadOnlyList<SampleOrganization> All { get; } =
    [
        BudiAB,
        SystembolagetAB,
        SJAB,
        PostNordSverigeAB,
        VattenfallAB,
        SamhallAB,
        LKAB,
        SverigesTelevisionAB,
        SverigesRadioAB,
        TeliaSverigeAB,
        Skatteverket,
        Forsakringskassan,
        Arbetsformedlingen,
        Bolagsverket,
        Trafikverket,
        Lantmateriet,
        Kronofogden,
        Transportstyrelsen,
        Migrationsverket,
        Tullverket,
        StockholmsKommun,
        SKR,
        SvenskaKyrkan,
        Folksam,
        Lantmannen,
        KooperativaForbundet,
    ];
}
