# Buildi.Primitives

[![Build](https://github.com/budiauktioner/Buildi.Primitives/actions/workflows/build.yml/badge.svg)](https://github.com/budiauktioner/Buildi.Primitives/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Buildi.Primitives)](https://www.nuget.org/packages/Buildi.Primitives)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Buildi.Primitives)](https://www.nuget.org/packages/Buildi.Primitives)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Docs](https://img.shields.io/badge/docs-budiauktioner.github.io-blue)](https://budiauktioner.github.io/Buildi.Primitives/)
[![Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://budiauktioner.github.io/Buildi.Primitives/demo/)

> **Early preview** - this package is under active development. APIs may change between releases. Feedback, suggestions, and bug reports are very welcome!

A class library of domain primitives for data validation and normalization in .NET, designed for use from a Swedish context - though many primitives (VAT numbers, IBAN, phone numbers, countries, currencies, and more) are relevant globally. Originally spun out of a set of validation classes built at [Budi Auktioner](https://www.budi.se/), the library has since grown well beyond that origin and is built for anyone, anywhere, who needs reliable Swedish-context validation in .NET.

Reference-data provenance and redistribution notes are documented in [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

## Key features

- 📦 **100+ value-object types** covering organization numbers, VAT, banking, addresses, vehicles, measurements, and more
- 🔁 **Consistent API** — every type exposes `TryParse`, `Parse`, `IsValid`, `Format`, `Normalize`, and `IsNormalized`
- ✅ **Structured validation** — many types provide a `Validate()` method with machine-readable reason codes and descriptions in English and Swedish
- 🔍 **Text scanning** — heuristic extraction of structured data from unstructured text with confidence scoring and overlap resolution
- 🔒 **PII masking** — `ToMaskedString()` extension methods for safe display of sensitive data
- 📏 **Natural unit display** — `ToNaturalString()` auto-selects the most human-readable unit for measurement values (e.g. bytes → GB, grams → kg)
- 🏷️ **Type metadata** — every type exposes a static `TypeInfo` property with English name, Swedish name, emoji, and source URLs
- 🧠 **In-memory only** — no external API calls; all validation uses rules, patterns, checksums, and reference data at build time

```csharp
// The core API stays consistent across very different domains.
using Buildi.Primitives.Organization;
using Buildi.Primitives.Product;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;
using Buildi.Primitives.Geography;

// 1) Structured identifier type
if (SwedishOrganizationNumber.TryParse("5592460421", out var orgNumber))
{
    Console.WriteLine(orgNumber.Value);                                  // "165592460421"
    Console.WriteLine(orgNumber.To10DigitString());                      // "559246-0421"
    Console.WriteLine(orgNumber.ToMaskedString(maskOrganizationNumbers: true)); // "559246-****"
}

var parsedOrgNumber = SwedishOrganizationNumber.Parse("559246-0421");
Console.WriteLine(parsedOrgNumber.IsPerson);                             // false

SwedishOrganizationNumber.IsValid("5592460421");                        // true
SwedishOrganizationNumber.Format("5592460421");                         // "559246-0421"
SwedishOrganizationNumber.Normalize("559246-0421");                     // "165592460421"
SwedishOrganizationNumber.IsNormalized("165592460421");                 // true

var orgValidation = SwedishOrganizationNumber.Validate("5592460420");
Console.WriteLine(orgValidation.IsValid);                               // false
Console.WriteLine(orgValidation.Issues[0].Reason);                      // InvalidCheckDigit

SwedishOrganizationNumber.TypeInfo.EnglishName;                         // "Organization Number"
SwedishOrganizationNumber.TypeInfo.LocalizedName;                       // "Organisationsnummer"

// 2) Measurement-backed product type
if (StorageCapacity.TryParse("549755813888 B", out var storage))
{
    Console.WriteLine(storage.Bytes);                                   // 549755813888
    Console.WriteLine(storage.Gigabytes);                               // 549.755813888
    Console.WriteLine(storage.ToNaturalString());                       // "512 GB"
}

var parsedStorage = StorageCapacity.Parse("2 TB");
Console.WriteLine(parsedStorage.ToNormalizedString());                  // "2000000000000 B"

StorageCapacity.IsValid("512");                                         // true
StorageCapacity.Format("2000");                                         // "2000 GB"
StorageCapacity.Format("2000", unit: DataSizeUnit.Terabyte);            // "2 TB"
StorageCapacity.Normalize("2 TB");                                      // "2000000000000 B"
StorageCapacity.IsNormalized("2000000000000 B");                        // true

StorageCapacity.TypeInfo.EnglishName;                                   // "Storage Capacity"
StorageCapacity.TypeInfo.LocalizedName;                                 // "Lagringskapacitet"

// Same Normalize() pattern across many other types
SwedishBankgiroNumber.Normalize("58056201");                            // "5805-6201"
PhoneNumber.Normalize("070-174 06 33");                                 // "0046701740633"
EmailAddress.Normalize("  User@GMAIL.COM  ");                           // "user@gmail.com"
Url.Normalize("example.se/path");                                       // "https://example.se/path"
Country.Normalize("Tyskland");                                          // "DE"
Iban.Normalize("SE45 5000 0000 0583 9825 7466");                        // "SE4550000000058398257466"
```

This package contains strongly typed, self-validating value objects covering:

- **Organization & identity** — org numbers, VAT numbers, DUNS, LEI, ELF codes, SNI codes, CFAR numbers, personal identity numbers, coordination numbers
- **Banking & finance** — IBAN, BIC, ISIN, bankgiro, plusgiro, Swish, clearing numbers, bank accounts, OCR references, currencies, money amounts
- **Contact & web** — phone numbers, email addresses, URLs, addresses (with strict country-specific variants for 32 European countries), person names, person age
- **Geography** — countries, counties, municipalities, geographic coordinates (with distance calculations), zip codes, phone calling codes
- **Property** — property designations, taxation codes
- **Vehicles** — registration numbers, VINs, driving license categories, fuel types, transmissions, EU vehicle categories, type approval numbers, operating hours, bolt patterns
- **Product & commerce** — GTINs, HS codes, Google product categories, colors, clothing genders, clothing/shoe sizes, IP ratings
- **Technology** — screen sizes/resolutions, energy efficiency classes, electrical phases, operating systems, storage/RAM capacity, processor speed, battery capacity
- **Measurements** — 21 unit types including length, area, volume, weight, energy, power, speed, temperature, pressure, voltage, electric current, flow rate, luminous flux, and more

Cross-cutting features: [`ToMaskedString()`](#masking-extension-methods) for safe PII display, [`Validate()`](#structured-validation) with machine-readable reason codes and bilingual descriptions, [text scanning](#text-scanning) for heuristic extraction from unstructured text with confidence scoring and bulk redaction, and [`ToNaturalString()`](#natural-unit-display) for automatic human-friendly unit selection on measurement types.

The package is focused on domain concepts that are especially relevant when building software for a Swedish context. Some primitives are also useful internationally, such as VAT numbers, countries, IBAN, and phone numbers, but the API and defaults are optimized from a Swedish perspective. The library is provided under the MIT license with no warranties or guarantees.

All validation and normalization in this package is performed in-memory using rules, patterns, checksums, and reference data available at build time. No external API calls, registry lookups, or live service checks are performed, so a value being considered valid means it matches the implemented format and rule set, not that the organization, person, bank account, country detail, or address has been verified against an authoritative external source.

## Table of contents

- [Supported types](#supported-types)
  - [Validation and formatting behavior](#validation-and-formatting-behavior)
  - [Structured validation](#structured-validation)
- [Usage](#usage)
  - [Installation](#installation)
  - [Organization](#organization)
    - [Organization numbers](#organization-numbers)
    - [VAT numbers](#vat-numbers)
    - [Organization identifier parser](#swedish-organization-identifier-parser)
    - [Organization name](#organization-name)
    - [SNI code](#sni-code)
    - [CFAR number](#cfar-number)
    - [ELF code](#elf-code)
  - [Banking](#banking)
    - [Banking number parser](#swedish-banking-number-parser)
    - [Bankgiro](#bankgiro)
    - [Plusgiro](#plusgiro)
    - [OCR reference number](#ocr-reference-number)
    - [BIC](#bic)
    - [IBAN](#iban)
    - [Clearing number](#clearing-number)
    - [Swedish bank account](#swedish-bank-account)
    - [Bank account holder name](#bank-account-holder-name)
    - [Swish number](#swish-number)
  - [Property](#property)
    - [Swedish property designation](#swedish-property-designation)
    - [Property taxation code](#property-taxation-code)
  - [Geography](#geography)
    - [Swedish county](#swedish-county)
    - [Swedish municipality](#swedish-municipality)
    - [Country](#country)
    - [Geo coordinate](#geo-coordinate)
  - [Finance](#finance)
    - [Currency](#currency)
    - [Money amount](#money-amount)
    - [ISIN](#isin)
  - [Contact](#contact)
    - [Address](#address)
    - [Swedish address](#swedish-address)
    - [Swedish zip code](#swedish-zip-code)
    - [Country-specific addresses](#country-specific-addresses)
    - [Zip code](#zip-code)
    - [City](#city)
    - [Street address](#street-address)
    - [Phone calling code](#phone-calling-code)
    - [Phone number](#phone-number)
    - [Test phone numbers](#test-phone-numbers)
    - [Contact info](#contact-info)
  - [Web](#web)
    - [Email address](#email-address)
    - [URL](#url)
  - [Vehicle](#vehicle)
    - [Vehicle registration number](#vehicle-registration-number)
    - [Vehicle identification number](#vehicle-identification-number)
    - [Euro emission class](#euro-emission-class)
    - [Tire dimension](#tire-dimension)
    - [Engine displacement](#engine-displacement)
    - [Engine power](#engine-power)
    - [Odometer reading](#odometer-reading)
    - [Driving license category](#driving-license-category)
    - [Fuel type](#fuel-type)
    - [Transmission type](#transmission-type)
    - [Swedish vehicle type](#swedish-vehicle-type)
    - [Swedish vehicle status](#swedish-vehicle-status)
    - [EU vehicle category](#eu-vehicle-category)
    - [EU type approval number](#eu-type-approval-number)
    - [Wheel rim dimension](#wheel-rim-dimension)
    - [Fuel consumption](#fuel-consumption)
    - [Emission rate](#emission-rate)
  - [Product](#product)
    - [GTIN](#gtin)
    - [GTIN-8](#gtin-8)
    - [GTIN-12](#gtin-12)
    - [GTIN-13](#gtin-13)
    - [GTIN-14](#gtin-14)
    - [HS code](#hs-code)
    - [Google product category](#google-product-category)
    - [Color](#color)
    - [Adult clothing size](#adult-clothing-size)
    - [Child clothing size](#child-clothing-size)
    - [Clothing size](#clothing-size)
    - [Adult shoe size](#adult-shoe-size)
    - [Child shoe size](#child-shoe-size)
    - [Shoe size](#shoe-size)
    - [Screen size](#screen-size)
    - [Screen resolution](#screen-resolution)
    - [Energy efficiency class](#energy-efficiency-class)
    - [Operating system name](#operating-system-name)
    - [Operating system version](#operating-system-version)
    - [Operating system info](#operating-system-info)
    - [Storage capacity](#storage-capacity)
    - [RAM capacity](#ram-capacity)
    - [Processor speed](#processor-speed)
    - [Battery capacity](#battery-capacity)
    - [Electrical phase](#electrical-phase)
    - [Clothing gender](#clothing-gender)
  - [Person](#person)
    - [Person given name](#person-given-name)
    - [Person family name](#person-family-name)
    - [Person full name](#person-full-name)
    - [Personal identity number](#personal-identity-number)
    - [Coordination number](#coordination-number)
  - [Person ↔ Organization conversions](#person--organization-conversions)
  - [Measurement](#measurement)
    - [Length](#length)
    - [Area](#area)
    - [Volume](#volume)
    - [Weight](#weight)
    - [Energy](#energy)
    - [Power](#power)
    - [Voltage](#voltage)
    - [Electric charge](#electric-charge)
    - [Torque](#torque)
    - [Frequency](#frequency)
    - [Speed](#speed)
    - [Temperature](#temperature)
    - [Data size](#data-size)
    - [Pressure](#pressure)
    - [Percentage](#percentage)
    - [Sound level](#sound-level)
    - [Rotational speed](#rotational-speed)
    - [Natural unit display](#natural-unit-display)
- [Masking sensitive data](#masking-sensitive-data)
  - [Summary of masked types](#summary-of-masked-types)
  - [Person masking](#person-masking)
  - [Organization masking](#organization-masking)
  - [Banking masking](#banking-masking)
  - [Contact masking](#contact-masking)
  - [Web masking](#web-masking)
  - [Geography masking](#geography-masking)
  - [Finance masking](#finance-masking)
- [Text scanning](#text-scanning)
  - [Per-type scanning](#per-type-scanning)
  - [Aggregate scanner](#aggregate-scanner)
  - [Masking and redaction](#masking-and-redaction)
  - [Overlap resolution](#overlap-resolution)
- [Misc](#misc)
  - [Type metadata](#type-metadata)
  - [Lookup URLs](#lookup-urls)
  - [Reference-data versioning](#reference-data-versioning)
  - [Language-aware formatting](#language-aware-formatting)
  - [Sample data](#sample-data)
- [Testing](#testing)
  - [Unit tests](#unit-tests)
  - [Performance benchmarks](#performance-benchmarks)
- [Versioning](#versioning)
- [Acknowledgements](#acknowledgements)
- [Contributing](#contributing)
- [FAQ](#faq)
- [License](#license)

## Supported types

For a compact, searchable reference of all types with their full namespace-qualified names, see [Supported Types](docs/articles/supported-types.md).

All types share a consistent API:


| Method                         | Returns   | Description                                                                                                                                   |
| ------------------------------ | --------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| `.TryParse(input, out result)` | `bool`    | Parse and validate, returning a typed instance                                                                                                |
| `.Parse(input)`                | instance  | Parse or throw `ArgumentException`                                                                                                            |
| `.IsValid(input)`              | `bool`    | Check validity without creating an instance                                                                                                   |
| `.Format(input)`               | `string?` | Formatted string if valid, `null` if invalid/empty. Pass `fallbackToTrimmedInputWhenInvalid: true` to get trimmed input instead of `null` on failure |
| `.Normalize(input)`            | `string?` | Canonical/machine-readable form, or `null` if invalid. Pass `fallbackToTrimmedInputWhenInvalid: true` to get trimmed input instead of `null` on failure (empty → `null`) |
| `.IsNormalized(input)`         | `bool`    | `true` if the input is valid and already in its normalized form                                                                               |


**Organization** (`Buildi.Primitives.Organization`)


| Type                                                              | Swedish             | Description                                                                                                                     |
| ----------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| [`SwedishOrganizationNumber`](#organization-numbers)              | Organisationsnummer | Swedish organization number with 10-digit statutory form and 12-digit normalized form, including sole-trader person-based forms |
| [`EuVatNumber`](#vat-numbers)                                       | Momsnummer          | EU VAT numbers with country-specific validation                                                                                 |
| [`SwedishOrganizationName`](#organization-name)                   | Organisationsnamn   | Organization/company name (whitespace-normalized, 2–200 chars)                                                                  |
| [`SwedishSniCode`](#sni-code)                                     | SNI-kod             | Swedish industrial classification code (SNI 2025, effective December 2024)                                                      |
| [`SwedishCfarNumber`](#cfar-number)                               | CFAR-nummer         | Swedish 8-digit establishment/workplace identifier                                                                              |
| [`DunsNumber`](#swedish-organization-identifier-parser)                   | DUNS-nummer         | D-U-N-S number (9 digits)                                                                                                       |
| [`LeiCode`](#swedish-organization-identifier-parser)                      | LEI-kod             | Legal Entity Identifier (20 chars, ISO 17442)                                                                                   |
| [`SwedishOrganizationIdentifierParser`](#swedish-organization-identifier-parser) | Identifierare       | Unified classifier for Swedish org/personal/coordination numbers, VAT, DUNS, and LEI                                            |
| [`ElfCode`](#elf-code)                                            | ELF-kod             | Entity Legal Form code (ISO 20275, 4-char alphanumeric) with Swedish legal form descriptions                                    |


**Banking** (`Buildi.Primitives.Banking`)


| Type                                                 | Swedish           | Description                                                              |
| ---------------------------------------------------- | ----------------- | ------------------------------------------------------------------------ |
| [`SwedishBankgiroNumber`](#bankgiro)                 | Bankgironummer    | Swedish Bankgiro number                                                  |
| [`SwedishPostgiroNumber`](#plusgiro)                 | Plusgironummer    | Swedish Plusgiro number                                                  |
| [`SwedishOcrReferenceNumber`](#ocr-reference-number) | OCR-nummer        | Swedish OCR reference number with optional agreement-specific controls   |
| [`Bic`](#bic)                                        | BIC/SWIFT-kod     | Business Identifier Code for international bank routing                  |
| [`Iban`](#iban)                                      | IBAN              | International Bank Account Number                                        |
| [`SwedishBankClearingNumber`](#clearing-number)      | Clearingnummer    | Swedish bank clearing number with bank identification                    |
| [`SwedishBankAccount`](#swedish-bank-account)        | Bankkonto         | Swedish bank account with clearing number and bank detection             |
| [`SwedishBankingNumberParser`](#swedish-banking-number-parser)      | Bankidentifierare | Unified parser for Bankgiro, Plusgiro, OCR, BIC, IBAN, and bank accounts |
| [`SwedishBankAccountHolderName`](#bank-account-holder-name) | Kontoinnehavare | Bank account holder name with auto-detection of person vs. organization |
| [`SwedishSwishNumber`](#swish-number) | Swish-nummer | Swish payment number (123-numbers, 90-numbers, and mobile) |


**Property** (`Buildi.Primitives.Property`)


| Type                                                          | Swedish              | Description                                                      |
| ------------------------------------------------------------- | -------------------- | ---------------------------------------------------------------- |
| [`SwedishPropertyDesignation`](#swedish-property-designation)         | Fastighetsbeteckning | Swedish property designation with tract name and register number |
| [`SwedishPropertyTaxationCode`](#property-taxation-code)              | Typkod               | Swedish 3-digit property taxation code from Skatteverket         |


**Geography** (`Buildi.Primitives.Geography`)


| Type                                           | Swedish | Description                                      |
| ---------------------------------------------- | ------- | ------------------------------------------------ |
| [`SwedishCounty`](#swedish-county)             | Län     | Swedish county with official code and name       |
| [`SwedishMunicipality`](#swedish-municipality) | Kommun  | Swedish municipality with official code and name |
| [`Country`](#country)                          | Land    | Country with names, ISO codes, calling code, ccTLD, continent, language, coordinates, capital, borders, regional flags, currency |
| `CountryCapital`                               | Huvudstad | Capital city with English, Swedish, and native names plus coordinates |
| [`Language`](#language)                        | Språk   | Language with ISO 639-1/639-2 codes, English/Swedish/native names, script, text direction |
| [`GeoCoordinate`](#geo-coordinate)             | Koordinat | Geographic coordinate (latitude, longitude) in WGS 84; parses DD, DMS, DDM formats with Haversine distance |
| `Continent`                                    | Kontinent | Geographic continent; enum used by `Country.Continent` |


**Finance** (`Buildi.Primitives.Finance`)


| Type                           | Swedish | Description                                                                            |
| ------------------------------ | ------- | -------------------------------------------------------------------------------------- |
| [`Currency`](#currency)        | Valuta  | ISO 4217 currency with code, English/Swedish names, symbol, and decimal places         |
| [`MoneyAmount`](#money-amount) | Belopp  | Monetary amount with currency - parses formats like `1 000 SEK`, `$500`, `1 000,50 kr` |
| [`Isin`](#isin)                | ISIN    | International Securities Identification Number (ISO 6166, 12-char with Luhn check)     |


**Contact** (`Buildi.Primitives.Contact`)


| Type                               | Swedish        | Description                                                                                                           |
| ---------------------------------- | -------------- | --------------------------------------------------------------------------------------------------------------------- |
| [`Address`](#address)              | Adress         | Unified address model composed of street, zip code, city, and country                                                 |
| [`SwedishAddress`](#swedish-address) | Svensk adress | Swedish-only address requiring 5-digit zip code and city; rejects non-SE countries                                  |
| [`SwedishAddressZipCode`](#swedish-zip-code) | Postnummer | Swedish 5-digit postal code only (NNN NN); rejects international formats                                     |
| [`PolishAddress`](#country-specific-addresses) | Adress (PL) | Polish address requiring street, 5-digit zip code (NN-NNN), and city |
| [`PolishAddressZipCode`](#country-specific-addresses) | Postnummer (PL) | Polish 5-digit postal code (NN-NNN) |
| [`EstonianAddress`](#country-specific-addresses) | Adress (EE) | Estonian address requiring street, 5-digit zip code, and city |
| [`EstonianAddressZipCode`](#country-specific-addresses) | Postnummer (EE) | Estonian 5-digit postal code |
| [`FinnishAddress`](#country-specific-addresses) | Adress (FI) | Finnish address requiring street, 5-digit zip code, and city |
| [`FinnishAddressZipCode`](#country-specific-addresses) | Postnummer (FI) | Finnish 5-digit postal code |
| [`LithuanianAddress`](#country-specific-addresses) | Adress (LT) | Lithuanian address requiring street, 5-digit zip code (LT-NNNNN), and city |
| [`LithuanianAddressZipCode`](#country-specific-addresses) | Postnummer (LT) | Lithuanian 5-digit postal code (LT-NNNNN) |
| [`RomanianAddress`](#country-specific-addresses) | Adress (RO) | Romanian address requiring street, 6-digit zip code, and city |
| [`RomanianAddressZipCode`](#country-specific-addresses) | Postnummer (RO) | Romanian 6-digit postal code |
| [`DanishAddress`](#country-specific-addresses) | Adress (DK) | Danish address requiring street, 4-digit zip code, and city |
| [`DanishAddressZipCode`](#country-specific-addresses) | Postnummer (DK) | Danish 4-digit postal code |
| [`NorwegianAddress`](#country-specific-addresses) | Adress (NO) | Norwegian address requiring street, 4-digit zip code, and city |
| [`NorwegianAddressZipCode`](#country-specific-addresses) | Postnummer (NO) | Norwegian 4-digit postal code |
| [`GermanAddress`](#country-specific-addresses) | Adress (DE) | German address requiring street, 5-digit zip code, and city |
| [`GermanAddressZipCode`](#country-specific-addresses) | Postnummer (DE) | German 5-digit postal code |
| [`BulgarianAddress`](#country-specific-addresses) | Adress (BG) | Bulgarian address requiring street, 4-digit zip code, and city |
| [`BulgarianAddressZipCode`](#country-specific-addresses) | Postnummer (BG) | Bulgarian 4-digit postal code |
| [`LatvianAddress`](#country-specific-addresses) | Adress (LV) | Latvian address requiring street, 4-digit zip code (LV-NNNN), and city |
| [`LatvianAddressZipCode`](#country-specific-addresses) | Postnummer (LV) | Latvian 4-digit postal code (LV-NNNN) |
| [`CzechAddress`](#country-specific-addresses) | Adress (CZ) | Czech address requiring street, 5-digit zip code (NNN NN), and city |
| [`CzechAddressZipCode`](#country-specific-addresses) | Postnummer (CZ) | Czech 5-digit postal code (NNN NN) |
| [`SpanishAddress`](#country-specific-addresses) | Adress (ES) | Spanish address requiring street, 5-digit zip code, and city |
| [`SpanishAddressZipCode`](#country-specific-addresses) | Postnummer (ES) | Spanish 5-digit postal code |
| [`DutchAddress`](#country-specific-addresses) | Adress (NL) | Dutch address requiring street, zip code (NNNN AA), and city |
| [`DutchAddressZipCode`](#country-specific-addresses) | Postnummer (NL) | Dutch postal code (NNNN AA — 4 digits + 2 letters) |
| [`GreekAddress`](#country-specific-addresses) | Adress (GR) | Greek address requiring street, 5-digit zip code (NNN NN), and city |
| [`GreekAddressZipCode`](#country-specific-addresses) | Postnummer (GR) | Greek 5-digit postal code (NNN NN) |
| [`ItalianAddress`](#country-specific-addresses) | Adress (IT) | Italian address requiring street, 5-digit zip code, and city |
| [`ItalianAddressZipCode`](#country-specific-addresses) | Postnummer (IT) | Italian 5-digit postal code |
| [`SlovenianAddress`](#country-specific-addresses) | Adress (SI) | Slovenian address requiring street, 4-digit zip code, and city |
| [`SlovenianAddressZipCode`](#country-specific-addresses) | Postnummer (SI) | Slovenian 4-digit postal code |
| [`CroatianAddress`](#country-specific-addresses) | Adress (HR) | Croatian address requiring street, 5-digit zip code, and city |
| [`CroatianAddressZipCode`](#country-specific-addresses) | Postnummer (HR) | Croatian 5-digit postal code |
| [`PortugueseAddress`](#country-specific-addresses) | Adress (PT) | Portuguese address requiring street, 7-digit zip code (NNNN-NNN), and city |
| [`PortugueseAddressZipCode`](#country-specific-addresses) | Postnummer (PT) | Portuguese 7-digit postal code (NNNN-NNN) |
| [`HungarianAddress`](#country-specific-addresses) | Adress (HU) | Hungarian address requiring street, 4-digit zip code, and city |
| [`HungarianAddressZipCode`](#country-specific-addresses) | Postnummer (HU) | Hungarian 4-digit postal code |
| [`FrenchAddress`](#country-specific-addresses) | Adress (FR) | French address requiring street, 5-digit zip code, and city |
| [`FrenchAddressZipCode`](#country-specific-addresses) | Postnummer (FR) | French 5-digit postal code |
| [`SlovakAddress`](#country-specific-addresses) | Adress (SK) | Slovak address requiring street, 5-digit zip code (NNN NN), and city |
| [`SlovakAddressZipCode`](#country-specific-addresses) | Postnummer (SK) | Slovak 5-digit postal code (NNN NN) |
| [`BelgianAddress`](#country-specific-addresses) | Adress (BE) | Belgian address requiring street, 4-digit zip code, and city |
| [`BelgianAddressZipCode`](#country-specific-addresses) | Postnummer (BE) | Belgian 4-digit postal code |
| [`BritishAddress`](#country-specific-addresses) | Adress (GB) | British address requiring street, alphanumeric postcode, and city |
| [`BritishAddressZipCode`](#country-specific-addresses) | Postnummer (GB) | British alphanumeric postcode |
| [`AustrianAddress`](#country-specific-addresses) | Adress (AT) | Austrian address requiring street, 4-digit zip code, and city |
| [`AustrianAddressZipCode`](#country-specific-addresses) | Postnummer (AT) | Austrian 4-digit postal code |
| [`CypriotAddress`](#country-specific-addresses) | Adress (CY) | Cypriot address requiring street, 4-digit zip code, and city |
| [`CypriotAddressZipCode`](#country-specific-addresses) | Postnummer (CY) | Cypriot 4-digit postal code |
| [`IcelandicAddress`](#country-specific-addresses) | Adress (IS) | Icelandic address requiring street, 3-digit zip code, and city |
| [`IcelandicAddressZipCode`](#country-specific-addresses) | Postnummer (IS) | Icelandic 3-digit postal code |
| [`SwissAddress`](#country-specific-addresses) | Adress (CH) | Swiss address requiring street, 4-digit zip code, and city |
| [`SwissAddressZipCode`](#country-specific-addresses) | Postnummer (CH) | Swiss 4-digit postal code |
| [`IrishAddress`](#country-specific-addresses) | Adress (IE) | Irish address requiring street, Eircode (ANN XXXX), and city |
| [`IrishAddressZipCode`](#country-specific-addresses) | Postnummer (IE) | Irish Eircode (7-character alphanumeric) |
| [`LuxembourgishAddress`](#country-specific-addresses) | Adress (LU) | Luxembourgish address requiring street, 4-digit zip code, and city |
| [`LuxembourgishAddressZipCode`](#country-specific-addresses) | Postnummer (LU) | Luxembourgish 4-digit postal code |
| [`MalteseAddress`](#country-specific-addresses) | Adress (MT) | Maltese address requiring street, zip code (AAA NNNN), and city |
| [`MalteseAddressZipCode`](#country-specific-addresses) | Postnummer (MT) | Maltese postal code (AAA NNNN) |
| [`LiechtensteinAddress`](#country-specific-addresses) | Adress (LI) | Liechtenstein address requiring street, 4-digit zip code, and city |
| [`LiechtensteinAddressZipCode`](#country-specific-addresses) | Postnummer (LI) | Liechtenstein 4-digit postal code |
| [`AddressZipCode`](#zip-code)      | Postnummer     | Postal/zip code - Swedish (5 digits) and international formats                                                        |
| [`AddressCity`](#city)             | Postort        | City name with Swedish-aware capitalization                                                                           |
| [`AddressStreet`](#street-address) | Gatuadress     | Street address with street name, house number, care-of, apartment number, and post box extraction                     |
| [`PhoneCallingCode`](#phone-calling-code) | Landsnummer | Phone country calling code (e.g. +46 for Sweden) with 190+ known codes, country mapping, and named constants |
| [`PhoneNumber`](#phone-number)     | Telefonnummer  | Phone number with country detection, 00-prefixed normalized form, E.164 output, and configurable default calling code |
| [`ContactAddress`](#contact-info)  | Kontaktuppgift | Composite of optional person name, organization name, and address                                                     |


**Web** (`Buildi.Primitives.Web`)


| Type                               | Swedish        | Description                                                                                                           |
| ---------------------------------- | -------------- | --------------------------------------------------------------------------------------------------------------------- |
| [`EmailAddress`](#email-address)   | E-postadress   | Email address with TLD, country mapping, public provider detection, and optional typo correction                      |
| [`Url`](#url)                      | Webbadress     | URL/URI with any scheme; TLD-to-country mapping, example-domain detection, auto-prefix for bare domains               |


**Vehicle** (`Buildi.Primitives.Vehicle`)


| Type                                                               | Swedish             | Description                                                                   |
| ------------------------------------------------------------------ | ------------------- | ----------------------------------------------------------------------------- |
| [`SwedishVehicleRegistrationNumber`](#vehicle-registration-number) | Registreringsnummer | Swedish vehicle plate (ABC 123 / ABC 12A) with tax payment month              |
| [`VehicleIdentificationNumber`](#vehicle-identification-number)    | Chassinummer / VIN  | 17-character ISO 3779 VIN with check digit validation and model year decoding |
| [`EuroEmissionClass`](#euro-emission-class) | Euro-utsläppsklass | European emission standard (Euro 1–7) with Swedish miljöklass mapping, Roman numerals |
| [`TireDimension`](#tire-dimension) | Däckdimension | Metric tire notation (e.g. 205/55R16, 315/80R22.5) with width, aspect ratio, rim diameter; supports commercial/truck formats |
| [`EngineDisplacement`](#engine-displacement) | Motorvolym | Engine displacement in cc/mL/liters, wraps `Volume` |
| [`EnginePower`](#engine-power) | Motoreffekt | Engine power in HP/hk/kW, wraps `Power`; supports approximate prefixes (ca, ~, circa) |
| [`OdometerReading`](#odometer-reading) | Mätarställning | Odometer reading in km/miles/mil, wraps `Length` |
| [`SwedishDrivingLicenseCategory`](#driving-license-category) | Körkortsbehörighet | EU driving license category (AM–DE) with vehicle group, age, and metadata |
| [`FuelType`](#fuel-type) | Drivmedel | Vehicle fuel/energy type (Petrol, Diesel, Electric, Hybrid, etc.) with Transportstyrelsen codes |
| [`TransmissionType`](#transmission-type) | Växellåda | Transmission/gearbox type (Manual, Automatic, CVT, DCT, etc.) |
| [`SwedishVehicleType`](#swedish-vehicle-type) | Fordonsslag | Swedish vehicle type classification with Transportstyrelsen codes (PB, LB, MC, etc.) |
| [`SwedishVehicleStatus`](#swedish-vehicle-status) | Fordonsstatus | Swedish vehicle registration status (I trafik, Avställd, Avregistrerad, etc.) |
| [`EuVehicleCategory`](#eu-vehicle-category) | Fordonskategori EU | EU vehicle category code (M1, N1G, L3e-A2, O4, etc.) per Regulation 2018/858 |
| [`EuTypeApprovalNumber`](#eu-type-approval-number) | Typgodkännandenummer | EU whole-vehicle type-approval number (e.g. e9\*2007/46\*6364\*09) |
| [`WheelRimDimension`](#wheel-rim-dimension) | Fälgdimension | Wheel rim size notation (e.g. 18x7J) with diameter, width, and flange type |
| [`FuelConsumption`](#fuel-consumption) | Bränsleförbrukning | Fuel consumption in l/100km, km/l, mpg, or kWh/100km with unit conversions |
| [`EmissionRate`](#emission-rate) | Utsläpp | Vehicle emission rate in g/km or mg/km for CO₂, NOₓ, etc. |
| [`OperatingHours`](#operating-hours) | Drifttimmar | Machine operating hours |
| [`BoltPattern`](#bolt-pattern) | Bultcirkelmått | Wheel bolt pattern |


**Product** (`Buildi.Primitives.Product`)


| Type                 | Swedish          | Description                                                                |
| -------------------- | ---------------- | -------------------------------------------------------------------------- |
| [`Gtin`](#gtin)      | GTIN             | General GTIN parser accepting 8, 12, 13, or 14 digit codes                 |
| [`Gtin8`](#gtin-8)   | GTIN-8 / EAN-8   | 8-digit GTIN (EAN-8)                                                       |
| [`Gtin12`](#gtin-12) | GTIN-12 / UPC-A  | 12-digit GTIN (UPC-A)                                                      |
| [`Gtin13`](#gtin-13) | GTIN-13 / EAN-13 | 13-digit GTIN (EAN-13) with GS1 prefix, country, and organization metadata |
| [`Gtin14`](#gtin-14) | GTIN-14 / ITF-14 | 14-digit GTIN (ITF-14) with indicator digit                                |
| [`HsCode`](#hs-code) | HS-kod / KN-nummer | Harmonized System code (2–10 digits) for classifying goods in international trade |
| [`GoogleProductCategory`](#google-product-category) | Google-produktkategori | Hierarchical product taxonomy path for Google Shopping / Merchant Center |
| [`Color`](#color) | Färg | Color value from CSS names, hex, rgb(), hsl() with English/Swedish names |
| [`AdultClothingSize`](#adult-clothing-size) | Klädstorlek (vuxen) | Adult clothing size with EU/US/UK/letter systems |
| [`ChildClothingSize`](#child-clothing-size) | Klädstorlek (barn) | Children's clothing size based on body height (cm) |
| [`ClothingSize`](#clothing-size) | Klädstorlek | Generic clothing size auto-detecting adult or child |
| [`AdultShoeSize`](#adult-shoe-size) | Skostorlek (vuxen) | Adult shoe size with EU/US/UK systems |
| [`ChildShoeSize`](#child-shoe-size) | Skostorlek (barn) | Children's shoe size (EU 16-39) |
| [`ShoeSize`](#shoe-size) | Skostorlek | Generic shoe size auto-detecting adult or child |
| [`ScreenSize`](#screen-size) | Skärmstorlek | Screen diagonal measurement, defaults to inches |
| [`ScreenResolution`](#screen-resolution) | Skärmupplösning | Screen resolution (e.g. 1920x1080, Full HD, 4K) |
| [`EuEnergyEfficiencyClass`](#energy-efficiency-class) | Energiklass | EU energy efficiency label (A+++–G) |
| [`OperatingSystemName`](#operating-system-name) | Operativsystem | Canonical OS name (Windows, macOS, Ubuntu, etc.) with family classification |
| [`OperatingSystemVersion`](#operating-system-version) | OS-version | Version string with major/minor/patch/build parsing and comparison |
| [`OperatingSystemInfo`](#operating-system-info) | Operativsysteminfo | Combined OS name + version, e.g. `Windows 11`, `macOS 14.5` |
| [`StorageCapacity`](#storage-capacity) | Lagringskapacitet | Hard drive/SSD capacity, wraps `DataSize`, defaults to GB |
| [`RamCapacity`](#ram-capacity) | Arbetsminne | RAM/memory capacity, wraps `DataSize`, defaults to GB |
| [`ProcessorSpeed`](#processor-speed) | Processorhastighet | CPU clock speed, wraps `Frequency`, defaults to GHz |
| [`BatteryCapacity`](#battery-capacity) | Batterikapacitet | Battery capacity in mAh or Wh, wraps `ElectricCharge`/`Energy` |
| [`ElectricalPhase`](#electrical-phase) | Fas | Electrical phase configuration (single-phase, two-phase, three-phase) |
| [`ClothingGender`](#clothing-gender) | Klädkön | Clothing target gender (male, female, unisex, boys, girls) |
| [`IpRating`](#ip-rating) | IP-klass | Ingress Protection rating |


**Person** (`Buildi.Primitives.Person`)


| Type                                                         | Swedish           | Description                                             |
| ------------------------------------------------------------ | ----------------- | ------------------------------------------------------- |
| [`PersonGivenName`](#person-given-name)  | Förnamn           | Given names with optional preferred name (tilltalsnamn) |
| [`PersonFamilyName`](#person-family-name)| Efternamn         | Family name (surname)                                   |
| [`PersonFullName`](#person-full-name)    | Fullständigt namn | Full name composed of given names and family name       |
| [`PersonAge`](#person-age)               | Ålder             | Age in years, months, or days with Swedish legal-age properties |
| [`SwedishPersonalIdentityNumber`](#personal-identity-number) | Personnummer      | Swedish personal identity number                        |
| [`SwedishCoordinationNumber`](#coordination-number)          | Samordningsnummer | Swedish coordination number                             |


**Measurement** (`Buildi.Primitives.Measurement`)


| Type | Swedish | Description |
| ---- | ------- | ----------- |
| [`Length`](#length) | Längd | Length/distance in meters, km, cm, inches, feet, miles, etc. |
| [`Area`](#area) | Area | Area in m², km², hectares, acres, etc. |
| [`Volume`](#volume) | Volym | Volume in liters, mL, gallons, etc. |
| [`Weight`](#weight) | Vikt | Weight/mass in kg, grams, pounds, etc. |
| [`Energy`](#energy) | Energi | Energy in joules, kWh, calories, BTU, etc. |
| [`Power`](#power) | Effekt | Power in watts, kW, horsepower, etc. |
| [`Voltage`](#voltage) | Spänning | Voltage in V, mV, kV |
| [`ElectricCharge`](#electric-charge) | Elektrisk laddning | Electric charge in Ah, mAh, coulombs |
| [`Torque`](#torque) | Vridmoment | Torque in Nm, ft-lb, kgf-m |
| [`Frequency`](#frequency) | Frekvens | Frequency in Hz, kHz, MHz, GHz, RPM |
| [`Speed`](#speed) | Hastighet | Speed in m/s, km/h, mph, knots |
| [`Temperature`](#temperature) | Temperatur | Temperature in °C, °F, K with offset conversions |
| [`DataSize`](#data-size) | Datastorlek | Data size in bytes, KB, MB, GB with SI and binary prefixes |
| [`Pressure`](#pressure) | Tryck | Pressure in Pa, bar, PSI, atm |
| [`Percentage`](#percentage) | Procent | Percentage (0-100% display, 0-1 decimal storage) |
| [`SoundLevel`](#sound-level) | Ljudnivå | Sound pressure level in dB with optional A/B/C/Z weighting |
| [`RotationalSpeed`](#rotational-speed) | Rotationshastighet | Rotational speed in rpm, rps, or rad/s with unit conversions |
| [`ElectricCurrent`](#electric-current) | Elektrisk ström | Electric current in A, mA, kA |
| [`FlowRate`](#flow-rate) | Flöde | Volumetric flow rate in L/min, m³/h |
| [`LuminousFlux`](#luminous-flux) | Ljusflöde | Luminous flux in lumens |


### Validation and formatting behavior

Validation also operates on the parsed and normalized interpretation of the input. This means `IsValid()` first trims and normalizes supported formatting variations, so a string may be considered valid even if the exact raw input is not already in its canonical or formatted form, as long as it can be successfully parsed into a valid normalized value.

`Format()` returns the display format chosen by the specific type, not necessarily the same representation family that was originally entered. For example, a country code may be parsed successfully and formatted back as a country name. Where a type has both Swedish and English representations, the default output depends on `PrimitivesDefaults.UICulture` - use `ToDisplayString()`, `ToEnglishString()`, or `ToNativeString()` on the parsed instance when you need explicit control. If you need exact typed access to a specific representation, parse into the typed model and use its explicit properties instead. By default, `Format()` returns `null` when the input is invalid. Pass `fallbackToTrimmedInputWhenInvalid: true` to get the trimmed original input back instead of `null` for non-empty invalid input.

### Structured validation

Some types expose a `Validate()` method that returns a `ValidationResult` with detailed information about *why* the input is invalid - not just a boolean. This is useful for building user-facing form validation, API error responses, or diagnostics where a simple valid/invalid answer is not enough.

`Validate()` returns a `ValidationResult` containing:

- `RawInput` - the original input string
- `IsValid` - whether the input is valid
- `Issues` - a list of `ValidationIssue` objects, each with:
  - `Reason` - a `ValidationErrorReason` enum value (e.g. `InputIsEmpty`, `InvalidCheckDigit`, `UnknownClearingRange`)
  - `EnglishDescription` - human-readable explanation in English
  - `LocalizedDescription` - human-readable explanation in the current locale (Swedish)
  - `Description` - convenience accessor that picks `LocalizedDescription` or `EnglishDescription` based on `PrimitivesDefaults.UseSwedishDisplayNames`

```csharp
using Buildi.Primitives.Banking;
using Buildi.Primitives.Validation;

var result = SwedishBankClearingNumber.Validate("9999");

Console.WriteLine(result.RawInput);   // "9999"
Console.WriteLine(result.IsValid);    // false
Console.WriteLine(result.Issues[0].Reason);              // UnknownClearingRange
Console.WriteLine(result.Issues[0].EnglishDescription);  // "Clearing number does not match any known bank."
Console.WriteLine(result.Issues[0].LocalizedDescription); // "Clearingnumret matchar ingen känd bank."

// Valid input returns no issues
var valid = SwedishBankClearingNumber.Validate("5001");
Console.WriteLine(valid.IsValid);     // true
Console.WriteLine(valid.Issues.Count); // 0
```

Types with `Validate()`: `SwedishBankClearingNumber`, `SwedishBankAccount`, `SwedishBankgiroNumber`, `SwedishPostgiroNumber`, `SwedishOcrReferenceNumber`, `Iban`, `Bic`, `SwedishOrganizationNumber`, `EuVatNumber`, `SwedishPersonalIdentityNumber`, `SwedishCoordinationNumber`, `SwedishVehicleRegistrationNumber`, `VehicleIdentificationNumber`, `EmailAddress`.

## Usage

### Installation

```shell
dotnet add package Buildi.Primitives
```

### Organization

#### Organization numbers

A Swedish organization number (*organisationsnummer*) is a unique 10-digit identifier regulated by lagen om identitetsbeteckning för juridiska personer m.fl. (SFS 1974:174). Organization numbers are assigned by the authority that registers the entity - Bolagsverket for companies and associations, Skatteverket for sole traders and estates, Länsstyrelsen for foundations, and others. For sole traders (*enskild firma*), the personal identity number itself serves as the organization number. `Normalize()` returns a 12-digit convenience form for uniform storage (`16NNNNNNNNNN` for legal entities, `YYYYMMDDXXXX` for person-based numbers), but the statutory form is always 10 digits as returned by `Format()`.

The number is structured as `NNNNNN-NNNN`. The first digit indicates entity type: `1` = estate (*dödsbo*), `2` = public sector, `5` = limited company (*aktiebolag*), `7` = economic association, `8` = non-profit/foundation, `9` = partnership. For legal entities, digits 3–4 are always ≥ 20; for person-based numbers, they represent a birth month (01–12). The 10th digit is a Luhn (mod-10) check digit computed on the preceding 9 digits.

Tests: [SwedishOrganizationNumberTests.cs](test/Buildi.Primitives.Tests/Organization/SwedishOrganizationNumberTests.cs)

- [Bolagsverket](https://bolagsverket.se) - Swedish Companies Registration Office
- [Skatteverket - Organisationsnummer](https://skatteverket.se/foretag/drivaforetag/startaforetag/organisationsnummer.html)
- [Wikipedia - Organisationsnummer](https://sv.wikipedia.org/wiki/Organisationsnummer)

```csharp
using Buildi.Primitives.Organization;

if (SwedishOrganizationNumber.TryParse("5592460421", out var orgNr))
{
    Console.WriteLine(orgNr.To10DigitString());  // 559246-0421 (statutory form)
    Console.WriteLine(orgNr.To10DigitsOnly());   // 5592460421 (digits only, no separator)
    Console.WriteLine(orgNr.To12DigitString());  // 165592460421 (12-digit storage form)
    Console.WriteLine(orgNr.IsPerson);           // false
}

// Organization type hint - Certain is derived from the number alone; BestGuess may refine it using the name
// Digit 5 → Aktiebolag: the number alone is definitive
var ab = SwedishOrganizationNumber.Parse("5592460421");
var abHint = ab.GetSwedishOrganizationTypeHint(name: "Budi AB");
Console.WriteLine(abHint.Certain);    // Aktiebolag (digit 5 = AB)
Console.WriteLine(abHint.BestGuess);  // Aktiebolag

// Digit 2 → OffentligSektor: certain of the broad category, name narrows it
var kommun = SwedishOrganizationNumber.Parse("212000-0142");
var kommunHint = kommun.GetSwedishOrganizationTypeHint(name: "Stockholms kommun");
Console.WriteLine(kommunHint.Certain);    // OffentligSektor (digit 2 = public sector)
Console.WriteLine(kommunHint.BestGuess);  // Kommun (inferred from "kommun" in the name)

// Digit 7 → EkonomiskForening: certain of the broad category, name narrows it
var brf = SwedishOrganizationNumber.Parse("769621-2716");
var brfHint = brf.GetSwedishOrganizationTypeHint(name: "BRF Nytorp");
Console.WriteLine(brfHint.Certain);    // EkonomiskForening (digit 7 = economic association)
Console.WriteLine(brfHint.BestGuess);  // Bostadsrattsforening (inferred from "BRF" in the name)

// Person-based number - can be either a sole trader or a private person
var pin = SwedishOrganizationNumber.Parse("380303-2394");
Console.WriteLine(pin.IsPerson);    // true
var pinHint = pin.GetSwedishOrganizationTypeHint(isPrivatePerson: true);
Console.WriteLine(pinHint.Certain);    // EnskildFirmaEllerPrivatperson (cannot distinguish from number)
Console.WriteLine(pinHint.BestGuess);  // Privatperson (refined by the isPrivatePerson hint)

SwedishOrganizationNumber.IsValid("5592460421");       // true
SwedishOrganizationNumber.Format("5592460421");        // "559246-0421"
SwedishOrganizationNumber.Normalize("559246-0421");    // "165592460421"

// Structured validation - get detailed reasons for invalidity
var result = SwedishOrganizationNumber.Validate("5592460420"); // bad check digit
Console.WriteLine(result.IsValid);                    // false
Console.WriteLine(result.Issues[0].Reason);           // InvalidCheckDigit
Console.WriteLine(result.Issues[0].EnglishDescription); // "Invalid Luhn check digit."
```

#### VAT numbers

A VAT identification number (*momsnummer* / *momsregistreringsnummer*) uniquely identifies businesses registered for value-added tax within the EU. Format and validation rules are country-specific - this library supports all EU member states.

Each VAT number begins with a 2-letter ISO country prefix followed by a country-specific body. Swedish VAT numbers have the form `SEXXXXXXXXXX01` (the 10-digit org number plus `01`). Other examples: German `DEXXXXXXXXX` (9 digits), Danish `DKXXXXXXXX` (8 digits with mod-11 check). Validation verifies both the country prefix format and the applicable country-specific checksum or pattern.

Tests: [EuVatNumberTests.cs](test/Buildi.Primitives.Tests/Organization/EuVatNumberTests.cs)

- [EU VIES](https://ec.europa.eu/taxation_customs/vies/#/vat-validation) - VAT Information Exchange System
- [Wikipedia - VAT identification number](https://en.wikipedia.org/wiki/VAT_identification_number)

```csharp
if (EuVatNumber.TryParse("SE559246042101", out var vat))
{
    Console.WriteLine(vat.VatPrefix);    // SE (the EU VAT country prefix, may differ from ISO code)
    Console.WriteLine(vat.CountryCode);  // SE (ISO 3166-1 alpha-2)
    Console.WriteLine(vat.CountryName);  // Sweden
    Console.WriteLine(vat.Body);         // 559246042101
}

// Parse a VAT number requiring a specific country
EuVatNumber.TryParseForCountry("DE811234567", "DE", out var deVat); // true - German VAT
EuVatNumber.TryParseForCountry("DE811234567", "SE", out _);         // false - wrong country

EuVatNumber.IsValid("SE559246042101");       // true
EuVatNumber.Format("SE559246042101");        // "SE559246042101"
EuVatNumber.Normalize("SE559246042101");     // "SE559246042101"
```

#### Swedish organization identifier parser

A unified classifier that identifies and classifies organization identifiers across multiple formats: Swedish organization numbers, personal identity numbers, coordination numbers, DUNS, LEI, and VAT numbers. Personal identity numbers and coordination numbers are included because sole traders (*enskild firma*) use their personal identity number as their organization number under Swedish law. Useful when the input type is unknown.

Classification is attempted in priority order: Swedish organization number → personal identity number → coordination number → VAT number → LEI code → DUNS number. The first match wins. When a company name or `isPrivatePerson` hint is supplied, the result also includes an organization type classification (e.g., aktiebolag, enskild firma, kommun).

Tests: [SwedishOrganizationIdentifierParserTests.cs](test/Buildi.Primitives.Tests/Organization/SwedishOrganizationIdentifierParserTests.cs)

- [Dun & Bradstreet - D-U-N-S Number](https://www.dnb.com/duns.html)
- [GLEIF - Legal Entity Identifier (LEI)](https://www.gleif.org/en/about-lei/iso-17442-the-lei-code-structure/)
- [Wikipedia - Data Universal Numbering System](https://en.wikipedia.org/wiki/Data_Universal_Numbering_System)
- [Wikipedia - Legal Entity Identifier](https://en.wikipedia.org/wiki/Legal_Entity_Identifier)

```csharp
// Basic parsing - type and normalized value
if (SwedishOrganizationIdentifierParser.TryParse("5592460421", out var result))
{
    Console.WriteLine(result.Type);                        // SwedishOrganizationNumber
    Console.WriteLine(result.NormalizedValue);             // 165592460421
    Console.WriteLine(result.SwedishOrganizationNumber);   // typed SwedishOrganizationNumber instance
}

// With name and person hints - improves SwedishOrganizationType classification
SwedishOrganizationIdentifierParser.TryParse("5592460421", out var hinted,
    organizationName: "Budi AB", isPrivatePerson: false);
Console.WriteLine(hinted.OrganizationTypeHintCertain);    // Unknown
Console.WriteLine(hinted.OrganizationTypeHintBestGuess);  // Aktiebolag

// Typed accessors for each recognized format
SwedishOrganizationIdentifierParser.TryParse("SE559246042101", out var vatResult);
Console.WriteLine(vatResult.EuVatNumber?.CountryCode);   // SE

SwedishOrganizationIdentifierParser.TryParse("804735132", out var dunsResult);
Console.WriteLine(dunsResult.DunsNumber?.Digits);      // 804735132

SwedishOrganizationIdentifierParser.TryParse("5493001KJTIIGC8Y1R12", out var leiResult);
Console.WriteLine(leiResult.LeiCode?.Value);           // 5493001KJTIIGC8Y1R12

// Country-specific parsing - restricts to one country; rejects DUNS and LEI
SwedishOrganizationIdentifierParser.TryParseForCountry("5592460421", "SE", out var seResult);
Console.WriteLine(seResult.Type);                      // SwedishOrganizationNumber
SwedishOrganizationIdentifierParser.TryParseForCountry("5592460421", "DE", out _); // false - no German match
```

#### Organization name

The registered trade name (*organisationsnamn* / *företagsnamn*) of a legal entity. Swedish company names are registered with Bolagsverket. Validation accepts names of 2–200 characters; normalization collapses whitespace.

No checksum or structural validation is applied - any non-empty string of 2–200 characters (after trimming) is accepted. Leading, trailing, and consecutive internal whitespace is normalized to a single space. Casing is preserved as entered.

Tests: [SwedishOrganizationNameTests.cs](test/Buildi.Primitives.Tests/Organization/SwedishOrganizationNameTests.cs)

- [Bolagsverket](https://bolagsverket.se) - Swedish Companies Registration Office

```csharp
if (SwedishOrganizationName.TryParse("  Budi   AB  ", out var name))
{
    Console.WriteLine(name.Value); // Budi AB
}

SwedishOrganizationName.IsValid("Budi AB");              // true
SwedishOrganizationName.IsValid("Al-Salam Handel AB");   // true
SwedishOrganizationName.IsValid("Café Björnen");         // true
SwedishOrganizationName.Format("  Budi  AB  ");          // "Budi AB"
SwedishOrganizationName.Normalize("  Budi  AB  ");       // "Budi AB"
```

#### SNI code

An SNI code (*SNI-kod*) is Statistics Sweden's classification code for a company's or establishment's economic activity. The most detailed SNI 2025 level uses 5 digits and is commonly displayed as `XX.XXX`.

The code is 2–5 digits forming a hierarchical classification: the first 2 digits identify the division (e.g., `62` = Computer programming), 3 digits the group, 4 digits the class, and 5 digits the subclass. Both the display and normalized form use the dot-separated canonical notation: `62.010`. Validation checks the 5-digit length and that the division (first 2 digits) is non-zero; the format is identical in SNI 2007 and SNI 2025.

Tests: [SwedishSniCodeTests.cs](test/Buildi.Primitives.Tests/Organization/SwedishSniCodeTests.cs)

- [SCB - Swedish Standard Industrial Classification (SNI)](https://www.scb.se/en/documentation/classifications-and-standards/swedish-standard-industrial-classification-sni/)
- [SCB - SNI search (SNI-Sök)](https://snisok.scb.se/en)

```csharp
if (SwedishSniCode.TryParse("62010", out var sni))
{
    Console.WriteLine(sni.Code);         // 62010
    Console.WriteLine(sni.Formatted);    // 62.010
    Console.WriteLine(sni.DivisionCode); // 62  (2-digit hierarchy level)
    Console.WriteLine(sni.GroupCode);    // 620 (3-digit hierarchy level)
    Console.WriteLine(sni.SubGroupCode); // 6201 (4-digit hierarchy level; null for 2–3 digit codes)
}

SwedishSniCode.IsValid("62.010");       // true
SwedishSniCode.Format("62010");         // "62.010"
SwedishSniCode.Normalize("62010");      // "62.010"
```

#### CFAR number

A CFAR number (*CFAR-nummer*) is Statistics Sweden's 8-digit identifier for an establishment or workplace in the Swedish business register. It is commonly used together with organization number and SNI data in company master data.

The number is exactly 8 digits with no internal structure or check digit beyond being numeric. Validation accepts any 8-digit string after stripping whitespace and non-digit characters. Normalized and display forms are identical.

Tests: [SwedishCfarNumberTests.cs](test/Buildi.Primitives.Tests/Organization/SwedishCfarNumberTests.cs)

- [SCB - CfarNr search](https://www.cfarnrsok.scb.se/)
- [SCB - Business Register variable descriptions](https://www.scb.se/vara-tjanster/bestall-data-och-statistik/foretagsregistret/variabelbeskrivning/)

```csharp
if (SwedishCfarNumber.TryParse("55667788", out var cfar))
{
    Console.WriteLine(cfar.Number); // 55667788
}

SwedishCfarNumber.IsValid("55667788");      // true
SwedishCfarNumber.Format("55667788");       // "55667788"
SwedishCfarNumber.Normalize("55667788");    // "55667788"
```

#### ELF code

An Entity Legal Form (ELF) code as defined by ISO 20275 identifies the legal form of an entity. The code is a 4-character alphanumeric identifier maintained by GLEIF. For example, `XTIQ` represents *Aktiebolag* (limited company) in Sweden. Any structurally valid 4-character alphanumeric code is accepted; known Swedish legal forms include descriptions in English and Swedish.

`Format()` returns the display name for known codes (e.g., `Aktiebolag`) or the code itself for unknown codes. `Normalize()` always returns the 4-character uppercase code.

Tests: [ElfCodeTests.cs](test/Buildi.Primitives.Tests/Organization/ElfCodeTests.cs)

- [GLEIF — ISO 20275 Entity Legal Forms Code List](https://www.gleif.org/en/about-lei/code-lists/iso-20275-entity-legal-forms-code-list)
- [ISO 20275](https://www.iso.org/standard/67462.html) — Entity Legal Forms (ELF)

```csharp
using Buildi.Primitives.Organization;

if (ElfCode.TryParse("XTIQ", out var elf))
{
    Console.WriteLine(elf.Code);          // XTIQ
    Console.WriteLine(elf.EnglishName);   // Limited company
    Console.WriteLine(elf.LocalizedName); // Aktiebolag
    Console.WriteLine(elf.IsKnown);       // true
}

ElfCode.IsValid("XTIQ");          // true
ElfCode.IsValid("ZZZZ");          // true (structurally valid)
ElfCode.Format("XTIQ");           // "Aktiebolag"
ElfCode.Format("ZZZZ");           // "ZZZZ" (unknown code)
ElfCode.Normalize("xtiq");        // "XTIQ"
```

---

### Banking

#### Swedish banking number parser

A unified parser for common banking identifiers used in Swedish payment flows. It attempts to classify an arbitrary input as Bankgiro, Plusgiro, OCR reference, BIC, IBAN, or Swedish bank account, and returns the matching typed model together with a normalized value.

Classification is attempted in priority order: IBAN → BIC → Swedish bank account → Bankgiro → Plusgiro → OCR reference. For ambiguous digit-only inputs, the parser prefers more specific Swedish payment/account types before OCR, and Bankgiro before Plusgiro for ambiguous 7–8 digit inputs. Standalone clearing numbers (4–5 digits) are not classified because they are too short and ambiguous.

Tests: [SwedishBankingNumberParserTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishBankingNumberParserTests.cs)

- [Bankgirot](https://www.bankgirot.se/) - Swedish payment infrastructure and reference formats
- [ISO 13616](https://www.iso.org/standard/81090.html) - IBAN standard

```csharp
using Buildi.Primitives.Banking;

if (SwedishBankingNumberParser.TryParse("SE45 5000 0000 0583 9825 7466", out var bankingNumber))
{
    Console.WriteLine(bankingNumber.Type);            // Iban
    Console.WriteLine(bankingNumber.NormalizedValue); // SE4550000000058398257466
    Console.WriteLine(bankingNumber.Iban!.Formatted); // SE45 5000 0000 0583 9825 7466
}

if (SwedishBankingNumberParser.TryParse("51000123456", out var domestic))
{
    Console.WriteLine(domestic.Type);                              // SwedishBankAccount
    Console.WriteLine(domestic.SwedishBankAccount!.Formatted);     // 5100-0123456
}

if (SwedishBankingNumberParser.TryParse("4779202-3", out var pg))
{
    Console.WriteLine(pg.Type);                              // SwedishPostgiroNumber
    Console.WriteLine(pg.SwedishPostgiroNumber!.Formatted);  // 4779202-3
}

if (SwedishBankingNumberParser.TryParse("NDEASESS", out var bic))
{
    Console.WriteLine(bic.Type);                    // Bic
    Console.WriteLine(bic.Bic!.Code);              // NDEASESS
}
```

For ambiguous digit-only inputs, the parser prefers more specific Swedish payment/account types before OCR. Bankgiro is also preferred before Plusgiro for ambiguous 7–8 digit-only inputs. Standalone clearing numbers (4–5 digits) are not classified by the parser because they are too short and ambiguous - use `SwedishBankClearingNumber.TryParse()` directly when you know the input is a clearing number.

#### Bankgiro

A Bankgiro number (*bankgironummer*) is a payment routing identifier in the Swedish Bankgiro system, operated by Bankgirot. It directs incoming payments to a bank account without exposing the account number itself.

The number is 7 or 8 digits. The last digit is a Luhn (mod-10) check digit. The display form places a dash before the last 4 digits: `5805-6201` (8 digits) or `234-5678` (7 digits). The normalized form is digits only without the dash.

Tests: [SwedishBankgiroNumberTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishBankgiroNumberTests.cs)

- [Bankgirot - Bankgiro number](https://www.bankgirot.se/en/services/incoming-payments/bankgiro-number/)
- [Wikipedia - Bankgirot](https://sv.wikipedia.org/wiki/Bankgirot)

```csharp
using Buildi.Primitives.Banking;

if (SwedishBankgiroNumber.TryParse("5805-6201", out var bg))
{
    Console.WriteLine(bg.Digits);                  // 58056201
    Console.WriteLine(bg.Formatted);               // 5805-6201
    Console.WriteLine(bg.ToDisplayString());       // Bankgiro 5805-6201
    Console.WriteLine(bg.ToShortDisplayString());  // BG 5805-6201
}

// Display name constants - useful for labels and UI
Console.WriteLine(SwedishBankgiroNumber.DisplayName);      // Bankgiro
Console.WriteLine(SwedishBankgiroNumber.DisplayNameShort); // BG

SwedishBankgiroNumber.IsValid("5805-6201");       // true
SwedishBankgiroNumber.Format("58056201");         // "5805-6201"
SwedishBankgiroNumber.Normalize("58056201");       // "5805-6201"
```

#### Plusgiro

A Plusgiro number (*plusgironummer*) is a payment identifier in the Swedish Plusgiro system, originally operated by the postal service and now managed by Bankgirot. Numbers are 2–8 digits with a Luhn check digit.

The last digit is a Luhn (mod-10) check digit. The display form places a dash before the last digit: `1234567-9`. The normalized form is digits only without the dash.

Tests: [SwedishPostgiroNumberTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishPostgiroNumberTests.cs)

- [Bankgirot](https://www.bankgirot.se/)
- [Wikipedia - Plusgirot](https://sv.wikipedia.org/wiki/Plusgirot)

```csharp
if (SwedishPostgiroNumber.TryParse("1234567-9", out var pg))
{
    Console.WriteLine(pg.Digits);                  // 12345679
    Console.WriteLine(pg.Formatted);               // 1234567-9
    Console.WriteLine(pg.ToDisplayString());       // Plusgiro 1234567-9
    Console.WriteLine(pg.ToShortDisplayString());  // PG 1234567-9
}

// Display name constants - useful for labels and UI
Console.WriteLine(SwedishPostgiroNumber.DisplayName);      // Plusgiro
Console.WriteLine(SwedishPostgiroNumber.DisplayNameShort); // PG

SwedishPostgiroNumber.IsValid("1234567-9");       // true
SwedishPostgiroNumber.Format("12345679");         // "1234567-9"
SwedishPostgiroNumber.Normalize("12345679");       // "1234567-9"
```

#### OCR reference number

An OCR reference number (*OCR-nummer* / *OCR-referens*) is the numeric reference commonly used in Swedish payment flows to reconcile incoming payments. Bankgirot supports MOD10 check-digit references as well as agreement-specific variants such as variable length digit and fixed-length control.

The reference is 2–25 digits. The last digit is a Luhn (mod-10) check digit. Bankgirot defines optional agreement-specific variants: *variable-length digit* adds a length-encoding check digit in the second-to-last position, and *fixed-length* constrains the reference to an agreed number of digits. Normalized and display forms are identical - digits only.

Tests: [SwedishOcrReferenceNumberTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishOcrReferenceNumberTests.cs)

- [Bankgirot - OCR reference control](https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/ocr-reference-control/)
- [Bankgirot - Technical information](https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/technical-information/)

```csharp
var basic = SwedishOcrReferenceNumber.Parse("123455");
Console.WriteLine(basic.Value);             // 123455
Console.WriteLine(basic.Length);            // 6
Console.WriteLine(basic.CheckDigit);        // 5
Console.WriteLine(basic.ToDisplayString()); // OCR 123455
Console.WriteLine(basic.ToShortDisplayString()); // OCR 123455

SwedishOcrReferenceNumber.IsValid("123455");       // true
SwedishOcrReferenceNumber.Format("123455");        // "123455"
SwedishOcrReferenceNumber.Normalize("123455");     // "123455"

// Variable-length digit variant - LengthDigit encodes the total reference length
SwedishOcrReferenceNumber.TryParseVariableLengthDigit("12345682", out var variable);
Console.WriteLine(variable!.LengthDigit);  // 8 (the length-encoding digit)
Console.WriteLine(variable.Length);        // 8

// Fixed-length variant - rejects references of any other length
SwedishOcrReferenceNumber.TryParse("123455", OcrReferenceOptions.FixedLength(6), out var fixedLength);
SwedishOcrReferenceNumber.TryParse("123455", OcrReferenceOptions.FixedLength([6, 8]), out var fixedLengths);
```

#### BIC

A BIC (*Business Identifier Code* / SWIFT code) identifies a financial institution in international payment messaging. It is often used together with IBAN for cross-border payments and incoming international transfers to Swedish bank accounts.

A BIC is 8 or 11 alphanumeric characters in the form `BBBBCCLL[BBB]`: 4-character institution code, 2-letter ISO country code, 2-character location code, and an optional 3-character branch code (defaults to `XXX` for the head office). No checksum is applied. The normalized form is uppercase without spaces.

Tests: [BicTests.cs](test/Buildi.Primitives.Tests/Banking/BicTests.cs)

- [ISO 9362](https://www.iso.org/cms/render/live/en/sites/isoorg/contents/data/standard/08/41/84108.html) - BIC standard
- [SWIFT - Business Identifier Code (BIC)](https://www.swift.com/standards/data-standards/bic-business-identifier-code)

```csharp
if (Bic.TryParse("NDEASESSXXX", out var bic))
{
    Console.WriteLine(bic.Code);            // NDEASESSXXX
    Console.WriteLine(bic.InstitutionCode); // NDEA
    Console.WriteLine(bic.CountryCode);     // SE
    Console.WriteLine(bic.LocationCode);    // SS
    Console.WriteLine(bic.BranchCode);      // XXX
}

Bic.IsValid("NDEASESS");            // true
Bic.Format("ndea sess");            // "NDEASESS"
Bic.Normalize("NDEASESSXXX");       // "NDEASESSXXX"
```

#### IBAN

The International Bank Account Number (IBAN) is an internationally agreed system for identifying bank accounts across borders, defined by ISO 13616. Validation uses MOD-97 check digits as specified in the standard.

An IBAN starts with a 2-letter ISO country code, followed by 2 check digits, and up to 30 alphanumeric characters forming the Basic Bank Account Number (BBAN). Total length is country-specific (e.g., 24 characters for Sweden). The check digits are validated using ISO 7064 MOD-97-10. The display form groups characters in blocks of four: `SE45 5000 0000 0583 9825 7466`. The normalized form is uppercase without spaces.

Tests: [IbanTests.cs](test/Buildi.Primitives.Tests/Banking/IbanTests.cs)

- [ISO 13616](https://www.iso.org/standard/81090.html) - IBAN standard
- [IBAN.com](https://www.iban.com/) - country formats and validation
- [Wikipedia - International Bank Account Number](https://en.wikipedia.org/wiki/International_Bank_Account_Number)

```csharp
if (Iban.TryParse("SE4550000000058398257466", out var iban))
{
    Console.WriteLine(iban.Value);                    // SE4550000000058398257466
    Console.WriteLine(iban.CountryCode);              // SE
    Console.WriteLine(iban.Formatted);                // SE45 5000 0000 0583 9825 7466
    Console.WriteLine(iban.ToDisplayString());        // IBAN SE45 5000 0000 0583 9825 7466
    Console.WriteLine(iban.ToShortDisplayString());   // IBAN SE45 5000 0000 0583 9825 7466
}

// Display name constants - useful for labels and UI
Console.WriteLine(Iban.DisplayName);       // IBAN
Console.WriteLine(Iban.DisplayNameShort);  // IBAN

// Parse a number requiring a specific country - rejects IBANs from other countries
Iban.TryParse("SE4550000000058398257466", "SE", out var seIban); // true
Iban.TryParse("SE4550000000058398257466", "DE", out _);          // false - not a German IBAN

Iban.IsValid("SE4550000000058398257466");       // true
Iban.Format("SE4550000000058398257466");        // "SE45 5000 0000 0583 9825 7466"
Iban.Normalize("SE45 5000 0000 0583 9825 7466"); // "SE4550000000058398257466"

// Structured validation
var ibanResult = Iban.Validate("XX1234567890");
Console.WriteLine(ibanResult.IsValid);                    // false
Console.WriteLine(ibanResult.Issues[0].Reason);           // UnknownCountryCode
Console.WriteLine(ibanResult.Issues[0].EnglishDescription); // "Unknown IBAN country code."
```

#### Clearing number

A Swedish clearing number (*clearingnummer*) identifies a bank and branch in the Swedish payments infrastructure. Most clearing numbers are 4 digits; Swedbank's series 8 uses 5 digits. Parsing resolves the associated bank when the clearing number falls within a known range.

Each bank is assigned specific clearing number ranges by BSAB - for example, 5000–5999 is SEB, 6000–6999 is Handelsbanken, and 8000–8999 is Swedbank. No check digit is applied. The display form for 5-digit Swedbank numbers uses a dash: `8123-4`. The normalized form is digits only.

Tests: [SwedishBankClearingNumberTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishBankClearingNumberTests.cs)

- [BSAB - Clearingnummer](https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable)
- [Wikipedia - Clearingnummer](https://sv.wikipedia.org/wiki/Clearingnummer)

```csharp
if (SwedishBankClearingNumber.TryParse("5001", out var clearing))
{
    Console.WriteLine(clearing.Digits);    // 5001
    Console.WriteLine(clearing.Bank);      // SwedishBank.SEB
    Console.WriteLine(clearing.BankName);  // SEB
}

SwedishBankClearingNumber.IsValid("81234");       // true (Swedbank 5-digit)
SwedishBankClearingNumber.Format("81234");        // "8123-4"
SwedishBankClearingNumber.Normalize("8123-4");    // "81234"

// Structured validation
var result = SwedishBankClearingNumber.Validate("9999");
Console.WriteLine(result.IsValid);                    // false
Console.WriteLine(result.Issues[0].Reason);           // UnknownClearingRange
Console.WriteLine(result.Issues[0].EnglishDescription); // "Clearing number does not match any known bank."
```

#### Swedish bank account

A Swedish bank account (*bankkonto*) is identified by a clearing number (4–5 digits indicating bank and branch) followed by an account number. Clearing number ranges and account length rules are defined by BSAB. The account exposes a typed `Clearing` property as a `SwedishBankClearingNumber` instance.

Total length (clearing + account) varies by bank, typically 11–15 digits. The valid account number length is bank-specific - for example, SEB uses 7 digits after the 4-digit clearing number while Swedbank uses 10 digits after the 5-digit clearing. Validation checks that the clearing number maps to a known bank and that the total digit count matches that bank's rules. The display form uses a dash after the clearing number: `5100-0123456`.

Tests: [SwedishBankAccountTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishBankAccountTests.cs)

- [BSAB - Clearingnummer](https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable)
- [Wikipedia - Clearingnummer](https://sv.wikipedia.org/wiki/Clearingnummer)

```csharp
if (SwedishBankAccount.TryParse("51000123456", out var acct))
{
    Console.WriteLine(acct.ClearingNumber);       // 5100
    Console.WriteLine(acct.Clearing.Bank);        // SwedishBank.SEB
    Console.WriteLine(acct.Clearing.BankName);    // SEB
    Console.WriteLine(acct.AccountNumber);        // 0123456
    Console.WriteLine(acct.BankName);             // SEB
    Console.WriteLine(acct.Bank);                 // SwedishBank.SEB
    Console.WriteLine(acct.Formatted);            // 5100-0123456
}

SwedishBankAccount.IsValid("51000123456");       // true
SwedishBankAccount.Format("51000123456");        // "5100-0123456"
SwedishBankAccount.Normalize("5100-0123456");    // "51000123456"
```

#### Bank account holder name

A bank account holder name (*kontoinnehavare*) as it appears in Swedish banking systems such as Bankgirot and SUS. Holder names are often transmitted in ALL CAPS. This type auto-detects whether the holder is a person or an organization based on Swedish corporate suffix heuristics (AB, HB, KB, BRF, etc.) and keywords (kommun, stiftelse, förening). Validation delegates to `PersonFullName` for person names and `SwedishOrganizationName` for organization names. All-caps input is automatically normalized to title case, with known abbreviations (AB, HB, etc.) preserved in uppercase.

Tests: [SwedishBankAccountHolderNameTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishBankAccountHolderNameTests.cs)

```csharp
using Buildi.Primitives.Banking;

if (SwedishBankAccountHolderName.TryParse("VOLVO AB", out var holder))
{
    Console.WriteLine(holder.Value);              // "Volvo AB"
    Console.WriteLine(holder.HolderType);         // Organization
    Console.WriteLine(holder.OrganizationName);   // SwedishOrganizationName { Value = "Volvo AB" }
    Console.WriteLine(holder.PersonName);         // null
}

if (SwedishBankAccountHolderName.TryParse("ANNA MARIA ANDERSSON", out var person))
{
    Console.WriteLine(person.Value);              // "Anna Maria Andersson"
    Console.WriteLine(person.HolderType);         // Person
    Console.WriteLine(person.PersonName!.FamilyName.Value); // "Andersson"
}

SwedishBankAccountHolderName.IsValid("BRF Solgläntan");    // true
SwedishBankAccountHolderName.Format("ICA GRUPPEN AB");     // "Ica Gruppen AB"
SwedishBankAccountHolderName.Format("anna andersson");     // "Anna Andersson"
```

#### Swish number

A Swish payment number (*Swish-nummer*) used on the Swedish Swish mobile payment platform. The type accepts three forms: corporate Swish 123 numbers (10 digits starting with `123`), 90-numbers (7-digit short form used by Insamlingskontroll/90-konto charities, expanded to `123` + 7 digits), and Swedish mobile phone numbers for private Swish. Properties expose the number type (`IsSwish123`, `Is90Number`, `IsMobileNumber`) and the underlying `PhoneNumber` when applicable.

Tests: [SwedishSwishNumberTests.cs](test/Buildi.Primitives.Tests/Banking/SwedishSwishNumberTests.cs)

- [Swish FAQ — Number format](https://www.swish.nu/faq/company/what-is-the-format-of-the-swish-number)
- [cisene/swish-123](https://github.com/cisene/swish-123) — community catalog of Swish 123 numbers
- [Insamlingskontroll](https://www.insamlingskontroll.se/) — 90-konto organizations

```csharp
using Buildi.Primitives.Banking;

// Corporate Swish 123 number
if (SwedishSwishNumber.TryParse("1236652895", out var swish))
{
    Console.WriteLine(swish.Value);        // "1236652895"
    Console.WriteLine(swish.Formatted);    // "123-665 28 95"
    Console.WriteLine(swish.IsSwish123);   // true
    Console.WriteLine(swish.Is90Number);   // false
    Console.WriteLine(swish.IsMobileNumber); // false
}

// 90-number (Insamlingskontroll charity)
if (SwedishSwishNumber.TryParse("902 00 33", out var charity))
{
    Console.WriteLine(charity.Value);      // "1239020033"
    Console.WriteLine(charity.Formatted);  // "902 00 33"
    Console.WriteLine(charity.Is90Number); // true
}

// Mobile phone number (private Swish)
if (SwedishSwishNumber.TryParse("0701234567", out var mobile))
{
    Console.WriteLine(mobile.IsMobileNumber); // true
    Console.WriteLine(mobile.PhoneNumber);    // 070-123 45 67
}

SwedishSwishNumber.IsValid("1236652895");           // true
SwedishSwishNumber.Format("9020033");               // "902 00 33"
SwedishSwishNumber.Normalize("123-665 28 95");      // "1236652895"
```

---

### Property

#### Swedish property designation

A Swedish property designation (*fastighetsbeteckning*) is the official identifier for a property in the Swedish real property register. It combines the municipality and tract or quarter name with a register number such as `75:2`, which together uniquely identify the property.

The designation consists of a name part (municipality and tract/quarter) followed by a register number in the form `block:unit` - e.g., `Stockholm Söder 75:2`. Validation requires at least a name and a colon-separated register number. Normalization applies Swedish title case to the name and collapses whitespace. ASCII fallback for Swedish characters is accepted during parsing (e.g., `Soder` for `Söder`).

Tests: [SwedishPropertyDesignationTests.cs](test/Buildi.Primitives.Tests/Property/SwedishPropertyDesignationTests.cs)

- [Lantmateriet - Find property designation](https://www.lantmateriet.se/sv/kartor/vara-karttjanster/min-karta/hitta-fastighetsbeteckning-i-min-karta/)
- [Lantmateriet - Property register contents](https://www2.lantmateriet.se/sv/fastighet-och-mark/information-om-fastigheter/Fastighetsregistret/registrets-innehall/)
- [Wikipedia - Fastighetsbeteckning](https://sv.wikipedia.org/wiki/Fastighetsbeteckning)

```csharp
using Buildi.Primitives.Property;

if (SwedishPropertyDesignation.TryParse("Stockholm Söder 75:2", out var designation))
{
    Console.WriteLine(designation.Value);             // Stockholm Söder 75:2
    Console.WriteLine(designation.DesignationName);   // Stockholm Söder
    Console.WriteLine(designation.RegisterNumber);    // 75:2
    Console.WriteLine(designation.BlockNumber);       // 75
    Console.WriteLine(designation.UnitNumber);        // 2
}

SwedishPropertyDesignation.IsValid("Gävle Olsbacka 11:1");      // true
SwedishPropertyDesignation.Format(" stockholm söder 75 : 2 ");   // "Stockholm Söder 75:2"
SwedishPropertyDesignation.Normalize("Stockholm Söder 75:2");    // "Stockholm Söder 75:2"

// ASCII fallback is accepted - the parser uses Unicode-aware matching
SwedishPropertyDesignation.IsValid("Stockholm Soder 75:2");      // true
```

#### Property taxation code

A Swedish property taxation code (*typkod*) is a 3-digit numeric code assigned by Skatteverket that classifies a property unit for tax assessment purposes. The first digit identifies the main property category: 1 = agricultural, 2 = small house, 3 = rental property, 4 = industrial, 5 = quarry, 6 = power production, 7 = special/exempt, 8 = other. Known codes include descriptions in both English and Swedish.

`Format()` returns the Swedish description for known codes (e.g., `Småhusenhet, bebyggd`) or the code itself for unknown codes. `Normalize()` returns the 3-digit code.

Tests: [SwedishPropertyTaxationCodeTests.cs](test/Buildi.Primitives.Tests/Property/SwedishPropertyTaxationCodeTests.cs)

- [Skatteverket — Fastighetstaxering](https://www.skatteverket.se/privat/fastigheterochbostad/fastighetstaxering.4.69ef368911e1304a625800013531.html)
- [Wikipedia — Typkod (fastighet)](https://sv.wikipedia.org/wiki/Typkod_(fastighet))

```csharp
using Buildi.Primitives.Property;

if (SwedishPropertyTaxationCode.TryParse("220", out var taxCode))
{
    Console.WriteLine(taxCode.Code);                 // 220
    Console.WriteLine(taxCode.NumericCode);           // 220
    Console.WriteLine(taxCode.Category);              // Smahusenhet
    Console.WriteLine(taxCode.EnglishDescription);    // Small house unit, developed
    Console.WriteLine(taxCode.LocalizedDescription);  // Småhusenhet, bebyggd
    Console.WriteLine(taxCode.IsKnown);               // true
}

SwedishPropertyTaxationCode.IsValid("220");      // true
SwedishPropertyTaxationCode.IsValid("199");      // true (valid structure, unknown code)
SwedishPropertyTaxationCode.Format("220");       // "Småhusenhet, bebyggd"
SwedishPropertyTaxationCode.Format("199");       // "199" (unknown code)
SwedishPropertyTaxationCode.Normalize("220");    // "220"
```

---

### Geography

#### Swedish county

A Swedish county (*län*) is one of SCB's 21 official county divisions. This type lets you work with both the official 2-digit county code and the county name, and parse by either code or name.

Sweden has 21 counties, each identified by a 2-digit code (`01`–`25`, with gaps). Parsing accepts the code, the Swedish name (e.g., `Stockholms län`), or the English name (e.g., `Stockholm County`). `Format()` returns the Swedish name; `Normalize()` returns the 2-digit code.

Tests: [SwedishCountyTests.cs](test/Buildi.Primitives.Tests/Geography/SwedishCountyTests.cs)

- [SCB - Counties and municipalities](https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/)
- [SCB - Counties and municipalities in numerical order](https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/)

```csharp
using Buildi.Primitives.Geography;

if (SwedishCounty.TryParse("01", out var county))
{
    Console.WriteLine(county.Code);        // 01
    Console.WriteLine(county.Name);        // Stockholms län
    Console.WriteLine(county.EnglishName); // Stockholm County
}

SwedishCounty.IsValid("Stockholms län");   // true
SwedishCounty.Format("01");                // "Stockholms län"
SwedishCounty.Normalize("Stockholm County"); // "01"

// Get all municipalities in a county
var municipalities = county.GetMunicipalities(); // IReadOnlyList<SwedishMunicipality>

// Get adjacent counties (counties that share a land border)
var neighbors = county.GetAdjacentCounties(); // e.g. for "01": Uppsala, Södermanland
```

#### Swedish municipality

A Swedish municipality (*kommun*) is one of SCB's official municipality divisions. This type lets you work with both the official 4-digit municipality code and the municipality name, and also exposes the related county.

Sweden has 290 municipalities, each identified by a 4-digit code where the first 2 digits correspond to the county code (e.g., `0180` = Stockholm municipality in county `01`). Parsing accepts both the code and the name. The related `SwedishCounty` is accessible via the `County` property. `Format()` returns the name; `Normalize()` returns the 4-digit code.

Tests: [SwedishMunicipalityTests.cs](test/Buildi.Primitives.Tests/Geography/SwedishMunicipalityTests.cs)

- [SCB - Counties and municipalities in numerical order](https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/)

```csharp
if (SwedishMunicipality.TryParse("0180", out var municipality))
{
    Console.WriteLine(municipality.Code);        // 0180
    Console.WriteLine(municipality.Name);        // Stockholm
    Console.WriteLine(municipality.CountyCode);  // 01
    Console.WriteLine(municipality.County.Name); // Stockholms län
}

SwedishMunicipality.IsValid("Stockholm");     // true
SwedishMunicipality.Format("0180");           // "Stockholm"
SwedishMunicipality.Normalize("Stockholm");   // "0180"

// Get adjacent municipalities (municipalities that share a land border)
// Island municipalities with no land borders (e.g. Ekerö, Gotland) return an empty list.
var adjacent = municipality.GetAdjacentMunicipalities();
```

#### Country

A country identified by its English name, Swedish name, native name (endonym), or ISO 3166-1 codes. Each country exposes its phone calling code, country-code top-level domain (ccTLD), continent, official languages, country-based European classifications (EU, EEA, Schengen, Nordics, SEPA), currency metadata, geographic coordinates, capital city, and land-border neighbors. `Currency` returns the primary legal-tender currency; `Currencies` returns all of them - for the vast majority of countries these are identical, but countries with multiple legal tenders (e.g. Cuba, Panama) will have more than one entry. `PrimaryLanguage` returns the main official language; `OfficialLanguages` returns all of them (e.g. Belgium has Dutch, French, and German). `Capital` returns the capital city with English, Swedish, and native names plus its geographic coordinates. `Coordinate` returns the country's representative geographic point (WGS 84). `FindNeighbors(radiusKm)` returns all countries within a given radius using Haversine distance. `GetBorderingCountries()` returns countries sharing a land border. Parsing accepts English names, Swedish names (e.g. "Tyskland", "Frankrike"), native names (e.g. "Deutschland"), alpha-2/alpha-3 codes, and common aliases (e.g. "USA", "UK", "Holland"). The `Continent` property is typed as the `Continent` class in `Buildi.Primitives.Geography`.

The library includes approximately 250 countries and territories aligned with ISO 3166-1. Each country exposes: alpha-2 code (`SE`), alpha-3 code (`SWE`), numeric code (`752`), phone calling code (`46`), ccTLD (`.se`), continent enum and names, geographic coordinates, capital city (with multilingual names and coordinates), and boolean classification flags for EU (27 members), EEA (30), Schengen (29), Nordics (5), and SEPA (44 modeled countries and territories, including the Crown dependencies represented separately in `Country`). `Format()` returns the Swedish name; `Normalize()` returns the alpha-2 code. Lookup by phone calling code is also supported via `TryFindByCallingCode()`.

Tests: [CountryTests.cs](test/Buildi.Primitives.Tests/Geography/CountryTests.cs) · [GeographyExtensionsTests.cs](test/Buildi.Primitives.Tests/Geography/GeographyExtensionsTests.cs)

- [ISO 3166-1](https://www.iso.org/iso-3166-country-codes.html) - country code standard followed by this type
- [ITU-T E.164](https://www.itu.int/rec/T-REC-E.164/) - country calling codes
- [IANA - ccTLD list](https://www.iana.org/domains/root/db)
- [Wikidata P625](https://www.wikidata.org/wiki/Property:P625) — geographic coordinates for each country and capital
- [Wikidata P36](https://www.wikidata.org/wiki/Property:P36) — capital city data
- [Wikidata P47](https://www.wikidata.org/wiki/Property:P47) — land borders between countries

```csharp
using Buildi.Primitives.Geography;

if (Country.TryParse("Tyskland", out var country))
{
    Console.WriteLine(country.EnglishName);          // Germany
    Console.WriteLine(country.SwedishName);          // Tyskland
    Console.WriteLine(country.NativeName);            // Deutschland
    Console.WriteLine(country.Alpha2Code);           // DE
    Console.WriteLine(country.Alpha3Code);           // DEU
    Console.WriteLine(country.NumericCode);          // 276
    Console.WriteLine(country.CallingCode);          // 49
    Console.WriteLine(country.TopLevelDomain);       // .de
    Console.WriteLine(country.Continent);            // Europe
    Console.WriteLine(country.ContinentCode);        // EU
    Console.WriteLine(country.ContinentEnglishName); // Europe
    Console.WriteLine(country.ContinentSwedishName); // Europa
    Console.WriteLine(country.IsInEuropeanUnion);    // True
    Console.WriteLine(country.IsInEea);              // True
    Console.WriteLine(country.IsInSchengen);         // True
    Console.WriteLine(country.IsInNordics);          // False
    Console.WriteLine(country.IsInSepa);             // True
    Console.WriteLine(country.CurrencyCode);         // EUR
    Console.WriteLine(country.Currency?.SwedishName); // Euro
    Console.WriteLine(country.UsesEuro);             // True
}

// Parses English, Swedish, local names, codes, and aliases
Country.TryParse("Germany", out _);       // true (English name)
Country.TryParse("Tyskland", out _);      // true (Swedish name)
Country.TryParse("Deutschland", out _);   // true (native name)
Country.TryParse("DE", out _);            // true (alpha-2)
Country.TryParse("DEU", out _);           // true (alpha-3)
Country.TryParse("USA", out _);           // true (alias)

Country.IsValid("Sverige");               // true
Country.Format("SE");                     // "Sverige"
Country.Normalize("Tyskland");            // "DE"

// Geographic coordinates (Wikidata P625)
Console.WriteLine(country.Coordinate);       // 51°N, 9°E
Console.WriteLine(country.Latitude);         // 51
Console.WriteLine(country.Longitude);        // 9

// Capital city (Wikidata P36/P625)
Console.WriteLine(country.Capital?.EnglishName);     // Berlin
Console.WriteLine(country.Capital?.LocalizedName);   // Berlin
Console.WriteLine(country.Capital?.NativeName);      // Berlin
Console.WriteLine(country.Capital?.Coordinate);      // 52.52°N, 13.41°E

// Land-border neighbors (Wikidata P47)
var borders = country.GetBorderingCountries();       // AT, BE, CZ, DK, FR, LU, NL, PL, CH

// Distance-based neighbors (Haversine)
var nearby = country.FindNeighbors(500);             // countries within 500 km

// Enumerate all known countries
foreach (var c in Country.All)
    Console.WriteLine($"{c.Alpha2Code}: {c.EnglishName} ({c.Capital?.EnglishName})");

// Countries by continent
var european = Continent.Europe.GetCountries();

// Static constants for common Nordic countries
var se = Country.Sweden;   // SE
var no = Country.Norway;   // NO
var fi = Country.Finland;  // FI
var dk = Country.Denmark;  // DK
```

#### Geo coordinate

A geographic coordinate pair (*geografisk koordinat*) representing latitude and longitude in the WGS 84 reference system. Parsing accepts decimal degrees (`59.3293, 18.0686`), decimal degrees with cardinal directions (`59.3293°N, 18.0686°E`), DMS (`59°19'45.5"N, 18°4'7"E`), and DDM (`59°19.758'N, 18°4.116'E`) formats. Swedish cardinal letters Ö (east) and V (west) are accepted alongside E and W. `Format()` returns cardinal-direction form; `Normalize()` returns signed decimal degrees. Includes Haversine distance calculation between coordinates.

Tests: [GeoCoordinateTests.cs](test/Buildi.Primitives.Tests/Geography/GeoCoordinateTests.cs)

- [Wikipedia — Geographic coordinate system](https://en.wikipedia.org/wiki/Geographic_coordinate_system)
- [EPSG:4326](https://epsg.io/4326) — WGS 84 coordinate reference system

```csharp
using Buildi.Primitives.Geography;

if (GeoCoordinate.TryParse("59.3293, 18.0686", out var coord))
{
    Console.WriteLine(coord.Latitude);           // 59.3293
    Console.WriteLine(coord.Longitude);          // 18.0686
    Console.WriteLine(coord.LatitudeDirection);   // N
    Console.WriteLine(coord.LongitudeDirection);  // E
    Console.WriteLine(coord.ToString());          // 59.3293°N, 18.0686°E
    Console.WriteLine(coord.ToNormalizedString()); // 59.3293, 18.0686
    Console.WriteLine(coord.ToDmsString());       // 59°19'45.5"N, 18°4'7"E
}

// Parses many common formats
GeoCoordinate.TryParse("59.3293, 18.0686", out _);          // true (decimal degrees)
GeoCoordinate.TryParse("59.3293 18.0686", out _);           // true (space-separated)
GeoCoordinate.TryParse("(59.3293, 18.0686)", out _);        // true (parenthesized)
GeoCoordinate.TryParse("59.3293°N, 18.0686°E", out _);     // true (cardinal suffix)
GeoCoordinate.TryParse("N 59.3293, E 18.0686", out _);     // true (cardinal prefix)
GeoCoordinate.TryParse("59°19'45.5\"N, 18°4'7\"E", out _); // true (DMS)
GeoCoordinate.TryParse("59°19.758'N, 18°4.116'E", out _);  // true (DDM)
GeoCoordinate.TryParse("59.3293°N, 18.0686°Ö", out _);     // true (Swedish Ö = East)

GeoCoordinate.IsValid("-33.8688, 151.2093");  // true
GeoCoordinate.Format("59.3293, 18.0686");     // "59.3293°N, 18.0686°E"
GeoCoordinate.Normalize("59.3293°N, 18.0686°E"); // "59.3293, 18.0686"

// Distance calculation (Haversine)
var stockholm = GeoCoordinate.Create(59.3293, 18.0686);
var gothenburg = GeoCoordinate.Create(57.7089, 11.9746);
Console.WriteLine(stockholm.DistanceTo(gothenburg)); // ~398 km
```

#### Language

A language (*språk*) identified by its ISO 639-1 two-letter code, ISO 639-2/T three-letter code, English name, Swedish name, or native name (endonym). Each language exposes its writing script and text direction (LTR/RTL). The `Country` type links to languages via `PrimaryLanguage` and `OfficialLanguages` properties.

The library includes all ISO 639-1 languages (~184 entries). `Format()` returns the display name in the current UI language; `Normalize()` returns the ISO 639-1 two-letter code. Static named properties are provided for commonly used languages (e.g. `Language.Swedish`, `Language.English`, `Language.German`).

Tests: [LanguageTests.cs](test/Buildi.Primitives.Tests/Geography/LanguageTests.cs)

- [ISO 639-2 — Library of Congress](https://www.loc.gov/standards/iso639-2/php/code_list.php) — code list with English names
- [Wikipedia — ISO 639-1 codes](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) — codes, names, native names, scripts

```csharp
using Buildi.Primitives.Geography;

if (Language.TryParse("de", out var lang))
{
    Console.WriteLine(lang.EnglishName);    // German
    Console.WriteLine(lang.LocalizedName);  // Tyska
    Console.WriteLine(lang.NativeName);     // Deutsch
    Console.WriteLine(lang.Alpha2Code);     // de
    Console.WriteLine(lang.Alpha3Code);     // deu
    Console.WriteLine(lang.Script);         // Latin
    Console.WriteLine(lang.Direction);      // LeftToRight
}

// Parses codes, English, Swedish, and native names
Language.TryParse("sv", out _);         // true (ISO 639-1)
Language.TryParse("swe", out _);        // true (ISO 639-2/T)
Language.TryParse("Swedish", out _);    // true (English)
Language.TryParse("Svenska", out _);    // true (Swedish)
Language.TryParse("Deutsch", out _);    // true (native name)

Language.Format("sv");                  // "Svenska" (or "Swedish" depending on UI culture)
Language.Normalize("Swedish");          // "sv"

// Static named properties
var sv = Language.Swedish;
var ar = Language.Arabic;
Console.WriteLine(ar.Direction);        // RightToLeft
Console.WriteLine(ar.Script);          // Arabic

// Country integration
Console.WriteLine(Country.Sweden.PrimaryLanguage);         // Swedish
Console.WriteLine(Country.Belgium.OfficialLanguages.Count); // 3 (Dutch, French, German)
Console.WriteLine(Country.Switzerland.OfficialLanguages.Count); // 4 (German, French, Italian, Romansh)
```

---

### Finance

#### Currency

An ISO 4217 currency identified by its three-letter code. Parsing accepts ISO codes (e.g. "SEK", "EUR") and common display names in English or Swedish (e.g. "Swedish krona", "Svensk krona"). Each currency exposes its English name, Swedish name, symbol, and the number of minor-unit decimal places. `Country` exposes typed `Currency` and `Currencies` properties backed by this type.

The 3-letter ISO 4217 code is the canonical identifier (e.g., `SEK`, `EUR`, `USD`, `JPY`). Each currency includes the number of decimal places for its minor unit (e.g., 2 for SEK, 0 for JPY). The library includes approximately 150 currencies. `Format()` returns the Swedish display name; `Normalize()` returns the ISO code.

Tests: [CurrencyTests.cs](test/Buildi.Primitives.Tests/Finance/CurrencyTests.cs)

- [ISO 4217](https://www.iso.org/iso-4217-currency-codes.html) - currency codes standard
- [Wikipedia - ISO 4217](https://en.wikipedia.org/wiki/ISO_4217)

```csharp
using Buildi.Primitives.Finance;
using Buildi.Primitives.Geography;

if (Currency.TryParse("SEK", out var currency))
{
    Console.WriteLine(currency.Code);          // SEK
    Console.WriteLine(currency.EnglishName);   // Swedish krona
    Console.WriteLine(currency.SwedishName);   // Svensk krona
    Console.WriteLine(currency.Symbol);        // kr
    Console.WriteLine(currency.DecimalPlaces); // 2
}

Currency.TryParse("Svensk krona", out _);     // true (Swedish name)
Currency.TryParse("Euro", out _);             // true (English/Swedish name)
Currency.IsValid("JPY");                       // true
Currency.Format("USD");                        // "Amerikansk dollar"
Currency.Normalize("Brittiskt pund");          // "GBP"

// Enumerate all known currencies
foreach (var c in Currency.All)
    Console.WriteLine($"{c.Code}: {c.SwedishName} ({c.Symbol})");

// Accessed from a country
var sweden = Country.Parse("SE");
Console.WriteLine(sweden.Currency?.Code);      // SEK
Console.WriteLine(sweden.Currency?.Symbol);    // kr

// Static constants for common currencies
var sek = Currency.SEK;  // Swedish krona
var eur = Currency.EUR;  // Euro
var usd = Currency.USD;  // United States dollar
var nok = Currency.NOK;  // Norwegian krone
var dkk = Currency.DKK;  // Danish krone
var gbp = Currency.GBP;  // Pound sterling
```

#### Money amount

A monetary amount composed of a decimal value and a currency. Parsing handles a wide range of everyday formats including prefix/suffix currency codes or symbols, thousands separators (space, period, comma), and various decimal separators. When the input does not contain a recognizable currency indicator, a fallback currency can be supplied.

The parser recognizes prefix and suffix currency codes (`SEK 1000`, `1000 SEK`), symbols (`$500`, `€1 000`), abbreviations (`kr`, `$`, `€`), European-style number formatting (comma as decimal, space or period as thousands separator), and US-style formatting (period as decimal, comma as thousands separator). `Format()` returns Swedish formatting with space as thousands separator and comma as decimal: `1 000,50 SEK`. `Normalize()` returns machine-readable form with period as decimal: `1000.50 SEK`.

Tests: [MoneyAmountTests.cs](test/Buildi.Primitives.Tests/Finance/MoneyAmountTests.cs)

- [ISO 4217](https://www.iso.org/iso-4217-currency-codes.html) - currency codes standard

```csharp
using Buildi.Primitives.Finance;

// Parse with currency in the string
if (MoneyAmount.TryParse("1 000,50 SEK", out var money))
{
    Console.WriteLine(money.Amount);              // 1000.50
    Console.WriteLine(money.Currency.Code);       // SEK
    Console.WriteLine(money.Currency.SwedishName); // Svensk krona
    Console.WriteLine(money.ToString());          // 1 000,50 SEK
    Console.WriteLine(money.ToNormalizedString()); // 1000.50 SEK
}

// Symbols work too
MoneyAmount.TryParse("$500", out var usdMoney);       // Amount=500, Currency=USD
MoneyAmount.TryParse("€1 000,00", out var eurMoney);  // Amount=1000, Currency=EUR
MoneyAmount.TryParse("1000 kr", out var krMoney);     // Amount=1000, Currency=SEK

// Various number formats
MoneyAmount.TryParse("1.000,50 EUR", out _);  // European: 1000.50 EUR
MoneyAmount.TryParse("1,000.50 USD", out _);  // US: 1000.50 USD
MoneyAmount.TryParse("SEK 1000", out _);      // Prefix: 1000 SEK
MoneyAmount.TryParse("USD500", out _);         // No space: 500 USD

// Fallback currency when not in the string
MoneyAmount.TryParse("1 000,50", Currency.SEK, out var fallback); // 1000.50 SEK

// Create directly
var price = MoneyAmount.Create(299.00m, Currency.SEK);

// Static shorthands
MoneyAmount.IsValid("500 SEK");            // true
MoneyAmount.IsValid("500", Currency.SEK);  // true (with fallback)
MoneyAmount.IsValid("500");                // false (no currency)
MoneyAmount.Format("1000 SEK");            // "1 000,00 SEK"
MoneyAmount.Normalize("1 000,50 SEK");     // "1000.50 SEK"
```

#### ISIN

An International Securities Identification Number (ISIN) as defined by ISO 6166 is a 12-character alphanumeric code that uniquely identifies a financial security. The first two characters are an ISO 3166-1 alpha-2 country code, followed by a 9-character alphanumeric National Securities Identifying Number (NSIN), and a single Luhn check digit.

Validation checks the 12-character length, the letter-only country prefix, and the Luhn check digit (computed by converting letters to their numeric equivalents A=10..Z=35). `Format()` and `Normalize()` both return the 12-character uppercase ISIN.

Tests: [IsinTests.cs](test/Buildi.Primitives.Tests/Finance/IsinTests.cs)

- [ISO 6166](https://www.iso.org/standard/78502.html) — International Securities Identification Number (ISIN)
- [Wikipedia — ISIN](https://en.wikipedia.org/wiki/International_Securities_Identification_Number)
- [ANNA — Association of National Numbering Agencies](https://www.anna-web.org/)

```csharp
using Buildi.Primitives.Finance;

if (Isin.TryParse("SE0000108656", out var isin))
{
    Console.WriteLine(isin.Value);       // SE0000108656
    Console.WriteLine(isin.CountryCode); // SE
    Console.WriteLine(isin.Nsin);        // 000010865
    Console.WriteLine(isin.CheckDigit);  // 6
}

Isin.IsValid("SE0000108656");            // true (Ericsson B)
Isin.IsValid("US0378331005");            // true (Apple Inc.)
Isin.IsValid("SE0000108657");            // false (bad check digit)
Isin.Format("se 0000 1086 56");          // "SE0000108656"
Isin.Normalize("se0000108656");          // "SE0000108656"
```

---

### Contact

#### Address

An address (*adress*) combines street, postal code, city, and country into one normalized model. This type is useful when you want to parse complete free-text addresses, pass already separated components, or construct an address from already parsed value objects.

The address is a composite of `AddressStreet`, `AddressZipCode`, `AddressCity`, and `Country` - all optional, but at least one must be present. Free-text parsing uses comma and whitespace heuristics to split components. The normalized form is comma-separated with the country as an ISO alpha-2 code: `Storgatan 12, 11453, Stockholm, SE`. The display form defaults to Swedish country names. When the country is Sweden, `ToString()`, `ToDisplayString()`, and `ToMultilineString()` omit the country - domestic Swedish addresses don't repeat it. `ToEnglishString()` always includes the country name. `ToMultilineString()` outputs one component per line.

Tests: [AddressTests.cs](test/Buildi.Primitives.Tests/Contact/AddressTests.cs)

- [PostNord](https://www.postnord.se/) - Swedish postal service
- [Lantmäteriet](https://www.lantmateriet.se/) - Swedish mapping, cadastral and land registration authority
- [ISO 3166-1](https://www.iso.org/iso-3166-country-codes.html) - country codes standard

```csharp
using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

// Parse free text
if (Address.TryParse("c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Sweden", out var address))
{
    Console.WriteLine(address.Street.Street);       // Storgatan 12
    Console.WriteLine(address.ApartmentNumber);     // 1201
    Console.WriteLine(address.ZipCode!.Formatted);  // 114 53
    Console.WriteLine(address.City!.Value);         // Stockholm
    Console.WriteLine(address.Country!.Alpha2Code); // SE
    Console.WriteLine(address.ToMultilineString());
}

// Parse from individual components
var fromComponents = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

// Construct from already parsed models
var constructed = new Address(
    AddressStreet.Parse("Box 123"),
    AddressZipCode.Parse("114 53"),
    AddressCity.Parse("Stockholm"),
    Country.Parse("Sweden"));

Address.IsValid("Storgatan 12, 114 53 Stockholm, Sweden");   // true
Address.IsValid("Friedrichstraße 42, 10117 Berlin, Germany"); // true
Address.Format("Storgatan 12, 114 53 Stockholm, Sweden");    // "Storgatan 12, 114 53 Stockholm" (Sweden omitted)
Address.Normalize("Storgatan 12, 114 53 Stockholm, Sweden"); // "Storgatan 12, 11453, Stockholm, SE"

// Sweden is omitted from Swedish-language output; always included in English output
var se = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");
se.ToString();          // "Storgatan 12, 114 53 Stockholm"
se.ToDisplayString();   // "Storgatan 12, 114 53 Stockholm"
se.ToEnglishString();   // "Storgatan 12, 114 53 Stockholm, Sweden"
se.ToNormalizedString(); // "Storgatan 12, 11453, Stockholm, SE"
```

#### Swedish address

A Swedish address (*svensk adress*) is a stricter variant of `Address` that requires a complete Swedish postal address: street or post box, a 5-digit Swedish zip code, and a city. Addresses with an explicitly non-Swedish country are rejected. When no country is provided, Sweden is assumed. The underlying `Address` instance is exposed via the `Address` property for interop with the generic API.

Use `SwedishAddress` when you know the input must be a Swedish address and want strict validation. Use `Address` when international addresses also need to be accepted.

Tests: [SwedishAddressTests.cs](test/Buildi.Primitives.Tests/Contact/SwedishAddressTests.cs)

- [PostNord](https://www.postnord.se/) — Swedish postal service
- [Lantmäteriet](https://www.lantmateriet.se/) — Swedish mapping, cadastral and land registration authority

```csharp
using Buildi.Primitives.Contact;

// Parse — requires street + Swedish zip + city
if (SwedishAddress.TryParse("Storgatan 12, 114 53 Stockholm", out var addr))
{
    addr.Street.StreetName;    // "Storgatan"
    addr.Street.StreetNumber;  // "12"
    addr.ZipCode.Value;        // "11453"
    addr.ZipCode.Formatted;    // "114 53"
    addr.City.Value;           // "Stockholm"
    addr.Address.Country;      // Geography.Country for Sweden (always SE)
}

// Parse from individual components
var fromParts = SwedishAddress.Parse("Storgatan 12", "114 53", "Stockholm");

// Rejects non-Swedish addresses
SwedishAddress.IsValid("Storgatan 12, 114 53 Stockholm");          // true
SwedishAddress.IsValid("Storgatan 12, 114 53 Stockholm, SE");      // true (SE is fine)
SwedishAddress.IsValid("Storgatan 12, 114 53 Stockholm, DE");      // false (wrong country)
SwedishAddress.IsValid("Baker Street 1, W1A 1AB London");          // false (international zip)
SwedishAddress.IsValid("Storgatan 1");                             // false (no zip/city)

// ToString omits country (domestic)
SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm").ToString();
// → "Storgatan 12, 114 53 Stockholm"

// Multiline for postal labels
SwedishAddress.Parse("c/o Anna, Storgatan 12, 114 53 Stockholm").ToMultilineString();
// c/o Anna
// Storgatan 12
// 114 53 Stockholm

// Access the underlying generic Address for interop
var generic = SwedishAddress.Parse("Storgatan 12, 114 53 Stockholm").Address;
generic.ToEnglishString(); // "Storgatan 12, 114 53 Stockholm, Sweden"
```

#### Swedish zip code

A Swedish postal code (*postnummer*) — strictly 5 digits in the format NNN NN, administered by PostNord. Rejects all international formats. Use `AddressZipCode` when international zip codes also need to be accepted. The underlying `AddressZipCode` is exposed via the `ZipCode` property.

Tests: [SwedishAddressZipCodeTests.cs](test/Buildi.Primitives.Tests/Contact/SwedishAddressZipCodeTests.cs)

- [PostNord](https://www.postnord.se/) — Swedish postal service

```csharp
using Buildi.Primitives.Contact;

if (SwedishAddressZipCode.TryParse("11453", out var zip))
{
    zip.Value;      // "11453"
    zip.Formatted;  // "114 53"
    zip.ZipCode;    // underlying AddressZipCode instance
}

SwedishAddressZipCode.IsValid("114 53");     // true
SwedishAddressZipCode.IsValid("11453");      // true
SwedishAddressZipCode.IsValid("DK-9000");    // false (international)
SwedishAddressZipCode.IsValid("1234");       // false (too few digits)
SwedishAddressZipCode.IsValid("W1A 1AB");    // false (letters)

SwedishAddressZipCode.Format("11453");       // "114 53"
SwedishAddressZipCode.Normalize("114 53");   // "11453"
```

#### Country-specific addresses

The library provides strict country-specific address and zip code types for 32 countries. All types implement the `ICountryAddress` / `ICountryAddressZipCode` interfaces and share logic through `CountryAddressBase` / `CountryAddressZipCodeBase`. They reject addresses with the wrong country code, require a valid country-specific zip code, and assume their own country when no country is provided.

You don't need the country-specific types for formatting — `Address` itself does it when the country is known:

```csharp
var addr = Address.Parse("ul. Wiejska 4/6/8", "00-902", "Warszawa", "PL");
addr.ToString();           // "ul. Wiejska 4/6/8, 00-902 Warszawa, Polen"
addr.ToDomesticString();   // "ul. Wiejska 4/6/8, 00-902 Warszawa" (country omitted)

// Upgrade to the country-specific type when needed
var country = addr.AsCountryAddress();  // ICountryAddress? → PolishAddress
country!.Country.Alpha2Code;            // "PL"
country.CountryZipCode.Formatted;       // "00-902"

// Polymorphic collections
var list = new List<ICountryAddress>
{
    SwedishAddress.Parse("Storgatan 12", "11453", "Stockholm"),
    DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag"),
};
foreach (var a in list)
    Console.WriteLine($"{a.Country.Alpha2Code}: {a}");

// 32 countries have specific types
Address.SupportedCountryAddressTypes.Count; // 32
```

All types follow the same API: `TryParse`, `Parse`, `IsValid`, `Format`, `Normalize`, `ToString` (domestic, country omitted), `ToMultilineString`, and `ToMaskedString`. The underlying generic `Address` is always accessible via the `Address` property.

| Country | Address type | Zip code type | Zip format | Example zip |
| ------- | ------------ | ------------- | ---------- | ----------- |
| Poland (PL) | `PolishAddress` | `PolishAddressZipCode` | NN-NNN | `00-001` |
| Estonia (EE) | `EstonianAddress` | `EstonianAddressZipCode` | NNNNN | `10143` |
| Finland (FI) | `FinnishAddress` | `FinnishAddressZipCode` | NNNNN | `00100` |
| Lithuania (LT) | `LithuanianAddress` | `LithuanianAddressZipCode` | LT-NNNNN | `LT-01103` |
| Romania (RO) | `RomanianAddress` | `RomanianAddressZipCode` | NNNNNN | `010071` |
| Denmark (DK) | `DanishAddress` | `DanishAddressZipCode` | NNNN | `1218` |
| Norway (NO) | `NorwegianAddress` | `NorwegianAddressZipCode` | NNNN | `0026` |
| Germany (DE) | `GermanAddress` | `GermanAddressZipCode` | NNNNN | `10117` |
| Bulgaria (BG) | `BulgarianAddress` | `BulgarianAddressZipCode` | NNNN | `1000` |
| Latvia (LV) | `LatvianAddress` | `LatvianAddressZipCode` | LV-NNNN | `LV-1050` |
| Czech Republic (CZ) | `CzechAddress` | `CzechAddressZipCode` | NNN NN | `110 00` |
| Spain (ES) | `SpanishAddress` | `SpanishAddressZipCode` | NNNNN | `28071` |
| Netherlands (NL) | `DutchAddress` | `DutchAddressZipCode` | NNNN AA | `1012 AB` |
| Greece (GR) | `GreekAddress` | `GreekAddressZipCode` | NNN NN | `106 74` |
| Italy (IT) | `ItalianAddress` | `ItalianAddressZipCode` | NNNNN | `00186` |
| Slovenia (SI) | `SlovenianAddress` | `SlovenianAddressZipCode` | NNNN | `1000` |
| Croatia (HR) | `CroatianAddress` | `CroatianAddressZipCode` | NNNNN | `10000` |
| Portugal (PT) | `PortugueseAddress` | `PortugueseAddressZipCode` | NNNN-NNN | `1100-148` |
| Hungary (HU) | `HungarianAddress` | `HungarianAddressZipCode` | NNNN | `1055` |
| France (FR) | `FrenchAddress` | `FrenchAddressZipCode` | NNNNN | `75008` |
| Slovakia (SK) | `SlovakAddress` | `SlovakAddressZipCode` | NNN NN | `811 01` |
| Belgium (BE) | `BelgianAddress` | `BelgianAddressZipCode` | NNNN | `1000` |
| United Kingdom (GB) | `BritishAddress` | `BritishAddressZipCode` | A(A)N(A) NAA | `SW1A 2AA` |
| Austria (AT) | `AustrianAddress` | `AustrianAddressZipCode` | NNNN | `1010` |
| Cyprus (CY) | `CypriotAddress` | `CypriotAddressZipCode` | NNNN | `1060` |
| Iceland (IS) | `IcelandicAddress` | `IcelandicAddressZipCode` | NNN | `101` |
| Switzerland (CH) | `SwissAddress` | `SwissAddressZipCode` | NNNN | `3005` |
| Ireland (IE) | `IrishAddress` | `IrishAddressZipCode` | ANN XXXX | `D02 XR20` |
| Luxembourg (LU) | `LuxembourgishAddress` | `LuxembourgishAddressZipCode` | NNNN | `1648` |
| Malta (MT) | `MalteseAddress` | `MalteseAddressZipCode` | AAA NNNN | `VLT 1535` |
| Liechtenstein (LI) | `LiechtensteinAddress` | `LiechtensteinAddressZipCode` | NNNN | `9490` |

Tests: See individual test files per country in [Contact tests](test/Buildi.Primitives.Tests/Contact/)

- [Universal Postal Union](https://www.upu.int/) — addressing standards
- See individual type XML docs for country-specific postal service links

```csharp
using Buildi.Primitives.Contact;

// Polish address — zip code in NN-NNN format
if (PolishAddress.TryParse("Plac Defilad 1", "00-901", "Warszawa", out var plAddr))
{
    plAddr.ZipCode.Formatted;    // "00-901"
    plAddr.City.Value;           // "Warszawa"
    plAddr.ToString();           // "Plac Defilad 1, 00-901 Warszawa"
    plAddr.Address.Country!.Alpha2Code; // "PL"
}

// Dutch address — zip code in NNNN AA format (4 digits + 2 letters)
var nlAddr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
nlAddr.ZipCode.Formatted;       // "2513 AA"
nlAddr.ZipCode.Value;           // "2513AA"
nlAddr.ToString();               // "Binnenhof 1, 2513 AA Den Haag"

// Czech address — zip code in NNN NN format
CzechAddress.TryParse("Sněmovní 4", "11000", "Praha", out var czAddr);
czAddr!.ZipCode.Formatted;      // "110 00"
czAddr.ToString();               // "Sněmovní 4, 110 00 Praha"

// Latvian address — zip code with LV- prefix
var lvAddr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
lvAddr.ZipCode.Formatted;       // "LV-1050"
lvAddr.ToString();               // "Jēkaba iela 11, LV-1050 Rīga"

// British address — alphanumeric postcode
var gbAddr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
gbAddr.ZipCode.Formatted;       // "SW1A 2AA"
gbAddr.ZipCode.Value;           // "SW1A2AA"
gbAddr.ToString();               // "10 Downing Street, SW1A 2AA London"

// Irish address — 7-character Eircode
var ieAddr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
ieAddr.ZipCode.Formatted;       // "D02 XR20"
ieAddr.ZipCode.Value;           // "D02XR20"
ieAddr.ToString();               // "Kildare Street, D02 XR20 Dublin"

// Maltese address — AAA NNNN format
var mtAddr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
mtAddr.ZipCode.Formatted;       // "VLT 1190"
mtAddr.ZipCode.Value;           // "VLT1190"
mtAddr.ToString();               // "Pjazza San Gorg, VLT 1190 Valletta"

// French address — 5-digit zip code
var frAddr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
frAddr.ToString();               // "Place de la Concorde 1, 75008 Paris"

// Swiss address — 4-digit zip code
var chAddr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
chAddr.ToString();               // "Bundesplatz 3, 3005 Bern"

// All country types reject the wrong country
PolishAddress.IsValid("Storgatan 12, 11453, Stockholm, SE"); // false
DutchAddress.IsValid("Storgatan 12, 11453, Stockholm");      // false (zip format mismatch)

// Zip code types work standalone
DutchAddressZipCode.IsValid("1012AB");     // true
DutchAddressZipCode.Format("1012ab");      // "1012 AB"
DutchAddressZipCode.Normalize("1012 AB");  // "1012AB"

IrishAddressZipCode.Format("D02XR20");     // "D02 XR20"
BritishAddressZipCode.Format("SW1A2AA");   // "SW1A 2AA"
MalteseAddressZipCode.Format("VLT1535");   // "VLT 1535"

LatvianAddressZipCode.Format("1050");      // "LV-1050"
CzechAddressZipCode.Format("11000");       // "110 00"
```

#### Zip code

A Swedish postal code (*postnummer*) is a 5-digit code in the format NNN NN, administered by PostNord. International formats (e.g. Danish `DK-9000`, Dutch `1012 AB`, British `W1A 1AB`) are also supported.

Swedish postal codes are exactly 5 digits. The display form inserts a space after the third digit: `114 53`. The normalized form is digits only without whitespace: `11453`. International formats are recognized by country-prefix patterns (e.g., `DK-9000`, `NO-0010`) or alphanumeric structure (e.g., British `W1A 1AB`). Non-Swedish codes are preserved as-is in both forms.

Tests: [AddressZipCodeTests.cs](test/Buildi.Primitives.Tests/Contact/AddressZipCodeTests.cs)

- [PostNord](https://www.postnord.se/) - Swedish postal service
- [Wikipedia - Postnummer i Sverige](https://sv.wikipedia.org/wiki/Postnummer_i_Sverige)

```csharp
using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

// Swedish zip codes (5 digits) - Country defaults to Sweden
if (AddressZipCode.TryParse("11453", out var zip))
{
    Console.WriteLine(zip.Value);               // 11453
    Console.WriteLine(zip.Formatted);           // 114 53
    Console.WriteLine(zip.IsSwedish);           // true
    Console.WriteLine(zip.Country!.Alpha2Code); // SE
}

// International formats - Country is inferred from known prefixes (DK, MD, AD)
if (AddressZipCode.TryParse("DK-9000", out var dk))
{
    Console.WriteLine(dk.Value);               // DK-9000
    Console.WriteLine(dk.IsSwedish);           // false
    Console.WriteLine(dk.Country!.Alpha2Code); // DK (inferred from "DK-" prefix)
}

// Country not inferrable - Country is null
AddressZipCode.TryParse("W1A1AB", out var uk);
Console.WriteLine(uk!.Country);               // null (UK format, no country prefix)

// Parse with an explicit country - sets Country on the result regardless of format
AddressZipCode.TryParse("1234", Country.Norway, out var no);
Console.WriteLine(no!.Country!.Alpha2Code);   // NO

// Country-specific strict parsing - only the matching format is accepted
AddressZipCode.TryParse("DK-9000", Country.Sweden, out _);  // false - not Swedish format
AddressZipCode.TryParse("11453", Country.Sweden, out _);     // true - Swedish format accepted

// Strict parsing - only accept Swedish or only accept international formats
AddressZipCode.TryParseSwedish("11453", out var swedish);              // true - Swedish only
AddressZipCode.TryParseSwedish("DK-9000", out _);                     // false - not Swedish
AddressZipCode.TryParseInternational("DK-9000", out var intl);        // true - international only
AddressZipCode.TryParseInternational("1234", Country.Norway, out var noIntl); // true, Country = Norway

AddressZipCode.IsValid("11453");          // true
AddressZipCode.IsValid("DK-9000");        // true
AddressZipCode.Format("11453");           // "114 53"
AddressZipCode.Normalize("114 53");       // "11453"
```

#### City

The postal city (*postort*) is the locality associated with a Swedish postal code, as defined by PostNord. Normalization applies Swedish-aware title casing, correctly handling characters like Å, Ä, Ö and hyphenated place names.

Validated for minimum length of 1 character after trimming. When all characters share the same case (all uppercase or all lowercase), normalization applies Swedish title casing using `sv-SE` culture - correctly handling Å, Ä, Ö and hyphenated names (e.g., `saltsjö-boo` → `Saltsjö-Boo`). Mixed-case input is preserved as-is.

Tests: [AddressCityTests.cs](test/Buildi.Primitives.Tests/Contact/AddressCityTests.cs)

- [PostNord](https://www.postnord.se/) - Swedish postal service

```csharp
if (AddressCity.TryParse("ÅKERSBERGA", out var city))
{
    Console.WriteLine(city.Value); // Åkersberga
}

// Handles hyphenated names with Swedish culture
if (AddressCity.TryParse("saltsjö-boo", out var city2))
{
    Console.WriteLine(city2.Value); // Saltsjö-Boo
}

AddressCity.IsValid("Stockholm");          // true
AddressCity.IsValid("München");            // true
AddressCity.Format("ÅKERSBERGA");          // "Åkersberga"
AddressCity.Format("KRAKÓW");              // "Kraków"
AddressCity.Normalize("saltsjö-boo");      // "Saltsjö-Boo"
```

#### Street address

A street address (*gatuadress*) is the house-level part of a Swedish postal address, including street name and house number. The parser also extracts care-of (c/o), apartment number (lgh/apt), and post box (Box) when present. Address data in Sweden is maintained by Lantmäteriet. Normalization handles whitespace, casing, and uppercase house number suffixes (e.g. `12a` becomes `12A`).

The parser extracts structured components when present: street name (`StreetName`), house number (`StreetNumber`), care-of address (`c/o` or `C/O` prefix), apartment number (`lgh` or `apt` followed by digits), and post box (`Box` followed by a number). House number letter suffixes are uppercased (e.g., `12a` → `12A`). When city and zip code strings are known, they can be passed to strip trailing address fragments from concatenated strings. `StreetName` and `StreetNumber` are `null` for post boxes.

Tests: [AddressStreetTests.cs](test/Buildi.Primitives.Tests/Contact/AddressStreetTests.cs)

- [Lantmäteriet](https://www.lantmateriet.se/) - Swedish mapping, cadastral and land registration authority

```csharp
if (AddressStreet.TryParse("STORGATAN12a", out var addr))
{
    Console.WriteLine(addr.Street);       // Storgatan 12A
    Console.WriteLine(addr.StreetName);   // Storgatan
    Console.WriteLine(addr.StreetNumber); // 12A
}

// Optionally strip trailing city and zip code from the street
if (AddressStreet.TryParse("Storgatan 12 114 53 Stockholm", "Stockholm", "114 53", out var cleaned))
{
    Console.WriteLine(cleaned.Street); // Storgatan 12
}

// Care-of address
if (AddressStreet.TryParse("c/o Anna Svensson, Storgatan 12", out var co))
{
    Console.WriteLine(co.CareOf);  // Anna Svensson
    Console.WriteLine(co.Street);  // Storgatan 12
}

// Apartment number
if (AddressStreet.TryParse("Storgatan 12 lgh 1201", out var apt))
{
    Console.WriteLine(apt.ApartmentNumber); // 1201
    Console.WriteLine(apt.Street);          // Storgatan 12
}

// Post box
if (AddressStreet.TryParse("Box 123", out var box))
{
    Console.WriteLine(box.PostBox);   // 123
    Console.WriteLine(box.IsPostBox); // True
    Console.WriteLine(box.Street);    // Box 123
}

AddressStreet.IsValid("Storgatan 12A");          // true
AddressStreet.IsValid("Friedrichstraße 42");     // true
AddressStreet.IsValid("Rue de la Paix 15");      // true
AddressStreet.Format("STORGATAN12a");            // "Storgatan 12A"
AddressStreet.Normalize("STORGATAN12a");         // "Storgatan 12A"
```

#### Phone calling code

A phone country calling code (*landsnummer*) such as `+46` for Sweden or `+1` for the US. The canonical form (`Value`) is the digits-only string (e.g. `"46"`), while `ToString()` returns the display form with `+` prefix (e.g. `"+46"`). The type is self-contained with its own hardcoded dictionary of all ~190 ITU-T E.164 calling codes mapped to ISO 3166-1 alpha-2 country codes. Named constants are provided for European countries and major world countries.

Tests: [PhoneCallingCodeTests.cs](test/Buildi.Primitives.Tests/Contact/PhoneCallingCodeTests.cs)

- [ITU-T E.164](https://www.itu.int/rec/T-REC-E.164/) — international phone number standard
- [Wikipedia — List of country calling codes](https://en.wikipedia.org/wiki/List_of_country_calling_codes)

```csharp
if (PhoneCallingCode.TryParse("+46", out var code))
{
    Console.WriteLine(code.Value);       // 46
    Console.WriteLine(code.CountryCode); // SE
    Console.WriteLine(code.ToString());  // +46
}

// Accepts digits-only, + prefix, and 00 prefix
PhoneCallingCode.IsValid("46");    // true
PhoneCallingCode.IsValid("+46");   // true
PhoneCallingCode.IsValid("0046");  // true

PhoneCallingCode.Format("46");     // "+46"
PhoneCallingCode.Normalize("+46"); // "46"

// Named constants for common countries
PhoneCallingCode.Sweden;        // +46
PhoneCallingCode.Norway;        // +47
PhoneCallingCode.UnitedStates;  // +1
PhoneCallingCode.UnitedKingdom; // +44

// Use with PhoneNumber parsing and formatting
var phone = PhoneNumber.Parse("0701740633", PhoneCallingCode.Sweden);
PhoneNumber.Format("0701740633", PhoneCallingCode.Sweden); // "0701-74 06 33"
```

#### Phone number

Phone numbers (*telefonnummer*) are normalized to international digit format with a `00` prefix (e.g. `0046701740633`). Swedish numbers are identified by country code 46 and regulated by PTS (Post- och telestyrelsen). Both local Swedish formats and international numbers are accepted. Use `ToE164String()` for the standard E.164 representation with `+` prefix (e.g. `+46701740633`). `ToInternationalString()` is a convenience alias that returns the same E.164 output. The `Country` property resolves the country from the calling code. When parsing numbers without an explicit country prefix (`+` or `00`), the default calling code is `46` (Sweden) - use the `defaultCallingCode` parameter to override for other countries.

The normalized form is `00` + country code + subscriber digits (e.g., `0046701740633`). The parser strips spaces, dashes, dots, and parentheses, and recognizes `+`, `00`, and bare country-code prefixes. Swedish mobile numbers begin with `07x` in local format (subscriber digits 7–9 characters). The calling code is extracted and mapped to a `Country` instance. When no international prefix is present, the default calling code (`46` for Sweden) is applied. `ToE164String()` returns `+`-prefixed E.164 form; `ToLocalString()` returns Swedish local format (e.g., `0701-74 06 33`) for Swedish numbers.

Tests: [PhoneNumberTests.cs](test/Buildi.Primitives.Tests/Contact/PhoneNumberTests.cs)

- [PTS](https://pts.se/) - Post- och telestyrelsen (Swedish Post and Telecom Authority)
- [ITU-T E.164](https://www.itu.int/rec/T-REC-E.164/) - international phone number standard
- [Wikipedia - E.164](https://en.wikipedia.org/wiki/E.164)

```csharp
if (PhoneNumber.TryParse("070-174 06 33", out var phone))
{
    Console.WriteLine(phone.Digits);                   // 0046701740633
    Console.WriteLine(phone.IsSwedish);                // true
    Console.WriteLine(phone.IsMobile);                 // true
    Console.WriteLine(phone.CountryCallingCode);       // +46
    Console.WriteLine(phone.CountryCallingCode.Value); // 46
    Console.WriteLine(phone.Country!.EnglishName);     // Sweden
    Console.WriteLine(phone.IsSwedishTestPhoneNumber); // false
    Console.WriteLine(phone.ToLocalString());          // 0701-74 06 33
    Console.WriteLine(phone.ToE164String());           // +46701740633 (same as ToInternationalString())
}

// Non-Swedish numbers are also accepted
if (PhoneNumber.TryParse("+44 20 7946 0958", out var ukPhone))
{
    Console.WriteLine(ukPhone.IsSwedish);              // false
    Console.WriteLine(ukPhone.CountryCallingCode);     // +44
    Console.WriteLine(ukPhone.Country!.EnglishName);   // United Kingdom
    Console.WriteLine(ukPhone.ToE164String());         // +442079460958
}

// Parse a local number with a non-Swedish default calling code
var noPhone = PhoneNumber.Parse("021-123 45 67", defaultCallingCode: "47"); // Norway
Console.WriteLine(noPhone.Country!.EnglishName);       // Norway
Console.WriteLine(noPhone.Digits);                     // 0047211234567

PhoneNumber.IsValid("0701740633");                // true
PhoneNumber.Format("0701740633");                 // "0701-74 06 33"
PhoneNumber.Normalize("070-174 06 33");           // "0046701740633"
PhoneNumber.Normalize("070-174 06 33", "47");     // "0047701740633"
```

#### Test phone numbers

PTS (Post- och telestyrelsen) reserves phone number ranges for use in fiction and testing - these numbers will never be assigned to real subscribers.
`PhoneNumberTestData` enumerates all 495 reserved numbers, and `PhoneNumber.IsSwedishTestPhoneNumber` checks if a parsed number is in a reserved range.

- [PTS - Telefonnummer till böcker och filmer](https://pts.se/internet-och-telefoni/telefonnummer-och-adressering/telefonnummer-till-bocker-och-filmer/)

```csharp
// Enumerate all reserved test numbers
foreach (var number in PhoneNumberTestData.All)
    Console.WriteLine(number); // 0701740605, 0701740606, ..., 0980319299

// Check specific ranges
PhoneNumberTestData.Mobile             // 070-174 06 05 – 070-174 06 99
PhoneNumberTestData.LandlineStockholm  // 08-4650 04 00 – 08-4650 04 99
PhoneNumberTestData.LandlineGothenburg // 031-390 06 00 – 031-390 06 99
PhoneNumberTestData.LandlineMalmo      // 040-628 04 00 – 040-628 04 99
PhoneNumberTestData.LandlineKiruna     // 0980-31 92 00 – 0980-31 92 99

// Detect test numbers
var test = PhoneNumber.Parse("0701740635");
Console.WriteLine(test.IsSwedishTestPhoneNumber); // true
```

#### Contact info

A composite type that brings together an optional person name, organization name, and address into a single model. Use `ContactAddress.Create(...)` with already-parsed primitives or `ContactAddress.Builder()` to build from raw strings. Useful for postal labels, invoices, and any context where a full "who + where" is needed. At least one component must be present.

Wraps an optional `PersonFullName`, `SwedishOrganizationName`, and `Address`. At least one component must be present. Construct from already-parsed primitives using `Create()` or from raw strings via the fluent `Builder()`. `ToMultilineString()` outputs one component per line - suitable for postal labels and invoices. Language-aware output is available via `ToDisplayString()`, `ToEnglishString()`, and `ToNativeString()`.

Tests: [ContactAddressTests.cs](test/Buildi.Primitives.Tests/Contact/ContactAddressTests.cs)

```csharp
// From parsed primitives
var person = PersonFullName.Parse("Anna Andersson");
var org = SwedishOrganizationName.Parse("Budi AB");
var address = Address.Parse("Storgatan 12", "114 53", "Stockholm", "SE");

var contact = ContactAddress.Create(person, org, address);
contact.ToString();          // "Anna Andersson, Budi AB, Storgatan 12, 114 53 Stockholm"
contact.ToEnglishString();   // "Anna Andersson, Budi AB, Storgatan 12, 114 53 Stockholm, Sweden"
contact.ToMultilineString(); // Anna Andersson
                             // Budi AB
                             // Storgatan 12
                             // 114 53 Stockholm
                             // (Sweden omitted - domestic address)

// Using the builder with raw strings
var ok = ContactAddress.Builder()
    .WithPersonName("Anna Andersson")
    .WithOrganizationName("Budi AB")
    .WithAddress("Storgatan 12", "114 53", "Stockholm", "SE")
    .TryBuild(out var contact2);

// Only some components
var orgOnly = ContactAddress.Create(organizationName: SwedishOrganizationName.Parse("Budi AB"));
orgOnly.HasPersonName;       // false
orgOnly.HasOrganizationName; // true
orgOnly.HasAddress;          // false
```

---

### Web

#### Email address

An email address (*e-postadress*). Normalization trims whitespace and lowercases the entire address - including the local part - as a pragmatic interoperability and deduplication choice. RFC 5321 §2.3.11 states that the local part is formally case-sensitive, but in practice virtually all mail providers treat it as case-insensitive, and lowercasing is the widely adopted convention for storage, comparison, and deduplication. The TLD is exposed as a separate property and mapped to a country for country-code TLDs. Well-known public email providers are identified automatically - including international providers (Gmail, Outlook, Yahoo, iCloud, ProtonMail, Tuta, Hey, etc.), European regional providers (Web.de, Mail.ru, Interia, Onet, Seznam, Libero, etc.), and Nordic ISPs (Telia, Bredband2, Ownit, Online.no, Kolumbus, etc.). An optional `tryCorrectTypos` parameter detects and corrects common domain misspellings such as `gmail.con`, `gnail.com`, `hotmil.com`, or `icloude.com`. Typo correction is opt-in and should only be used when silent correction is acceptable in the application flow - otherwise intentionally unusual domains may be "fixed" without the user's knowledge.

The address must contain exactly one `@` separating a non-empty local part from a domain with at least one dot. Both parts are lowercased. The TLD is extracted from the domain and mapped to a country for country-code TLDs (e.g., `.se` → Sweden, `.de` → Germany). Over 60 public email providers are recognized and flagged via the `IsPublicEmailProvider` property and `Provider` enum. The optional `tryCorrectTypos` parameter detects approximately 40 common domain misspellings of major providers.

Tests: [EmailAddressTests.cs](test/Buildi.Primitives.Tests/Web/EmailAddressTests.cs)

- [RFC 5321 §2.3.11](https://datatracker.ietf.org/doc/html/rfc5321#section-2.3.11) - Mailbox and Address (local-part case sensitivity)
- [RFC 5321](https://datatracker.ietf.org/doc/html/rfc5321) - Simple Mail Transfer Protocol

```csharp
using Buildi.Primitives.Web;

if (EmailAddress.TryParse("User@Example.COM", out var email))
{
    Console.WriteLine(email.Value);               // user@example.com
    Console.WriteLine(email.LocalPart);           // user
    Console.WriteLine(email.Domain);              // example.com
    Console.WriteLine(email.TopLevelDomain);      // com
    Console.WriteLine(email.Country);             // (null - generic TLD)
    Console.WriteLine(email.Provider);            // (null - not a known public provider)
    Console.WriteLine(email.IsPublicEmailProvider); // false
}

if (EmailAddress.TryParse("user@gmail.com", out var gmail))
{
    Console.WriteLine(gmail.Provider);             // Gmail
    Console.WriteLine(gmail.IsPublicEmailProvider); // true
}

if (EmailAddress.TryParse("user@company.se", out var se))
{
    Console.WriteLine(se.TopLevelDomain);          // se
    Console.WriteLine(se.Country?.EnglishName);    // Sweden
}

// Typo correction (opt-in)
if (EmailAddress.TryParse("user@gnail.com", tryCorrectTypos: true, out var corrected))
{
    Console.WriteLine(corrected.Value);           // user@gmail.com
    Console.WriteLine(corrected.WasCorrected);    // true
    Console.WriteLine(corrected.OriginalDomain);  // gnail.com
    Console.WriteLine(corrected.Provider);        // Gmail
}

// Without typo correction, the misspelled domain is preserved
EmailAddress.TryParse("user@gnail.com", out var uncorrected);
Console.WriteLine(uncorrected!.Domain);           // gnail.com
Console.WriteLine(uncorrected.WasCorrected);      // false

EmailAddress.IsValid("user@example.com");                           // true
EmailAddress.Format("  User@Example.COM  ");                        // "user@example.com"
EmailAddress.Format("user@gmail.con", tryCorrectTypos: true);       // "user@gmail.com"
EmailAddress.Normalize("user@hotmil.com", tryCorrectTypos: true);   // "user@hotmail.com"

// Structured validation
var emailResult = EmailAddress.Validate("not-an-email");
Console.WriteLine(emailResult.IsValid);                    // false
Console.WriteLine(emailResult.Issues[0].Reason);           // MissingAtSign
Console.WriteLine(emailResult.Issues[0].EnglishDescription); // "Missing '@' sign."
```

#### URL

A URL or URI (*webbadress*). Accepts any valid URI scheme — `http`, `https`, `mailto`, `tel`, `ssh`, custom schemes, etc. Parsing delegates to `System.Uri` for validation and normalization (lowercase scheme/host, default-port removal). For hierarchical URIs (those with `://`), exposes the host, top-level domain, and the country associated with country-code TLDs. Bare domains like `example.com` are auto-prefixed with `https://`. Reserved example/test domains per RFC 2606 / RFC 6761 are flagged via `IsExampleDomain` but are still considered valid.

Tests: [UrlTests.cs](test/Buildi.Primitives.Tests/Web/UrlTests.cs)

- [RFC 3986](https://datatracker.ietf.org/doc/html/rfc3986) — Uniform Resource Identifier (URI): Generic Syntax
- [RFC 2606](https://datatracker.ietf.org/doc/html/rfc2606) — Reserved Top Level DNS Names
- [RFC 6761](https://datatracker.ietf.org/doc/html/rfc6761) — Special-Use Domain Names

```csharp
using Buildi.Primitives.Web;

// Hierarchical URLs
if (Url.TryParse("https://www.example.se/path?key=val#top", out var url))
{
    url.Value;            // "https://www.example.se/path?key=val#top"
    url.Scheme;           // "https"
    url.Host;             // "www.example.se"
    url.Port;             // null (default port omitted)
    url.Path;             // "/path"
    url.Query;            // "key=val"
    url.Fragment;         // "top"
    url.TopLevelDomain;   // "se"
    url.Country;          // Geography.Country for Sweden
    url.IsSecure;         // true
    url.IsExampleDomain;  // false
    url.IsIpAddress;      // false
}

// Any scheme is accepted
Url.IsValid("mailto:user@example.com");  // true
Url.IsValid("tel:+46701234567");         // true
Url.IsValid("ssh://github.com/repo");    // true
Url.IsValid("spotify://track/123");      // true

// Bare domains auto-prefix with https://
Url.Parse("example.com").Scheme;         // "https"
Url.Parse("example.com:8080").Port;      // 8080

// Normalization
Url.Normalize("HTTP://EXAMPLE.COM/PATH");   // "http://example.com/PATH"
Url.Format("example.com");                  // "https://example.com/"

// Country-code TLD mapping (same as EmailAddress)
Url.Parse("https://vg.no").Country!.Alpha2Code;       // "NO"

// Example/test domain detection — valid, just flagged
Url.Parse("https://example.com").IsExampleDomain;     // true
Url.Parse("https://test.test").IsExampleDomain;       // true
Url.Parse("https://google.com").IsExampleDomain;      // false

// Non-hierarchical URIs have empty host
Url.Parse("mailto:user@example.com").Scheme;   // "mailto"
Url.Parse("tel:+46701234567").Host;             // "" (empty)
```

---

### Vehicle

#### Vehicle registration number

A Swedish vehicle registration number (*registreringsnummer*) is assigned by Transportstyrelsen. The classic format is three letters followed by three digits (ABC 123). Since 2019 the last digit may be replaced by a letter (ABC 12A) to expand capacity. Letters **I, Q, V, Å, Ä, Ö** are excluded from all positions; **O** is additionally excluded from the final position to avoid confusion with zero. `TaxPaymentDigit` and `TaxPaymentMonth` are derived from the last numeric digit according to Transportstyrelsen's standard payment-month mapping - this is a structural derivation from the registration number, not a live tax status lookup for any specific vehicle.

The plate is 6 characters: 3 letters + 3 digits (classic: `ABC 123`) or 3 letters + 2 digits + 1 letter (new format since 2019: `ABC 12A`). Allowed letters exclude I, Q, V, Å, Ä, Ö in all positions and additionally O in the final position. The last numeric digit determines the vehicle tax payment month via Transportstyrelsen's mapping (e.g., digit `3` → June). The normalized form is 6 uppercase characters without space; the display form inserts a space after the 3rd character.

Tests: [SwedishVehicleRegistrationNumberTests.cs](test/Buildi.Primitives.Tests/Vehicle/SwedishVehicleRegistrationNumberTests.cs)

- [Transportstyrelsen - Licence plates](https://www.transportstyrelsen.se/en/road/vehicles/licence-plates/)
- [Transportstyrelsen - Vehicle tax](https://www.transportstyrelsen.se/en/road/vehicles/taxes-and-fees/vehicle-tax/)
- [Wikipedia - Vehicle registration plates of Sweden](https://en.wikipedia.org/wiki/Vehicle_registration_plates_of_Sweden)

```csharp
using Buildi.Primitives.Vehicle;

if (SwedishVehicleRegistrationNumber.TryParse("ABC 123", out var reg))
{
    Console.WriteLine(reg.Value);            // ABC123
    Console.WriteLine(reg.Formatted);        // ABC 123
    Console.WriteLine(reg.Letters);          // ABC
    Console.WriteLine(reg.Suffix);           // 123
    Console.WriteLine(reg.IsNewFormat);      // false
    Console.WriteLine(reg.TaxPaymentDigit);  // 3
    Console.WriteLine(reg.TaxPaymentMonth);  // 6 (June)
}

// New format (2019+)
if (SwedishVehicleRegistrationNumber.TryParse("ABC 12A", out var newReg))
{
    Console.WriteLine(newReg.IsNewFormat);      // true
    Console.WriteLine(newReg.TaxPaymentDigit);  // 2
    Console.WriteLine(newReg.TaxPaymentMonth);  // 5 (May)
}

SwedishVehicleRegistrationNumber.IsValid("ABC 123");        // true
SwedishVehicleRegistrationNumber.IsValid("ABC 12O");        // false (O excluded in final position)
SwedishVehicleRegistrationNumber.Format("abc123");           // "ABC 123"
SwedishVehicleRegistrationNumber.Normalize("ABC 123");       // "ABC123"
```

#### Vehicle identification number

A Vehicle Identification Number (VIN), also known as chassis number (*chassinummer*), is a 17-character internationally standardized code (ISO 3779) that uniquely identifies a motor vehicle. Letters I, O, and Q are excluded to avoid confusion with digits. Position 9 is a check digit (mandatory for North American vehicles, commonly present on European vehicles).

The VIN is exactly 17 alphanumeric characters. Characters 1–3 form the World Manufacturer Identifier (WMI), 4–9 the Vehicle Descriptor Section (VDS), and 10–17 the Vehicle Identifier Section (VIS). Position 9 is a check digit validated using a weighted-sum algorithm (ISO 3779 MOD-11). Position 10 encodes the model year and position 11 the assembly plant. Letters I, O, and Q are excluded. The normalized form is uppercase.

The model year code at position 10 follows a 30-year repeating cycle (ISO 3779 / SAE J853), using letters A–Y (excluding I, O, Q, U) and digits 1–9. The `ModelYears` property decodes this into the list of possible calendar years - most codes map to two years, for example `M` → 1991 and 2021.

Tests: [VehicleIdentificationNumberTests.cs](test/Buildi.Primitives.Tests/Vehicle/VehicleIdentificationNumberTests.cs)

- [ISO 3779](https://www.iso.org/standard/52200.html) - Road vehicles - Vehicle identification number (VIN)
- [Wikipedia - Vehicle identification number](https://en.wikipedia.org/wiki/Vehicle_identification_number)

```csharp
if (VehicleIdentificationNumber.TryParse("1HGBH41JXMN109186", out var vin))
{
    Console.WriteLine(vin.Value);               // 1HGBH41JXMN109186
    Console.WriteLine(vin.Wmi);                 // 1HG  (World Manufacturer Identifier, positions 1–3)
    Console.WriteLine(vin.Vds);                 // BH41JX (Vehicle Descriptor Section, positions 4–9)
    Console.WriteLine(vin.Vis);                 // MN109186 (Vehicle Identifier Section, positions 10–17)
    Console.WriteLine(vin.CheckDigit);          // X   (position 9)
    Console.WriteLine(vin.HasValidCheckDigit);  // true (North American VINs enforce the check digit)
    Console.WriteLine(vin.ModelYearCode);       // M         (position 10 - encodes the model year)
    Console.WriteLine(vin.ModelYears);          // [1991, 2021] (decoded possible model years)
    Console.WriteLine(vin.AssemblyPlantCode);   // N         (position 11)
    Console.WriteLine(vin.SequentialNumber);    // 109186    (positions 12–17)
}

VehicleIdentificationNumber.IsValid("1HGBH41JXMN109186");  // true
VehicleIdentificationNumber.Format("1hgbh41jxmn109186");    // "1HGBH41JXMN109186"
VehicleIdentificationNumber.Normalize("1HGBH41JXMN109186"); // "1HGBH41JXMN109186"
```

#### Euro emission class

A European vehicle emission standard (*Euro-utsläppsklass*) ranging from Euro 1 through Euro 7, with the corresponding Swedish environmental class label (*miljöklass*). The type supports parsing Euro codes (`Euro 6d`), Swedish labels (`Miljöklass 2005`), the electric vehicle class (`El`), Roman numerals (`Euro VI`), and additional Swedish synonyms (`2005PM`, `EEV`, `Hybrid`). Each instance exposes the numeric level, optional sub-level suffix, introduction year, and the Swedish miljöklass mapping.

Tests: [EuroEmissionClassTests.cs](test/Buildi.Primitives.Tests/Vehicle/EuroEmissionClassTests.cs)

- [European Commission — European emission standards](https://commission.europa.eu/energy-climate-change-environment/standards-tools-and-labels/labels-certificates-and-standards/vehicles-and-transport-labelling-standards/european-emission-standards_en)
- [Transportstyrelsen — fordonsregler](https://www.transportstyrelsen.se/)

```csharp
if (EuroEmissionClass.TryParse("Euro 6d", out var emission))
{
    Console.WriteLine(emission.EuroClass);          // Euro 6d
    Console.WriteLine(emission.Level);              // 6
    Console.WriteLine(emission.SubLevel);           // d
    Console.WriteLine(emission.SwedishMiljoklass);  // Miljöklass 2005
    Console.WriteLine(emission.IntroductionYear);   // 2020
}

EuroEmissionClass.IsValid("Euro 5b");               // true
EuroEmissionClass.IsValid("Miljöklass 2005");        // true
EuroEmissionClass.Format("euro6d");                  // "Euro 6d"
EuroEmissionClass.Normalize("Miljöklass 2005");      // "Euro 6"
```

#### Tire dimension

Metric tire size notation (*däckdimension*) per ISO 4000-1, e.g. `205/55R16`. Parses section width, aspect ratio, construction type (radial/diagonal/bias belt), and rim diameter, with optional load index and speed rating (e.g. `205/55R16 91H`). Also supports commercial/truck tire formats with half-inch rim diameters (e.g. `315/80R22.5`), commercial `C` suffix (e.g. `195/65R15C`), and dual load indices (e.g. `154/150M`).

Tests: [TireDimensionTests.cs](test/Buildi.Primitives.Tests/Vehicle/TireDimensionTests.cs)

- [ISO 4000-1 — Passenger car tyres and rims](https://www.iso.org/standard/4600.html)
- [Wikipedia — Tire code](https://en.wikipedia.org/wiki/Tire_code)

```csharp
if (TireDimension.TryParse("205/55R16 91H", out var tire))
{
    Console.WriteLine(tire.WidthMm);            // 205
    Console.WriteLine(tire.AspectRatio);         // 55
    Console.WriteLine(tire.Construction);        // R
    Console.WriteLine(tire.RimDiameterInches);   // 16
    Console.WriteLine(tire.LoadIndex);           // 91
    Console.WriteLine(tire.SpeedRating);         // H
}

TireDimension.IsValid("205/55R16");              // true
TireDimension.Format("205/55r16");               // "205/55 R 16"
TireDimension.Normalize("205/55 R 16");          // "205/55R16"
```

#### Engine displacement

Engine displacement (*motorvolym* / *slagvolym*) representing the total swept volume of an engine's cylinders. Wraps `Volume` internally. Bare numbers are interpreted as cubic centimeters (cc). Also accepts the `cc` suffix as an alias for milliliters. Requires positive displacement.

Tests: [EngineDisplacementTests.cs](test/Buildi.Primitives.Tests/Vehicle/EngineDisplacementTests.cs)

```csharp
if (EngineDisplacement.TryParse("1998", out var displacement))
{
    Console.WriteLine(displacement.CubicCentimeters);   // 1998
    Console.WriteLine(displacement.Liters);             // 1.998
    Console.WriteLine(displacement.Value);              // "1998 mL"
}

EngineDisplacement.IsValid("2.0 L");                    // true
EngineDisplacement.IsValid("1998cc");                   // true
EngineDisplacement.Format("2000");                      // "2000 mL"
EngineDisplacement.Format("2 L");                       // "2 L"
```

#### Engine power

Engine power output (*motoreffekt*) wrapping `Power`. Bare numbers are interpreted as horsepower (HP). Also accepts the Swedish abbreviation `hk` (*hästkraft*). Requires positive power. Approximate prefixes like `ca`, `circa`, `~`, `approx`, and `ungefär` are automatically stripped (e.g. `ca 150 hk` → `150 HP`).

Tests: [EnginePowerTests.cs](test/Buildi.Primitives.Tests/Vehicle/EnginePowerTests.cs)

```csharp
if (EnginePower.TryParse("150", out var ep))
{
    Console.WriteLine(ep.Horsepower);   // 150
    Console.WriteLine(ep.Kilowatts);    // 110.325...
    Console.WriteLine(ep.Value);        // "150 HP"
}

EnginePower.IsValid("110 kW");          // true
EnginePower.IsValid("150 hk");          // true (Swedish for HP)
EnginePower.Format("150 hk");           // "150 HP"
EnginePower.Format("110 kW");           // "110 kW"
```

#### Odometer reading

A vehicle odometer reading (*mätarställning*) wrapping `Length`. Bare numbers are interpreted as kilometers. Supports Swedish miles (`mil`, 1 mil = 10 km) and English miles (`mi`). Zero is valid (new vehicle). Normalizes to kilometers.

Tests: [OdometerReadingTests.cs](test/Buildi.Primitives.Tests/Vehicle/OdometerReadingTests.cs)

```csharp
if (OdometerReading.TryParse("15000", out var odo))
{
    Console.WriteLine(odo.Kilometers);     // 15000
    Console.WriteLine(odo.SwedishMiles);   // 1500
    Console.WriteLine(odo.Miles);          // 9320.56...
    Console.WriteLine(odo.Value);          // "15000 km"
}

OdometerReading.IsValid("0");              // true (new vehicle)
OdometerReading.IsValid("1500 mil");       // true (Swedish miles)
OdometerReading.Format("1500 mil");        // "1500 mil"
OdometerReading.Normalize("1500 mil");     // "15000 km"
```

#### Driving license category

A Swedish/EU driving license category (*körkortsbehörighet*) as defined by Transportstyrelsen and EU Directive 2006/126/EC. The 15 categories range from AM (moped) through DE (bus with heavy trailer). Each category exposes metadata including Swedish and English names, a description of entitled vehicles, minimum age requirement, vehicle group classification, trailer eligibility, and license validity period. Parsing accepts category codes (`B`, `C1E`), Swedish names (`Personbil`, `Tung motorcykel`), and common aliases (`MC`, `Lastbil`, `B utökad`).

Tests: [SwedishDrivingLicenseCategoryTests.cs](test/Buildi.Primitives.Tests/Vehicle/SwedishDrivingLicenseCategoryTests.cs)

- [Transportstyrelsen — Driving licence categories](https://www.transportstyrelsen.se/en/road/driving-licences/im-going-to-take-my-driving-licence/driving-licence-categories/)
- [Wikipedia — Driving licence in Sweden](https://en.wikipedia.org/wiki/Driving_licence_in_Sweden)
- EU Directive 2006/126/EC — harmonized EU driving licence categories

```csharp
using Buildi.Primitives.Vehicle;

if (SwedishDrivingLicenseCategory.TryParse("C1E", out var cat))
{
    Console.WriteLine(cat.Code);              // "C1E"
    Console.WriteLine(cat.LocalizedName);     // "Medeltung lastbil med tungt släp"
    Console.WriteLine(cat.EnglishName);       // "Medium heavy lorry with heavy trailer"
    Console.WriteLine(cat.MinimumAge);        // 18
    Console.WriteLine(cat.VehicleGroup);      // Truck
    Console.WriteLine(cat.IsTrailerCategory); // true
    Console.WriteLine(cat.ValidityYears);     // 5
}

SwedishDrivingLicenseCategory.IsValid("B");          // true
SwedishDrivingLicenseCategory.IsValid("Motorcykel"); // true (alias for A)
SwedishDrivingLicenseCategory.Format("b");           // "B — Personbil och lätt lastbil"
SwedishDrivingLicenseCategory.Normalize("Personbil"); // "B"
```

#### Fuel type

A vehicle fuel or energy type (*drivmedel*) with canonical English identifier, Swedish display name, and Transportstyrelsen code. The type covers 16 fuel/energy types including Petrol, Diesel, Electric, Hybrid, Mild Hybrid (48V/MHEV), Plug-in Hybrid, Ethanol, Natural Gas, LPG, Hydrogen, Biodiesel, Methane, Methanol, HVO, Kerosene, and Other. Mild Hybrid is kept distinct from full Hybrid since they have different tax treatment, environmental classification, and driving characteristics. Parsing accepts over 80 aliases including Swedish names, English names, Transportstyrelsen codes, and common abbreviations.

Tests: [FuelTypeTests.cs](test/Buildi.Primitives.Tests/Vehicle/FuelTypeTests.cs)

- [Transportstyrelsen — Fordonsdata](https://www.transportstyrelsen.se/) — Swedish vehicle registration authority fuel codes
- [Wikipedia — Alternative fuel vehicle](https://en.wikipedia.org/wiki/Alternative_fuel_vehicle)

```csharp
using Buildi.Primitives.Vehicle;

if (FuelType.TryParse("bensin", out var fuel))
{
    Console.WriteLine(fuel.Value);         // "Petrol"
    Console.WriteLine(fuel.LocalizedName); // "Bensin"
    Console.WriteLine(fuel.EnglishName);   // "Petrol"
    Console.WriteLine(fuel.Code);          // "BE"
}

FuelType.IsValid("diesel");                // true
FuelType.IsValid("PHEV");                  // true (alias for Plug-in Hybrid)
FuelType.Normalize("gasol");               // "LPG"
```

#### Transmission type

A vehicle transmission or gearbox type (*växellåda*) with 7 predefined types: Manual, Automatic, CVT, Dual clutch, Sequential, Semi-automatic, and AMT. Parsing accepts over 50 aliases including brand-specific names (DSG, PDK, Tiptronic, Steptronic, S tronic, Multitronic), Swedish equivalents (Automat, Manuell), and common abbreviations (A/T, M/T).

Tests: [TransmissionTypeTests.cs](test/Buildi.Primitives.Tests/Vehicle/TransmissionTypeTests.cs)

```csharp
if (TransmissionType.TryParse("DSG", out var trans))
{
    Console.WriteLine(trans.Value);          // "Dual clutch"
    Console.WriteLine(trans.LocalizedName);  // "Dubbelkoppling"
    Console.WriteLine(trans.EnglishName);    // "Dual clutch"
}

TransmissionType.IsValid("automat");         // true
TransmissionType.IsValid("Tiptronic");       // true (alias for Automatic)
TransmissionType.Normalize("stick");         // "Manual"
```

#### Swedish vehicle type

Swedish vehicle type classification (*fordonsslag*) as used by Transportstyrelsen. The type covers 17 vehicle types from passenger cars (PB) and trucks (LB) to snowmobiles (SNSK) and A-tractors (AT). Parsing accepts Transportstyrelsen codes, Swedish names, English names, and common synonyms.

Tests: [SwedishVehicleTypeTests.cs](test/Buildi.Primitives.Tests/Vehicle/SwedishVehicleTypeTests.cs)

- [Transportstyrelsen — Fordonsslag](https://www.transportstyrelsen.se/) — official Swedish vehicle type codes

```csharp
if (SwedishVehicleType.TryParse("personbil", out var vt))
{
    Console.WriteLine(vt.Code);            // "PB"
    Console.WriteLine(vt.LocalizedName);   // "Personbil"
    Console.WriteLine(vt.EnglishName);     // "Passenger car"
}

SwedishVehicleType.IsValid("MC");           // true
SwedishVehicleType.IsValid("lastbil");      // true
SwedishVehicleType.Normalize("lastbil");    // "LB"
```

#### Swedish vehicle status

Swedish vehicle registration status (*fordonsstatus*) from Transportstyrelsen. The type covers 7 statuses: In service, Deregistered, Unregistered, Stolen, Exported, Scrapped, and Reported stolen. Parsing handles aliases with and without diacritics, spaces, and underscores.

Tests: [SwedishVehicleStatusTests.cs](test/Buildi.Primitives.Tests/Vehicle/SwedishVehicleStatusTests.cs)

- [Transportstyrelsen — Fordonsstatus](https://www.transportstyrelsen.se/) — official Swedish vehicle registration statuses

```csharp
if (SwedishVehicleStatus.TryParse("avställd", out var status))
{
    Console.WriteLine(status.Code);          // "AVST"
    Console.WriteLine(status.LocalizedName); // "Avställd"
    Console.WriteLine(status.EnglishName);   // "Deregistered"
}

SwedishVehicleStatus.IsValid("ITRAFIK");     // true
SwedishVehicleStatus.IsValid("i trafik");    // true
SwedishVehicleStatus.Normalize("i trafik");  // "ITRAFIK"
```

#### EU vehicle category

EU vehicle category code (*fordonskategori*) per Regulation (EU) 2018/858 and Directive 2007/46/EC. Supports base categories (M, N, O, L, T, C, R, S) with numeric sub-categories, optional off-road `G` suffix, and extended sub-suffixes (e.g. `L3e-A2`). Each parsed category provides English and Swedish descriptions.

Tests: [EuVehicleCategoryTests.cs](test/Buildi.Primitives.Tests/Vehicle/EuVehicleCategoryTests.cs)

- [Regulation (EU) 2018/858](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32018R0858) — EU vehicle type-approval framework
- [Wikipedia — European Union vehicle categories](https://en.wikipedia.org/wiki/European_Union_vehicle_categories)

```csharp
if (EuVehicleCategory.TryParse("N1G", out var cat))
{
    Console.WriteLine(cat.Value);                // "N1G"
    Console.WriteLine(cat.BaseCategory);         // "N"
    Console.WriteLine(cat.CategoryNumber);       // 1
    Console.WriteLine(cat.IsOffRoad);            // true
    Console.WriteLine(cat.EnglishDescription);   // "Light commercial vehicle"
}

EuVehicleCategory.IsValid("M1");                 // true
EuVehicleCategory.IsValid("L3e-A2");             // true
EuVehicleCategory.Normalize(" m1 ");             // "M1"
```

#### EU type approval number

EU whole-vehicle type-approval number (*typgodkännandenummer*), e.g. `e9*2007/46*6364*09`. The format consists of an E-mark country code, a directive/regulation reference, a type number, and an extension number separated by asterisks. The type identifies the approval country by E-mark code (1=Germany, 2=France, 3=Italy, 5=Sweden, 9=Spain, etc.).

Tests: [EuTypeApprovalNumberTests.cs](test/Buildi.Primitives.Tests/Vehicle/EuTypeApprovalNumberTests.cs)

- [UNECE — 1958 Agreement](https://unece.org/transport/vehicle-regulations/agreements-and-regulations/1958-agreement) — E-mark country codes

```csharp
if (EuTypeApprovalNumber.TryParse("e9*2007/46*6364*09", out var approval))
{
    Console.WriteLine(approval.Value);                 // "e9*2007/46*6364*09"
    Console.WriteLine(approval.ApprovalCountryCode);   // 9
    Console.WriteLine(approval.ApprovalCountryName);   // "Spain"
    Console.WriteLine(approval.Directive);             // "2007/46"
    Console.WriteLine(approval.TypeNumber);            // "6364"
    Console.WriteLine(approval.Extension);             // "09"
}

EuTypeApprovalNumber.IsValid("e5*2018/858*1234*01");   // true (Sweden, e5)
EuTypeApprovalNumber.Normalize("E9*2007/46*6364*09");  // "e9*2007/46*6364*09"
```

#### Wheel rim dimension

Wheel rim/wheel size notation (*fälgdimension*), e.g. `18x7J`. Parses rim diameter in inches, width in inches, and optional flange type (J, JJ, JK, B, K, etc.). Accepts both standard format (`18x7J`) and reversed format (`7Jx18`), with optional spaces and decimal values.

Tests: [WheelRimDimensionTests.cs](test/Buildi.Primitives.Tests/Vehicle/WheelRimDimensionTests.cs)

- [Wikipedia — Rim (wheel)](https://en.wikipedia.org/wiki/Rim_(wheel))
- [ETRTO Standards Manual](https://www.etrto.org/)

```csharp
if (WheelRimDimension.TryParse("7J x 18", out var rim))
{
    Console.WriteLine(rim.DiameterInches);    // 18
    Console.WriteLine(rim.WidthInches);       // 7
    Console.WriteLine(rim.FlangeType);        // "J"
}

WheelRimDimension.IsValid("18x7J");           // true
WheelRimDimension.IsValid("7Jx16");           // true (reversed format)
WheelRimDimension.Format("18x7J");            // "18 x 7 J"
WheelRimDimension.Normalize("18 x 7 j");      // "18x7J"
```

#### Fuel consumption

Vehicle fuel consumption (*bränsleförbrukning*), stored internally as liters per 100 km. Supports parsing multiple input units: l/100km, km/l, mpg (US and Imperial), and kWh/100km (for EVs). Provides computed conversions between all supported units.

Tests: [FuelConsumptionTests.cs](test/Buildi.Primitives.Tests/Vehicle/FuelConsumptionTests.cs)

- [Wikipedia — Fuel economy](https://en.wikipedia.org/wiki/Fuel_economy_in_automobiles)

```csharp
if (FuelConsumption.TryParse("12 km/l", out var fc))
{
    Console.WriteLine(fc.LitersPer100Km);      // 8.333...
    Console.WriteLine(fc.KilometersPerLiter);   // 12
    Console.WriteLine(fc.MilesPerGallonUs);     // 28.25...
    Console.WriteLine(fc.IsElectric);           // false
}

FuelConsumption.IsValid("15 kWh/100km");        // true (EV consumption)
FuelConsumption.IsValid("8.3 l/100km");         // true
FuelConsumption.Normalize("28 mpg");            // liters per 100km canonical form
```

#### Emission rate

Vehicle emission rate (*utsläpp per körd sträcka*), stored internally as grams per kilometer. Supports g/km, mg/km, and g/mi (US miles) with automatic conversions. Used for reporting CO₂, NOₓ, PM, and other emission measurements.

Tests: [EmissionRateTests.cs](test/Buildi.Primitives.Tests/Vehicle/EmissionRateTests.cs)

- [European Commission — CO₂ emission performance standards](https://commission.europa.eu/energy-climate-change-environment/standards-tools-and-labels/labels-certificates-and-standards/vehicles-and-transport-labelling-standards_en)

```csharp
if (EmissionRate.TryParse("95 g/km", out var emission))
{
    Console.WriteLine(emission.GramsPerKm);       // 95
    Console.WriteLine(emission.MilligramsPerKm);  // 95000
    Console.WriteLine(emission.GramsPerMile);     // 152.88...
}

EmissionRate.IsValid("221 g/km");                 // true
EmissionRate.IsValid("95.7 mg/km");               // true
EmissionRate.Normalize("355 g/mi");               // converted to g/km form
```

#### Operating Hours

Machine operating hours (*drifttimmar*) — the time-based equivalent of an odometer reading. Common for construction equipment, forklifts, and generators. Bare numbers are interpreted as hours.

Tests: [OperatingHoursTests.cs](test/Buildi.Primitives.Tests/Vehicle/OperatingHoursTests.cs)

```csharp
OperatingHours.TryParse("5600 timmar", out var hours);
// hours.Hours → 5600

OperatingHours.IsValid("1234");         // true (bare number = hours)
OperatingHours.IsValid("1234 h");       // true
OperatingHours.Format("5600 timmar");   // "5600 h"
OperatingHours.Normalize("1234");       // "1234 h"
```

#### Bolt Pattern

Wheel bolt pattern (*bultcirkelmått*) describing bolt count and pitch circle diameter, e.g. `5x114.3`.

Tests: [BoltPatternTests.cs](test/Buildi.Primitives.Tests/Vehicle/BoltPatternTests.cs)

- [Wikipedia — Bolt pattern](https://en.wikipedia.org/wiki/Bolt_pattern)

```csharp
BoltPattern.TryParse("5x114.3", out var bp);
// bp.BoltCount → 5, bp.PitchCircleDiameter → 114.3

BoltPattern.IsValid("5 x 114.3");      // true
BoltPattern.Format("5x114.3");         // "5 x 114.3"
BoltPattern.Normalize("5 X 114,3");    // "5x114.3"
```

---

### Product

#### GTIN

A Global Trade Item Number (GTIN) is a product identifier managed by GS1. The general `Gtin` class accepts any valid length (8, 12, 13, or 14 digits) and validates the GS1 modulo-10 check digit.

The last digit is a check digit computed using the GS1 modulo-10 algorithm (alternating weight factors ×3 and ×1 on the preceding digits). `ToGtin14Digits()` left-pads any valid GTIN to 14 digits for uniform storage and comparison.

Tests: [GtinTests.cs](test/Buildi.Primitives.Tests/Product/GtinTests.cs)

- [GS1 - GTIN](https://www.gs1.org/standards/id-keys/gtin)
- [Wikipedia - Global Trade Item Number](https://en.wikipedia.org/wiki/Global_Trade_Item_Number)

```csharp
using Buildi.Primitives.Product;

if (Gtin.TryParse("7310865001764", out var gtin))
{
    Console.WriteLine(gtin.Digits);          // 7310865001764
    Console.WriteLine(gtin.Length);           // 13
    Console.WriteLine(gtin.CheckDigit);      // 4
    Console.WriteLine(gtin.ToGtin14Digits()); // 07310865001764
}
```

#### GTIN-8

An 8-digit GTIN (EAN-8) typically used for small retail packages where a full 13-digit barcode would not fit physically. Validated with the GS1 mod-10 check digit.

Tests: [Gtin8Tests.cs](test/Buildi.Primitives.Tests/Product/Gtin8Tests.cs)

```csharp
if (Gtin8.TryParse("96385074", out var g8))
{
    Console.WriteLine(g8.Digits);          // 96385074
    Console.WriteLine(g8.CheckDigit);      // 4
    Console.WriteLine(g8.ToGtin14Digits()); // 00000096385074
}
```

#### GTIN-12

A 12-digit GTIN (GTIN-12), also known as UPC-A (Universal Product Code). The standard barcode format for retail products in North America. Validated with the GS1 mod-10 check digit.

Tests: [Gtin12Tests.cs](test/Buildi.Primitives.Tests/Product/Gtin12Tests.cs)

- [GS1 - GTIN](https://www.gs1.org/standards/id-keys/gtin)
- [Wikipedia - Universal Product Code](https://en.wikipedia.org/wiki/Universal_Product_Code)

```csharp
if (Gtin12.TryParse("614141000036", out var g12))
{
    Console.WriteLine(g12.Digits);          // 614141000036
    Console.WriteLine(g12.CheckDigit);      // 6
    Console.WriteLine(g12.ToGtin14Digits()); // 00614141000036
}
```

#### GTIN-13

A 13-digit GTIN (EAN-13), the most common barcode format for retail products worldwide. The first 1–3 digits form the GS1 prefix identifying the issuing GS1 member organization. The prefix is resolved to a country or organization name and, when applicable, an ISO country code based on the [GS1 prefix list](https://www.gs1.org/standards/id-keys/company-prefix).

The GS1 prefix (1–3 digits) identifies the origin: `730`–`739` = Sweden, `400`–`440` = Germany, `978`–`979` = Books (ISBN), `977` = Periodicals (ISSN). The prefix metadata is exposed via `Gs1Prefix`, `Gs1PrefixName`, and `Gs1PrefixCountryCode`. Note that the GS1 prefix indicates where the barcode was issued, not necessarily where the product was manufactured. Validated with the GS1 mod-10 check digit.

Tests: [Gtin13Tests.cs](test/Buildi.Primitives.Tests/Product/Gtin13Tests.cs)

- [GS1 - GTIN](https://www.gs1.org/standards/id-keys/gtin)
- [GS1 - Company prefix](https://www.gs1.org/standards/id-keys/company-prefix)

```csharp
if (Gtin13.TryParse("7310865001764", out var g13))
{
    Console.WriteLine(g13.Digits);              // 7310865001764
    Console.WriteLine(g13.Gs1Prefix);           // 731
    Console.WriteLine(g13.Gs1PrefixName);       // Sweden
    Console.WriteLine(g13.Gs1PrefixCountryCode); // SE
    Console.WriteLine(g13.CheckDigit);          // 4
    Console.WriteLine(g13.ToGtin14Digits());    // 07310865001764
}

// Non-country prefixes
if (Gtin13.TryParse("9789100000004", out var isbn))
{
    Console.WriteLine(isbn.Gs1PrefixName);       // Books (ISBN)
    Console.WriteLine(isbn.Gs1PrefixCountryCode); // (null)
}
```

#### GTIN-14

A 14-digit GTIN (ITF-14) used for outer packaging and logistics units. The first digit is an indicator digit (1–8 for packaging levels, 9 for variable measure).

The indicator digit signals the packaging level: `1`–`8` represent different groupings of the same product (e.g., single item, 6-pack, case), and `9` indicates variable-measure trade items. The inner 13-digit GTIN-13 (with recalculated check digit) is accessible via `InnerGtin13Digits`. Validated with the GS1 mod-10 check digit.

Tests: [Gtin14Tests.cs](test/Buildi.Primitives.Tests/Product/Gtin14Tests.cs)

```csharp
if (Gtin14.TryParse("17310865001761", out var g14))
{
    Console.WriteLine(g14.Digits);              // 17310865001761
    Console.WriteLine(g14.IndicatorDigit);      // 1
    Console.WriteLine(g14.InnerGtin13Digits);   // 7310865001764
    Console.WriteLine(g14.CheckDigit);          // 1
}
```

#### HS code

A Harmonized System (HS) code is an international classification code for goods in trade, maintained by the World Customs Organization (WCO). The universal HS level uses 6 digits (~5,600 subheadings across 97 chapters). The EU extends this with the Combined Nomenclature (CN, 8 digits, Swedish: *KN-nummer*) and TARIC (10 digits). This type accepts codes at all five hierarchy levels: chapter (2 digits), heading (4), subheading (6), CN subheading (8), and TARIC code (10).

Tests: [HsCodeTests.cs](test/Buildi.Primitives.Tests/Product/HsCodeTests.cs)

- [WCO - What is the Harmonized System?](https://www.wcoomd.org/en/topics/nomenclature/overview/what-is-the-harmonized-system.aspx) - international standard maintained by the World Customs Organization
- [EU - Combined Nomenclature](https://taxation-customs.ec.europa.eu/customs/common-customs-tariff-cct/tariff-classification-goods/combined-nomenclature_en) - the EU's 8-digit extension of the HS
- [Tullverket](https://www.tullverket.se) - Swedish Customs, administers the Combined Nomenclature (KN) in Sweden
- [Wikipedia - Harmonized System](https://en.wikipedia.org/wiki/Harmonized_System)
- [Wikipedia - Combined Nomenclature](https://en.wikipedia.org/wiki/Combined_Nomenclature)

```csharp
using Buildi.Primitives.Product;

if (HsCode.TryParse("847130", out var hs))
{
    Console.WriteLine(hs.Digits);    // 847130
    Console.WriteLine(hs.Chapter);   // 84
    Console.WriteLine(hs.Level);     // Subheading
    Console.WriteLine(hs.ToString());          // 8471.30
    Console.WriteLine(hs.ToNormalizedString()); // 847130
}

HsCode.IsValid("8471.30");          // true
HsCode.IsValid("84.71");            // true (4-digit heading)
HsCode.IsValid("8471.30.00");       // true (8-digit CN code)
HsCode.IsValid("8471.30.00.00");    // true (10-digit TARIC code)

HsCode.Format("847130");            // "8471.30"
HsCode.Format("84713000");          // "8471.30.00"
HsCode.Normalize("847130");          // "8471.30"
HsCode.Normalize("8471.30.00.00");  // "8471.30.00.00"
```

#### Google product category

A Google product category is a hierarchical classification path used by Google Shopping and Merchant Center to categorize products. Categories are expressed as `>`-separated segments, e.g. `Electronics > Computers > Laptops`. The taxonomy is maintained by Google and contains 5,000+ categories across top-level verticals such as *Animals & Pet Supplies*, *Apparel & Accessories*, *Electronics*, *Food, Beverages & Tobacco*, and more. This type validates and normalizes the path structure (separator spacing, trimming) without verifying against the full taxonomy list.

Tests: [GoogleProductCategoryTests.cs](test/Buildi.Primitives.Tests/Product/GoogleProductCategoryTests.cs)

- [Google Merchant Center - google_product_category](https://support.google.com/merchants/answer/6324436) - product data specification
- [Google Product Taxonomy](https://www.google.com/basepages/producttype/taxonomy.en-US.txt) - full category list (en-US)
- [Google Merchant Center - Supported product categories](https://support.google.com/merchants/answer/160081)

```csharp
using Buildi.Primitives.Product;

if (GoogleProductCategory.TryParse("Animals & Pet Supplies > Pet Supplies > Bird Supplies", out var gpc))
{
    Console.WriteLine(gpc.Path);          // Animals & Pet Supplies > Pet Supplies > Bird Supplies
    Console.WriteLine(gpc.Depth);         // 3
    Console.WriteLine(gpc.RootCategory);  // Animals & Pet Supplies
    Console.WriteLine(gpc.LeafCategory);  // Bird Supplies
    Console.WriteLine(gpc.Segments[1]);   // Pet Supplies
}

GoogleProductCategory.IsValid("Electronics > Computers > Laptops");  // true
GoogleProductCategory.IsValid("Electronics");                        // true (root category)
GoogleProductCategory.IsValid("Electronics>Computers>Laptops");      // true (separator spacing normalized)

GoogleProductCategory.Format("Electronics>Computers");               // "Electronics > Computers"
GoogleProductCategory.Normalize("  Electronics > Computers  ");      // "Electronics > Computers"
```

#### Color

An sRGB color (*färg*) parsed from CSS hex codes (`#RGB`, `#RRGGBB`), `rgb()`, `hsl()` functions, or named CSS/Swedish color names. Each parsed color exposes R/G/B channels, HSL values, and English/Swedish names when the RGB matches a known named color.

Tests: [ColorTests.cs](test/Buildi.Primitives.Tests/Product/ColorTests.cs)

- [W3C — CSS Color Module — named colors](https://www.w3.org/TR/css-color-4/#named-colors)

```csharp
if (Color.TryParse("#FF0000", out var color))
{
    Console.WriteLine(color.R);             // 255
    Console.WriteLine(color.Hex);           // #FF0000
    Console.WriteLine(color.NameEnglish);   // red
    Console.WriteLine(color.NameSwedish);   // röd
}

Color.IsValid("rgb(0, 128, 0)");            // true
Color.Format("rgb(0, 128, 0)");             // "green"
Color.Normalize("#f00");                     // "#FF0000"
```

#### Adult clothing size

Adult clothing size (*klädstorlek*) with conversions between EU numeric (32–56), US, UK, and letter (XXS–XXXXL) systems. Internally stored as an EU numeric size.

Tests: [AdultClothingSizeTests.cs](test/Buildi.Primitives.Tests/Product/AdultClothingSizeTests.cs)

```csharp
if (AdultClothingSize.TryParse("L", out var size))
{
    Console.WriteLine(size.EuSize);       // 40
    Console.WriteLine(size.UsSize);       // 10
    Console.WriteLine(size.UkSize);       // 12
    Console.WriteLine(size.LetterSize);   // L
}

AdultClothingSize.IsValid("EU 42");       // true
AdultClothingSize.Format("42");           // "XL"
AdultClothingSize.Normalize("L");         // "EU 40"
```

#### Child clothing size

Children's clothing size (*barnklädesstorlek*) based on body height in centimetres (EU scale, e.g. 56–176 cm). Provides approximate US, UK, and age-range labels.

Tests: [ChildClothingSizeTests.cs](test/Buildi.Primitives.Tests/Product/ChildClothingSizeTests.cs)

```csharp
if (ChildClothingSize.TryParse("128", out var childSize))
{
    Console.WriteLine(childSize.HeightCm);    // 128
    Console.WriteLine(childSize.EuSize);      // 128
}

ChildClothingSize.IsValid("EU 104");          // true
ChildClothingSize.Normalize("104");           // "EU 104"
```

#### Clothing size

Generic clothing size (*klädstorlek*) that auto-detects whether the input is an adult or child size. Supports explicit `adult`/`vuxen` or `child`/`barn` prefixes to force the category.

Tests: [ClothingSizeTests.cs](test/Buildi.Primitives.Tests/Product/ClothingSizeTests.cs)

```csharp
if (ClothingSize.TryParse("M", out var cs))
{
    Console.WriteLine(cs.IsAdult);    // true
    Console.WriteLine(cs.Value);      // M
}

ClothingSize.IsValid("child 128");    // true
ClothingSize.Format("M");            // "M"
ClothingSize.Normalize("M");         // "EU 38"
```

#### Adult shoe size

Adult shoe size (*skostorlek*) stored as EU size, with US men's, US women's, and UK conversions.

Tests: [AdultShoeSizeTests.cs](test/Buildi.Primitives.Tests/Product/AdultShoeSizeTests.cs)

```csharp
if (AdultShoeSize.TryParse("EU 42", out var shoe))
{
    Console.WriteLine(shoe.EuSize);       // 42
    Console.WriteLine(shoe.UsMenSize);    // 9
    Console.WriteLine(shoe.UsWomenSize);  // 10.5
    Console.WriteLine(shoe.UkSize);       // 8.5
}

AdultShoeSize.IsValid("US 10");           // true
AdultShoeSize.Normalize("US 10");         // "EU 43"
```

#### Child shoe size

Children's shoe size (*barnskostorlek*) in EU sizes (16–39) with US child (C/Y) and UK display conversions.

Tests: [ChildShoeSizeTests.cs](test/Buildi.Primitives.Tests/Product/ChildShoeSizeTests.cs)

```csharp
if (ChildShoeSize.TryParse("EU 28", out var childShoe))
{
    Console.WriteLine(childShoe.EuSize);    // 28
    Console.WriteLine(childShoe.UsSize);    // 10.5C
    Console.WriteLine(childShoe.UkSize);    // 10
}

ChildShoeSize.IsValid("US 10.5C");          // true
ChildShoeSize.Normalize("US 10.5C");        // "EU 28"
```

#### Shoe size

Generic shoe size (*skostorlek*) that auto-detects adult or child based on the input. US sizes with `C` or `Y` suffix are parsed as children's; `adult`/`vuxen` or `child`/`barn` prefixes force the category.

Tests: [ShoeSizeTests.cs](test/Buildi.Primitives.Tests/Product/ShoeSizeTests.cs)

```csharp
if (ShoeSize.TryParse("EU 42", out var shoe))
{
    Console.WriteLine(shoe.IsAdult);    // true
    Console.WriteLine(shoe.Value);      // EU 42
}

ShoeSize.IsValid("US 10.5C");          // true (child)
ShoeSize.Normalize("US 10.5C");        // "EU 28"
```

#### Screen size

Display diagonal screen size (*skärmstorlek*) as a length measurement, defaulting to inches when the input is a bare number (e.g. `15` means 15 inches). Supports explicit units like `cm` or `in`.

Tests: [ScreenSizeTests.cs](test/Buildi.Primitives.Tests/Product/ScreenSizeTests.cs)

```csharp
if (ScreenSize.TryParse("27", out var screen))
{
    Console.WriteLine(screen.Inches);        // 27
    Console.WriteLine(screen.Centimeters);   // 68.58
    Console.WriteLine(screen.Value);         // 27 in
}

ScreenSize.IsValid("15.6 in");               // true
ScreenSize.Format("15");                     // "15 in"
ScreenSize.Normalize("15.6 inches");         // "15.6 in"
```

#### Screen resolution

Pixel dimensions of a display or video mode (*skärmupplösning*), supporting both `WxH` notation (e.g. `1920x1080`) and marketing names (e.g. `Full HD`, `4K`). Exposes width, height, aspect ratio, total pixels, and known marketing name when applicable.

Tests: [ScreenResolutionTests.cs](test/Buildi.Primitives.Tests/Product/ScreenResolutionTests.cs)

- [Wikipedia — Display resolution](https://en.wikipedia.org/wiki/Display_resolution)

```csharp
if (ScreenResolution.TryParse("1920x1080", out var res))
{
    Console.WriteLine(res.Width);         // 1920
    Console.WriteLine(res.Height);        // 1080
    Console.WriteLine(res.Name);          // Full HD
    Console.WriteLine(res.AspectRatio);   // 16:9
    Console.WriteLine(res.TotalPixels);   // 2073600
}

ScreenResolution.IsValid("4K");           // true
ScreenResolution.Format("4K");            // "3840x2160"
ScreenResolution.Normalize("Full HD");    // "1920x1080"
```

#### Energy efficiency class

EU energy efficiency class (*energimärkning*) for appliance labels, supporting both the pre-2021 A+++ scale and the rescaled A–G (2021) scheme. Each value has a numeric rank for ordering (lower is better).

Tests: [EuEnergyEfficiencyClassTests.cs](test/Buildi.Primitives.Tests/Product/EuEnergyEfficiencyClassTests.cs)

- [European Commission — EU energy labels](https://energy.ec.europa.eu/topics/energy-efficiency/energy-label-and-ecodesign/energy-label_en)

```csharp
if (EuEnergyEfficiencyClass.TryParse("A++", out var energy))
{
    Console.WriteLine(energy.Label);        // A++
    Console.WriteLine(energy.Scale);        // Old
    Console.WriteLine(energy.NumericRank);  // 1
}

EuEnergyEfficiencyClass.IsValid("A+++");      // true
EuEnergyEfficiencyClass.Format("a+");         // "A+"
EuEnergyEfficiencyClass.Normalize("A++");     // "A++"
```

#### Operating system name

A recognized operating system name (*operativsystem*) resolved from common aliases to a canonical form. Supports Windows, macOS, Linux distributions (Ubuntu, Debian, Fedora, CentOS, Arch, etc.), Android, iOS, iPadOS, ChromeOS, watchOS, and more. Each OS is classified into an `OperatingSystemFamily` enum.

Tests: [OperatingSystemNameTests.cs](test/Buildi.Primitives.Tests/Product/OperatingSystemNameTests.cs)

```csharp
using Buildi.Primitives.Product;

if (OperatingSystemName.TryParse("mac os x", out var os))
{
    Console.WriteLine(os.Value);    // "macOS"
    Console.WriteLine(os.Family);   // MacOS
}

OperatingSystemName.IsValid("ubuntu");           // true
OperatingSystemName.Format("win");               // "Windows"
OperatingSystemName.Format("rhel");              // "Red Hat"
OperatingSystemName.Normalize("mac os");         // "macOS"
```

#### Operating system version

An operating system version string with structured major, minor, patch, and build components. Supports versions like `11`, `14.5`, `10.15.7`, and `22.04`. Preserves the original format (e.g. leading zeros in `22.04` are retained). Implements `IComparable<T>` for version ordering.

Tests: [OperatingSystemVersionTests.cs](test/Buildi.Primitives.Tests/Product/OperatingSystemVersionTests.cs)

```csharp
if (OperatingSystemVersion.TryParse("10.15.7", out var ver))
{
    Console.WriteLine(ver.Value);   // "10.15.7"
    Console.WriteLine(ver.Major);   // 10
    Console.WriteLine(ver.Minor);   // 15
    Console.WriteLine(ver.Patch);   // 7
}

OperatingSystemVersion.IsValid("22.04");          // true
OperatingSystemVersion.Format("v11");             // "11"
OperatingSystemVersion.Normalize("v10.15.7");     // "10.15.7"
```

#### Operating system info

A combined operating system name and version (*operativsysteminfo*), e.g. `Windows 11` or `macOS 14.5`. Parses the input by greedily matching the longest known OS name prefix and treating the remainder as a version string. Name-only inputs (e.g. `Ubuntu`) are also valid.

Tests: [OperatingSystemInfoTests.cs](test/Buildi.Primitives.Tests/Product/OperatingSystemInfoTests.cs)

```csharp
if (OperatingSystemInfo.TryParse("Ubuntu 22.04", out var info))
{
    Console.WriteLine(info.Value);             // "Ubuntu 22.04"
    Console.WriteLine(info.Name.Value);        // "Ubuntu"
    Console.WriteLine(info.Name.Family);       // Linux
    Console.WriteLine(info.Version!.Value);    // "22.04"
}

OperatingSystemInfo.IsValid("Windows 11");         // true
OperatingSystemInfo.IsValid("macOS");              // true (version optional)
OperatingSystemInfo.Format("win 11");              // "Windows 11"
OperatingSystemInfo.Format("mac os x 14.5");       // "macOS 14.5"
```

#### Storage capacity

Storage capacity (*lagringskapacitet*) for hard drives, SSDs, and USB drives. Wraps `DataSize` internally. Bare numbers are interpreted as gigabytes. Requires positive capacity. Supports all `DataSize` units including binary prefixes (GiB, TiB).

Tests: [StorageCapacityTests.cs](test/Buildi.Primitives.Tests/Product/StorageCapacityTests.cs)

```csharp
if (StorageCapacity.TryParse("512", out var storage))
{
    Console.WriteLine(storage.Gigabytes);    // 512
    Console.WriteLine(storage.Terabytes);    // 0.512
    Console.WriteLine(storage.Value);        // "512 GB"
}

StorageCapacity.IsValid("1 TB");             // true
StorageCapacity.Format("2000");              // "2000 GB"
StorageCapacity.Format("2000", unit: DataSizeUnit.Terabyte);  // "2 TB"
```

#### RAM capacity

RAM / memory capacity (*arbetsminne*). Wraps `DataSize` internally. Bare numbers are interpreted as gigabytes. Requires positive capacity.

Tests: [RamCapacityTests.cs](test/Buildi.Primitives.Tests/Product/RamCapacityTests.cs)

```csharp
if (RamCapacity.TryParse("16", out var ram))
{
    Console.WriteLine(ram.Gigabytes);    // 16
    Console.WriteLine(ram.Megabytes);    // 16000
    Console.WriteLine(ram.Value);        // "16 GB"
}

RamCapacity.IsValid("32 GB");           // true
RamCapacity.IsValid("4096 MB");         // true
RamCapacity.Format("8");                // "8 GB"
```

#### Processor speed

Processor clock speed (*processorhastighet*). Wraps `Frequency` internally. Bare numbers are interpreted as GHz. Requires positive speed.

Tests: [ProcessorSpeedTests.cs](test/Buildi.Primitives.Tests/Product/ProcessorSpeedTests.cs)

```csharp
if (ProcessorSpeed.TryParse("3.5", out var cpu))
{
    Console.WriteLine(cpu.Gigahertz);    // 3.5
    Console.WriteLine(cpu.Megahertz);    // 3500
    Console.WriteLine(cpu.Value);        // "3.5 GHz"
}

ProcessorSpeed.IsValid("2400 MHz");      // true
ProcessorSpeed.Format("3.5");           // "3.5 GHz"
ProcessorSpeed.Format("2400 MHz");       // "2400 MHz"
```

#### Battery capacity

Battery capacity (*batterikapacitet*) supporting both charge-based (mAh, Ah) and energy-based (Wh, kWh) units. Bare numbers are interpreted as mAh. The type stores either an `ElectricCharge` or `Energy` value depending on the input unit, with separate properties for each.

Tests: [BatteryCapacityTests.cs](test/Buildi.Primitives.Tests/Product/BatteryCapacityTests.cs)

```csharp
if (BatteryCapacity.TryParse("5000", out var battery))
{
    Console.WriteLine(battery.Value);          // "5000 mAh"
    Console.WriteLine(battery.Charge);         // ElectricCharge (5000 mAh)
    Console.WriteLine(battery.EnergyValue);    // null
}

if (BatteryCapacity.TryParse("50 Wh", out var laptopBattery))
{
    Console.WriteLine(laptopBattery.Value);          // "50 Wh"
    Console.WriteLine(laptopBattery.Charge);         // null
    Console.WriteLine(laptopBattery.EnergyValue);    // Energy (50 Wh)
}

BatteryCapacity.IsValid("5000 mAh");     // true
BatteryCapacity.IsValid("99.8 Wh");      // true
BatteryCapacity.Format("5000");          // "5000 mAh"
```

#### Electrical phase

Electrical phase configuration (*fas*), representing single-phase (*enfas*), two-phase (*tvåfas*), or three-phase (*trefas*) power supply. Parsing accepts over 30 aliases including Swedish forms (`enfas`, `trefas`), shorthand (`1P`, `3P`), symbolic notation (`1~`, `3~`), and voltage-prefixed inputs (`230V 1-fas`, `400V 3-fas`). Each instance exposes the phase count.

Tests: [ElectricalPhaseTests.cs](test/Buildi.Primitives.Tests/Product/ElectricalPhaseTests.cs)

```csharp
using Buildi.Primitives.Product;

if (ElectricalPhase.TryParse("trefas", out var phase))
{
    Console.WriteLine(phase.PhaseCount);     // 3
    Console.WriteLine(phase.LocalizedName);  // "Trefas"
    Console.WriteLine(phase.EnglishName);    // "Three-phase"
}

ElectricalPhase.IsValid("enfas");            // true
ElectricalPhase.IsValid("3~");               // true
ElectricalPhase.Normalize("trefas");         // "3-phase"
```

#### Clothing gender

Clothing target gender (*klädkön*) as used in product feeds and fashion e-commerce. Parses English (`male`, `female`, `unisex`, `boys`, `girls`), Swedish (`herr`, `dam`, `pojke`, `flicka`), and common synonyms (`man`, `kvinna`, `kille`, `tjej`, `herrkläder`, `damkläder`, etc.) — all case-insensitive.

Tests: [ClothingGenderTests.cs](test/Buildi.Primitives.Tests/Product/ClothingGenderTests.cs)

- [Google Merchant Center — gender attribute](https://support.google.com/merchants/answer/6324479)
- [Schema.org — GenderType](https://schema.org/GenderType)

```csharp
using Buildi.Primitives.Product;

if (ClothingGender.TryParse("dam", out var gender))
{
    Console.WriteLine(gender.Value);          // "Female"
    Console.WriteLine(gender.LocalizedName);  // "Dam"
    Console.WriteLine(gender.EnglishName);    // "Female"
}

ClothingGender.IsValid("herr");               // true
ClothingGender.IsValid("kvinna");             // true
ClothingGender.Normalize("herr");             // "Male"
ClothingGender.Normalize("pojke");            // "Boys"
```

#### IP Rating

Ingress Protection rating (*IP-klass* / *kapslingsklass*) per IEC 60529. Parses the two-character code indicating solids (0-6/X) and liquids (0-9/X) protection levels.

Tests: [IpRatingTests.cs](test/Buildi.Primitives.Tests/Product/IpRatingTests.cs)

- [Wikipedia — IP code](https://en.wikipedia.org/wiki/IP_code)
- [IEC 60529](https://webstore.iec.ch/en/publication/2452) — Degrees of protection

```csharp
IpRating.TryParse("IP65", out var ip);
// ip.SolidsProtection → '6', ip.SolidsDescription → "Dust tight"
// ip.LiquidsProtection → '5', ip.LiquidsDescription → "Water jets"

IpRating.IsValid("IPX4");           // true
IpRating.Format("ip 65");           // "IP65"
IpRating.Normalize("IP-65");        // "IP65"
```

---

### Person

#### Person given name

A person's given names (*förnamn*) as defined by Swedish naming law ([namnlagen, SFS 2016:1013](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/)). A person may have one or more given names. One of them can be explicitly designated as the preferred name (*tilltalsnamn*) - the name the person goes by in everyday use. When all letters share the same case, normalization auto-capitalizes; mixed case is preserved.

One or more space-separated names are parsed into a `Names` list. When all characters are the same case (all uppercase or all lowercase), each name is auto-capitalized (e.g., `anna maria` → `Anna Maria`); mixed-case input like `AnnCharlotte` is preserved. A preferred name (*tilltalsnamn*) can be set via `WithPreferredName()` and must match one of the given names - it is not derived automatically.

Tests: [PersonGivenNameTests.cs](test/Buildi.Primitives.Tests/Person/PersonGivenNameTests.cs)

- [Skatteverket - Namn](https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html)
- [Namnlagen (SFS 2016:1013)](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/) - Swedish naming law

```csharp
using Buildi.Primitives.Person;

if (PersonGivenName.TryParse("anna maria", out var given))
{
    Console.WriteLine(given.Value);           // Anna Maria
    Console.WriteLine(given.Names.Count);     // 2
    Console.WriteLine(given.Names[0]);        // Anna
    Console.WriteLine(given.Names[1]);        // Maria
    Console.WriteLine(given.PreferredName);   // (null)
}

// Explicitly set the tilltalsnamn (preferred name)
var withPref = PersonGivenName.Parse("Anna Maria", "Maria");
Console.WriteLine(withPref.PreferredName);    // Maria

// Or use the fluent API
var fluent = PersonGivenName.Parse("Anna Maria").WithPreferredName("Maria");
Console.WriteLine(fluent.PreferredName);      // Maria

PersonGivenName.IsValid("Anna");              // true
PersonGivenName.IsValid("Fatima");            // true
PersonGivenName.IsValid("Bo");               // true (short names are valid)
PersonGivenName.IsValid("Null");             // true (the string "Null" is a valid name)
PersonGivenName.Format("ANNA MARIA");         // "Anna Maria"
PersonGivenName.Format("josé");              // "José"
PersonGivenName.Normalize("anna");            // "Anna"
```

#### Person family name

A person's family name (*efternamn*) as defined by Swedish naming law. The family name is the surname a person bears, which may be acquired by birth, marriage, or application. When all letters share the same case, normalization auto-capitalizes; mixed case is preserved.

A single name token. When all characters are the same case, normalization auto-capitalizes (e.g., `andersson` → `Andersson`); mixed-case input is preserved (e.g., `McDonald` stays as-is). Validated for minimum length of 1 character after trimming.

Tests: [PersonFamilyNameTests.cs](test/Buildi.Primitives.Tests/Person/PersonFamilyNameTests.cs)

- [Skatteverket - Namn](https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html)
- [Namnlagen (SFS 2016:1013)](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/) - Swedish naming law

```csharp
if (PersonFamilyName.TryParse("andersson", out var family))
{
    Console.WriteLine(family.Value); // Andersson
}

PersonFamilyName.IsValid("Andersson");         // true
PersonFamilyName.IsValid("Al-Rashid");         // true
PersonFamilyName.IsValid("Ek");               // true (two-letter names like the Swedish surname Ek)
PersonFamilyName.IsValid("Null");             // true (real surname, not confused with null)
PersonFamilyName.Format("ANDERSSON");          // "Andersson"
PersonFamilyName.Format("nguyễn");            // "Nguyễn"
PersonFamilyName.Normalize("andersson");       // "Andersson"
```

#### Person full name

A person's full name (*fullständigt namn*), composed of given names and a family name. Can be parsed from free text (all tokens except the last become given names; the last becomes the family name) or constructed from already-parsed parts. Supports setting a preferred name (*tilltalsnamn*) that must match one of the given names.

When parsed from free text, all space-separated tokens except the last become given names; the last token becomes the family name. At least two tokens are required. Can also be constructed from separately parsed `PersonGivenName` and `PersonFamilyName` instances via `Create()`. The preferred name, when set, carries through from the given name.

Tests: [PersonFullNameTests.cs](test/Buildi.Primitives.Tests/Person/PersonFullNameTests.cs)

- [Skatteverket - Namn](https://skatteverket.se/privat/folkbokforing/namn.4.18e1b10334ebe8bc80004083.html)
- [Namnlagen (SFS 2016:1013)](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/lag-20161013-om-personnamn_sfs-2016-1013/) - Swedish naming law

```csharp
using Buildi.Primitives.Person;

if (PersonFullName.TryParse("anna maria elisabeth andersson", out var full))
{
    Console.WriteLine(full.Value);                  // Anna Maria Elisabeth Andersson
    Console.WriteLine(full.GivenName.Value);        // Anna Maria Elisabeth
    Console.WriteLine(full.GivenName.Names.Count);  // 3
    Console.WriteLine(full.FamilyName.Value);       // Andersson
    Console.WriteLine(full.PreferredName);          // (null)
}

// Set preferred name when parsing
var withPref = PersonFullName.Parse("Anna Maria Andersson", "Maria");
Console.WriteLine(withPref.PreferredName);          // Maria

// Or construct from parsed parts
var given = PersonGivenName.Parse("Anna Maria", "Anna");
var family = PersonFamilyName.Parse("Andersson");
var constructed = PersonFullName.Create(given, family);
Console.WriteLine(constructed.PreferredName);       // Anna

PersonFullName.IsValid("Anna Andersson");            // true
PersonFullName.IsValid("Fatima Al-Rashid");          // true
PersonFullName.IsValid("Bo Ek");                     // true (short names)
PersonFullName.IsValid("Jennifer Null");             // true (Null is a real surname)
PersonFullName.Format("ANNA ANDERSSON");             // "Anna Andersson"
PersonFullName.Format("josé garcía");               // "José García"
PersonFullName.Normalize("anna andersson");          // "Anna Andersson"
```

#### Person age

A person's age (*ålder*), expressed as completed years. Can be parsed from a numeric string with optional Swedish or English unit labels (`25`, `25 år`, `8 månader`, `300 dagar`), or constructed from a birth date. Exposes Swedish legal-age threshold properties: *myndig* (18), *straffmyndig* (15), and *pensionsålder* (65). For ages under one year, `ToString()` displays months instead of years.

Tests: [PersonAgeTests.cs](test/Buildi.Primitives.Tests/Person/PersonAgeTests.cs)

- [Föräldrabalken (SFS 1949:381)](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/foraldrabalk-1949381_sfs-1949-381/) — Swedish legal age of majority (18)
- [Brottsbalken (SFS 1962:700) 1 kap. 6 §](https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/brottsbalk-1962700_sfs-1962-700/) — Swedish age of criminal responsibility (15)

```csharp
using Buildi.Primitives.Person;

if (PersonAge.TryParse("25 år", out var age))
{
    Console.WriteLine(age.Years);                          // 25
    Console.WriteLine(age.TotalDays);                      // 9125
    Console.WriteLine(age.IsOfSwedishLegalAge);            // True
    Console.WriteLine(age.IsSwedishCriminallyResponsible); // True
    Console.WriteLine(age.IsOfSwedishRetirementAge);       // False
    Console.WriteLine(age.ToString());                     // "25 år"
}

// Parse months and days
PersonAge.IsValid("8 månader");                            // true
PersonAge.IsValid("300 dagar");                            // true
PersonAge.Format("8 months");                              // "8 månader"
PersonAge.Normalize("25 år");                              // "25"

// Factory methods
var fromYears = PersonAge.FromYears(17);
Console.WriteLine(fromYears.IsOfSwedishLegalAge);          // False

var infant = PersonAge.FromMonths(8);
Console.WriteLine(infant.ToString());                      // "8 månader"

// From birth date (uses TimeProvider.System for today, or pass explicit reference date)
var fromBirth = PersonAge.FromBirthDate(new DateOnly(2000, 1, 15), new DateOnly(2025, 6, 1));
Console.WriteLine(fromBirth.Years);                        // 25
Console.WriteLine(fromBirth.TotalMonths);                  // 304
```

#### Personal identity number

A Swedish personal identity number (*personnummer*) is a national identification number assigned by Skatteverket at birth or immigration. The format is YYYYMMDD-NNNC where the last digit is a Luhn check digit.

The 12-digit form is `YYYYMMDDNNNC`; the 10-digit display form is `YYMMDD-NNNC` (with `-` for age under 100, `+` for 100 or older). The birth number (digits 7–9 in the 10-digit form) encodes gender: odd = male, even = female. The check digit (C) is a Luhn (mod-10) digit computed on the 10-digit form (without century). The normalized form is 12 digits: `199908072391`; `Format()` returns the 10-digit display form: `990807-2391`. Underlying parsing is provided by ActiveLogin.Identity.

Tests: [SwedishPersonalIdentityNumberTests.cs](test/Buildi.Primitives.Tests/Person/SwedishPersonalIdentityNumberTests.cs)

- [Skatteverket - Personnummer](https://skatteverket.se/privat/folkbokforing/personnummer.4.3810a01c150939e893f18c29.html)
- [Wikipedia - Personnummer i Sverige](https://sv.wikipedia.org/wiki/Personnummer_i_Sverige)
- [Wikipedia - Luhn algorithm](https://en.wikipedia.org/wiki/Luhn_algorithm)
- [ActiveLogin.Identity](https://github.com/ActiveLogin/ActiveLogin.Identity) - underlying parsing and validation

```csharp
using Buildi.Primitives.Person;

if (SwedishPersonalIdentityNumber.TryParse("990807-2391", out var pin))
{
    Console.WriteLine(pin.Formatted);          // 990807-2391
    Console.WriteLine(pin.Value);              // 199908072391
    Console.WriteLine(pin.DateOfBirthHint);    // 1999-08-07
    Console.WriteLine(pin.AgeHint);            // 26 (approximate age based on birth date)
    Console.WriteLine(pin.GenderHint);         // Male
    Console.WriteLine(pin.To10DigitString());  // 990807-2391
    Console.WriteLine(pin.To12DigitString());  // 199908072391
}

SwedishPersonalIdentityNumber.IsValid("990807-2391");       // true
SwedishPersonalIdentityNumber.Format("199908072391");       // "990807-2391"
SwedishPersonalIdentityNumber.Normalize("990807-2391");     // "199908072391"
```

#### Coordination number

A Swedish coordination number (*samordningsnummer*) is assigned by Skatteverket to individuals not registered in the Swedish population register. It uses the same format as a personal identity number but with 60 added to the birth day.

The format is identical to a personnummer except the day component has 60 added (e.g., day `04` becomes `64`, making the day range 61–91). The `RealDay` property extracts the actual birth day by subtracting 60. The check digit is computed identically using the Luhn algorithm. Underlying parsing is provided by ActiveLogin.Identity.

Tests: [SwedishCoordinationNumberTests.cs](test/Buildi.Primitives.Tests/Person/SwedishCoordinationNumberTests.cs)

- [Skatteverket - Samordningsnummer](https://skatteverket.se/privat/folkbokforing/samordningsnummer.4.5c281c7015abecc2e201130b.html)
- [Wikipedia - Samordningsnummer](https://sv.wikipedia.org/wiki/Samordningsnummer)
- [ActiveLogin.Identity](https://github.com/ActiveLogin/ActiveLogin.Identity) - underlying parsing and validation

```csharp
if (SwedishCoordinationNumber.TryParse("680164-2395", out var cn))
{
    Console.WriteLine(cn.Formatted);          // 680164-2395
    Console.WriteLine(cn.Value);              // 196801642395
    Console.WriteLine(cn.RealDay);            // 4 (birth day with 60 subtracted)
    Console.WriteLine(cn.DateOfBirthHint);    // 1968-01-04 (null when birth date is uncertain)
    Console.WriteLine(cn.AgeHint);            // 57 (approximate age; null when birth date is uncertain)
    Console.WriteLine(cn.GenderHint);         // Male
    Console.WriteLine(cn.To10DigitString());  // 680164-2395
    Console.WriteLine(cn.To12DigitString());  // 196801642395
}

SwedishCoordinationNumber.IsValid("680164-2395");       // true
SwedishCoordinationNumber.Format("196801642395");       // "680164-2395"
SwedishCoordinationNumber.Normalize("680164-2395");     // "196801642395"
```

---

### Person ↔ Organization conversions

Extension methods in `OrganizationExtensions` let you convert between person numbers, organization numbers, and VAT numbers. These are useful when working with sole traders (*enskild firma*) - in Sweden, a sole trader's personal identity number and their organization number are the same number, and the VAT number is derived from it. All conversions are in-memory using the formats defined by Skatteverket; no external lookup is performed.

```csharp
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;

// Personal identity number ↔ organization number
var pin = SwedishPersonalIdentityNumber.Parse("990807-2391");
var orgFromPin = pin.ToSwedishOrganizationNumber(); // same digits, org-number form
var vatFromPin  = pin.ToEuVatNumber();                // SE + digits + "01"

// Coordination number ↔ organization number
var coord = SwedishCoordinationNumber.Parse("680164-2395");
var orgFromCoord = coord.ToSwedishOrganizationNumber();
var vatFromCoord  = coord.ToEuVatNumber();

// Organization number → personal identity number / coordination number (returns null when not applicable)
var org = SwedishOrganizationNumber.Parse("990807-2391");
var pinFromOrg    = org.ToPersonalIdentityNumber();  // SwedishPersonalIdentityNumber? - non-null when person-based
var coordFromOrg  = org.ToCoordinationNumber();       // SwedishCoordinationNumber? - non-null when coord-based
var vatFromOrg    = org.ToEuVatNumber();               // EuVatNumber - always valid for any org number

// VAT number → Swedish types (returns null when not a Swedish VAT)
var vat = EuVatNumber.Parse("SE559246042101");
var orgFromVat  = vat.ToSwedishOrganizationNumber(); // SwedishOrganizationNumber?
var pinFromVat  = vat.ToPersonalIdentityNumber();    // SwedishPersonalIdentityNumber?
var coordFromVat = vat.ToCoordinationNumber();        // SwedishCoordinationNumber?
```

---

### Measurement

All measurement types live in the `Buildi.Primitives.Measurement` namespace. Each type stores a value in a canonical SI base unit internally and supports parsing from multiple unit notations (metric and imperial where applicable). Values can be converted between units using typed properties or the generic `In(unit)` method. All measurement types implement `IComparable<T>` and `IEquatable<T>`.

#### Length

A length/distance value (*längd*) stored in meters with conversions to mm, cm, km, inches, feet, yards, miles, nautical miles, and Swedish miles (*mil* = 10 km).

Tests: [LengthTests.cs](test/Buildi.Primitives.Tests/Measurement/LengthTests.cs)

```csharp
using Buildi.Primitives.Measurement;

if (Length.TryParse("5.5 km", out var length))
{
    Console.WriteLine(length.Kilometers);   // 5.5
    Console.WriteLine(length.Meters);       // 5500
    Console.WriteLine(length.Miles);        // 3.417...
}

Length.IsValid("10 cm");                    // true
Length.Format("1000 m");                    // "1000 m"
Length.Normalize("1000 m");                 // "1000 m"
```

#### Area

Area (*area*) stored in square meters with conversions to m², km², hectares, acres, square feet, etc.

Tests: [AreaTests.cs](test/Buildi.Primitives.Tests/Measurement/AreaTests.cs)

```csharp
if (Area.TryParse("2.5 ha", out var area))
{
    Console.WriteLine(area.Hectares);       // 2.5
    Console.WriteLine(area.SquareMeters);   // 25000
}

Area.IsValid("100 m²");                    // true
```

#### Volume

Volume (*volym*) stored in liters with conversions to mL, L, m³, gallons, etc.

Tests: [VolumeTests.cs](test/Buildi.Primitives.Tests/Measurement/VolumeTests.cs)

```csharp
if (Volume.TryParse("500 ml", out var vol))
{
    Console.WriteLine(vol.Milliliters);     // 500
    Console.WriteLine(vol.Liters);          // 0.5
}

Volume.IsValid("1 gal");                   // true
```

#### Weight

A mass/weight value (*vikt*) stored in kilograms with conversions to mg, g, kg, metric tons, pounds, ounces, and stones.

Tests: [WeightTests.cs](test/Buildi.Primitives.Tests/Measurement/WeightTests.cs)

```csharp
if (Weight.TryParse("2.5 kg", out var weight))
{
    Console.WriteLine(weight.Kilograms);    // 2.5
    Console.WriteLine(weight.Grams);        // 2500
    Console.WriteLine(weight.Pounds);       // 5.511...
}

Weight.IsValid("500 g");                    // true
Weight.Format("2500 g");                    // "2500 g"
Weight.Normalize("2500 g");                 // "2500 g"
```

#### Energy

Energy (*energi*) stored in joules with conversions to kJ, kWh, Wh, calories, kcal, BTU, etc.

Tests: [EnergyTests.cs](test/Buildi.Primitives.Tests/Measurement/EnergyTests.cs)

```csharp
if (Energy.TryParse("3.6 kWh", out var energy))
{
    Console.WriteLine(energy.KilowattHours);   // 3.6
    Console.WriteLine(energy.Joules);          // 12960000
}

Energy.IsValid("100 kcal");                    // true
```

#### Power

Power (*effekt*) stored in watts with conversions to kW, MW, horsepower (metric and mechanical), etc.

Tests: [PowerTests.cs](test/Buildi.Primitives.Tests/Measurement/PowerTests.cs)

```csharp
if (Power.TryParse("150 hk", out var power))
{
    Console.WriteLine(power.MetricHorsepower);   // 150
    Console.WriteLine(power.Kilowatts);          // 110.325...
}

Power.IsValid("75 kW");                          // true
```

#### Voltage

Electric potential (*spänning*) stored in volts with conversions to mV, V, and kV.

Tests: [VoltageTests.cs](test/Buildi.Primitives.Tests/Measurement/VoltageTests.cs)

```csharp
if (Voltage.TryParse("230 V", out var voltage))
{
    Console.WriteLine(voltage.Volts);         // 230
    Console.WriteLine(voltage.Millivolts);    // 230000
    Console.WriteLine(voltage.Kilovolts);     // 0.23
}

Voltage.IsValid("3.3 V");                    // true
```

#### Electric charge

Electric charge (*elektrisk laddning*) stored in ampere-hours with conversions to Ah, mAh, and coulombs.

Tests: [ElectricChargeTests.cs](test/Buildi.Primitives.Tests/Measurement/ElectricChargeTests.cs)

```csharp
if (ElectricCharge.TryParse("5000 mAh", out var charge))
{
    Console.WriteLine(charge.MilliampereHours);   // 5000
    Console.WriteLine(charge.AmpereHours);        // 5
}

ElectricCharge.IsValid("100 Ah");                 // true
```

#### Torque

Torque (*vridmoment*) stored in newton-meters with conversions to Nm, ft-lb, and kgf·m.

Tests: [TorqueTests.cs](test/Buildi.Primitives.Tests/Measurement/TorqueTests.cs)

```csharp
if (Torque.TryParse("350 Nm", out var torque))
{
    Console.WriteLine(torque.NewtonMeters);   // 350
}

Torque.IsValid("250 ft-lb");                 // true
```

#### Frequency

Frequency (*frekvens*) stored in hertz with conversions to Hz, kHz, MHz, GHz, and RPM.

Tests: [FrequencyTests.cs](test/Buildi.Primitives.Tests/Measurement/FrequencyTests.cs)

```csharp
if (Frequency.TryParse("2.4 GHz", out var freq))
{
    Console.WriteLine(freq.Gigahertz);    // 2.4
    Console.WriteLine(freq.Megahertz);    // 2400
}

Frequency.IsValid("3000 RPM");           // true
```

#### Speed

Speed (*hastighet*) stored in meters per second with conversions to m/s, km/h, mph, and knots.

Tests: [SpeedTests.cs](test/Buildi.Primitives.Tests/Measurement/SpeedTests.cs)

```csharp
if (Speed.TryParse("120 km/h", out var speed))
{
    Console.WriteLine(speed.KilometersPerHour);   // 120
    Console.WriteLine(speed.MetersPerSecond);     // 33.333...
}

Speed.IsValid("60 mph");                          // true
```

#### Temperature

An absolute temperature (*temperatur*) stored in kelvin with correct offset conversions between Celsius, Fahrenheit, and kelvin scales.

Tests: [TemperatureTests.cs](test/Buildi.Primitives.Tests/Measurement/TemperatureTests.cs)

```csharp
if (Temperature.TryParse("100 °C", out var temp))
{
    Console.WriteLine(temp.Celsius);       // 100
    Console.WriteLine(temp.Fahrenheit);    // 212
    Console.WriteLine(temp.Kelvin);        // 373.15
}

Temperature.IsValid("72 °F");             // true
Temperature.Format("0 °C");              // "0 °C"
Temperature.Normalize("32 °F");          // "32 °F"
```

#### Data size

Data size (*datastorlek*) stored in bytes with conversions using both SI (KB, MB, GB) and binary (KiB, MiB, GiB) prefixes.

Tests: [DataSizeTests.cs](test/Buildi.Primitives.Tests/Measurement/DataSizeTests.cs)

```csharp
if (DataSize.TryParse("1.5 GB", out var ds))
{
    Console.WriteLine(ds.Gigabytes);       // 1.5
    Console.WriteLine(ds.Megabytes);       // 1500
}

DataSize.IsValid("512 MB");               // true
```

#### Pressure

Pressure (*tryck*) stored in pascals with conversions to Pa, hPa, kPa, bar, mbar, PSI, and atm.

Tests: [PressureTests.cs](test/Buildi.Primitives.Tests/Measurement/PressureTests.cs)

```csharp
if (Pressure.TryParse("1013.25 hPa", out var pressure))
{
    Console.WriteLine(pressure.Hectopascals);   // 1013.25
    Console.WriteLine(pressure.Bars);           // 1.01325
}

Pressure.IsValid("32 PSI");                    // true
```

#### Percentage

A percentage (*procent*) stored as a 0–1 fraction internally, displayed as 0–100%. Accepts `%` suffix or word suffixes like `procent`/`percent`.

Tests: [PercentageTests.cs](test/Buildi.Primitives.Tests/Measurement/PercentageTests.cs)

```csharp
if (Percentage.TryParse("85%", out var pct))
{
    Console.WriteLine(pct.Percent);    // 85
    Console.WriteLine(pct.Value);      // 0.85
}

Percentage.IsValid("25 procent");      // true
Percentage.Format("0.5");             // "0.5"
Percentage.Normalize("50%");          // "50%"
```

#### Sound level

Sound pressure level (*ljudnivå*) with optional frequency weighting. Stores decibels and a `SoundWeighting` (A, B, C, Z, or Unweighted). Parses various dB formats including `dB`, `dB(A)`, `dBA`, `dB(C)`, etc. Note that conversions between different weightings are not performed since they are frequency-dependent.

Tests: [SoundLevelTests.cs](test/Buildi.Primitives.Tests/Measurement/SoundLevelTests.cs)

- [Wikipedia — A-weighting](https://en.wikipedia.org/wiki/A-weighting)
- [Wikipedia — Sound pressure level](https://en.wikipedia.org/wiki/Sound_pressure#Sound_pressure_level)

```csharp
using Buildi.Primitives.Measurement;

if (SoundLevel.TryParse("69 dB(A)", out var sound))
{
    Console.WriteLine(sound.Decibels);     // 69
    Console.WriteLine(sound.Weighting);    // A
}

SoundLevel.IsValid("85 dB");               // true
SoundLevel.IsValid("69 dBA");              // true
SoundLevel.Normalize("69 DB ( A )");       // "69 dB(A)"
```

#### Rotational speed

Rotational speed (*rotationshastighet* / *varvtal*) stored internally as revolutions per minute (RPM). Parses multiple units: RPM (`rpm`, `r/min`, `varv/min`), revolutions per second (`rps`, `r/s`), and radians per second (`rad/s`). Provides computed conversions between all supported units.

Tests: [RotationalSpeedTests.cs](test/Buildi.Primitives.Tests/Measurement/RotationalSpeedTests.cs)

```csharp
if (RotationalSpeed.TryParse("100 rps", out var speed))
{
    Console.WriteLine(speed.Rpm);                  // 6000
    Console.WriteLine(speed.Rps);                  // 100
    Console.WriteLine(speed.RadiansPerSecond);     // 628.318...
    Console.WriteLine(speed.OriginalUnit);         // Rps
}

RotationalSpeed.IsValid("5200 rpm");               // true
RotationalSpeed.IsValid("100 varv/min");           // true
RotationalSpeed.Normalize("100 rps");              // "6000 rpm"
```

#### Electric Current

Electric current value (*elektrisk ström*) supporting amperes with SI prefixes (µA, mA, A, kA).

Tests: [ElectricCurrentTests.cs](test/Buildi.Primitives.Tests/Measurement/ElectricCurrentTests.cs)

- [BIPM SI base units](https://www.bipm.org/en/measurement-units/si-base-units) — ampere definition
- [Wikipedia — Ampere](https://en.wikipedia.org/wiki/Ampere)

```csharp
ElectricCurrent.TryParse("16 A", out var current);
// current.Amperes → 16, current.Milliamperes → 16000

ElectricCurrent.IsValid("500 mA");    // true
ElectricCurrent.Format("500 mA");     // "500 mA"
ElectricCurrent.Normalize("2.5 kA");  // "2500 A"
```

#### Flow Rate

Volumetric flow rate (*flöde*) for pumps, ventilation, and hydraulic systems. Supports L/min, L/h, L/s, m³/h, m³/min, gal/min.

Tests: [FlowRateTests.cs](test/Buildi.Primitives.Tests/Measurement/FlowRateTests.cs)

- [Wikipedia — Volumetric flow rate](https://en.wikipedia.org/wiki/Volumetric_flow_rate)

```csharp
FlowRate.TryParse("100 L/min", out var flow);
// flow.LitersPerMinute → 100, flow.LitersPerHour → 6000

FlowRate.IsValid("10 m³/h");      // true
FlowRate.Format("50 l/min");      // "50 L/min"
FlowRate.Normalize("60 L/h");     // "1 L/min"
```

#### Luminous Flux

Luminous flux (*ljusflöde*) for lighting products. Supports lumens (lm) and kilolumens (klm).

Tests: [LuminousFluxTests.cs](test/Buildi.Primitives.Tests/Measurement/LuminousFluxTests.cs)

- [BIPM SI derived units](https://www.bipm.org/en/measurement-units/si-derived-units) — lumen definition
- [Wikipedia — Lumen](https://en.wikipedia.org/wiki/Lumen_(unit))

```csharp
LuminousFlux.TryParse("800 lm", out var flux);
// flux.Lumens → 800, flux.Kilolumens → 0.8

LuminousFlux.IsValid("2.5 klm");      // true
LuminousFlux.Format("800 lm");        // "800 lm"
LuminousFlux.Normalize("2.5 klm");    // "2500 lm"
```

#### Natural unit display

Measurement types that span many orders of magnitude support automatic selection of the most human-readable unit via `NaturalUnit` and `ToNaturalString()`. The algorithm picks the largest unit in the type's `NaturalScale` where the absolute value is at least 1 — so bytes become KB/MB/GB, grams become kg/tonnes, and milliwatts become W/kW as appropriate.

Tests: [NaturalUnitTests.cs](test/Buildi.Primitives.Tests/Measurement/NaturalUnitTests.cs)

```csharp
using Buildi.Primitives.Measurement;

// DataSize — bytes to the most readable unit
DataSize.Parse("549755813888 B").ToNaturalString();    // "512 GB"
DataSize.Parse("1536 B").ToNaturalString();            // "1.5 KB"
DataSize.Parse("42 B").ToNaturalString();              // "42 B"

// Weight — grams to the most readable unit
Weight.Parse("2500 g").ToNaturalString();              // "2.5 kg"
Weight.Parse("1500000 g").ToNaturalString();           // "1.5 t"

// Power — watts to the most readable unit
Power.Parse("1500 W").ToNaturalString();               // "1.5 kW"
Power.Parse("750 W").ToNaturalString();                // "750 W"

// Control decimal places
DataSize.Parse("1536 B").ToNaturalString(decimals: 0); // "2 KB"
DataSize.Parse("1536 B").ToNaturalString(decimals: 4); // "1.5 KB"

// Access the selected unit directly
var size = DataSize.Parse("512 GB");
Console.WriteLine(size.NaturalUnit);                   // Gigabyte
Console.WriteLine(size.NaturalUnit.Symbol);            // "GB"
```

Each unit type defines a `NaturalScale` — an ordered list of units from smallest to largest that are appropriate for everyday display. Niche, binary (IEC), or non-metric units are excluded from the natural scale to keep output predictable:

| Type | Natural scale |
|------|---------------|
| DataSize | B → KB → MB → GB → TB → PB → EB |
| Length | mm → cm → m → km |
| Weight | mg → g → kg → t |
| Area | mm² → cm² → m² → ha → km² |
| Volume | mL → cL → dL → L → m³ |
| Energy | mWh → Wh → kWh → MWh |
| Power | mW → W → kW → MW → GW |
| Frequency | Hz → kHz → MHz → GHz → THz |
| Voltage | mV → V → kV |
| ElectricCharge | mAh → Ah → kAh |
| Pressure | Pa → hPa → kPa → bar → MPa |

Product and vehicle wrapper types (`StorageCapacity`, `RamCapacity`, `ProcessorSpeed`, `BatteryCapacity`, `EnginePower`, `EngineDisplacement`, `OdometerReading`) also expose `ToNaturalString()`, delegating to their inner measurement type.

## Masking sensitive data

Many types in this library may contain personally identifiable information (PII) - personal identity numbers, bank accounts, phone numbers, email addresses, and person names. To support safe logging, audit trails, customer-facing UIs, and other contexts where you need to display a value without revealing all of it, each sensitive type provides a `ToMaskedString()` extension method.

```csharp
using Buildi.Primitives.Person;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;

// Personal identity number - birth date visible, individual digits masked
SwedishPersonalIdentityNumber.Parse("990807-2391").ToMaskedString();  // "990807-****"

// Organization number - person-based numbers are auto-masked, legal entities are not
SwedishOrganizationNumber.Parse("990807-2391").ToMaskedString();      // "990807-****"  (sole trader)
SwedishOrganizationNumber.Parse("559246-0421").ToMaskedString();      // "559246-0421"  (company, public info)

// Bank account - clearing number visible, account masked
SwedishBankAccount.Parse("50011234567").ToMaskedString();             // "5001-*******"

// Email - first character + domain visible
EmailAddress.Parse("peter@example.com").ToMaskedString();            // "p***@example.com"

// Phone - area code visible, subscriber digits masked, last 2 visible
PhoneNumber.Parse("0701740633").ToMaskedString();                    // "0701-** ** 33"

// Person name - fully masked by default, initials as alternative
PersonFullName.Parse("Anna Maria Andersson").ToMaskedString();       // "*** *** ***"
PersonFullName.Parse("Anna Maria Andersson").ToMaskedString(useInitials: true); // "A. M. A."
```

All masking methods use `*` as the mask character and are designed with sensible defaults - person-based identifiers are always masked, public identifiers (like company org numbers) are left unmasked unless you explicitly opt in.

### Summary of masked types

The following table lists every type that provides a `ToMaskedString()` extension method, organized by namespace.

**Person** (`Buildi.Primitives.Person`) - `PersonMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `SwedishPersonalIdentityNumber` | `990807-****` | `maskBirthDate: true` → `******-****` |
| `SwedishCoordinationNumber` | `680164-****` | `maskBirthDate: true` → `******-****` |
| `PersonGivenName` | `*** ***` (each name masked) | `useInitials: true` → `A. M.` |
| `PersonFamilyName` | `***` | `useInitials: true` → `A.` |
| `PersonFullName` | `*** *** ***` (all parts masked) | `useInitials: true` → `A. M. A.`, `showGivenName: true` → `Anna Maria ***` |
| `PersonAge` | `** år` | - |

**Organization** (`Buildi.Primitives.Organization`) - `OrganizationMaskingExtensions`, `SwedishOrganizationIdentifierMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `SwedishOrganizationNumber` (person) | `990807-****` | `maskBirthDate: true` → `******-****` |
| `SwedishOrganizationNumber` (legal entity) | `559246-0421` (unmasked) | `maskOrganizationNumbers: true` → `559246-****` |
| `EuVatNumber` (SE, person-based) | `SE990807****01` (auto-detected) | - |
| `EuVatNumber` (SE, non-person) | `SE559246042101` (unmasked) | `alwaysMask: true` → `SE559246****01` |
| `EuVatNumber` (non-SE) | unmasked | `alwaysMask: true` → `DE1234*****` |
| `LeiCode` | `5493****************` (LOU prefix visible) | - |
| `DunsNumber` | `*********` | - |
| `SwedishOrganizationName` | `V**** C*** AB` (first letter per word) | - |

**Banking** (`Buildi.Primitives.Banking`) - `BankingMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `SwedishBankAccount` | `5100-*******` (clearing visible) | `maskClearingNumber: true` → `****-*******` |
| `Iban` | `SE45 **** **** **** **** ****` | - |
| `SwedishBankgiroNumber` | `****-6201` (last digits visible) | `showLastDigits: false` → `5805-****` |
| `SwedishPostgiroNumber` | `*******-3` (control digit visible) | `showControlDigit: false` → `4779202-*` |
| `Bic` | `****SE**` (country code visible) | - |
| `SwedishOcrReferenceNumber` | `***********` | - |
| `SwedishBankClearingNumber` | `5***` (first digit visible) | - |
| `SwedishBankAccountHolderName` | `A*** A********` (first letter per word) | - |

**Contact** (`Buildi.Primitives.Contact`) - `ContactMaskingExtensions`, `AddressMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `PhoneNumber` (Swedish) | `0701-** ** 33` (area code + last 2) | `visibleDigitsAtEnd: 0` → all subscriber digits masked |
| `PhoneNumber` (international) | `+44********58` | same parameter |
| `SwedishAddress` | `Storgatan **, *** ** Stockholm` | delegates to `Address` masking |
| `SwedishAddressZipCode` | `*** **` | delegates to `AddressZipCode` masking |
| `PolishAddress`, `BritishAddress`, etc. | delegates to `Address` masking | all 31 country-specific types |
| `PolishAddressZipCode`, `IrishAddressZipCode`, etc. | delegates to `AddressZipCode` masking | all 31 country-specific zip types |
| `AddressStreet` | `Storgatan **` (street name visible, number masked) | - |
| `AddressCity` | `S********` (first letter visible) | - |

**Web** (`Buildi.Primitives.Web`) - `WebMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `EmailAddress` | `p***@example.com` | `maskDomain: true` → `p***@e***.com` |
| `Url` (hierarchical) | `https://example.com/***` (scheme + host visible) | - |
| `Url` (non-hierarchical) | `mailto:***` (scheme only) | - |

**Geography** (`Buildi.Primitives.Geography`) - `GeographyMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `GeoCoordinate` | `59.3***, 18.0***` (≈11 km precision) | - |
| `SwedishMunicipality` | `S********` (first letter visible) | - |
| `SwedishCounty` | `S************ ***` (first letter visible) | - |

**Finance** (`Buildi.Primitives.Finance`) - `FinanceMaskingExtensions`

| Type | Default | Options |
| ---- | ------- | ------- |
| `MoneyAmount` | `*** SEK` (amount hidden, currency visible) | - |
| `Isin` | `SE**********` (country code visible) | - |

### Person masking

Extension methods in `PersonMaskingExtensions` mask personal identity numbers, coordination numbers, and person names.

Tests: [PersonMaskingExtensionsTests.cs](test/Buildi.Primitives.Tests/Person/PersonMaskingExtensionsTests.cs)

```csharp
using Buildi.Primitives.Person;

// Personal identity number
var pin = SwedishPersonalIdentityNumber.Parse("990807-2391");
pin.ToMaskedString();                     // "990807-****"
pin.ToMaskedString(maskBirthDate: true);  // "******-****"

// Coordination number
var cn = SwedishCoordinationNumber.Parse("680164-2395");
cn.ToMaskedString();                     // "680164-****"
cn.ToMaskedString(maskBirthDate: true);  // "******-****"

// Given name - fully masked by default, initials as alternative
PersonGivenName.Parse("Anna Maria").ToMaskedString();                  // "*** ***"
PersonGivenName.Parse("Anna Maria").ToMaskedString(useInitials: true); // "A. M."

// Family name - fully masked by default, initial as alternative
PersonFamilyName.Parse("Andersson").ToMaskedString();                  // "***"
PersonFamilyName.Parse("Andersson").ToMaskedString(useInitials: true); // "A."

// Full name - fully masked by default, with options
var name = PersonFullName.Parse("Anna Maria Andersson");
name.ToMaskedString();                                             // "*** *** ***"
name.ToMaskedString(useInitials: true);                            // "A. M. A."
name.ToMaskedString(showGivenName: true);                          // "Anna Maria ***"
name.ToMaskedString(showGivenName: true, useInitials: true);       // "Anna Maria A."

// Age - numeric value masked, unit preserved
PersonAge.Parse("25").ToMaskedString();                             // "** år"
PersonAge.Parse("8 månader").ToMaskedString();                      // "* månader"
```

### Organization masking

Extension methods in `OrganizationMaskingExtensions` mask organization numbers and VAT numbers. Organization numbers that represent a person (sole trader / *enskild firma*) are automatically masked since they contain a personal identity number. Legal entity numbers are public information and returned unmasked by default.

Tests: [OrganizationMaskingExtensionsTests.cs](test/Buildi.Primitives.Tests/Organization/OrganizationMaskingExtensionsTests.cs)

```csharp
using Buildi.Primitives.Organization;

// Person-based org number (sole trader) - always masked
var person = SwedishOrganizationNumber.Parse("990807-2391");
person.ToMaskedString();                       // "990807-****"
person.ToMaskedString(maskBirthDate: true);    // "******-****"

// Legal entity org number - unmasked by default (public info)
var company = SwedishOrganizationNumber.Parse("559246-0421");
company.ToMaskedString();                                      // "559246-0421"
company.ToMaskedString(maskOrganizationNumbers: true);         // "559246-****"

// Swedish VAT - auto-detects person-based org numbers
EuVatNumber.Parse("SE990807239101").ToMaskedString();            // "SE990807****01"
EuVatNumber.Parse("SE559246042101").ToMaskedString();            // "SE559246042101" (company)
EuVatNumber.Parse("SE559246042101").ToMaskedString(alwaysMask: true); // "SE559246****01"

// Non-SE VAT - unmasked by default
EuVatNumber.Parse("DE123456789").ToMaskedString();               // "DE123456789"
EuVatNumber.Parse("DE123456789").ToMaskedString(alwaysMask: true); // "DE1234*****"

// Organization name - first letter of each word visible
SwedishOrganizationName.Parse("Volvo Cars AB").ToMaskedString();   // "V**** C*** AB"
```

### Banking masking

Extension methods in `BankingMaskingExtensions` mask bank accounts, IBAN, Bankgiro, and Plusgiro numbers.

Tests: [BankingMaskingExtensionsTests.cs](test/Buildi.Primitives.Tests/Banking/BankingMaskingExtensionsTests.cs)

```csharp
using Buildi.Primitives.Banking;

// Swedish bank account - clearing number visible, account masked
var account = SwedishBankAccount.Parse("50011234567");
account.ToMaskedString();                           // "5001-*******"
account.ToMaskedString(maskClearingNumber: true);   // "****-*******"

// IBAN - country code + check digits visible
Iban.Parse("SE4550000000058398257466").ToMaskedString();  // "SE45 **** **** **** **** ****"

// Bankgiro - last digits visible by default
var bg = SwedishBankgiroNumber.Parse("54649652");
bg.ToMaskedString();                        // "****-9652"
bg.ToMaskedString(showLastDigits: false);   // "5464-****"

// Plusgiro - control digit visible by default
var pg = SwedishPostgiroNumber.Parse("47792023");
pg.ToMaskedString();                          // "*******-3"
pg.ToMaskedString(showControlDigit: false);   // "4779202-*"

// Clearing number - first digit visible (identifies bank range)
SwedishBankClearingNumber.Parse("5001").ToMaskedString();           // "5***"

// Account holder name - first letter per word visible
SwedishBankAccountHolderName.Parse("Anna Andersson").ToMaskedString(); // "A*** A********"
```

### Contact masking

Extension methods in `ContactMaskingExtensions` and `AddressMaskingExtensions` mask phone numbers, street addresses, and city names. Phone masking preserves the formatted separators (dashes and spaces) and reveals the area/country prefix plus a configurable number of trailing digits.

Tests: [ContactMaskingExtensionsTests.cs](test/Buildi.Primitives.Tests/Contact/ContactMaskingExtensionsTests.cs)

```csharp
using Buildi.Primitives.Contact;

// Swedish phone - area code prefix + last 2 digits visible
var phone = PhoneNumber.Parse("0701740633");
phone.ToMaskedString();                        // "0701-** ** 33"
phone.ToMaskedString(visibleDigitsAtEnd: 0);   // "0701-** ** **"
phone.ToMaskedString(visibleDigitsAtEnd: 4);   // "0701-** 0633"

// International phone - country code + last 2 digits
PhoneNumber.Parse("+44 20 7946 0958").ToMaskedString();  // "+44********58"

// Street address - street name visible, number masked
AddressStreet.Parse("Storgatan 12").ToMaskedString();    // "Storgatan **"

// City name - first letter visible
AddressCity.Parse("Stockholm").ToMaskedString();          // "S********"
```

### Web masking

Extension methods in `WebMaskingExtensions` mask email addresses and URLs. URL masking shows the scheme and host but obscures the path, query, and fragment; non-hierarchical URIs (e.g. `mailto:`, `tel:`) show only the scheme.

Tests: [WebMaskingExtensionsTests.cs](test/Buildi.Primitives.Tests/Web/WebMaskingExtensionsTests.cs)

```csharp
using Buildi.Primitives.Web;

// Email - first character of local part + full domain
var email = EmailAddress.Parse("peter@example.com");
email.ToMaskedString();                     // "p***@example.com"
email.ToMaskedString(maskDomain: true);     // "p***@e***.com"

// URL - scheme + host visible, path/query/fragment masked
Url.Parse("https://www.example.com/secret?key=val").ToMaskedString();  // "https://www.example.com/***?***"
Url.Parse("https://example.com:8080/path").ToMaskedString();           // "https://example.com:8080/***"
Url.Parse("https://example.com").ToMaskedString();                     // "https://example.com/" (root path unchanged)

// Non-hierarchical URIs - scheme only
Url.Parse("mailto:user@example.com").ToMaskedString();   // "mailto:***"
Url.Parse("tel:+46701234567").ToMaskedString();           // "tel:***"
```

### Geography masking

Extension methods in `GeographyMaskingExtensions` mask geographic coordinates (reducing precision to ≈11 km), municipality names, and county names.

```csharp
using Buildi.Primitives.Geography;

// Coordinate - reduced precision (1 decimal ≈ 11 km)
GeoCoordinate.Parse("59.3293, 18.0686").ToMaskedString();   // "59.3***, 18.0***"

// Municipality - first letter visible
SwedishMunicipality.Parse("Stockholm").ToMaskedString();     // "S********"

// County - first letter visible
SwedishCounty.Parse("Stockholms län").ToMaskedString();      // "S************ ***"
```

### Finance masking

Extension methods in `FinanceMaskingExtensions` mask monetary amounts (hiding the value but preserving the currency) and ISIN codes.

```csharp
using Buildi.Primitives.Finance;

// Money amount - amount hidden, currency visible
MoneyAmount.Parse("1000 SEK").ToMaskedString();              // "*** SEK"

// ISIN - country code visible
Isin.Parse("SE0000108656").ToMaskedString();                 // "SE**********"
```

## Text scanning

> **Heuristic-based - no guarantees.** Text scanning uses pattern matching and validation to find *potential* structured values in unstructured text. Candidates may be false positives, and valid values may be missed. Never use scanning results as a substitute for authoritative data extraction.

Each scannable type exposes a static `FindCandidatesInText(string text)` method that returns a list of `TextCandidate<T>` objects. Each candidate includes position information, all string forms (original, normalized, formatted, masked), a confidence level, and the parsed instance.

The `TextScanner` class orchestrates scanning for all supported types at once and returns a `TextScanResult` with typed accessors and bulk replacement methods.

### Per-type scanning

```csharp
using Buildi.Primitives.Web;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Person;
using Buildi.Primitives.Organization;

// Find emails in text
var emails = EmailAddress.FindCandidatesInText("Kontakta info@example.com för mer info.");
var candidate = emails[0];
// candidate.Value          → EmailAddress instance (the parsed object)
// candidate.StartIndex     → 9          (zero-based position in the input string)
// candidate.Length          → 16         (number of characters matched)
// candidate.EndIndex        → 25         (StartIndex + Length, exclusive)
// candidate.OriginalText    → "info@example.com"  (the raw substring from the input)
// candidate.TypeName        → "EmailAddress"       (short type name)
// candidate.Category        → TextCandidateCategory.Contact
// candidate.NormalizedForm  → "info@example.com"   (machine-comparable canonical form)
// candidate.FormattedForm   → "info@example.com"   (human-readable display form)
// candidate.MaskedForm      → "i***@e******.com"   (sensitive content masked)
// candidate.Confidence      → TextMatchConfidence.High (heuristic confidence level)

// Find personal identity numbers
var pins = SwedishPersonalIdentityNumber.FindCandidatesInText("Personnummer: 990807-2391");
// pins[0].NormalizedForm → "199908072391"
// pins[0].Confidence     → TextMatchConfidence.High

// Find organization numbers
var orgs = SwedishOrganizationNumber.FindCandidatesInText("Org.nr 559246-0421");
// orgs[0].FormattedForm → "559246-0421"
```

**Supported types for scanning:**

| Type | Category | Typical confidence |
| --- | --- | --- |
| `EmailAddress` | Contact | High |
| `PhoneNumber` | Contact | Medium |
| `Url` | Contact | High |
| `AddressZipCode` | Contact | Low |
| `Address` | Contact | Medium |
| `SwedishAddress` | Contact | Medium |
| `Country` | Geography | Low |
| `SwedishCounty` | Geography | Low |
| `SwedishMunicipality` | Geography | Low |
| `SwedishPersonalIdentityNumber` | PersonalIdentifier | High |
| `SwedishCoordinationNumber` | PersonalIdentifier | High |
| `SwedishOrganizationNumber` | OrganizationIdentifier | High |
| `EuVatNumber` | OrganizationIdentifier | High |
| `LeiCode` | OrganizationIdentifier | High |
| `DunsNumber` | OrganizationIdentifier | Low |
| `Iban` | Financial | High |
| `Bic` | Financial | Medium |
| `SwedishBankgiroNumber` | Financial | High |
| `SwedishPostgiroNumber` | Financial | Medium |
| `SwedishBankAccount` | Financial | Low |
| `SwedishOcrReferenceNumber` | Financial | Low |
| `SwedishVehicleRegistrationNumber` | Vehicle | High |
| `VehicleIdentificationNumber` | Vehicle | Medium–High |
| `SwedishPropertyDesignation` | Property | Medium |
| `Gtin13` | Product | High |
| `Gtin8` | Product | High |

### Aggregate scanner

```csharp
using Buildi.Primitives.TextScanning;

var scanner = new TextScanner();
var result = scanner.Scan("Maila info@example.com, org 559246-0421, BG 5805-6201");

// Typed accessors
result.Emails                // IReadOnlyList<TextCandidate<EmailAddress>>
result.OrganizationNumbers   // IReadOnlyList<TextCandidate<SwedishOrganizationNumber>>
result.BankgiroNumbers       // IReadOnlyList<TextCandidate<SwedishBankgiroNumber>>

// Flat lists
result.All                   // all candidates, sorted by position (may overlap)
result.ResolvedCandidates    // non-overlapping subset after resolution
result.TotalCount            // total candidate count

// Filtering by category or confidence
result.CountByCategory(TextCandidateCategory.Financial);
result.CountByConfidence(TextMatchConfidence.High);

// Scan with options
var options = new TextScannerOptions
{
    IncludeCategories = new HashSet<TextCandidateCategory>
    {
        TextCandidateCategory.PersonalIdentifier,
        TextCandidateCategory.Contact
    },
    MinimumConfidence = TextMatchConfidence.Medium
};
var filtered = scanner.Scan(text, options);
```

### Masking and redaction

```csharp
var text = "Ring 070-174 06 33 eller maila info@example.com";
var result = scanner.Scan(text);

// Mask all detected values using each type's ToMaskedString()
result.MaskAll(text);
// → "Ring ***-*** ** ** eller maila i***@e******.com"

// Redact with a fixed string
result.RedactAll(text);
// → "Ring [REDACTED] eller maila [REDACTED]"

// Custom replacement
result.ReplaceAll(text, c => $"[{c.TypeName}]");
// → "Ring [PhoneNumber] eller maila [EmailAddress]"
```

### Overlap resolution

When the same text span matches multiple types (e.g. an email whose local part is a valid org number), all interpretations are kept in `result.All`. The `result.ResolvedCandidates` list contains a non-overlapping subset determined by:

1. **Containment** - the longer enclosing match wins (e.g. the full email beats the org number embedded in it)
2. **Confidence** - higher `TextMatchConfidence` wins
3. **Length** - longer span wins
4. **Category priority** - PersonalIdentifier > Contact > Financial > OrganizationIdentifier > Vehicle > Property > Product

Replacement methods (`MaskAll`, `RedactAll`, `ReplaceAll`) operate on `ResolvedCandidates` in a single right-to-left pass, so character positions remain valid throughout the replacement.

#### Example: scanning a company description

```csharp
var aboutText = """
    Acme Sweden AB (559246-0421) är ett registrerat aktiebolag.
    Momsnr: SE559246042101. Bankgiro: 5805-6201.
    Kontakt: 5592460421@example.com eller ring 070-174 06 33.
    Besök oss på Storgatan 1, 114 53 Stockholm.
    """;

var scanner = new TextScanner();
var result = scanner.Scan(aboutText);

// result.All contains EVERY candidate, including overlapping ones.
// The email "5592460421@example.com" is detected as an EmailAddress,
// but "5592460421" (the local part) is ALSO detected as a SwedishOrganizationNumber.
// Both appear in result.All so you can inspect all interpretations:
foreach (var c in result.All)
    Console.WriteLine($"  [{c.TypeName}] \"{c.OriginalText}\" @ {c.StartIndex}..{c.EndIndex} ({c.Confidence})");

// Output (illustrative):
//   [SwedishOrganizationNumber] "559246-0421"          @ 16..27  (High)
//   [EuVatNumber]                 "SE559246042101"        @ 51..65  (High)
//   [SwedishBankgiroNumber]     "5805-6201"             @ 77..86  (High)
//   [SwedishOrganizationNumber] "5592460421"            @ 97..107 (High)   ← inside email
//   [EmailAddress]              "5592460421@example.com"@ 97..119 (High)   ← contains org number
//   [PhoneNumber]               "070-174 06 33"         @ 131..144(Medium)
//   [AddressZipCode]            "114 53"                @ 164..170(Low)

// result.ResolvedCandidates is the non-overlapping subset.
// The email "5592460421@example.com" fully contains "5592460421", so the
// email wins - the contained org number is removed from the resolved list.
foreach (var c in result.ResolvedCandidates)
    Console.WriteLine($"  [{c.TypeName}] \"{c.OriginalText}\"");

// Output (illustrative):
//   [SwedishOrganizationNumber] "559246-0421"
//   [EuVatNumber]                 "SE559246042101"
//   [SwedishBankgiroNumber]     "5805-6201"
//   [EmailAddress]              "5592460421@example.com"   ← email wins, org number removed
//   [PhoneNumber]               "070-174 06 33"
//   [AddressZipCode]            "114 53"

// MaskAll uses the resolved list - no double-masking, no position corruption:
var masked = result.MaskAll(aboutText);
// → "Acme Sweden AB (556036-****) är ett registrerat aktiebolag.
//     Momsnr: SE55********01. Bankgiro: ****-****.
//     Kontakt: 5***********@e******.com eller ring ***-*** ** **.
//     Besök oss på Storgatan 1, *** ** Stockholm."

// ReplaceAll for annotation/debugging:
var annotated = result.ReplaceAll(aboutText, c => $"[{c.TypeName}]");
// → "Acme Sweden AB ([SwedishOrganizationNumber]) är ett registrerat aktiebolag.
//     Momsnr: [EuVatNumber]. Bankgiro: [SwedishBankgiroNumber].
//     Kontakt: [EmailAddress] eller ring [PhoneNumber].
//     Besök oss på Storgatan 1, [AddressZipCode] Stockholm."
```

## Misc

### Type metadata

Every value-object type exposes a static `TypeInfo` property of type `PrimitiveTypeInfo`, providing human-readable metadata about the type itself — its English name, localized (Swedish) name, a representative emoji, and a list of reference source URLs.

```csharp
using Buildi.Primitives;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Measurement;

// Access metadata on any type
var info = SwedishOrganizationNumber.TypeInfo;
info.EnglishName;   // "Organization Number"
info.LocalizedName; // "Organisationsnummer"
info.Emoji;         // "🏢"
info.Sources;       // ["https://bolagsverket.se", "https://skatteverket.se/...", ...]

// Works on all types
Iban.TypeInfo.Emoji;                   // "🏦"
Iban.TypeInfo.LocalizedName;           // "IBAN"
SwedishCounty.TypeInfo.Emoji;          // "🏛️"
SwedishCounty.TypeInfo.LocalizedName;  // "Län"
Length.TypeInfo.Emoji;                 // "📏"
Length.TypeInfo.LocalizedName;         // "Längd"

// Useful for building UIs, documentation, and tooling
foreach (var type in new[] { SwedishOrganizationNumber.TypeInfo, Iban.TypeInfo, SwedishCounty.TypeInfo })
{
    Console.WriteLine($"{type.Emoji} {type.EnglishName} ({type.LocalizedName})");
    // 🏢 Organization Number (Organisationsnummer)
    // 🏦 IBAN (IBAN)
    // 🏛️ County (Län)
}
```

The `PrimitiveTypeInfo` record:

```csharp
public sealed record PrimitiveTypeInfo(
    string EnglishName,
    string LocalizedName,
    string Emoji,
    IReadOnlyList<string> Sources);
```

Source URLs correspond to the references listed in each type's XML documentation `<remarks>` section, linking to official standards, government agencies, and authoritative sources.

### Lookup URLs

Many types expose extension methods that generate URLs to external lookup services. These are useful for linking to public registries, maps, or search engines.

```csharp
using Buildi.Primitives.Organization;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Property;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Vehicle;

// Organization
var org = SwedishOrganizationNumber.Parse("5592460421");
org.GetBolagsverketUrl();   // https://foretagsinfo.bolagsverket.se/sok-foretagsinformation-web/foretag/559246-0421
org.GetAllabolagUrl();      // https://www.allabolag.se/5592460421

var lei = LeiCode.Parse("5493001KJTIIGC8Y1R12");
lei.GetGleifUrl();          // https://search.gleif.org/#/record/5493001KJTIIGC8Y1R12

// Banking
var bg = SwedishBankgiroNumber.Parse("235-9321");
bg.GetBankgirotUrl();       // https://www.bankgirot.se/sok-bankgironummer/?bgnr=235-9321

// Contact
var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "SE");
address.GetGoogleMapsUrl(); // https://www.google.com/maps/search/?api=1&query=...
address.GetBingMapsUrl();   // https://www.bing.com/maps?q=...

var phone = PhoneNumber.Parse("+46701740633");
phone.GetHittaUrl();        // https://www.hitta.se/sök?vad=...

// Property
var prop = SwedishPropertyDesignation.Parse("Stockholm Söder 75:2");
prop.GetLantmaterietUrl();  // https://minkarta.lantmateriet.se/?search=...
prop.GetHittaUrl();         // https://www.hitta.se/sök?vad=...

// Geography
var county = SwedishCounty.Parse("01");
county.GetScbUrl();         // https://www.scb.se/hitta-statistik/...
county.GetWikipediaUrl();   // https://sv.wikipedia.org/wiki/Stockholms_län
county.GetGoogleMapsUrl();  // https://www.google.com/maps/search/?api=1&query=...

var muni = SwedishMunicipality.Parse("0180");
muni.GetScbUrl();           // https://www.scb.se/hitta-statistik/...
muni.GetWikipediaUrl();     // https://sv.wikipedia.org/wiki/Stockholms_kommun
muni.GetGoogleMapsUrl();    // https://www.google.com/maps/search/?api=1&query=...

// Vehicle
var reg = SwedishVehicleRegistrationNumber.Parse("ABC123");
reg.GetBiluppgifterUrl();       // https://biluppgifter.se/fordon/ABC123
reg.GetCarInfoUrl();            // https://www.car.info/sv-se/license-plate/S/ABC123
```

### Reference-data versioning

All reference data (countries, currencies, counties, municipalities, bank clearing numbers, EU/EEA/Schengen/SEPA membership lists, email provider lists, postal patterns) is embedded at build time. The `ReferenceData` class lets you inspect the source and last-verified date for every packaged dataset.

```csharp
using Buildi.Primitives;

// Inspect a specific dataset
var counties = ReferenceData.SwedishCounties;
Console.WriteLine(counties.Name);          // SwedishCounties
Console.WriteLine(counties.EntryCount);    // 21
Console.WriteLine(counties.Source);        // SCB
Console.WriteLine(counties.SourceUrl);     // https://www.scb.se/...
Console.WriteLine(counties.LastVerified);  // 2025-03-01
Console.WriteLine(counties.Description);  // 21 Swedish counties (län) with ...

// Enumerate all datasets
foreach (var ds in ReferenceData.All)
    Console.WriteLine($"{ds.Name}: {ds.Source}, verified {ds.LastVerified}");
```

Available datasets: `SwedishCounties`, `SwedishMunicipalities`, `Countries`, `EuropeanUnionMembers`, `EeaMembers`, `SchengenMembers`, `SepaMembers`, `Currencies`, `SniCodes`, `SwedishBankClearingNumbers`, `PublicEmailProviders`, `PostalPatterns`, `Gs1Prefixes`.

### Language-aware formatting

Types that have both Swedish and English names expose three display methods: `ToDisplayString()`, `ToEnglishString()`, and `ToNativeString()`. `ToString()` delegates to `ToDisplayString()`. The language returned by `ToDisplayString()` is controlled by `PrimitivesDefaults.UICulture` (defaults to Swedish).

- `ToDisplayString()` — returns the name in the current UI culture (Swedish by default).
- `ToEnglishString()` — always returns the English name.
- `ToNativeString()` — always returns the native/endonym name (for Country: the country's own local name; for Swedish types: Swedish).

```csharp
using Buildi.Primitives;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

// Country
var country = Country.Parse("DE");
country.ToDisplayString();  // "Tyskland" (Swedish by default)
country.ToEnglishString();  // "Germany"
country.ToNativeString();   // "Deutschland"
country.ToString();         // "Tyskland" (delegates to ToDisplayString)

// County
var county = SwedishCounty.Parse("01");
county.ToDisplayString();   // "Stockholms län"
county.ToEnglishString();   // "Stockholm County"

// Municipality
var muni = SwedishMunicipality.Parse("0180");
muni.ToDisplayString();     // "Stockholm"
muni.ToEnglishString();     // "Stockholm" (names are not translated)

// Currency
Currency.SEK.ToDisplayString();  // "Svensk krona"
Currency.SEK.ToEnglishString();  // "Swedish krona"

// Address - country name follows the language choice
var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "DE");
address.ToDisplayString();  // "Storgatan 1, 114 53 Stockholm, Tyskland"
address.ToEnglishString();  // "Storgatan 1, 114 53 Stockholm, Germany"

// Multiline with language choice
address.ToMultilineString();                  // ...Tyskland
address.ToMultilineString(useEnglish: true);  // ...Germany

// Swedish addresses omit the country in display output
var seAddress = Address.Parse("Storgatan 1", "114 53", "Stockholm", "SE");
seAddress.ToDisplayString();  // "Storgatan 1, 114 53 Stockholm"   (Sverige omitted)
seAddress.ToEnglishString();  // "Storgatan 1, 114 53 Stockholm, Sweden"

// Change UI culture to English
PrimitivesDefaults.UICulture = PrimitivesUICulture.English;
country.ToDisplayString();  // "Germany" (now English)
country.ToString();         // "Germany" (ToString delegates to ToDisplayString)
```

### Sample data

The `Buildi.Primitives.SampleData` namespace provides pre-parsed, publicly sourced sample data for testing and development. All data comes from publicly published sources only - no real personal data is included. See [TEST_AND_SAMPLE_DATA.md](TEST_AND_SAMPLE_DATA.md) for the full data strategy and source links.

**Per-type sample data** - static classes with named properties and an `.All` collection:

```csharp
using Buildi.Primitives.SampleData.Organization;
using Buildi.Primitives.SampleData.Banking;
using Buildi.Primitives.SampleData.Contact;
using Buildi.Primitives.SampleData.Web;
using Buildi.Primitives.SampleData.Geography;
using Buildi.Primitives.SampleData.Property;

// Individual named samples
var org = OrganizationNumberSampleData.Vattenfall;          // 556036-2138
var bg  = SwedishBankgiroNumberSampleData.SvenskaKyrkan;    // 900-1223
var pg  = SwedishPostgiroNumberSampleData.SvenskaKyrkan;    // 900122-3
var bic = BicSampleData.Nordea;                             // NDEASESS

// All samples for a type
foreach (var number in OrganizationNumberSampleData.All)
    Console.WriteLine(number);

// Available per-type classes:
// OrganizationNumberSampleData, EuVatNumberSampleData, DunsNumberSampleData,
// LeiCodeSampleData, SwedishSniCodeSampleData,
// SwedishBankgiroNumberSampleData, SwedishPostgiroNumberSampleData,
// BicSampleData, IbanSampleData, SwedishSwishNumberSampleData, PhoneNumberSampleData,
// EmailSampleData (in SampleData.Web), SwedishPropertyDesignationSampleData,
// SwedishCountySampleData, SwedishMunicipalitySampleData
```

**Aggregated per organization** - `SampleOrganizations` groups data by organization:

```csharp
using Buildi.Primitives.SampleData;

var vattenfall = SampleOrganizations.Vattenfall;
Console.WriteLine(vattenfall.Name);                 // Vattenfall AB
Console.WriteLine(vattenfall.OrganizationNumber);   // 556036-2138
Console.WriteLine(vattenfall.EuVatNumber);            // SE556036213801
Console.WriteLine(vattenfall.AddressCity);          // Solna

// Iterate all sample organizations
foreach (var org in SampleOrganizations.All)
    Console.WriteLine($"{org.Name}: {org.OrganizationNumber}");
```

## Testing

Every type in this library is backed by a rigorous test suite covering valid inputs, invalid inputs, edge cases (null, empty, whitespace, boundary lengths, mixed case, separators), checksum validation, formatting, and normalization. The test project currently contains 2,100+ parameterized test cases.

### Unit tests

Run all tests from the repository root:

```shell
dotnet test
```

Test files mirror the source layout - each type has a corresponding test class:

```
test/Buildi.Primitives.Tests/
  Organization/SwedishOrganizationNumberTests.cs
  Banking/IbanTests.cs
  Contact/PhoneNumberTests.cs
  Web/EmailAddressTests.cs
  Person/SwedishPersonalIdentityNumberTests.cs
  ...
```

### Performance benchmarks

Performance benchmarks for all types are implemented using [BenchmarkDotNet](https://benchmarkdotnet.org/). The benchmarks exercise `TryParse` with both valid and invalid inputs for every type across all namespaces (Organization, Banking, Contact, Web, Person, Vehicle, Product, Geography, Property, Finance, Measurement).

Run all benchmarks:

```shell
dotnet run --project test/Buildi.Primitives.Benchmarks -c Release -- --filter "*"
```

Run benchmarks for a specific namespace:

```shell
dotnet run --project test/Buildi.Primitives.Benchmarks -c Release -- --filter "*Banking*"
```

Each benchmark class uses `[MemoryDiagnoser]` to track heap allocations and `[ShortRunJob]` for fast feedback during development. Remove `[ShortRunJob]` for full statistical analysis.

## Versioning

This library follows [Semantic Versioning](https://semver.org/).

**Before 1.0.0** the API is under active development and breaking changes may occur in any release.

**From 1.0.0** a new minor or major version signals one of two things:

- **Breaking API change** — renamed, removed, or signature-changed public types, methods, or properties.
- **Changed normalized form** — if `ToNormalizedString()` or `Normalize()` returns a different value for the same input, that is a breaking change. Consumers store and compare normalized values, so their meaning must remain stable.

**Not considered breaking:** accepting *more* input formats as valid. As real-world patterns are discovered, types may start parsing and normalizing inputs that were previously rejected. This means `IsValid()`, `TryParse()`, `Format()`, and `Normalize()` may begin returning successful results for inputs that previously failed. Callers that depend on specific inputs being *rejected* should pin to a known version.

## Acknowledgements

Personal identity number and coordination number parsing is powered by [ActiveLogin.Identity](https://github.com/ActiveLogin/ActiveLogin.Identity), an excellent open-source .NET library by [Active Solution](https://www.activesolution.se/). Thank you for making high-quality Swedish identity handling available to the community.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## FAQ

**Where does this library come from?**

This library was originally spun out of a set of validation classes built internally at [Budi Auktioner](https://www.budi.se/). It has since grown well beyond that starting point and is now a general-purpose library for anyone who needs domain primitives with a Swedish focus. You do not need any affiliation with Budi Auktioner to use it.

**Who is this for?**

Anyone building .NET software that deals with Swedish domain concepts - organization numbers, personal identity numbers, banking details, addresses, and more. Many of the primitives (VAT numbers, IBAN, phone numbers, countries, currencies, measurements, etc.) are useful internationally as well.

**What about the test and sample data?**

All test data and sample data in this repository is chosen purely at random from publicly available sources. Any resemblance or correlation with Budi Auktioner's own data is entirely coincidental and not intentional. No personal identifiable data should be included in the test and sample data, instead we should have use synthetic data provided by swedish authorities or info for public companies/organizations.

## License


The library is provided under the [MIT license](LICENSE) with no warranties or guarantees of any kind. Use it at your own discretion.

See [LICENSE](LICENSE).