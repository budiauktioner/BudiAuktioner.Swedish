# Breaking changes

Backwards-incompatible changes between versions of `Buildi.Primitives`. Newest version on top.
Non-breaking additions belong in regular release notes / commit history, not here.

## Conventions

- One `## Breaking changes <previous-version> - <new-version>` heading per release that contains breaking changes (e.g. `## Breaking changes 0.13.0 - 0.14.0`). The range makes it explicit which upgrade hop the entries apply to.
- One `### <short title>` heading per breaking change inside that version. Use as many sub-sections per change as you need: **What changed**, **Why**, **Migration**, **Examples**, etc.
- Order versions from newest to oldest.

## Breaking changes 0.13.0 - 0.14.0

### `SwedishOrganizationName` narrowed back to strict Bolagsverket charset

**Area:** `Buildi.Primitives.Organization`

**What changed**

The `SwedishOrganizationName` type's allowed character set was reverted to the strict
Bolagsverket-style set: Unicode letters (`\p{L}`), digits, whitespace, and the punctuation
`- ' & . , / : ( ) +`. The following are no longer part of the type:

- The pipe character `|` is no longer accepted by the regex (`SwedishOrganizationName.IsValid("Volvo AB||Cars")` now returns `false`).
- The ASCII double-quote character `"` is no longer accepted by the regex (`SwedishOrganizationName.IsValid("SIA \"Example LV\"")` now returns `false`).
- The `LegalName`, `TradeName`, and `HasTradeName` instance properties were removed.
- The `TrySplitLegalAndTrade(string?, out string, out string?)` static method was removed.
- `NormalizeCasing` no longer treats `|` or `"` as word boundaries.

**Why**

Those features only existed to handle data from cross-border sources (Greek GEMI exports
joining legal and trade names with `||`, Baltic/Slavic registries that legally enclose the
distinctive name in double quotes). Mixing that into the Swedish type bloated its scope
and caused inputs that Bolagsverket would never accept to validate as Swedish names.

**Migration**

Use the new `EuOrganizationName` type (also in `Buildi.Primitives.Organization`) for any
input that may originate from a cross-border feed (VIES, EORI, GEMI, Dun & Bradstreet,
etc.). It preserves the broader character set and exposes the same split API:

```csharp
// Before (0.13.x)
SwedishOrganizationName.TryParse("Volvo AB||Volvo Cars", out var name);
var legal = name!.LegalName;     // "Volvo AB"
var trade = name.TradeName;      // "Volvo Cars"

// After (0.14.0)
EuOrganizationName.TryParse("Volvo AB||Volvo Cars", out var name);
var legal = name!.LegalName;     // "Volvo AB"
var trade = name.TradeName;      // "Volvo Cars"
```

`EuOrganizationName` does **not** infer a Swedish organization type — for that, parse the
extracted `LegalName` with `SwedishOrganizationName` (or call
`SwedishOrganizationName.InferSwedishOrganizationType(name.LegalName)`).
