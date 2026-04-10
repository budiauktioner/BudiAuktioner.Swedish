# AGENTS.md — Buildi.Primitives

Rules and conventions for AI agents working in this codebase.

## Project structure

```
src/Buildi.Primitives/
  {Namespace}/              ← domain types, one file per type
  SampleData/
    {Namespace}/            ← per-type sample data classes
    SampleOrganization.cs   ← aggregated record for organization data
    SampleOrganizations.cs  ← static catalog of 16+ public organizations
test/Buildi.Primitives.Tests/
  {Namespace}/              ← test classes mirroring src layout
  TestData/                 ← sample data integrity tests
samples/Buildi.Primitives.Demo/
  Pages/Home.razor          ← Blazor WASM demo app (TypeRegistry, graphs)
  wwwroot/js/               ← JS interop for interactive visualizations
README.md                   ← usage docs, type tables, code examples
TEST_AND_SAMPLE_DATA.md     ← data strategy, sourcing rules
docs/articles/
  supported-types.md        ← searchable type reference (name, svenska, namespace)
```

Namespaces: `Organization`, `Banking`, `Contact`, `Web`, `Person`, `Property`, `Geography`, `Vehicle`, `Finance`, `Measurement`, `Product`.

## Adding a new value-object type

Every type follows the same sealed-class pattern. Use an existing type as a template (e.g. `Vehicle/SwedishVehicleRegistrationNumber.cs` for a simple type, `Contact/EmailAddress.cs` for one with extra properties).

### 1. Create the type class

File: `src/Buildi.Primitives/{Namespace}/{TypeName}.cs`

Required structure:

```csharp
namespace Buildi.Primitives.{Namespace};

/// <summary>
/// One-sentence description with Swedish term in <c>parens</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="...">Source name</see> — description</description></item>
/// </list>
/// </remarks>
public sealed class TypeName
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Type Name",
        "Typnamn",
        "emoji",
        ["https://source1", "https://source2"]);

    // Properties (Value, Formatted, etc.)
    public string Value { get; }

    // Private constructor — no public constructors
    private TypeName(string value) { Value = value; }

    // --- Required static API (in this order) ---
    public static bool TryParse(string? input, out TypeName? result);
    public static TypeName Parse(string input);  // throws ArgumentException
    public static bool IsValid(string? input);

    /// <summary>XML doc with example output, e.g. <c>ABC 123</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false);
    // → valid: formatted display string
    // → invalid: null (or trimmed original input when fallbackToTrimmedInputWhenInvalid is true)
    // → null/empty: null

    /// <summary>XML doc with example output.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false);
    // → valid: canonical/machine-readable form
    // → invalid: null (or trimmed original input when fallbackToTrimmedInputWhenInvalid is true; empty → null)

    /// <summary>Returns true if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input);
    // → input is not null && Normalize(input) == input

    // --- Required instance API ---
    /// <summary>XML doc with example output.</summary>
    public string ToNormalizedString();
    /// <summary>XML doc with example output.</summary>
    public override string ToString();  // returns display/formatted form
}
```

Rules:
- Class must be `sealed` with a `private` constructor.
- `TypeInfo` must be the very first member of the class, exposing English name, Swedish name, emoji, and source URLs (from the XML `<remarks>`). Use the `PrimitiveTypeInfo` record from `Buildi.Primitives`.
- `TryParse` is the single source of truth — `Parse`, `IsValid`, `Format`, `Normalize`, `IsNormalized` all delegate to it.
- `Format()` returns `null` on failure by default. Pass `fallbackToTrimmedInputWhenInvalid: true` to get the trimmed original input back instead of `null` for non-empty invalid input.
- `Normalize()` returns `null` on failure by default. Pass `fallbackToTrimmedInputWhenInvalid: true` to get the trimmed original input back instead of `null` for non-empty invalid input (empty strings become `null`).
- `IsNormalized()` returns `true` only when the input is valid and identical to its normalized form.
- Add XML `<summary>` docs on `Format`, `Normalize`, `ToNormalizedString`, `ToString` with concrete example output showing the format the caller will get.
- Add XML `<remarks>` with sources on the class itself — keep in sync with the README section for that type.
- Enums related to a type go in the same file (e.g. `PublicEmailProvider` in `EmailAddress.cs`).
- Use `RegexOptions.Compiled` for all regex.
- Use `IBudiTimeProvider` if time-dependent (see workspace rules).

### 2. Create tests

File: `test/Buildi.Primitives.Tests/{Namespace}/{TypeName}Tests.cs`

Required test methods (use `[Theory]` + `[InlineData]` for parameterized tests):

```csharp
public class TypeNameTests
{
    // Valid inputs
    [Theory]
    [InlineData("valid1")]
    [InlineData("valid2")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input);

    // Invalid inputs (always include null, "", " ")
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid1")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input);

    // TryParse valid — assert properties
    [Theory]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(string input, ...);

    // TryParse invalid — assert null result
    [Theory]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input);

    // Parse throws for invalid
    [Theory]
    public void Parse_Throws_ForInvalidInputs(string input);

    // Format and Normalize output
    [Theory]
    public void Format_ReturnsExpected(string? input, string? expected);

    // ToString
    [Theory]
    public void ToString_ReturnsFormattedValue(string input, string expected);
}
```

Test edge cases:
- Null, empty, whitespace-only inputs.
- Leading/trailing whitespace (should be trimmed).
- Mixed case (should normalize as defined).
- Values with and without separators (dashes, spaces, dots).
- Boundary-length values (too short, exact, too long).
- Invalid check digits when applicable.

### 3. Add sample data (when applicable)

File: `src/Buildi.Primitives/SampleData/{Namespace}/{TypeName}SampleData.cs`

```csharp
namespace Buildi.Primitives.SampleData.{Namespace};

/// <summary>
/// Valid <see cref="Swedish.{Namespace}.{TypeName}"/> values from publicly known Swedish organizations
/// and generic examples.
/// </summary>
public static class TypeNameSampleData
{
    public static Swedish.{Namespace}.{TypeName} ExampleOrg { get; } = Swedish.{Namespace}.{TypeName}.Parse("...");

    public static IReadOnlyList<Swedish.{Namespace}.{TypeName}> All { get; } = [ExampleOrg, ...];
}
```

Rules:
- Use `Swedish.{Namespace}.{TypeName}` as the fully-qualified type to avoid namespace collisions with the `SampleData.{Namespace}` sub-namespace.
- Use `Parse()` (not `TryParse`) — sample data must be valid; parse failures surface as build-time exceptions.
- Only include data from publicly known organizations or well-known generic/reserved values. See `TEST_AND_SAMPLE_DATA.md` for sourcing rules.
- For generic examples: use RFC 2606 reserved domains for email (`example.com`, `example.org`), well-known international test values for IBAN/BIC, etc. Do NOT use `.se` domains unless verified as reserved.
- Always expose `public static IReadOnlyList<T> All { get; }` listing all entries.
- If the type is organization-related, also add it to `SampleOrganization.cs` (the record) and populate it in `SampleOrganizations.cs` for relevant organizations.

### 4. Add sample data test coverage

File: `test/Buildi.Primitives.Tests/TestData/PublicOrganizationSampleTests.cs`

Add a `[Fact]` method verifying the new sample data class is accessible:

```csharp
[Fact]
public void PerType_TypeNameSamples_AreAccessible()
{
    Assert.NotNull(SampleData.{Namespace}.TypeNameSampleData.ExampleOrg);
    Assert.True(SampleData.{Namespace}.TypeNameSampleData.All.Count >= 1);
}
```

### 5. Update README.md and SUPPORTED_TYPES.md

Four places to update:

1. **Opening paragraph** — add the new type to the feature list.
2. **Supported types table** — add a row in the correct namespace section:
   ```
   | [`TypeName`](#heading-anchor) | Svenskt namn | Short description |
   ```
3. **SUPPORTED_TYPES.md** — add a row in the correct namespace section:
   ```
   | English Name | Svenska | Short description | `Buildi.Primitives.{Namespace}.{TypeName}` |
   ```
4. **Usage section** — add under the correct `###` namespace heading:
   ```
   #### Heading

   One-paragraph description with Swedish term and context.

   Tests: [TypeNameTests.cs](test/.../TypeNameTests.cs)

   - [Source](https://...) — description
   - [Source](https://...) — description

   ```csharp
   // TryParse example showing key properties
   // IsValid, Format, Normalize examples
   ```
   ```

Keep the README sources, SUPPORTED_TYPES.md, and the class XML `<remarks>` sources in sync.

### 6. Update the demo app

File: `samples/Buildi.Primitives.Demo/Pages/Home.razor`

Add a new entry to the `TypeRegistry` array in the correct category section (`// ── {Category} ──`):

```csharp
new("TypeName", "Category", "sample value",
    s => TypeName.IsValid(s),
    s => TypeName.Format(s),
    s => TypeName.Normalize(s),
    s => TypeName.IsNormalized(s),
    s => TypeName.TryParse(s, out var v) ? v!.ToMaskedString() : null,  // or _ => null if no masking
    s => TypeName.TryParse(s, out var v) ? Metadata($"Prop: {v!.Prop}", ...) : null,  // parsed properties
    NaturalFn: s => TypeName.TryParse(s, out var v) ? v!.ToNaturalString() : null,  // only for measurement types
    TypeMeta: TypeName.TypeInfo),
```

Rules:
- **Category** must match an existing section: `Contact`, `Person`, `Organization`, `Banking`, `Vehicle`, `Property`, `Product`, `Geography`, `Finance`, `Measurement`.
- **SampleValue** should be a valid, recognizable value that demonstrates the type well.
- **MaskedFn** — use `v!.ToMaskedString()` if the type has a masking extension, otherwise `_ => null`.
- **VariantsFn (metadata)** — use the `Metadata(...)` helper to expose interesting parsed properties (country codes, component parts, flags, EN/SV names). Pass `null` entries for conditional properties; the helper filters them out. Skip if the type has no interesting properties beyond Value.
- **NaturalFn** — only for measurement types and their wrappers that expose `ToNaturalString()`. Use the `NaturalFn:` named parameter.
- **TypeMeta** — always pass `TypeMeta: TypeName.TypeInfo` to link the entry to the type's static metadata (emoji, English/Swedish name, sources). The demo renders the emoji before the type name and shows names/sources as a tooltip.
- The `Metadata` helper accepts `params string?[]` and returns `IReadOnlyList<string>?` (null if all entries are null).

### 7. Add masking extension

File: `src/Buildi.Primitives/{Namespace}/{Namespace}MaskingExtensions.cs`

Every type needs a `ToMaskedString()` extension method. Add it to the existing masking extensions file for the namespace:

```csharp
/// <summary>
/// Returns a masked value, e.g. <c>5 x 114.3</c> → <c>* x *****</c>.
/// </summary>
public static string ToMaskedString(this TypeName value) =>
    // Mask sensitive/identifying parts while preserving structural characters
```

Rules:
- Masking files are per-namespace: `VehicleMaskingExtensions.cs`, `ProductMaskingExtensions.cs`, `MeasurementMaskingExtensions.cs`, etc.
- Use `*` as the mask character (defined as `private const char MaskChar = '*';` in each file).
- Preserve structural separators and unit suffixes (e.g. `x` in bolt patterns, `IP` prefix in IP ratings, unit symbols in measurements).
- Mask the numeric/identifying parts by replacing each character with `MaskChar`.

### 8. Add text scanning (when applicable)

Types that can reasonably be found in unstructured text need three things:

**a) `FindCandidatesInText` method on the type class** — see the "Text scanning conventions" section below for the implementation pattern.

**b) Registration in `TextScanner.Scan()`** — add a `ScanType(...)` call in `src/Buildi.Primitives/TextScanning/TextScanner.cs` with the correct `TextCandidateCategory`, and pass the result to the `TextScanResult` constructor.

**c) Typed accessor on `TextScanResult`** — in `src/Buildi.Primitives/TextScanning/TextScanResult.cs`, add:
1. A public `IReadOnlyList<TextCandidate<TypeName>>` property.
2. A constructor parameter for the list.
3. Assignment in the constructor body.
4. An `AddRange(all, ...)` call in the `All` list assembly block.

**d) Tests** — add `FindCandidatesInText` tests in `test/.../TextScanning/FindCandidatesInTextTests.cs` or in the type's own test file.

Skip text scanning for types whose values are too ambiguous to find in prose (e.g. single bare numbers with no distinguishing prefix or suffix).

## Naming conventions

- **Namespace** matches the domain area: `Organization`, `Banking`, `Contact`, `Web`, `Person`, `Property`, `Geography`, `Vehicle`, `Finance`, `Measurement`, `Product`.
- **Swedish-specific types** are prefixed with `Swedish` (e.g. `SwedishOrganizationNumber`, `SwedishBankAccount`, `SwedishCounty`).
- **EU-specific types** are prefixed with `Eu` (e.g. `EuVatNumber`, `EuEnergyEfficiencyClass`).
- **International types** used from a Swedish perspective have no prefix (e.g. `Iban`, `PhoneNumber`).
- **Geography types**: `Country` and `Continent` live in the `Geography` namespace alongside `SwedishCounty` and `SwedishMunicipality`.
- **Contact types**: address-related types are prefixed with `Address` (e.g. `AddressCity`, `AddressStreet`, `AddressZipCode`). Person name types are prefixed with `Person` (e.g. `PersonGivenName`, `PersonFamilyName`, `PersonFullName`).
- **Enums**: use `Unknown = 0` as the default value.
- **Properties**: avoid naming a property `Value` when a more specific name is appropriate (e.g. `Street` for the street part of `AddressStreet`, `Digits` for digit-only form of bankgiro). Use `Value` only for the canonical string form when no better name exists.

## Code style

- Target: .NET 10 (`net10.0`), `LangVersion` latest, nullable enabled, implicit usings enabled.
- Use `sealed class` with `private` constructor for all value objects.
- Use `RegexOptions.Compiled` for all regex patterns.
- Use Swedish culture (`CultureInfo.GetCultureInfo("sv-SE")`) for culture-sensitive operations.
- Internal helpers go in the same namespace with `internal` visibility (e.g. `PersonNameNormalization`).
- No comments that just narrate what the code does.
- XML docs are required on all public members with concrete examples of output.

## Validation philosophy

- All validation is in-memory. No external API calls.
- `IsValid()` normalizes first — a raw string may be considered valid if it can be successfully parsed.
- `Format()` returns a display-friendly form; it may differ from the input representation (e.g. country code → country name).
- `Normalize()` returns a machine-comparable, storage-safe canonical form.
- `IsNormalized()` checks if the input is already in its normalized form (`input == Normalize(input)`).
- Types with checksums (Luhn, MOD-97, MOD-11, etc.) validate checksums during `TryParse`.
- For types where check digit validation is not universally enforced (e.g. European VINs), parse successfully but expose a `HasValidCheckDigit` property.

## Structured validation (`Validate`)

Types with meaningful, enumerable failure reasons expose a `public static ValidationResult Validate(string? input)` method. This returns a `ValidationResult` containing the raw input, a validity flag, and a list of `ValidationIssue` objects — each with a `ValidationErrorReason` enum value plus English and localized (Swedish) descriptions.

### When to add `Validate`

Add it when a type's `TryParse` has multiple distinct failure paths that a consumer would benefit from knowing about (e.g. wrong length, invalid check digit, unknown bank). Do **not** add it for types where failure is always a single generic "invalid format" (e.g. `AddressCity`, `PersonGivenName`).

### Infrastructure

Shared types live in `src/Buildi.Primitives/Validation/`:

- `ValidationErrorReason` — single shared enum across all types, grouped by domain area. Add new values at the end of the appropriate group.
- `ValidationResult` — sealed class with `RawInput`, `IsValid`, `Issues`. Created via `Valid(rawInput)`, `Invalid(rawInput, reason, english, localized)`, or `Invalid(rawInput, List<ValidationIssue>)`.
- `ValidationIssue` — sealed class with `Reason`, `EnglishDescription`, `LocalizedDescription`, and `Description` (locale-aware via `PrimitivesDefaults.UseSwedishDisplayNames`).

### Adding `Validate` to a type

1. Add `using Buildi.Primitives.Validation;` to the type file.
2. Add the method after the existing `Format`/`Normalize` methods and before private helpers:

```csharp
/// <summary>
/// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
/// or a valid result when the input is a valid {type}.
/// </summary>
public static ValidationResult Validate(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
        return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
            "Input is empty or whitespace.", "Värdet är tomt.");

    // Mirror TryParse logic, returning specific reasons at each failure point
    // For single-issue early returns:
    //   return ValidationResult.Invalid(input, ValidationErrorReason.XXX, "english", "swedish");
    // For collecting multiple issues:
    //   var issues = new List<ValidationIssue>();
    //   issues.Add(new ValidationIssue(ValidationErrorReason.XXX, "english", "swedish"));
    //   return ValidationResult.Invalid(input, issues);

    return ValidationResult.Valid(input);
}
```

3. Add tests in the type's existing test file:

```csharp
[Theory]
[InlineData(null, false, ValidationErrorReason.InputIsEmpty)]
[InlineData("", false, ValidationErrorReason.InputIsEmpty)]
[InlineData("invalid", false, ValidationErrorReason.XXX)]
[InlineData("valid", true, null)]
public void Validate_ReturnsExpectedResult(string? input, bool expectedIsValid, ValidationErrorReason? expectedReason)

[Fact]
public void Validate_Issues_ContainBothLanguageDescriptions()

[Theory]
public void Validate_IsValid_MatchesIsValid(string? input)
// Assert.Equal(TypeName.IsValid(input), TypeName.Validate(input).IsValid);
```

### Naming conventions

- Reuse existing `ValidationErrorReason` values where they fit (e.g. `InputIsEmpty`, `InvalidCheckDigit`, `InvalidLength`).
- Only add new enum values when no existing value captures the meaning.
- English descriptions should be concise, user-facing sentences.
- Swedish descriptions (`LocalizedDescription`) should match the English meaning in natural Swedish.

### Types with `Validate`

`SwedishBankClearingNumber`, `SwedishBankAccount`, `SwedishVehicleRegistrationNumber`, `SwedishOrganizationNumber`, `Iban`, `SwedishBankgiroNumber`, `SwedishPostgiroNumber`, `SwedishOcrReferenceNumber`, `SwedishPersonalIdentityNumber`, `SwedishCoordinationNumber`, `Bic`, `EuVatNumber`, `EmailAddress`, `VehicleIdentificationNumber`.

## Text scanning conventions

Types that can reasonably be found in unstructured text expose a static `FindCandidatesInText(string text)` method. These live in the type's own class (not in a separate file) and follow this pattern:

1. Define a `private static readonly Regex ScanPattern` — an unanchored pattern with appropriate boundaries (`\b`, `(?<!\d)`, `(?!\d)`, etc.) to find potential matches in prose.
2. Use `RegexOptions.Compiled` on the scan regex.
3. Each regex match is passed to `TryParse`. Only successful parses produce candidates.
4. `MaskedForm` is populated by calling the type's `ToMaskedString()` extension method.
5. `Confidence` is assigned based on the type's characteristics (e.g. `High` for checksum-validated types, `Low` for raw digit sequences).
6. The XML doc on `FindCandidatesInText` must include a disclaimer about heuristic nature and false positives.

Infrastructure types live in `src/Buildi.Primitives/TextScanning/`:

- `TextMatchConfidence` — enum: Low, Medium, High
- `TextCandidateCategory` — enum for broad classification
- `TextCandidate` / `TextCandidate<T>` — non-generic base + generic with `Value`
- `TextScanner` — aggregate scanner
- `TextScanResult` — typed accessors, overlap resolution, MaskAll/RedactAll/ReplaceAll
- `TextScannerOptions` — include/exclude categories, min confidence

When adding a new scannable type, also:
- Add a `ToMaskedString()` extension if one doesn't exist
- Add `FindCandidatesInText` tests in `test/.../TextScanning/FindCandidatesInTextTests.cs`
- Register the type in `TextScanner.Scan()` and `TextScanResult`
- Add a typed accessor property on `TextScanResult`

## Building and testing

```shell
dotnet test   # from repository root — builds and runs all tests
```

All tests must pass before completing any change. 