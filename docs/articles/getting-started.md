# Getting started

## Installation

Install the package from NuGet:

```shell
dotnet add package Buildi.Primitives
```

Or add it to your `.csproj`:

```xml
<PackageReference Include="Buildi.Primitives" Version="0.8.0" />
```

## Basic usage

Every type in Buildi.Primitives follows the same API pattern. Here's how to work with any primitive type:

### Parsing and validation

```csharp
using Buildi.Primitives.Organization;

// TryParse — safe parsing, returns bool
if (SwedishOrganizationNumber.TryParse("5592460421", out var orgNumber))
{
    Console.WriteLine(orgNumber.Value);      // "165592460421"
    Console.WriteLine(orgNumber.ToString()); // "559246-0421"
}

// Parse — throws ArgumentException on invalid input
var org = SwedishOrganizationNumber.Parse("5592460421");

// IsValid — quick check without creating an instance
bool valid = SwedishOrganizationNumber.IsValid("5592460421"); // true
```

### Formatting and normalization

All types expose `Format` and `Normalize` as static methods:

```csharp
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;

// Format — display-friendly representation
SwedishBankgiroNumber.Format("58056201");    // "5805-6201"
PhoneNumber.Format("0701740633");            // "070-174 06 33"

// Normalize — canonical, machine-comparable form
PhoneNumber.Normalize("070-174 06 33");      // "0046701740633"
EmailAddress.Normalize("User@GMAIL.COM");    // "user@gmail.com"

// IsNormalized — check if already in canonical form
PhoneNumber.IsNormalized("0046701740633");   // true
```

Both methods accept a `fallbackToTrimmedInputWhenInvalid` parameter. When `true`, invalid input returns the trimmed original string instead of `null`.

### Structured validation

Many types provide a `Validate()` method that explains *why* an input is invalid:

```csharp
using Buildi.Primitives.Organization;
using Buildi.Primitives.Validation;

var result = SwedishOrganizationNumber.Validate("123");

if (!result.IsValid)
{
    foreach (var issue in result.Issues)
    {
        Console.WriteLine(issue.Reason);              // e.g. InvalidLength
        Console.WriteLine(issue.EnglishDescription);  // "The input has an invalid length."
        Console.WriteLine(issue.LocalizedDescription); // "Värdet har en ogiltig längd."
    }
}
```

### Text scanning

Extract structured data from unstructured text:

```csharp
using Buildi.Primitives.TextScanning;

var text = "Kontakta oss på info@example.com eller ring 08-123 456.";
var result = TextScanner.Scan(text);

foreach (var candidate in result.All)
{
    Console.WriteLine($"{candidate.Category}: {candidate.Value} (confidence: {candidate.Confidence})");
}
```

## Next steps

- Browse the [API Reference](../api/index.md) for all available types
- See the full [README on GitHub](https://github.com/budiauktioner/Buildi.Primitives/blob/main/README.md) for detailed documentation of every type with examples
