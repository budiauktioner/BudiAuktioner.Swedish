namespace Buildi.Primitives.TextScanning;

/// <summary>
/// A potential match of a structured type found in unstructured text.
/// This is the non-generic base class that enables mixed-type collections in aggregate scan results.
/// </summary>
/// <remarks>
/// <para>Text scanning is heuristic-based. Candidates may be false positives, and valid
/// occurrences may be missed. No guarantee is made that the matched text actually represents
/// the identified type in its original context.</para>
/// </remarks>
public abstract class TextCandidate
{
    /// <summary>Zero-based start position in the scanned text.</summary>
    public int StartIndex { get; }

    /// <summary>Number of characters in the original matched span.</summary>
    public int Length { get; }

    /// <summary>Exclusive end position (<see cref="StartIndex"/> + <see cref="Length"/>).</summary>
    public int EndIndex => StartIndex + Length;

    /// <summary>The raw substring from the input text that was matched.</summary>
    public string OriginalText { get; }

    /// <summary>Short type name, e.g. <c>EmailAddress</c> or <c>SwedishPersonalIdentityNumber</c>.</summary>
    public string TypeName { get; }

    /// <summary>Broad category of the matched type.</summary>
    public TextCandidateCategory Category { get; }

    /// <summary>Machine-comparable canonical form of the matched value.</summary>
    public string NormalizedForm { get; }

    /// <summary>Human-readable display form of the matched value.</summary>
    public string FormattedForm { get; }

    /// <summary>
    /// Display form with sensitive content masked, e.g. <c>990807-****</c>.
    /// Produced by the type's existing <c>ToMaskedString()</c> extension method.
    /// </summary>
    public string MaskedForm { get; }

    /// <summary>Heuristic confidence level. See <see cref="TextMatchConfidence"/> for guidance.</summary>
    public TextMatchConfidence Confidence { get; }

    private protected TextCandidate(
        int startIndex,
        int length,
        string originalText,
        string typeName,
        TextCandidateCategory category,
        string normalizedForm,
        string formattedForm,
        string maskedForm,
        TextMatchConfidence confidence)
    {
        StartIndex = startIndex;
        Length = length;
        OriginalText = originalText;
        TypeName = typeName;
        Category = category;
        NormalizedForm = normalizedForm;
        FormattedForm = formattedForm;
        MaskedForm = maskedForm;
        Confidence = confidence;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this candidate's span overlaps with <paramref name="other"/>.
    /// </summary>
    public bool Overlaps(TextCandidate other) =>
        StartIndex < other.EndIndex && other.StartIndex < EndIndex;

    /// <summary>
    /// Returns <see langword="true"/> if this candidate's span fully contains <paramref name="other"/>.
    /// </summary>
    public bool Contains(TextCandidate other) =>
        StartIndex <= other.StartIndex && EndIndex >= other.EndIndex;
}

/// <summary>
/// A potential match of type <typeparamref name="T"/> found in unstructured text,
/// carrying the parsed instance alongside position and display information.
/// </summary>
/// <remarks>
/// <para>Text scanning is heuristic-based. Candidates may be false positives, and valid
/// occurrences may be missed. No guarantee is made that the matched text actually represents
/// the identified type in its original context.</para>
/// </remarks>
/// <typeparam name="T">The value-object type (e.g. <c>EmailAddress</c>, <c>SwedishPersonalIdentityNumber</c>).</typeparam>
public sealed class TextCandidate<T> : TextCandidate
{
    /// <summary>The successfully parsed instance of the matched type.</summary>
    public T Value { get; }

    public TextCandidate(
        int startIndex,
        int length,
        string originalText,
        string typeName,
        TextCandidateCategory category,
        string normalizedForm,
        string formattedForm,
        string maskedForm,
        TextMatchConfidence confidence,
        T value)
        : base(startIndex, length, originalText, typeName, category, normalizedForm, formattedForm, maskedForm, confidence)
    {
        Value = value;
    }
}
