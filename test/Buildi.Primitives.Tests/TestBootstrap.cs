using System.Globalization;
using System.Runtime.CompilerServices;

namespace Buildi.Primitives.Tests;

/// <summary>
/// Ensures the test process runs with sv-SE culture regardless of the CI runner's locale.
/// Without this, <see cref="PrimitivesDefaults"/> auto-detects from <see cref="CultureInfo.CurrentCulture"/>
/// and tests that rely on Swedish defaults (phone numbers, zip codes, addresses, money formatting)
/// fail on en-US runners (e.g. GitHub Actions).
/// </summary>
internal static class TestBootstrap
{
    [ModuleInitializer]
    internal static void SetDefaultCulture()
    {
        var swedish = CultureInfo.GetCultureInfo("sv-SE");
        CultureInfo.DefaultThreadCurrentCulture = swedish;
        CultureInfo.DefaultThreadCurrentUICulture = swedish;
    }
}
