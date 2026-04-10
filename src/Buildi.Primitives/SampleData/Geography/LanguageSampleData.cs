namespace Buildi.Primitives.SampleData.Geography;

/// <summary>
/// Valid <see cref="Primitives.Geography.Language"/> values representing commonly
/// referenced languages in a Swedish context.
/// </summary>
public static class LanguageSampleData
{
    public static Primitives.Geography.Language Swedish { get; } = Primitives.Geography.Language.Parse("sv");
    public static Primitives.Geography.Language English { get; } = Primitives.Geography.Language.Parse("en");
    public static Primitives.Geography.Language German { get; } = Primitives.Geography.Language.Parse("de");
    public static Primitives.Geography.Language French { get; } = Primitives.Geography.Language.Parse("fr");
    public static Primitives.Geography.Language Spanish { get; } = Primitives.Geography.Language.Parse("es");
    public static Primitives.Geography.Language Arabic { get; } = Primitives.Geography.Language.Parse("ar");
    public static Primitives.Geography.Language Finnish { get; } = Primitives.Geography.Language.Parse("fi");
    public static Primitives.Geography.Language Norwegian { get; } = Primitives.Geography.Language.Parse("no");
    public static Primitives.Geography.Language Danish { get; } = Primitives.Geography.Language.Parse("da");

    public static IReadOnlyList<Primitives.Geography.Language> All { get; } =
        [Swedish, English, German, French, Spanish, Arabic, Finnish, Norwegian, Danish];
}
