# Supported Types

A searchable reference of all value-object types in Buildi.Primitives, organized by namespace.

See the [README](../../README.md) for full usage examples and detailed documentation.

## Organization

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Swedish Organization Number | Organisationsnummer | Swedish organization number (10/12-digit) with Luhn check digit | `Buildi.Primitives.Organization.SwedishOrganizationNumber` |
| VAT Number | Momsnummer | EU VAT numbers with country-specific validation | `Buildi.Primitives.Organization.EuVatNumber` |
| Swedish Organization Name | Organisationsnamn | Strict Swedish organization/company name (2–200 chars, whitespace-normalized; Bolagsverket-style charset) | `Buildi.Primitives.Organization.SwedishOrganizationName` |
| EU Organization Name | Europeiskt organisationsnamn | Multi-jurisdictional name for VIES/EORI/GEMI feeds; permits `LEGAL\|\|TRADE` split and Baltic/Slavic `SIA "Name"` quoting | `Buildi.Primitives.Organization.EuOrganizationName` |
| Swedish SNI Code | SNI-kod | Swedish industrial classification code (SNI 2025) | `Buildi.Primitives.Organization.SwedishSniCode` |
| Swedish CFAR Number | CFAR-nummer | Swedish 8-digit establishment/workplace identifier | `Buildi.Primitives.Organization.SwedishCfarNumber` |
| DUNS Number | DUNS-nummer | D-U-N-S number (9 digits) | `Buildi.Primitives.Organization.DunsNumber` |
| LEI Code | LEI-kod | Legal Entity Identifier (20 chars, ISO 17442) | `Buildi.Primitives.Organization.LeiCode` |
| Swedish Organization Identifier | Identifierare | Unified classifier for org/personal/coordination numbers, VAT, DUNS, and LEI | `Buildi.Primitives.Organization.SwedishOrganizationIdentifier` |
| ELF Code | ELF-kod | Entity Legal Form code (ISO 20275, 4-char alphanumeric) | `Buildi.Primitives.Organization.ElfCode` |

## Banking

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Swedish Bankgiro Number | Bankgironummer | Swedish Bankgiro payment routing number | `Buildi.Primitives.Banking.SwedishBankgiroNumber` |
| Swedish Plusgiro Number | Plusgironummer | Swedish Plusgiro payment number | `Buildi.Primitives.Banking.SwedishPostgiroNumber` |
| Swedish OCR Reference Number | OCR-nummer | Swedish OCR reference number with check digit | `Buildi.Primitives.Banking.SwedishOcrReferenceNumber` |
| BIC | BIC/SWIFT-kod | Business Identifier Code for international bank routing | `Buildi.Primitives.Banking.Bic` |
| IBAN | IBAN | International Bank Account Number (ISO 13616) | `Buildi.Primitives.Banking.Iban` |
| Swedish Bank Clearing Number | Clearingnummer | Swedish bank clearing number with bank identification | `Buildi.Primitives.Banking.SwedishBankClearingNumber` |
| Swedish Bank Account | Bankkonto | Swedish bank account with clearing number and bank detection | `Buildi.Primitives.Banking.SwedishBankAccount` |
| Banking Number | Bankidentifierare | Unified parser for Bankgiro, Plusgiro, OCR, BIC, IBAN, and bank accounts | `Buildi.Primitives.Banking.SwedishBankingNumber` |
| Bank Account Holder Name | Kontoinnehavare | Bank account holder name with person/organization detection | `Buildi.Primitives.Banking.SwedishBankAccountHolderName` |
| Swish Number | Swish-nummer | Swish payment number (123-numbers, 90-numbers, and mobile) | `Buildi.Primitives.Banking.SwedishSwishNumber` |

## Finance

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Currency | Valuta | ISO 4217 currency with code, names, symbol, and decimal places | `Buildi.Primitives.Finance.Currency` |
| Money Amount | Belopp | Monetary amount with currency | `Buildi.Primitives.Finance.MoneyAmount` |
| ISIN | ISIN | International Securities Identification Number (ISO 6166) | `Buildi.Primitives.Finance.Isin` |
| Exchange Rates | Växelkurser | Exchange rate collection with automatic inverse derivation | `Buildi.Primitives.Finance.ExchangeRates` |

## Person

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Person Given Name | Förnamn | Given names with optional preferred name (tilltalsnamn) | `Buildi.Primitives.Person.PersonGivenName` |
| Person Family Name | Efternamn | Family name (surname) | `Buildi.Primitives.Person.PersonFamilyName` |
| Person Full Name | Fullständigt namn | Full name composed of given names and family name | `Buildi.Primitives.Person.PersonFullName` |
| Person Age | Ålder | Age in years, months, or days with Swedish legal-age properties | `Buildi.Primitives.Person.PersonAge` |
| Swedish Personal Identity Number | Personnummer | Swedish personal identity number (YYYYMMDD-NNNC) | `Buildi.Primitives.Person.SwedishPersonalIdentityNumber` |
| Swedish Coordination Number | Samordningsnummer | Swedish coordination number for non-registered individuals | `Buildi.Primitives.Person.SwedishCoordinationNumber` |

## Contact

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Address | Adress | Unified address model (street, zip, city, country) | `Buildi.Primitives.Contact.Address` |
| Swedish Address | Svensk adress | Swedish-only address requiring 5-digit zip code and city | `Buildi.Primitives.Contact.SwedishAddress` |
| Swedish Address Zip Code | Postnummer (SE) | Swedish 5-digit postal code only (NNN NN) | `Buildi.Primitives.Contact.SwedishAddressZipCode` |
| Zip Code | Postnummer | Postal code — Swedish and international formats | `Buildi.Primitives.Contact.AddressZipCode` |
| City | Postort | City name with Swedish-aware capitalization | `Buildi.Primitives.Contact.AddressCity` |
| Street Address | Gatuadress | Street address with house number, c/o, apartment, and box extraction | `Buildi.Primitives.Contact.AddressStreet` |
| Phone Calling Code | Landsnummer | Phone country calling code (e.g. +46) | `Buildi.Primitives.Contact.PhoneCallingCode` |
| Phone Number | Telefonnummer | Phone number with country detection and E.164 output | `Buildi.Primitives.Contact.PhoneNumber` |
| Contact Address | Kontaktuppgift | Composite of person name, organization name, and address | `Buildi.Primitives.Contact.ContactAddress` |

### Country-specific addresses

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Austrian Address | Adress (AT) | Austrian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.AustrianAddress` |
| Austrian Zip Code | Postnummer (AT) | Austrian 4-digit postal code | `Buildi.Primitives.Contact.AustrianAddressZipCode` |
| Belgian Address | Adress (BE) | Belgian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.BelgianAddress` |
| Belgian Zip Code | Postnummer (BE) | Belgian 4-digit postal code | `Buildi.Primitives.Contact.BelgianAddressZipCode` |
| British Address | Adress (GB) | British address (street, alphanumeric postcode, city) | `Buildi.Primitives.Contact.BritishAddress` |
| British Zip Code | Postnummer (GB) | UK postcode (e.g. SW1A 1AA) | `Buildi.Primitives.Contact.BritishAddressZipCode` |
| Bulgarian Address | Adress (BG) | Bulgarian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.BulgarianAddress` |
| Bulgarian Zip Code | Postnummer (BG) | Bulgarian 4-digit postal code | `Buildi.Primitives.Contact.BulgarianAddressZipCode` |
| Croatian Address | Adress (HR) | Croatian address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.CroatianAddress` |
| Croatian Zip Code | Postnummer (HR) | Croatian 5-digit postal code | `Buildi.Primitives.Contact.CroatianAddressZipCode` |
| Cypriot Address | Adress (CY) | Cypriot address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.CypriotAddress` |
| Cypriot Zip Code | Postnummer (CY) | Cypriot 4-digit postal code | `Buildi.Primitives.Contact.CypriotAddressZipCode` |
| Czech Address | Adress (CZ) | Czech address (street, 5-digit zip NNN NN, city) | `Buildi.Primitives.Contact.CzechAddress` |
| Czech Zip Code | Postnummer (CZ) | Czech 5-digit postal code (NNN NN) | `Buildi.Primitives.Contact.CzechAddressZipCode` |
| Danish Address | Adress (DK) | Danish address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.DanishAddress` |
| Danish Zip Code | Postnummer (DK) | Danish 4-digit postal code | `Buildi.Primitives.Contact.DanishAddressZipCode` |
| Dutch Address | Adress (NL) | Dutch address (street, zip NNNN AA, city) | `Buildi.Primitives.Contact.DutchAddress` |
| Dutch Zip Code | Postnummer (NL) | Dutch postal code (NNNN AA) | `Buildi.Primitives.Contact.DutchAddressZipCode` |
| Estonian Address | Adress (EE) | Estonian address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.EstonianAddress` |
| Estonian Zip Code | Postnummer (EE) | Estonian 5-digit postal code | `Buildi.Primitives.Contact.EstonianAddressZipCode` |
| Finnish Address | Adress (FI) | Finnish address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.FinnishAddress` |
| Finnish Zip Code | Postnummer (FI) | Finnish 5-digit postal code | `Buildi.Primitives.Contact.FinnishAddressZipCode` |
| French Address | Adress (FR) | French address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.FrenchAddress` |
| French Zip Code | Postnummer (FR) | French 5-digit postal code | `Buildi.Primitives.Contact.FrenchAddressZipCode` |
| German Address | Adress (DE) | German address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.GermanAddress` |
| German Zip Code | Postnummer (DE) | German 5-digit postal code | `Buildi.Primitives.Contact.GermanAddressZipCode` |
| Greek Address | Adress (GR) | Greek address (street, 5-digit zip NNN NN, city) | `Buildi.Primitives.Contact.GreekAddress` |
| Greek Zip Code | Postnummer (GR) | Greek 5-digit postal code (NNN NN) | `Buildi.Primitives.Contact.GreekAddressZipCode` |
| Hungarian Address | Adress (HU) | Hungarian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.HungarianAddress` |
| Hungarian Zip Code | Postnummer (HU) | Hungarian 4-digit postal code | `Buildi.Primitives.Contact.HungarianAddressZipCode` |
| Icelandic Address | Adress (IS) | Icelandic address (street, 3-digit zip, city) | `Buildi.Primitives.Contact.IcelandicAddress` |
| Icelandic Zip Code | Postnummer (IS) | Icelandic 3-digit postal code | `Buildi.Primitives.Contact.IcelandicAddressZipCode` |
| Irish Address | Adress (IE) | Irish address (street, Eircode ANN XXXX, city) | `Buildi.Primitives.Contact.IrishAddress` |
| Irish Zip Code | Postnummer (IE) | Irish Eircode (7-character alphanumeric) | `Buildi.Primitives.Contact.IrishAddressZipCode` |
| Italian Address | Adress (IT) | Italian address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.ItalianAddress` |
| Italian Zip Code | Postnummer (IT) | Italian 5-digit postal code | `Buildi.Primitives.Contact.ItalianAddressZipCode` |
| Latvian Address | Adress (LV) | Latvian address (street, 4-digit zip LV-NNNN, city) | `Buildi.Primitives.Contact.LatvianAddress` |
| Latvian Zip Code | Postnummer (LV) | Latvian 4-digit postal code (LV-NNNN) | `Buildi.Primitives.Contact.LatvianAddressZipCode` |
| Liechtenstein Address | Adress (LI) | Liechtenstein address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.LiechtensteinAddress` |
| Liechtenstein Zip Code | Postnummer (LI) | Liechtenstein 4-digit postal code | `Buildi.Primitives.Contact.LiechtensteinAddressZipCode` |
| Lithuanian Address | Adress (LT) | Lithuanian address (street, 5-digit zip LT-NNNNN, city) | `Buildi.Primitives.Contact.LithuanianAddress` |
| Lithuanian Zip Code | Postnummer (LT) | Lithuanian 5-digit postal code (LT-NNNNN) | `Buildi.Primitives.Contact.LithuanianAddressZipCode` |
| Luxembourgish Address | Adress (LU) | Luxembourgish address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.LuxembourgishAddress` |
| Luxembourgish Zip Code | Postnummer (LU) | Luxembourgish 4-digit postal code | `Buildi.Primitives.Contact.LuxembourgishAddressZipCode` |
| Maltese Address | Adress (MT) | Maltese address (street, zip AAA NNNN, city) | `Buildi.Primitives.Contact.MalteseAddress` |
| Maltese Zip Code | Postnummer (MT) | Maltese postal code (AAA NNNN) | `Buildi.Primitives.Contact.MalteseAddressZipCode` |
| Norwegian Address | Adress (NO) | Norwegian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.NorwegianAddress` |
| Norwegian Zip Code | Postnummer (NO) | Norwegian 4-digit postal code | `Buildi.Primitives.Contact.NorwegianAddressZipCode` |
| Polish Address | Adress (PL) | Polish address (street, 5-digit zip NN-NNN, city) | `Buildi.Primitives.Contact.PolishAddress` |
| Polish Zip Code | Postnummer (PL) | Polish 5-digit postal code (NN-NNN) | `Buildi.Primitives.Contact.PolishAddressZipCode` |
| Portuguese Address | Adress (PT) | Portuguese address (street, 7-digit zip NNNN-NNN, city) | `Buildi.Primitives.Contact.PortugueseAddress` |
| Portuguese Zip Code | Postnummer (PT) | Portuguese 7-digit postal code (NNNN-NNN) | `Buildi.Primitives.Contact.PortugueseAddressZipCode` |
| Romanian Address | Adress (RO) | Romanian address (street, 6-digit zip, city) | `Buildi.Primitives.Contact.RomanianAddress` |
| Romanian Zip Code | Postnummer (RO) | Romanian 6-digit postal code | `Buildi.Primitives.Contact.RomanianAddressZipCode` |
| Slovak Address | Adress (SK) | Slovak address (street, 5-digit zip NNN NN, city) | `Buildi.Primitives.Contact.SlovakAddress` |
| Slovak Zip Code | Postnummer (SK) | Slovak 5-digit postal code (NNN NN) | `Buildi.Primitives.Contact.SlovakAddressZipCode` |
| Slovenian Address | Adress (SI) | Slovenian address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.SlovenianAddress` |
| Slovenian Zip Code | Postnummer (SI) | Slovenian 4-digit postal code | `Buildi.Primitives.Contact.SlovenianAddressZipCode` |
| Spanish Address | Adress (ES) | Spanish address (street, 5-digit zip, city) | `Buildi.Primitives.Contact.SpanishAddress` |
| Spanish Zip Code | Postnummer (ES) | Spanish 5-digit postal code | `Buildi.Primitives.Contact.SpanishAddressZipCode` |
| Swiss Address | Adress (CH) | Swiss address (street, 4-digit zip, city) | `Buildi.Primitives.Contact.SwissAddress` |
| Swiss Zip Code | Postnummer (CH) | Swiss 4-digit postal code | `Buildi.Primitives.Contact.SwissAddressZipCode` |

## Web

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Email Address | E-postadress | Email address with TLD, country mapping, and provider detection | `Buildi.Primitives.Web.EmailAddress` |
| URL | Webbadress | URL/URI with any scheme; auto-prefix for bare domains | `Buildi.Primitives.Web.Url` |

## Property

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Swedish Property Designation | Fastighetsbeteckning | Swedish property designation with tract name and register number | `Buildi.Primitives.Property.SwedishPropertyDesignation` |
| Property Taxation Code | Typkod | Swedish 3-digit property taxation code from Skatteverket | `Buildi.Primitives.Property.SwedishPropertyTaxationCode` |

## Geography

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Swedish County | Län | Swedish county with 2-digit code and name | `Buildi.Primitives.Geography.SwedishCounty` |
| Swedish Municipality | Kommun | Swedish municipality with 4-digit code, name, and county | `Buildi.Primitives.Geography.SwedishMunicipality` |
| Country | Land | Country with names, ISO codes, calling code, ccTLD, continent, language, coordinates, capital, borders, and flags | `Buildi.Primitives.Geography.Country` |
| Country Capital | Huvudstad | Capital city with English, Swedish, and native names plus coordinates | `Buildi.Primitives.Geography.CountryCapital` |
| Language | Språk | Language with ISO 639-1/639-2 codes, names, script, and text direction | `Buildi.Primitives.Geography.Language` |
| Geo Coordinate | Koordinat | Geographic coordinate (WGS 84) with DD, DMS, DDM parsing and Haversine distance | `Buildi.Primitives.Geography.GeoCoordinate` |
| Continent | Kontinent | Geographic continent (7 recognized) | `Buildi.Primitives.Geography.Continent` |

## Vehicle

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Swedish Vehicle Registration Number | Registreringsnummer | Swedish vehicle plate (ABC 123 / ABC 12A) with tax payment month | `Buildi.Primitives.Vehicle.SwedishVehicleRegistrationNumber` |
| Vehicle Identification Number | Chassinummer / VIN | 17-character ISO 3779 VIN with check digit and model year decoding | `Buildi.Primitives.Vehicle.VehicleIdentificationNumber` |
| Euro Emission Class | Euro-utsläppsklass | European emission standard (Euro 1–7) with Swedish miljöklass | `Buildi.Primitives.Vehicle.EuroEmissionClass` |
| Tire Dimension | Däckdimension | Metric tire notation (e.g. 205/55R16) with width, aspect ratio, rim diameter | `Buildi.Primitives.Vehicle.TireDimension` |
| Engine Displacement | Motorvolym | Engine displacement in cc/mL/liters | `Buildi.Primitives.Vehicle.EngineDisplacement` |
| Engine Power | Motoreffekt | Engine power in HP/hk/kW | `Buildi.Primitives.Vehicle.EnginePower` |
| Odometer Reading | Mätarställning | Odometer reading in km/miles/mil | `Buildi.Primitives.Vehicle.OdometerReading` |
| Driving License Category | Körkortsbehörighet | EU driving license category (AM–DE) with vehicle group and age | `Buildi.Primitives.Vehicle.SwedishDrivingLicenseCategory` |
| Fuel Type | Drivmedel | Vehicle fuel/energy type with Transportstyrelsen codes | `Buildi.Primitives.Vehicle.FuelType` |
| Transmission Type | Växellåda | Transmission/gearbox type (Manual, Automatic, CVT, etc.) | `Buildi.Primitives.Vehicle.TransmissionType` |
| Swedish Vehicle Type | Fordonsslag | Swedish vehicle type classification (PB, LB, MC, etc.) | `Buildi.Primitives.Vehicle.SwedishVehicleType` |
| Swedish Vehicle Status | Fordonsstatus | Swedish vehicle registration status | `Buildi.Primitives.Vehicle.SwedishVehicleStatus` |
| EU Vehicle Category | Fordonskategori EU | EU vehicle category code (M1, N1G, L3e-A2, etc.) | `Buildi.Primitives.Vehicle.EuVehicleCategory` |
| EU Type Approval Number | Typgodkännandenummer | EU whole-vehicle type-approval number | `Buildi.Primitives.Vehicle.EuTypeApprovalNumber` |
| Wheel Rim Dimension | Fälgdimension | Wheel rim size notation (e.g. 18x7J) | `Buildi.Primitives.Vehicle.WheelRimDimension` |
| Operating Hours | Drifttimmar | Machine operating hours meter reading | `Buildi.Primitives.Vehicle.OperatingHours` |
| Bolt Pattern | Bultcirkelmått | Wheel bolt pattern, e.g. 5x114.3 | `Buildi.Primitives.Vehicle.BoltPattern` |
| Fuel Consumption | Bränsleförbrukning | Fuel consumption in l/100km, km/l, mpg, kWh/100km | `Buildi.Primitives.Vehicle.FuelConsumption` |
| Fuel Consumption Norm | Förbrukningsnorm | Regulatory test cycle for fuel/energy consumption (NEDC, WLTP, EPA, etc.) | `Buildi.Primitives.Vehicle.FuelConsumptionNorm` |
| Emission Rate | Utsläpp | Vehicle emission rate in g/km or mg/km | `Buildi.Primitives.Vehicle.EmissionRate` |
| Boat CE Design Category | Båt CE-konstruktionskategori | EU Recreational Craft design category A/B/C/D (ISO 12217) with wave/wind ratings | `Buildi.Primitives.Vehicle.BoatCeDesignCategory` |
| Boat Hull Material | Båt skrovmaterial | Boat hull material (Glasfiber/Aluminium/Stål/Trä/Plast/Kolfiber/Hypalon) | `Buildi.Primitives.Vehicle.BoatHullMaterial` |
| Swedish Eco-Vehicle Classification | Miljöbilsklassning | Swedish miljöbil/klimatbonusbil classification with year ranges | `Buildi.Primitives.Vehicle.SwedishEcoVehicleClassification` |

## Product

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| GTIN | GTIN | General GTIN parser (8, 12, 13, or 14 digits) | `Buildi.Primitives.Product.Gtin` |
| GTIN-8 | GTIN-8 / EAN-8 | 8-digit GTIN (EAN-8) | `Buildi.Primitives.Product.Gtin8` |
| GTIN-12 | GTIN-12 / UPC-A | 12-digit GTIN (UPC-A) | `Buildi.Primitives.Product.Gtin12` |
| GTIN-13 | GTIN-13 / EAN-13 | 13-digit GTIN (EAN-13) with GS1 prefix and country metadata | `Buildi.Primitives.Product.Gtin13` |
| GTIN-14 | GTIN-14 / ITF-14 | 14-digit GTIN (ITF-14) with indicator digit | `Buildi.Primitives.Product.Gtin14` |
| HS Code | HS-kod / KN-nummer | Harmonized System code (2–10 digits) for international trade | `Buildi.Primitives.Product.HsCode` |
| Google Product Category | Google-produktkategori | Hierarchical product taxonomy for Google Shopping | `Buildi.Primitives.Product.GoogleProductCategory` |
| Color | Färg | Color from CSS names, hex, rgb(), hsl() with English/Swedish names | `Buildi.Primitives.Product.Color` |
| Adult Clothing Size | Klädstorlek (vuxen) | Adult clothing size with EU/US/UK/letter systems | `Buildi.Primitives.Product.AdultClothingSize` |
| Child Clothing Size | Klädstorlek (barn) | Children's clothing size based on body height (cm) | `Buildi.Primitives.Product.ChildClothingSize` |
| Clothing Size | Klädstorlek | Generic clothing size auto-detecting adult or child | `Buildi.Primitives.Product.ClothingSize` |
| Adult Shoe Size | Skostorlek (vuxen) | Adult shoe size with EU/US/UK systems | `Buildi.Primitives.Product.AdultShoeSize` |
| Child Shoe Size | Skostorlek (barn) | Children's shoe size (EU 16–39) | `Buildi.Primitives.Product.ChildShoeSize` |
| Shoe Size | Skostorlek | Generic shoe size auto-detecting adult or child | `Buildi.Primitives.Product.ShoeSize` |
| Screen Size | Skärmstorlek | Screen diagonal measurement, defaults to inches | `Buildi.Primitives.Product.ScreenSize` |
| Screen Resolution | Skärmupplösning | Screen resolution (e.g. 1920x1080, Full HD, 4K) | `Buildi.Primitives.Product.ScreenResolution` |
| Aspect Ratio | Bildförhållande | Width-to-height aspect ratio (4:3, 16:9, 21:9, 32:9, decimal) | `Buildi.Primitives.Product.AspectRatio` |
| Energy Efficiency Class | Energiklass | EU energy efficiency label (A+++–G) | `Buildi.Primitives.Product.EuEnergyEfficiencyClass` |
| Operating System Name | Operativsystem | Canonical OS name (Windows, macOS, Ubuntu, etc.) | `Buildi.Primitives.Product.OperatingSystemName` |
| Operating System Version | OS-version | Version string with major/minor/patch parsing | `Buildi.Primitives.Product.OperatingSystemVersion` |
| Operating System Info | Operativsysteminfo | Combined OS name + version (e.g. Windows 11) | `Buildi.Primitives.Product.OperatingSystemInfo` |
| Storage Capacity | Lagringskapacitet | HDD/SSD capacity, defaults to GB | `Buildi.Primitives.Product.StorageCapacity` |
| RAM Capacity | Arbetsminne | RAM/memory capacity, defaults to GB | `Buildi.Primitives.Product.RamCapacity` |
| Processor Speed | Processorhastighet | CPU clock speed, defaults to GHz | `Buildi.Primitives.Product.ProcessorSpeed` |
| Battery Capacity | Batterikapacitet | Battery capacity in mAh or Wh | `Buildi.Primitives.Product.BatteryCapacity` |
| Battery Chemistry | Batterikemi | Battery chemistry (Li-ion, LiFePO4, AGM, Pb-Acid, Gel, NiMH, etc.) with cell voltage | `Buildi.Primitives.Product.BatteryChemistry` |
| Refrigerant | Köldmedium | ASHRAE 34 refrigerant (R134a, R290, R744, R1234yf, etc.) with GWP and safety class | `Buildi.Primitives.Product.Refrigerant` |
| Electrical Phase | Fas | Electrical phase configuration (single/three-phase) | `Buildi.Primitives.Product.ElectricalPhase` |
| Clothing Gender | Klädkön | Clothing target gender (male, female, unisex, boys, girls) | `Buildi.Primitives.Product.ClothingGender` |
| IP Rating | IP-klass | Ingress Protection rating per IEC 60529 | `Buildi.Primitives.Product.IpRating` |

## Measurement

| English Name | Svenska | Description | Full Name |
| --- | --- | --- | --- |
| Length | Längd | Length/distance in meters, km, cm, inches, feet, miles, etc. | `Buildi.Primitives.Measurement.Length` |
| Length Unit | Längdenhet | Unit definition for length with symbol and conversion factor | `Buildi.Primitives.Measurement.LengthUnit` |
| Area | Area | Area in m², km², hectares, acres, etc. | `Buildi.Primitives.Measurement.Area` |
| Area Unit | Areaenhet | Unit definition for area | `Buildi.Primitives.Measurement.AreaUnit` |
| Volume | Volym | Volume in liters, mL, gallons, etc. | `Buildi.Primitives.Measurement.Volume` |
| Volume Unit | Volymenhet | Unit definition for volume | `Buildi.Primitives.Measurement.VolumeUnit` |
| Weight | Vikt | Weight/mass in kg, grams, pounds, etc. | `Buildi.Primitives.Measurement.Weight` |
| Weight Unit | Viktenhet | Unit definition for weight | `Buildi.Primitives.Measurement.WeightUnit` |
| Energy | Energi | Energy in joules, kWh, calories, BTU, etc. | `Buildi.Primitives.Measurement.Energy` |
| Energy Unit | Energienhet | Unit definition for energy | `Buildi.Primitives.Measurement.EnergyUnit` |
| Power | Effekt | Power in watts, kW, horsepower, etc. | `Buildi.Primitives.Measurement.Power` |
| Power Unit | Effektenhet | Unit definition for power | `Buildi.Primitives.Measurement.PowerUnit` |
| Voltage | Spänning | Voltage in V, mV, kV | `Buildi.Primitives.Measurement.Voltage` |
| Voltage Unit | Spänningsenhet | Unit definition for voltage | `Buildi.Primitives.Measurement.VoltageUnit` |
| Electric Charge | Elektrisk laddning | Electric charge in Ah, mAh, coulombs | `Buildi.Primitives.Measurement.ElectricCharge` |
| Electric Charge Unit | Laddningsenhet | Unit definition for electric charge | `Buildi.Primitives.Measurement.ElectricChargeUnit` |
| Electric Current | Elektrisk ström | Electric current in A, mA, kA | `Buildi.Primitives.Measurement.ElectricCurrent` |
| Electric Current Unit | Strömenhet | Unit definition for electric current | `Buildi.Primitives.Measurement.ElectricCurrentUnit` |
| Flow Rate | Flöde | Volumetric flow rate in L/min, m³/h, etc. | `Buildi.Primitives.Measurement.FlowRate` |
| Flow Rate Unit | Flödesenhet | Unit definition for flow rate | `Buildi.Primitives.Measurement.FlowRateUnit` |
| Luminous Flux | Ljusflöde | Luminous flux in lumens, kilolumens | `Buildi.Primitives.Measurement.LuminousFlux` |
| Luminous Flux Unit | Ljusflödesenhet | Unit definition for luminous flux | `Buildi.Primitives.Measurement.LuminousFluxUnit` |
| Torque | Vridmoment | Torque in Nm, ft-lb, kgf-m | `Buildi.Primitives.Measurement.Torque` |
| Torque Unit | Vridmomentsenhet | Unit definition for torque | `Buildi.Primitives.Measurement.TorqueUnit` |
| Frequency | Frekvens | Frequency in Hz, kHz, MHz, GHz, RPM | `Buildi.Primitives.Measurement.Frequency` |
| Frequency Unit | Frekvensenhet | Unit definition for frequency | `Buildi.Primitives.Measurement.FrequencyUnit` |
| Speed | Hastighet | Speed in m/s, km/h, mph, knots | `Buildi.Primitives.Measurement.Speed` |
| Speed Unit | Hastighetsenhet | Unit definition for speed | `Buildi.Primitives.Measurement.SpeedUnit` |
| Temperature | Temperatur | Temperature in °C, °F, K with offset conversions | `Buildi.Primitives.Measurement.Temperature` |
| Temperature Unit | Temperaturenhet | Unit definition for temperature | `Buildi.Primitives.Measurement.TemperatureUnit` |
| Temperature Delta | Temperaturskillnad | Temperature difference (supports °C and °F deltas) | `Buildi.Primitives.Measurement.TemperatureDelta` |
| Data Size | Datastorlek | Data size in bytes, KB, MB, GB with SI and binary prefixes | `Buildi.Primitives.Measurement.DataSize` |
| Data Size Unit | Datastorleksenhet | Unit definition for data size | `Buildi.Primitives.Measurement.DataSizeUnit` |
| Pressure | Tryck | Pressure in Pa, bar, PSI, atm | `Buildi.Primitives.Measurement.Pressure` |
| Pressure Unit | Tryckenhet | Unit definition for pressure | `Buildi.Primitives.Measurement.PressureUnit` |
| Percentage | Procent | Percentage (0–100% display, 0–1 decimal storage) | `Buildi.Primitives.Measurement.Percentage` |
| Sound Level | Ljudnivå | Sound pressure level in dB with optional A/B/C/Z weighting | `Buildi.Primitives.Measurement.SoundLevel` |
| Rotational Speed | Rotationshastighet | Rotational speed in rpm, rps, or rad/s | `Buildi.Primitives.Measurement.RotationalSpeed` |
| Rotational Speed Unit | Rotationshastighetsenhet | Unit definition for rotational speed | `Buildi.Primitives.Measurement.RotationalSpeedUnit` |
| Year | År | Four-digit calendar year (e.g. 2024) for manufacture/model year fields | `Buildi.Primitives.Measurement.Year` |
| Year Month | År-månad | Year-and-month value (YYYY-MM) for inspection-valid-until and similar fields | `Buildi.Primitives.Measurement.YearMonth` |
