namespace Buildi.Primitives.TextScanning;

/// <summary>
/// Configuration for the text scanner controlling which types to scan for
/// and the minimum confidence threshold.
/// </summary>
public sealed class TextScannerOptions
{
    /// <summary>
    /// When set, only categories in this set are scanned. Mutually exclusive with <see cref="ExcludeCategories"/>.
    /// </summary>
    public IReadOnlySet<TextCandidateCategory>? IncludeCategories { get; init; }

    /// <summary>
    /// When set, categories in this set are skipped. Ignored if <see cref="IncludeCategories"/> is set.
    /// </summary>
    public IReadOnlySet<TextCandidateCategory>? ExcludeCategories { get; init; }

    /// <summary>
    /// Candidates below this confidence level are discarded from results.
    /// Defaults to <see cref="TextMatchConfidence.Low"/> (keep everything).
    /// </summary>
    public TextMatchConfidence MinimumConfidence { get; init; } = TextMatchConfidence.Low;

    internal bool ShouldScan(TextCandidateCategory category)
    {
        if (IncludeCategories is not null)
            return IncludeCategories.Contains(category);

        if (ExcludeCategories is not null)
            return !ExcludeCategories.Contains(category);

        return true;
    }
}
