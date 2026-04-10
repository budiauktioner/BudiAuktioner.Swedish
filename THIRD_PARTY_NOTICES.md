# Third-Party Notices

`Buildi.Primitives` source code is licensed under the MIT license. This file documents the provenance of embedded reference data and clarifies which third-party sources the package follows.

## What ships in the package

The package ships maintainer-authored C# source code plus embedded reference data compiled into that source code.

The package does **not** ship:

- raw third-party country data files
- machine-readable ISO data tables
- Wikipedia dumps or other third-party bulk exports

References to ISO standards in the codebase identify the standards the library follows, not a redistributed ISO dataset.

## Embedded reference data sources

The current package uses or aligns with the following public/open reference sources:

- **SCB** for Swedish counties and municipalities
  - https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/
- **PTS** for Swedish fictional/test phone-number ranges
  - https://pts.se/internet-och-telefoni/telefonnummer-och-adressering/telefonnummer-till-bocker-och-filmer/
- **IANA Root Zone Database** for ccTLD data
  - https://www.iana.org/domains/root/db
- **ITU-T E.164** for country calling-code allocations
  - https://www.itu.int/rec/T-REC-E.164/
- **Wikidata** for country coordinates, capitals, and land-border relationships
  - P625 (coordinate location): https://www.wikidata.org/wiki/Property:P625
  - P36 (capital): https://www.wikidata.org/wiki/Property:P36
  - P47 (shares border with): https://www.wikidata.org/wiki/Property:P47
- **European Payments Council** for SEPA scheme geography
  - https://www.europeanpaymentscouncil.eu/document-library/other/epc-list-sepa-scheme-countries

## Maintainer-curated geography data

Current country-border, capital, and coordinate metadata should be treated as maintainer-curated data aligned to the public sources listed above, primarily Wikidata and EPC documentation.
