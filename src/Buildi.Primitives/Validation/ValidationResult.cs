namespace Buildi.Primitives.Validation;

/// <summary>
/// The outcome of a <c>Validate</c> call, carrying the original input, a validity flag,
/// and zero or more <see cref="ValidationIssue"/> entries that explain why the input is invalid.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>The raw input string that was validated.</summary>
    public string? RawInput { get; }

    /// <summary><see langword="true"/> when the input is valid; otherwise <see langword="false"/>.</summary>
    public bool IsValid { get; }

    /// <summary>
    /// The validation issues detected. Empty when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues { get; }

    private ValidationResult(string? rawInput, bool isValid, IReadOnlyList<ValidationIssue> issues)
    {
        RawInput = rawInput;
        IsValid = isValid;
        Issues = issues;
    }

    internal static ValidationResult Valid(string? rawInput) =>
        new(rawInput, true, []);

    internal static ValidationResult Invalid(string? rawInput, List<ValidationIssue> issues) =>
        new(rawInput, false, issues);

    internal static ValidationResult Invalid(string? rawInput, ValidationErrorReason reason, string englishDescription, string localizedDescription) =>
        new(rawInput, false, [new ValidationIssue(reason, englishDescription, localizedDescription)]);
}
