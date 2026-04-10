namespace Buildi.Primitives.Validation;

/// <summary>
/// Describes why a value failed validation. Shared across all types that support
/// the <c>Validate</c> method. Not every reason applies to every type.
/// </summary>
public enum ValidationErrorReason
{
    // --- Common ---
    InputIsEmpty,
    InputTooLong,
    InputTooShort,
    InvalidLength,
    InvalidFormat,
    InvalidCharacters,
    InvalidCheckDigit,

    // --- Banking / clearing ---
    UnknownClearingRange,
    InvalidSwedbankFormat,
    InvalidAccountLengthForBank,
    InvalidClearingNumber,

    // --- Identity / organization ---
    InvalidDate,
    InvalidPrefix,
    InvalidEntityPattern,

    // --- International / country ---
    UnknownCountryCode,
    InvalidCountryPrefix,
    InvalidLengthForCountry,

    // --- OCR ---
    InvalidLengthDigit,

    // --- Email ---
    MissingAtSign,
    InvalidLocalPart,
    InvalidDomain,
}
