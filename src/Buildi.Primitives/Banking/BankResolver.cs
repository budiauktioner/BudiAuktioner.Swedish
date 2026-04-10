namespace Buildi.Primitives.Banking;

internal static class BankResolver
{
    internal static (SwedishBank?, string?) Resolve(int clearing4)
    {
        if (IsNordeaPersonkonto(clearing4)) return (SwedishBank.NordeaPersonkonto, "Nordea Personkonto");
        if (IsSverigesRiksbank(clearing4)) return (SwedishBank.SverigesRiksbank, "Sveriges riksbank");
        if (IsAvanzaBank(clearing4)) return (SwedishBank.AvanzaBank, "Avanza Bank");
        if (IsAionBank(clearing4)) return (SwedishBank.AionBank, "AION Bank");
        if (IsBnpParibas(clearing4)) return (SwedishBank.BnpParibas, "BNP Paribas S.A. Bankfilial Sverige");
        if (IsCitibankEurope(clearing4)) return (SwedishBank.CitibankEurope, "Citibank Europe plc");
        if (IsHSBBank(clearing4)) return (SwedishBank.HSBBank, "HSB Bank");
        if (IsCalyonBank(clearing4)) return (SwedishBank.CalyonBank, "Calyon Bank");
        if (IsRoyalBankOfScotland(clearing4)) return (SwedishBank.RoyalBankOfScotland, "Royal Bank of Scotland");
        if (IsDanskeBankSweden(clearing4)) return (SwedishBank.DanskeBank, "Danske Bank");
        if (IsDNBSweden(clearing4)) return (SwedishBank.DNBSverige, "DNB Sverige");
        if (IsEkobanken(clearing4)) return (SwedishBank.Ekobanken, "Ekobanken");
        if (IsEnityBankGroup(clearing4)) return (SwedishBank.EnityBankGroup, "Enity Bank Group");
        if (IsEpBank(clearing4)) return (SwedishBank.EpBank, "EP Bank");
        if (IsHandelsbanken(clearing4)) return (SwedishBank.Handelsbanken, "Handelsbanken");
        if (IsIcaBanken(clearing4)) return (SwedishBank.IcaBanken, "ICA Banken");
        if (IsIkanoBank(clearing4)) return (SwedishBank.IkanoBank, "Ikano Bank");
        if (IsJAKMedlemsbank(clearing4)) return (SwedishBank.JAKMedlemsbank, "JAK Medlemsbank");
        if (IsKlarnaBank(clearing4)) return (SwedishBank.KlarnaBank, "Klarna Bank");
        if (IsLandshypotekBank(clearing4)) return (SwedishBank.LandshypotekBank, "Landshypotek Bank");
        if (IsLunarBank(clearing4)) return (SwedishBank.LunarBank, "Lunar Bank");
        if (IsLansforsakringarBank(clearing4)) return (SwedishBank.Lansforsakringar, "Länsförsäkringar Bank");
        if (IsLanOchSparBank(clearing4)) return (SwedishBank.LanOchSparBank, "Lån & Spar Bank");
        if (IsMarginalenBank(clearing4)) return (SwedishBank.MarginalenBank, "Marginalen Bank");
        if (IsMultitudeBank(clearing4)) return (SwedishBank.MultitudeBank, "Multitude Bank");
        if (IsNobaBankGroup(clearing4)) return (SwedishBank.NobaBankGroup, "Noba Bank Group");
        if (IsNordeaPlusgirot(clearing4)) return (SwedishBank.NordeaPlusgirot, "Nordea (Plusgirot)");
        if (IsNordeaOrdinary(clearing4)) return (SwedishBank.Nordea, "Nordea");
        if (IsNordnetBank(clearing4)) return (SwedishBank.NordnetBank, "Nordnet Bank");
        if (IsNorthmillBank(clearing4)) return (SwedishBank.NorthmillBank, "Northmill Bank");
        if (IsResursBank(clearing4)) return (SwedishBank.ResursBank, "Resurs Bank");
        if (IsSantander(clearing4)) return (SwedishBank.Santander, "Santander");
        if (IsSBABBank(clearing4)) return (SwedishBank.SBABBank, "SBAB Bank");
        if (IsSEB(clearing4)) return (SwedishBank.SEB, "SEB");
        if (IsSkandiabanken(clearing4)) return (SwedishBank.Skandiabanken, "Skandiabanken");
        if (IsSparbankenSyd(clearing4)) return (SwedishBank.SparbankenSyd, "Sparbanken Syd");
        if (IsSwedbank7DigitsAccountNumber(clearing4) || IsSwedbank10DigitsAccountNumber(clearing4)) return (SwedishBank.Swedbank, "Swedbank");
        if (IsSveaBank(clearing4)) return (SwedishBank.SveaBank, "Svea Bank");
        if (IsAlandsbanken(clearing4)) return (SwedishBank.Alandsbanken, "Ålandsbanken Abp (Finland) svensk filial");
        if (IsVolvofinansBank(clearing4)) return (SwedishBank.VolvofinansBank, "Volvofinans Bank");
        if (IsStadshypotekBank(clearing4)) return (SwedishBank.StadshypotekBank, "Stadshypotek Bank");
        if (IsGjensidigeNorSparebank(clearing4)) return (SwedishBank.GjensidigeNorSparebank, "Gjensidige NOR Sparebank");
        if (IsParetoOhman(clearing4)) return (SwedishBank.ParetoOhman, "Pareto Öhman");
        if (IsForex(clearing4)) return (SwedishBank.Forex, "Forex");
        if (IsParexBank(clearing4)) return (SwedishBank.ParexBank, "Parex Bank");
        if (IsBriteAb(clearing4)) return (SwedishBank.BriteAb, "Brite AB");
        if (IsBankingCircle(clearing4)) return (SwedishBank.BankingCircle, "Banking Circle");
        if (IsBankOfChinaLuxembourg(clearing4)) return (SwedishBank.BankOfChinaLuxembourg, "Bank of China (Luxembourg)");
        if (IsMedMeraBank(clearing4)) return (SwedishBank.MedMeraBank, "MedMera Bank");
        if (IsFolkia(clearing4)) return (SwedishBank.Folkia, "Folkia");
        if (IsNetfondsBank(clearing4)) return (SwedishBank.NetfondsBank, "Netfonds Bank");
        if (IsNasdaqOmx(clearing4)) return (SwedishBank.NasdaqOmx, "Nasdaq-OMX");
        if (IsRiksgalden(clearing4)) return (SwedishBank.Riksgalden, "Riksgälden");
        if (IsPrivatgirot(clearing4)) return (SwedishBank.Privatgirot, "Privatgirot");
        if (IsIntergiro(clearing4)) return (SwedishBank.Intergiro, "Intergiro");
        if (IsNykredit(clearing4)) return (SwedishBank.Nykredit, "Nykredit");
        if (IsTellerBranchNorway(clearing4)) return (SwedishBank.TellerBranchNorway, "Teller Branch Norway");
        if (IsBankernasAutomatbolag(clearing4)) return (SwedishBank.BankernasAutomatbolag, "Bankernas Automatbolag");
        if (IsTellerBranchSweden(clearing4)) return (SwedishBank.TellerBranchSweden, "Teller Branch Sweden");
        if (IsKortacceptNordic(clearing4)) return (SwedishBank.KortacceptNordic, "Kortaccept Nordic");
        if (IsKommuninvest(clearing4)) return (SwedishBank.Kommuninvest, "Kommuninvest");
        if (IsVpSecurities(clearing4)) return (SwedishBank.VpSecurities, "VP Securities");

        return (null, null);
    }

    internal static bool IsNordeaPersonkonto(int clearing4) => clearing4 == 3300 || clearing4 == 3782;

    internal static bool IsSEB(int clearing4) =>
        InRange(clearing4, 5000, 5999) ||
        InRange(clearing4, 9120, 9124) ||
        InRange(clearing4, 9130, 9149);

    internal static bool IsHandelsbanken(int clearing4) =>
        InRange(clearing4, 6000, 6999);

    internal static bool IsNordeaOrdinary(int clearing4) =>
        InRange(clearing4, 1100, 1199) ||
        InRange(clearing4, 1400, 2099) ||
        (InRange(clearing4, 3000, 3399) && clearing4 != 3300) ||
        (InRange(clearing4, 3410, 3999) && clearing4 != 3782) ||
        InRange(clearing4, 4000, 4999);

    internal static bool IsDanskeBankSweden(int clearing4) =>
        InRange(clearing4, 1200, 1399) ||
        InRange(clearing4, 2400, 2499) ||
        InRange(clearing4, 9180, 9189);

    internal static bool IsLansforsakringarBank(int clearing4) =>
        InRange(clearing4, 3400, 3409) ||
        InRange(clearing4, 9020, 9029) ||
        InRange(clearing4, 9060, 9069);

    internal static bool IsIcaBanken(int clearing4) => InRange(clearing4, 9270, 9279);
    internal static bool IsSkandiabanken(int clearing4) => InRange(clearing4, 9150, 9169);
    internal static bool IsSBABBank(int clearing4) => InRange(clearing4, 9250, 9259);
    internal static bool IsSverigesRiksbank(int clearing4) => InRange(clearing4, 1000, 1099);
    internal static bool IsAvanzaBank(int clearing4) => InRange(clearing4, 9550, 9569);
    internal static bool IsAionBank(int clearing4) => InRange(clearing4, 9580, 9589);
    internal static bool IsBnpParibas(int clearing4) => InRange(clearing4, 9470, 9479);
    internal static bool IsCitibankEurope(int clearing4) => InRange(clearing4, 9040, 9049);
    internal static bool IsHSBBank(int clearing4) => InRange(clearing4, 9050, 9059);
    internal static bool IsCalyonBank(int clearing4) => InRange(clearing4, 9080, 9089);
    internal static bool IsRoyalBankOfScotland(int clearing4) => InRange(clearing4, 9090, 9099);
    internal static bool IsDNBSweden(int clearing4) => InRange(clearing4, 9190, 9199);
    internal static bool IsEkobanken(int clearing4) => InRange(clearing4, 9700, 9709);
    internal static bool IsEnityBankGroup(int clearing4) => InRange(clearing4, 9680, 9689);
    internal static bool IsEpBank(int clearing4) => InRange(clearing4, 9590, 9599);
    internal static bool IsIkanoBank(int clearing4) => InRange(clearing4, 9170, 9179);
    internal static bool IsJAKMedlemsbank(int clearing4) => InRange(clearing4, 9670, 9679);
    internal static bool IsKlarnaBank(int clearing4) => InRange(clearing4, 9780, 9789);
    internal static bool IsLandshypotekBank(int clearing4) => InRange(clearing4, 9390, 9399);
    internal static bool IsLunarBank(int clearing4) => InRange(clearing4, 9710, 9719);
    internal static bool IsLanOchSparBank(int clearing4) => InRange(clearing4, 9630, 9639);
    internal static bool IsMarginalenBank(int clearing4) => InRange(clearing4, 9230, 9239);
    internal static bool IsMultitudeBank(int clearing4) => InRange(clearing4, 9070, 9079);
    internal static bool IsNobaBankGroup(int clearing4) => InRange(clearing4, 9640, 9649);

    internal static bool IsNordeaPlusgirot(int clearing4) =>
        InRange(clearing4, 9500, 9549) || InRange(clearing4, 9960, 9969);

    internal static bool IsNordnetBank(int clearing4) => InRange(clearing4, 9100, 9109);
    internal static bool IsNorthmillBank(int clearing4) => InRange(clearing4, 9750, 9759);
    internal static bool IsResursBank(int clearing4) => InRange(clearing4, 9280, 9289);
    internal static bool IsSantander(int clearing4) => InRange(clearing4, 9460, 9469);
    internal static bool IsSparbankenSyd(int clearing4) => InRange(clearing4, 9570, 9579);
    internal static bool IsSwedbank7DigitsAccountNumber(int clearing4) => InRange(clearing4, 7000, 7999);

    internal static bool IsSwedbank10DigitsAccountNumber(int clearing4) =>
        InRange(clearing4, 8000, 8999) || InRange(clearing4, 9300, 9349);

    internal static bool IsSveaBank(int clearing4) => InRange(clearing4, 9660, 9669);
    internal static bool IsAlandsbanken(int clearing4) => InRange(clearing4, 2300, 2399);
    internal static bool IsVolvofinansBank(int clearing4) => InRange(clearing4, 9610, 9619);
    internal static bool IsStadshypotekBank(int clearing4) => InRange(clearing4, 9200, 9209);
    internal static bool IsGjensidigeNorSparebank(int clearing4) => InRange(clearing4, 9260, 9269);
    internal static bool IsParetoOhman(int clearing4) => InRange(clearing4, 9380, 9389);
    internal static bool IsForex(int clearing4) => InRange(clearing4, 9400, 9449);
    internal static bool IsParexBank(int clearing4) => InRange(clearing4, 9480, 9489);
    internal static bool IsBriteAb(int clearing4) => InRange(clearing4, 9490, 9499);
    internal static bool IsBankingCircle(int clearing4) => InRange(clearing4, 9600, 9609);
    internal static bool IsBankOfChinaLuxembourg(int clearing4) => InRange(clearing4, 9620, 9629);
    internal static bool IsMedMeraBank(int clearing4) => InRange(clearing4, 9650, 9659);
    internal static bool IsFolkia(int clearing4) => InRange(clearing4, 9690, 9699);
    internal static bool IsNetfondsBank(int clearing4) => InRange(clearing4, 9720, 9729);
    internal static bool IsNasdaqOmx(int clearing4) => InRange(clearing4, 9870, 9879);
    internal static bool IsRiksgalden(int clearing4) => InRange(clearing4, 9880, 9889);
    internal static bool IsPrivatgirot(int clearing4) => InRange(clearing4, 9860, 9869);
    internal static bool IsIntergiro(int clearing4) => InRange(clearing4, 9770, 9779);
    internal static bool IsNykredit(int clearing4) => clearing4 == 9950;
    internal static bool IsTellerBranchNorway(int clearing4) => clearing4 == 9951;
    internal static bool IsBankernasAutomatbolag(int clearing4) => clearing4 == 9952;
    internal static bool IsTellerBranchSweden(int clearing4) => clearing4 == 9953;
    internal static bool IsKortacceptNordic(int clearing4) => clearing4 == 9954;
    internal static bool IsKommuninvest(int clearing4) => clearing4 == 9955;
    internal static bool IsVpSecurities(int clearing4) => clearing4 == 9956;

    internal static bool InRange(int value, int startInclusive, int endInclusive) =>
        value >= startInclusive && value <= endInclusive;
}
