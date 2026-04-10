---
_layout: landing
---

# Buildi.Primitives

[![NuGet](https://img.shields.io/nuget/v/Buildi.Primitives)](https://www.nuget.org/packages/Buildi.Primitives)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Buildi.Primitives)](https://www.nuget.org/packages/Buildi.Primitives)
[![Build](https://github.com/budiauktioner/Buildi.Primitives/actions/workflows/build.yml/badge.svg)](https://github.com/budiauktioner/Buildi.Primitives/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/budiauktioner/Buildi.Primitives/blob/main/LICENSE)

A .NET class library of strongly typed, self-validating domain primitives for data validation and normalization — designed for a Swedish context, with many types useful globally.

## Quick example

```csharp
using Buildi.Primitives.Organization;
using Buildi.Primitives.Banking;
using Buildi.Primitives.Contact;

if (SwedishOrganizationNumber.TryParse("5592460421", out var orgNumber))
{
    Console.WriteLine(orgNumber.Value);      // "165592460421"
    Console.WriteLine(orgNumber.ToString()); // "559246-0421"
}

SwedishOrganizationNumber.IsValid("5592460421");    // true
SwedishOrganizationNumber.Format("5592460421");     // "559246-0421"
SwedishOrganizationNumber.Normalize("559246-0421"); // "165592460421"

SwedishBankgiroNumber.Format("58056201");           // "5805-6201"
Iban.IsValid("SE4550000000058398257466");            // true

PhoneNumber.Normalize("070-174 06 33");             // "0046701740633"
EmailAddress.Format("  User@GMAIL.COM  ");          // "user@gmail.com"
```

## Key features

- 📦 **100+ value-object types** covering organization numbers, VAT, banking, addresses, vehicles, measurements, and more
- 🔁 **Consistent API** — every type exposes `TryParse`, `Parse`, `IsValid`, `Format`, `Normalize`, and `IsNormalized`
- ✅ **Structured validation** — many types provide a `Validate()` method with machine-readable reason codes and descriptions in English and Swedish
- 🔍 **Text scanning** — heuristic extraction of structured data from unstructured text with confidence scoring and overlap resolution
- 🔒 **PII masking** — `ToMaskedString()` extension methods for safe display of sensitive data
- 🧠 **In-memory only** — no external API calls; all validation uses rules, patterns, checksums, and reference data at build time

## Get started

```shell
dotnet add package Buildi.Primitives
```

See the [Getting started](articles/getting-started.md) guide for a walkthrough, or jump straight to the [API reference](api/index.md).

## Links

- [GitHub repository](https://github.com/budiauktioner/Buildi.Primitives)
- [NuGet package](https://www.nuget.org/packages/Buildi.Primitives)
- [Full README](https://github.com/budiauktioner/Buildi.Primitives/blob/main/README.md) — complete type reference with examples
