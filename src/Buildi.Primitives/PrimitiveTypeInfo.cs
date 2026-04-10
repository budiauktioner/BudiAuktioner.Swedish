namespace Buildi.Primitives;

/// <summary>
/// Metadata describing a primitive value-object type — its human-readable names,
/// representative emoji, and reference source URLs.
/// </summary>
public sealed record PrimitiveTypeInfo(
    string EnglishName,
    string LocalizedName,
    string Emoji,
    IReadOnlyList<string> Sources);
