namespace Buildi.Primitives.TextScanning;

/// <summary>
/// Indicates the confidence level of a candidate found by text scanning.
/// Higher confidence means stronger structural evidence, but no level guarantees
/// the absence of false positives.
/// </summary>
public enum TextMatchConfidence
{
    /// <summary>
    /// Pattern match only. High false-positive risk — the matched text merely fits a
    /// syntactic pattern (e.g. a run of digits that happens to be the right length).
    /// </summary>
    Low = 0,

    /// <summary>
    /// Structural match with partial validation — the format is correct but no checksum
    /// or strong distinguishing feature confirms the match.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Strong match with checksum or format-specific validation (e.g. a personal identity
    /// number with a valid Luhn check digit and plausible date, or an IBAN with valid MOD-97).
    /// </summary>
    High = 2
}
