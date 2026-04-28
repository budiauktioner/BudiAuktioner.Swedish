# Breaking changes

Backwards-incompatible changes between versions of `Buildi.Primitives`. Newest version on top.
Non-breaking additions belong in regular release notes / commit history, not here.

## Conventions

- One `## Breaking changes <previous-version> - <new-version>` heading per release that contains breaking changes (e.g. `## Breaking changes 0.13.0 - 0.14.0`). The range makes it explicit which upgrade hop the entries apply to.
- One `### <short title>` heading per breaking change inside that version. Use as many sub-sections per change as you need: **What changed**, **Why**, **Migration**, **Examples**, etc.
- Order versions from newest to oldest.

## Breaking changes 0.17.1 - 0.18.0

### `BodyType` split overlapping aliases into dedicated canonicals

**Area:** `Buildi.Primitives.Vehicle`

**What changed**

`BodyType` previously collapsed several distinct Swedish vehicle categories into broader
canonicals, which lost regulatory and market meaning. Two new canonicals have been added
and a number of aliases have been re-routed or removed.

- New canonicals on `BodyType`: `LightTruck` (English `Light truck`, Swedish `Lätt lastbil`)
  and `OffRoad` (English `Off-road vehicle`, Swedish `Terrängbil`).
- `BodyType.Normalize("Lätt lastbil")` now returns `"Light truck"` (was `"Van"`).
- `BodyType.Normalize("Terrängbil")` now returns `"Off-road"` (was `"SUV"`).
- `BodyType.Normalize("Off-roader")` now returns `"Off-road"` (was `"SUV"`).
- `BodyType.Normalize("Minibuss")` now returns `"Bus"` (was `"Minivan"`).
- `BodyType.Minivan.LocalizedName` changed from `"Minibuss"` to `"Minivan"` (loanword).
  This also changes `BodyType.Minivan.ToString()` under a Swedish UI culture from
  `"Minibuss"` to `"Minivan"`. The change was needed to avoid `Minibuss` being
  auto-claimed by `Minivan` via the canonical's localized name, which prevented
  routing it to `Bus` (the more accurate Swedish meaning of *minibuss*).
- `BodyType.Normalize("Articulated dump truck")` now resolves to `"Dumper"` (previously
  the `"Dumptruck"` alias was silently absorbed by `Tipper` due to a whitespace-stripping
  alias-key collision with `"Dump truck"`).
- The following aliases were removed entirely because they were too generic to reliably
  carry a single canonical meaning. Inputs that previously parsed as the listed canonical
  now return `null` from `TryParse`:
  - `4-door` (was `Sedan`)
  - `Familjebil` (was `MPV`)
  - `Öppen` (was `Convertible`)

**Why**

The Swedish vehicle register and Swedish auction listings draw distinctions between
*Lätt lastbil* (≤ 3.5 t, EU category N1, B-licence) and *Tung lastbil* (> 3.5 t,
EU category N2/N3), and between *Terrängbil* (a separate Transportstyrelsen vehicle
type) and SUV. Collapsing `Lätt lastbil` into `Van` and `Terrängbil` into `SUV` lost
this distinction. The `Dump truck` / `Dumptruck` alias collision additionally caused
`Bergsdumper`-style inputs to silently lose to `Tipper` based on alias insertion order.

**Migration**

For systems that depend on the old canonical strings, remap as follows:

```csharp
// Before (0.17.x)
BodyType.Normalize("Lätt lastbil");   // "Van"
BodyType.Normalize("Terrängbil");     // "SUV"
BodyType.Normalize("Off-roader");     // "SUV"
BodyType.Normalize("Minibuss");       // "Minivan"
BodyType.Normalize("Familjebil");     // "MPV"
BodyType.Normalize("Öppen");          // "Convertible"
BodyType.Normalize("4-door");         // "Sedan"

// After (0.18.0)
BodyType.Normalize("Lätt lastbil");   // "Light truck"
BodyType.Normalize("Terrängbil");     // "Off-road"
BodyType.Normalize("Off-roader");     // "Off-road"
BodyType.Normalize("Minibuss");       // "Bus"
BodyType.Normalize("Familjebil");     // null
BodyType.Normalize("Öppen");          // null
BodyType.Normalize("4-door");         // null
```

If you relied on the dropped generic aliases, normalise them upstream before parsing
(e.g. mark `"Familjebil"` inputs as `MPV` or `Stationwagon` based on additional context).

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
