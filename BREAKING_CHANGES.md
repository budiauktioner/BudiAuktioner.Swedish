# Breaking changes

Backwards-incompatible changes between versions of `Buildi.Primitives`. Newest version on top.
Non-breaking additions belong in regular release notes / commit history, not here.

## Conventions

- One `## Breaking changes <previous-version> - <new-version>` heading per release that contains breaking changes (e.g. `## Breaking changes 0.13.0 - 0.14.0`). The range makes it explicit which upgrade hop the entries apply to.
- One `### <short title>` heading per breaking change inside that version. Use as many sub-sections per change as you need: **What changed**, **Why**, **Migration**, **Examples**, etc.
- Order versions from newest to oldest.

## Breaking changes 0.18.0 - 0.19.0

### `PhoneNumber.TryParse` strips trunk-prefix `0` consistently across all input branches

**Area:** `Buildi.Primitives.Contact`

**What changed**

`PhoneNumber.TryParse` now strips a single leading trunk-prefix `0` from the subscriber
portion of the number in **all** input branches, not just the local-format and
default-calling-code branches. Concretely:

- Inputs with an explicit `+` country-code prefix (e.g. `"+46 0701740633"`,
  `"+370 0738031398"`, `"0044 020 7946 0958"`) now have the stray trunk `0` after the
  country code stripped. Previously these passed through verbatim, producing canonical
  forms with an extra `0` such as `"00460701740633"` and `"003700738031398"`.
- Inputs with the `00` international prefix get the same fix.
- All branches now strip **at most one** leading `0`, replacing the previous
  `TrimStart('0')` (which would silently flatten any number of leading zeros and could
  mask malformed input).
- A new allow-list `CallingCodesWhereLeadingZeroIsSignificant` carves out countries where
  the leading `0` is part of the subscriber number rather than a national trunk prefix.
  Currently only Italy (`+39`) is on the list — Italian landline area codes (`02` Milan,
  `06` Rome, `011` Turin, `081` Naples, …) start with `0` and that `0` must be preserved.
  For these countries `TryParse` no longer strips the `0` from any branch.

**Why**

The four input branches (`+`-prefix, `00`-prefix, default-calling-code-match, and
local-`0`) had inconsistent trunk-prefix handling. Same logical input expressed three
different ways could yield three different normalized forms — including ones that were
not valid E.164. The fix makes branch behavior consistent and adds the Italy carve-out
that the previous greedy `TrimStart('0')` got wrong.

**Migration**

Most callers will not need to change anything — the new behavior is what you'd intuitively
expect. The two cases worth knowing about:

```csharp
// Before (0.18.0)                                              // After (0.19.0)
PhoneNumber.Parse("+46 0701740633").Digits;                     PhoneNumber.Parse("+46 0701740633").Digits;
// "00460701740633"   (extra 0 preserved — bug)                 // "0046701740633"  (single trunk 0 stripped)

PhoneNumber.Parse("+370 0738031398").Digits;                    PhoneNumber.Parse("+370 0738031398").Digits;
// "003700738031398"  (extra 0 preserved — bug)                 // "00370738031398" (single trunk 0 stripped)

PhoneNumber.Parse("+39 06 12345678").Digits;                    PhoneNumber.Parse("+39 06 12345678").Digits;
// "00390612345678"   (correct — pre-existing behavior)         // "00390612345678" (unchanged — Italy carve-out)

PhoneNumber.Parse("0612345678", "39").Digits;                   PhoneNumber.Parse("0612345678", "39").Digits;
// "0039612345678"    (0 stripped — wrong for Italy)            // "00390612345678" (Italy carve-out keeps the 0)
```

If you have **stored canonical digits from 0.18.0 or earlier**, two categories of stored
values are now wrong and would benefit from re-normalising the original raw input through
0.19.0's `TryParse`:

1. Inputs prefixed with `+CC` or `00CC` that contained a stray trunk `0` (e.g. user-pasted
   `+46 0701234567`, `+370 0738031398`). Stored as `"00460701234567"` / `"003700738031398"`;
   should be `"0046701234567"` / `"00370738031398"`.
2. Italian inputs in local format parsed with default calling code `"39"` (e.g.
   `"0612345678"` for Rome). Stored as `"0039612345678"` (the leading `0` was stripped);
   should be `"00390612345678"` (the `0` is part of the area code).

All other previously-stored canonical digits are unaffected.

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
