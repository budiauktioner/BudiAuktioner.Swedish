namespace Buildi.Primitives.Validation;

/// <summary>
/// A single validation problem detected during <c>Validate</c>, carrying a machine-readable
/// <see cref="Reason"/> and human-readable descriptions in English and the current locale.
/// </summary>
public sealed class ValidationIssue
{
    /// <summary>Machine-readable reason code.</summary>
    public ValidationErrorReason Reason { get; }

    /// <summary>Human-readable description in English.</summary>
    public string EnglishDescription { get; }

    /// <summary>Human-readable description in the current locale (Swedish when available).</summary>
    public string LocalizedDescription { get; }

    /// <summary>
    /// Convenience accessor that returns <see cref="LocalizedDescription"/> when
    /// <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/> is <see langword="true"/>,
    /// otherwise <see cref="EnglishDescription"/>.
    /// </summary>
    public string Description => PrimitivesDefaults.UseLocalizedDisplayNames
        ? LocalizedDescription
        : EnglishDescription;

    public ValidationIssue(ValidationErrorReason reason, string englishDescription, string localizedDescription)
    {
        Reason = reason;
        EnglishDescription = englishDescription;
        LocalizedDescription = localizedDescription;
    }
}
