# Test and Sample Data Strategy

This document describes what test and sample data is allowed in the `Buildi.Primitives` package, how it should be sourced, and what must remain synthetic to avoid privacy and GDPR issues.

All validation in this library is performed in-memory using rules, patterns, checksums, and reference data available at build time. "Valid" means the value matches the implemented format and rule set — it does not mean the organization, person, bank account, or address has been verified against an authoritative external source.

## Allowed data per type

### Organization

Real public organization names, organization numbers, VAT numbers, D-U-N-S numbers, SNI codes, CFAR numbers, and public business addresses are allowed when sourced from official or clearly public pages.

Organization data for well-known Swedish entities is generally public record and available through Bolagsverket. Use official company websites, annual reports, or Bolagsverket filings as the primary source.

### Banking

Public payment identifiers (Bankgiro, Plusgiro) are allowed when explicitly published by the organization on their own website, for example on invoice/billing pages or supplier information pages.

Domestic clearing/account numbers and IBAN/BIC are allowed only when explicitly and intentionally published by the organization itself, for example on official payment instruction pages. Do not use bank account numbers found only in secondary directories.

OCR reference numbers should be synthetic (constructed to pass MOD-10 validation) rather than taken from real invoices.

### Contact

Real business addresses (street, zip code, city) are allowed for public organizations when sourced from their official websites or public filings.

Personal phone numbers and personal email addresses must never be used. For phone-specific tests, prefer PTS-reserved test numbers (see [PTS — Telefonnummer till böcker och filmer](https://pts.se/internet-och-telefoni/telefonnummer-och-adressering/telefonnummer-till-bocker-och-filmer/)). The `PhoneNumberTestData` class in the package enumerates all 495 reserved numbers.

Email addresses in tests should use synthetic domains (e.g. `user@example.com`) per [RFC 2606](https://datatracker.ietf.org/doc/html/rfc2606), or public organizational contact addresses explicitly published on official websites.

### Identity

Do not use real Swedish personal identity numbers (personnummer) or coordination numbers (samordningsnummer). These are personal data under GDPR.

Skatteverket publishes official test personnummer (~40,000 numbers) and test samordningsnummer (~2,000 numbers) as open data. These numbers are formally correct (valid checksum, valid date structure) but are permanently blocked and will never be assigned to a real person. Use these official test numbers instead of inventing synthetic values.

- [Skatteverket — Testpersonnummer som öppen data](https://skatteverket.se/omoss/digitalasamarbeten/omvaraoppnadata/testpersonnummersomoppendata.4.5b35a6251761e6914202df9.html)
- [Skatteverket — Testpersonnummer (Utvecklarportalen)](https://www7.skatteverket.se/portal/apier-och-oppna-data/utvecklarportalen/oppetdata/Test%C2%AD%C2%ADpersonnummer)
- [Skatteverket — Testsamordningsnummer (Utvecklarportalen)](https://www7.skatteverket.se/portal/apier-och-oppna-data/utvecklarportalen/oppetdata/Test%C2%AD%C2%ADsamordningsnummer)
- [Swedish.IdentityInfo.net — Testdata](https://swedish.identityinfo.net/personalidentitynumber/testdata) — enhanced browsable version maintained by the [ActiveLogin](https://github.com/ActiveLogin/ActiveLogin.Identity) project

Skatteverket refreshes the open data files annually (in December) with new numbers for the upcoming year. The numbers must only be used in test environments — never in production systems. Sending test personnummer to Skatteverket's production environment is classified as a personal data incident.

### Geography

County codes and municipality codes are official open reference data from SCB and can be used freely. Use the official code and name values from [SCB — Counties and municipalities](https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/).

### Property

Swedish property designations (fastighetsbeteckning) should use synthetic or well-known public examples. Avoid designations that could identify a specific private property owner. Municipality and tract names are public, but specific register numbers tied to identifiable private owners should be avoided unless the property is clearly public (e.g. government buildings).

### Country

Country data is based on ISO 3166-1 and other international standards. All country names, codes, calling codes, and classification flags are public reference data and can be used freely.

## Source hierarchy

When collecting sample data, prefer sources in this order:

1. **Official organization websites** — contact pages, supplier/invoice pages, annual reports, terms, legal pages.
2. **Official standards and registries** — PTS, ISO, SCB, Lantmäteriet, Bankgirot, Skatteverket, Bolagsverket.
3. **Public business directories** (allabolag.se, hitta.se) — acceptable as corroborating fallback only, not as the sole source when the organization publishes the same data itself.

### Source transparency

Each sample organization entry in the test catalog carries source links. The sources justify which fields are included:

- **Legal identity** (org number, VAT, DUNS): official company websites, Bolagsverket, allabolag.se.
- **Postal address**: official contact/about pages.
- **Payment identifiers** (Bankgiro, Plusgiro, IBAN, BIC): official invoice/billing/supplier pages published by the organization.
- If an official source is missing and a weaker public source is temporarily used, the field should be marked as provisional or excluded.

## Sample organization catalog

The test project includes a reusable catalog of public organizations in `test/Buildi.Primitives.Tests/TestData/`. The catalog is seeded with the organizations listed below.

The catalog deliberately includes a mix of organization forms:
- Aktiebolag (AB) and publikt aktiebolag (publ)
- Municipalities and public-sector bodies
- Membership organizations and employer organizations
- Religious organizations
- Mutual insurance companies
- Cooperative entities (ekonomisk förening)

### Included organizations and sources

The organizations below were chosen because they are large, well-known Swedish household names whose public registration data is easy to verify and widely recognizable as test data. Their inclusion does not imply any connection to or endorsement by Budi.

#### Budi AB

| Field | Value |
|-------|-------|
| Organization number | 559246-0421 |
| VAT number | SE559246042101 |
| D-U-N-S number | 350827673 |
| Address | Linta Gårdsväg 5A, 168 74 Bromma |
| Bankgiro | 235-9321 |

Sources:
- Provided directly by the organization (package author)

#### Systembolaget AB

| Field | Value |
|-------|-------|
| Organization number | 556059-9473 |
| Address | Kungsträdgårdsgatan 14, 103 84 Stockholm |

Sources:
- [Systembolaget — Kontakta oss](https://www.systembolaget.se/kontakta-oss/)
- [allabolag.se](https://www.allabolag.se/5560599473/systembolaget-aktiebolag) (corroborating)

#### SJ AB

| Field | Value |
|-------|-------|
| Organization number | 556196-1599 |
| Address | Vasagatan 10, 111 20 Stockholm |

Sources:
- [SJ — Kontakt](https://www.sj.se/kundservice/kontakt)
- [allabolag.se](https://www.allabolag.se/5561961599) (corroborating)

#### PostNord Sverige AB

| Field | Value |
|-------|-------|
| Organization number | 556711-5695 |
| Address | Terminalvägen 24, 171 73 Solna |

Sources:
- [PostNord — Fakturering](https://www.postnord.se/foretag/forberedelser/fakturering)
- [allabolag.se](https://www.allabolag.se/5567115695) (corroborating)

#### Vattenfall AB

| Field | Value |
|-------|-------|
| Organization number | 556036-2138 |
| Address | Evenemangsgatan 13, 169 79 Solna |

Payment identifiers below are published by Vattenfall Eldistribution for the billing entity Vattenfall Kundservice AB:

| Field | Value |
|-------|-------|
| Bankgiro | 5110-8348 |
| Plusgiro | 4131300-8 |
| IBAN | SE7495000099604203849767 |
| BIC | NDEASESS |

Sources:
- [Vattenfall Eldistribution — Betalningsuppgifter](https://www.vattenfalleldistribution.se/abonnemang-och-avgifter/faktura-och-betalning/betalningsuppgifter/)
- [Vattenfall — Fakturaguide](https://www.vattenfall.se/foretag/kundservice/fakturaguide/)
- [allabolag.se](https://www.allabolag.se/5560362138) (corroborating)

#### Samhall AB

| Field | Value |
|-------|-------|
| Organization number | 556448-1397 |
| Address | Hammarbybacken 31, 120 30 Stockholm |

Sources:
- [Samhall — Kontakt](https://samhall.se/kontakt/)
- [allabolag.se](https://www.allabolag.se/5564481397) (corroborating)

#### Luossavaara-Kiirunavaara AB (LKAB)

| Field | Value |
|-------|-------|
| Organization number | 556001-5835 |
| Address | Varvsgatan 45, 972 33 Luleå |

Sources:
- [LKAB — Kontakta oss](https://lkab.com/kontakta-oss/)
- [allabolag.se](https://www.allabolag.se/5560015835) (corroborating)

#### Sveriges Television AB

| Field | Value |
|-------|-------|
| Organization number | 556033-4285 |

Sources:
- [SVT B2B — Fakturering](https://b2b.svt.se/program/fakturering.html)
- [allabolag.se](https://www.allabolag.se/5560334285) (corroborating)

#### Sveriges Radio AB

| Field | Value |
|-------|-------|
| Organization number | 556419-3232 |
| Address | Oxenstiernsgatan 20, 105 10 Stockholm |

Sources:
- [Sveriges Radio — Information för kunder och leverantörer](https://www.sverigesradio.se/artikel/information-for-kunder-och-leverantorer)
- [Sveriges Radio — Kontakt](https://www.sverigesradio.se/artikel/8912268)

#### Stockholms kommun

| Field | Value |
|-------|-------|
| Organization number | 212000-0142 |
| Address | Ragnar Östbergs Plan 1, 105 35 Stockholm |

Sources:
- [Stockholm stad](https://www.stockholm.se/)
- [allabolag.se](https://www.allabolag.se/2120000142) (corroborating)

#### Sveriges Kommuner och Regioner (SKR)

| Field | Value |
|-------|-------|
| Organization number | 222000-0315 |
| Address | Hornsgatan 20, 118 82 Stockholm |

Sources:
- [SKR — Kontakta SKR](https://skr.se/omskr/kontaktaskr.8317.html)

#### Trossamfundet Svenska kyrkan

| Field | Value |
|-------|-------|
| Organization number | 252002-6135 |
| Address | Sysslomansgatan 4, 751 70 Uppsala |

Sources:
- [Svenska kyrkan — Kyrkokansliet](https://svenskakyrkan.se/kyrkokansliet)
- [allabolag.se](https://www.allabolag.se/foretag/trossamfundet-svenska-kyrkan-med-firma-trossamfundet-svenska-kyrkan/-/f%C3%B6reningar/15OCWLZI63IGG) (corroborating)

#### Folksam ömsesidig sakförsäkring

| Field | Value |
|-------|-------|
| Organization number | 502006-1619 |
| Address | Bohusgatan 14, 106 60 Stockholm |

Sources:
- [Folksam — Allmän information](https://www.folksam.se/om-oss/om-folksam/det-har-ar-vi/allman-information-om-folksam)
- [Folksam — Kontakta oss](https://www.folksam.se/om-oss/om-folksam/det-har-ar-vi/kontakta-oss)

#### Lantmännen ek för

| Field | Value |
|-------|-------|
| Organization number | 769605-2856 |
| Address | Sankt Göransgatan 160 A, 112 17 Stockholm |

Sources:
- [Lantmännen — Contact and Support](https://www.lantmannen.com/about-lantmannen/contact/lantmannen-supplier-portal/contact-and-support/)
- [allabolag.se](https://www.allabolag.se/7696052856/verksamhet) (corroborating)

#### Kooperativa Förbundet ekonomisk förening

| Field | Value |
|-------|-------|
| Organization number | 702001-1693 |
| Address | Englundavägen 4, 171 41 Solna |

Sources:
- [KF — Kontakt](https://kf.se/kontakt/)
- [allabolag.se](https://www.allabolag.se/7020011693) (corroborating)
