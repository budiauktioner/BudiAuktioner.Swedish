using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;
using Buildi.Primitives.Product;
using Buildi.Primitives.Property;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class TextScanningBenchmarks
{
    private const string ShortText = "Kontakta info@example.com eller ring 070-174 06 33.";

    private const string MediumText =
        "Acme Sweden AB (559246-0421) är ett registrerat aktiebolag. " +
        "Momsnr: SE559246042101. Bankgiro: 5805-6201. " +
        "Kontakt: info@example.com eller ring 070-174 06 33. " +
        "Besök oss på Storgatan 1, 114 53 Stockholm.";

    private const string LongText =
        "Acme Sweden AB (559246-0421) är ett registrerat aktiebolag. " +
        "Momsnr: SE559246042101. Bankgiro: 5805-6201. " +
        "Kontakt: info@example.com eller ring 070-174 06 33. " +
        "IBAN: SE4550000000058398257466. BIC: NDEASESS. " +
        "Besök oss på Storgatan 1, 114 53 Stockholm. " +
        "Bilen ABC 123 parkerad utanför. VIN: WBA3A5C55CF256789. " +
        "LEI: 5493001KJTIIGC8Y1R12. DUNS: 362498394. " +
        "Fastighet: Stockholm Söder 75:2. EAN: 5901234123457. " +
        "Personnummer: 990807-2391. Samordningsnummer: 680164-2395. " +
        "Det här är extra text utan strukturerad data som gör strängen längre " +
        "och testar prestanda med en realistisk text som inte bara består av identifierare. " +
        "Ytterligare en rad med vanlig löptext för att simulera en verklig beskrivning. " +
        "Avslutningsvis vill vi nämna att alla uppgifter ovan är fiktiva.";

    private const string NoMatchText =
        "Det här är en helt vanlig svensk text utan några strukturerade identifierare. " +
        "Den innehåller bara meningar och ord, inga personnummer, e-postadresser " +
        "eller organisationsnummer. Avsikten är att mäta baseline-prestanda.";

    private readonly TextScanner _scanner = new();
    private TextScanResult? _mediumResult;

    [GlobalSetup]
    public void Setup()
    {
        _mediumResult = _scanner.Scan(MediumText);
    }

    // --- Per-type FindCandidatesInText ---

    [Benchmark]
    public int Email_FindCandidates() =>
        EmailAddress.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int PIN_FindCandidates() =>
        SwedishPersonalIdentityNumber.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int OrgNumber_FindCandidates() =>
        SwedishOrganizationNumber.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int Iban_FindCandidates() =>
        Iban.FindCandidatesInText(LongText).Count;

    [Benchmark]
    public int EuVatNumber_FindCandidates() =>
        EuVatNumber.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int Bankgiro_FindCandidates() =>
        SwedishBankgiroNumber.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int Phone_FindCandidates() =>
        PhoneNumber.FindCandidatesInText(MediumText).Count;

    [Benchmark]
    public int VehicleReg_FindCandidates() =>
        SwedishVehicleRegistrationNumber.FindCandidatesInText(LongText).Count;

    [Benchmark]
    public int Vin_FindCandidates() =>
        VehicleIdentificationNumber.FindCandidatesInText(LongText).Count;

    [Benchmark]
    public int Gtin13_FindCandidates() =>
        Gtin13.FindCandidatesInText(LongText).Count;

    [Benchmark]
    public int Address_FindCandidates() =>
        Address.FindCandidatesInText(LongText).Count;

    // --- TextScanner aggregate ---

    [Benchmark]
    public int Scanner_ShortText() =>
        _scanner.Scan(ShortText).TotalCount;

    [Benchmark]
    public int Scanner_MediumText() =>
        _scanner.Scan(MediumText).TotalCount;

    [Benchmark]
    public int Scanner_LongText() =>
        _scanner.Scan(LongText).TotalCount;

    [Benchmark]
    public int Scanner_NoMatches() =>
        _scanner.Scan(NoMatchText).TotalCount;

    // --- Filtering ---

    [Benchmark]
    public int Scanner_FilterByCategory()
    {
        var options = new TextScannerOptions
        {
            IncludeCategories = new HashSet<TextCandidateCategory> { TextCandidateCategory.Contact }
        };
        return _scanner.Scan(MediumText, options).TotalCount;
    }

    [Benchmark]
    public int Scanner_FilterByConfidence()
    {
        var options = new TextScannerOptions
        {
            MinimumConfidence = TextMatchConfidence.High
        };
        return _scanner.Scan(MediumText, options).TotalCount;
    }

    // --- MaskAll / RedactAll / ReplaceAll ---

    [Benchmark]
    public string MaskAll_MediumText() =>
        _mediumResult!.MaskAll(MediumText);

    [Benchmark]
    public string RedactAll_MediumText() =>
        _mediumResult!.RedactAll(MediumText);

    [Benchmark]
    public string ReplaceAll_MediumText() =>
        _mediumResult!.ReplaceAll(MediumText, c => $"[{c.TypeName}]");

    // --- Overlap resolution (pre-computed in constructor, measured via full scan) ---

    [Benchmark]
    public int OverlapResolution_EmailContainingOrgNumber()
    {
        var text = "kontakt: 5592460421@example.com plus 559246-0421";
        return _scanner.Scan(text).ResolvedCandidates.Count;
    }
}
