# API Reference

Browse the auto-generated API documentation for all public types in [Buildi.Primitives](https://www.nuget.org/packages/Buildi.Primitives).

The API is organized by namespace:

| Namespace | Description |
|---|---|
| `Buildi.Primitives.Organization` | Organization numbers, VAT numbers, SNI codes, CFAR numbers, LEI, DUNS |
| `Buildi.Primitives.Banking` | Bankgiro, plusgiro, OCR, BIC, IBAN, clearing numbers, bank accounts, Swish |
| `Buildi.Primitives.Contact` | Addresses, zip codes, phone numbers, person names, contact info |
| `Buildi.Primitives.Web` | Email addresses, URLs |
| `Buildi.Primitives.Person` | Personal identity numbers, coordination numbers |
| `Buildi.Primitives.Property` | Property designations |
| `Buildi.Primitives.Geography` | Countries, continents, counties, municipalities |
| `Buildi.Primitives.Finance` | Currencies, money amounts |
| `Buildi.Primitives.Vehicle` | Registration numbers, VINs, fuel types, transmissions, EU categories |
| `Buildi.Primitives.Measurement` | Length, area, volume, weight, energy, power, speed, temperature, and more |
| `Buildi.Primitives.Product` | GTINs, HS codes, Google product categories, electrical phases |
| `Buildi.Primitives.Validation` | Shared validation infrastructure (ValidationResult, ValidationIssue) |
| `Buildi.Primitives.TextScanning` | Text scanning infrastructure (TextScanner, TextCandidate) |
| `Buildi.Primitives.SampleData` | Sample data for testing and demos |

Every type follows the same sealed-class pattern with `TryParse`, `Parse`, `IsValid`, `Format`, `Normalize`, and `IsNormalized` as static methods.
