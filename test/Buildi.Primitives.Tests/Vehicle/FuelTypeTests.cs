using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class FuelTypeTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(16, FuelType.All.Count);
    }

    [Theory]
    [InlineData("Petrol")]
    [InlineData("petrol")]
    [InlineData("Bensin")]
    [InlineData("BENSIN")]
    [InlineData("BE")]
    [InlineData("Gasoline")]
    [InlineData("Gas")]
    [InlineData("Blyfri")]
    [InlineData("Blyfri bensin")]
    [InlineData("95")]
    [InlineData("98")]
    [InlineData("Diesel")]
    [InlineData("DIESEL")]
    [InlineData("DI")]
    [InlineData("Dieselolja")]
    [InlineData("Dieselbränsle")]
    [InlineData("Electric")]
    [InlineData("El")]
    [InlineData("EL")]
    [InlineData("Elmotor")]
    [InlineData("Elektrisk")]
    [InlineData("Elbil")]
    [InlineData("BEV")]
    [InlineData("Battery Electric")]
    [InlineData("Ethanol")]
    [InlineData("Etanol")]
    [InlineData("E85")]
    [InlineData("ET")]
    [InlineData("Flexifuel")]
    [InlineData("Flex Fuel")]
    [InlineData("FFV")]
    [InlineData("Natural gas")]
    [InlineData("Naturgas")]
    [InlineData("CNG")]
    [InlineData("Fordonsgas")]
    [InlineData("Komprimerad naturgas")]
    [InlineData("NGV")]
    [InlineData("LPG")]
    [InlineData("Gasol")]
    [InlineData("Autogas")]
    [InlineData("Propan")]
    [InlineData("Hybrid")]
    [InlineData("HYBRID")]
    [InlineData("Elhybrid")]
    [InlineData("HEV")]
    [InlineData("Bensin/El")]
    [InlineData("Mild hybrid")]
    [InlineData("MHEV")]
    [InlineData("48V")]
    [InlineData("Mildhybrid")]
    [InlineData("Plug-in hybrid")]
    [InlineData("Laddhybrid")]
    [InlineData("PHEV")]
    [InlineData("Plug-in")]
    [InlineData("Laddbar hybrid")]
    [InlineData("Hydrogen")]
    [InlineData("Vätgas")]
    [InlineData("VÄTGAS")]
    [InlineData("H2")]
    [InlineData("Bränslecell")]
    [InlineData("Fuel Cell")]
    [InlineData("FCEV")]
    [InlineData("Vätgasbil")]
    [InlineData("Biodiesel")]
    [InlineData("B100")]
    [InlineData("FAME")]
    [InlineData("BIO")]
    [InlineData("RME")]
    [InlineData("Methane")]
    [InlineData("Metangas")]
    [InlineData("Biogas")]
    [InlineData("CBG")]
    [InlineData("Metan")]
    [InlineData("Biometan")]
    [InlineData("LBG")]
    [InlineData("Flytande biogas")]
    [InlineData("Methanol")]
    [InlineData("Metanol")]
    [InlineData("M85")]
    [InlineData("HVO")]
    [InlineData("HVO100")]
    [InlineData("Hydrotreated Vegetable Oil")]
    [InlineData("Förnybar diesel")]
    [InlineData("Kerosene")]
    [InlineData("Fotogen")]
    [InlineData("FOTOGEN")]
    [InlineData("Paraffin")]
    [InlineData("FO")]
    [InlineData("Other")]
    [InlineData("Annat")]
    [InlineData("Övrigt")]
    [InlineData("ÖVR")]
    [InlineData("Övrig")]
    [InlineData("  Diesel  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(FuelType.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("InvalidFuel")]
    [InlineData("Water")]
    [InlineData("Steam")]
    [InlineData("Nuclear")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(FuelType.IsValid(input));
    }

    [Theory]
    [InlineData("Petrol",         "Petrol",         "Petrol",         "Bensin",     "BE")]
    [InlineData("Bensin",         "Petrol",         "Petrol",         "Bensin",     "BE")]
    [InlineData("BE",             "Petrol",         "Petrol",         "Bensin",     "BE")]
    [InlineData("Gasoline",       "Petrol",         "Petrol",         "Bensin",     "BE")]
    [InlineData("95",             "Petrol",         "Petrol",         "Bensin",     "BE")]
    [InlineData("Diesel",         "Diesel",         "Diesel",         "Diesel",     "DI")]
    [InlineData("DI",             "Diesel",         "Diesel",         "Diesel",     "DI")]
    [InlineData("Dieselolja",     "Diesel",         "Diesel",         "Diesel",     "DI")]
    [InlineData("Electric",       "Electric",       "Electric",       "El",         "EL")]
    [InlineData("El",             "Electric",       "Electric",       "El",         "EL")]
    [InlineData("BEV",            "Electric",       "Electric",       "El",         "EL")]
    [InlineData("Elbil",          "Electric",       "Electric",       "El",         "EL")]
    [InlineData("Ethanol",        "Ethanol",        "Ethanol",        "Etanol",     "ET")]
    [InlineData("E85",            "Ethanol",        "Ethanol",        "Etanol",     "ET")]
    [InlineData("Flexifuel",      "Ethanol",        "Ethanol",        "Etanol",     "ET")]
    [InlineData("Naturgas",       "Natural gas",    "Natural gas",    "Naturgas",   "CNG")]
    [InlineData("CNG",            "Natural gas",    "Natural gas",    "Naturgas",   "CNG")]
    [InlineData("LPG",            "LPG",            "LPG",            "Gasol",      "LPG")]
    [InlineData("Gasol",          "LPG",            "LPG",            "Gasol",      "LPG")]
    [InlineData("Propan",         "LPG",            "LPG",            "Gasol",      "LPG")]
    [InlineData("Hybrid",         "Hybrid",         "Hybrid",         "Elhybrid",   "HEV")]
    [InlineData("Elhybrid",       "Hybrid",         "Hybrid",         "Elhybrid",   "HEV")]
    [InlineData("Bensin/El",      "Hybrid",         "Hybrid",         "Elhybrid",   "HEV")]
    [InlineData("MHEV",           "Mild hybrid",    "Mild hybrid",    "Mildhybrid", "MHEV")]
    [InlineData("Mild hybrid",    "Mild hybrid",    "Mild hybrid",    "Mildhybrid", "MHEV")]
    [InlineData("48V",            "Mild hybrid",    "Mild hybrid",    "Mildhybrid", "MHEV")]
    [InlineData("Plug-in hybrid", "Plug-in hybrid", "Plug-in hybrid", "Laddhybrid", "PHEV")]
    [InlineData("PHEV",           "Plug-in hybrid", "Plug-in hybrid", "Laddhybrid", "PHEV")]
    [InlineData("Laddhybrid",     "Plug-in hybrid", "Plug-in hybrid", "Laddhybrid", "PHEV")]
    [InlineData("Hydrogen",       "Hydrogen",       "Hydrogen",       "Vätgas",     "H2")]
    [InlineData("H2",             "Hydrogen",       "Hydrogen",       "Vätgas",     "H2")]
    [InlineData("FCEV",           "Hydrogen",       "Hydrogen",       "Vätgas",     "H2")]
    [InlineData("Biodiesel",      "Biodiesel",      "Biodiesel",      "Biodiesel",  "BIO")]
    [InlineData("FAME",           "Biodiesel",      "Biodiesel",      "Biodiesel",  "BIO")]
    [InlineData("Methane",        "Methane",        "Methane",        "Metangas",   "MET")]
    [InlineData("Biogas",         "Methane",        "Methane",        "Metangas",   "MET")]
    [InlineData("Methanol",       "Methanol",       "Methanol",       "Metanol",    "M85")]
    [InlineData("M85",            "Methanol",       "Methanol",       "Metanol",    "M85")]
    [InlineData("HVO",            "HVO",            "HVO",            "HVO",        "HVO")]
    [InlineData("HVO100",         "HVO",            "HVO",            "HVO",        "HVO")]
    [InlineData("Kerosene",       "Kerosene",       "Kerosene",       "Fotogen",    "FO")]
    [InlineData("Fotogen",        "Kerosene",       "Kerosene",       "Fotogen",    "FO")]
    [InlineData("Other",          "Other",          "Other",          "Annat",      "ÖVR")]
    [InlineData("Övrigt",         "Other",          "Other",          "Annat",      "ÖVR")]
    public void TryParse_ReturnsExpectedProperties(
        string input, string expectedValue, string expectedEnglish, string expectedSwedish, string expectedCode)
    {
        var ok = FuelType.TryParse(input, out var ft);
        Assert.True(ok);
        Assert.NotNull(ft);
        Assert.Equal(expectedValue, ft.Value);
        Assert.Equal(expectedEnglish, ft.EnglishName);
        Assert.Equal(expectedSwedish, ft.LocalizedName);
        Assert.Equal(expectedCode, ft.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("InvalidFuel")]
    [InlineData("Water")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = FuelType.TryParse(input, out var ft);
        Assert.False(ok);
        Assert.Null(ft);
    }

    [Theory]
    [InlineData("InvalidFuel")]
    [InlineData("")]
    [InlineData("Water")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => FuelType.Parse(input));
    }

    [Theory]
    [InlineData("Petrol", "Petrol", "Bensin")]
    [InlineData("Bensin", "Petrol", "Bensin")]
    [InlineData("BE", "Petrol", "Bensin")]
    [InlineData("Diesel", "Diesel", "Diesel")]
    [InlineData("El", "Electric", "El")]
    [InlineData("MHEV", "Mild hybrid", "Mildhybrid")]
    [InlineData("PHEV", "Plug-in hybrid", "Laddhybrid")]
    [InlineData("H2", "Hydrogen", "Vätgas")]
    public void TryParse_HasExpectedDisplayNames(string input, string expectedEnglish, string expectedSwedish)
    {
        Assert.True(FuelType.TryParse(input, out var ft));
        Assert.NotNull(ft);
        Assert.Equal(expectedEnglish, ft.EnglishName);
        Assert.Equal(expectedSwedish, ft.LocalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Water")]
    public void Format_ReturnsNull_ForInvalidInputs(string? input)
    {
        Assert.Null(FuelType.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", FuelType.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("Petrol", "Petrol")]
    [InlineData("Bensin", "Petrol")]
    [InlineData("BE", "Petrol")]
    [InlineData("95", "Petrol")]
    [InlineData("Diesel", "Diesel")]
    [InlineData("DI", "Diesel")]
    [InlineData("El", "Electric")]
    [InlineData("BEV", "Electric")]
    [InlineData("E85", "Ethanol")]
    [InlineData("Naturgas", "Natural gas")]
    [InlineData("Gasol", "LPG")]
    [InlineData("Elhybrid", "Hybrid")]
    [InlineData("MHEV", "Mild hybrid")]
    [InlineData("Laddhybrid", "Plug-in hybrid")]
    [InlineData("Vätgas", "Hydrogen")]
    [InlineData("FAME", "Biodiesel")]
    [InlineData("Biogas", "Methane")]
    [InlineData("M85", "Methanol")]
    [InlineData("HVO100", "HVO")]
    [InlineData("Fotogen", "Kerosene")]
    [InlineData("Övrigt", "Other")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("Water", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FuelType.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", FuelType.Normalize(" x ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(FuelType.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(FuelType.Normalize("  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("Petrol", true)]
    [InlineData("petrol", false)]
    [InlineData("Bensin", false)]
    [InlineData("Diesel", true)]
    [InlineData("Electric", true)]
    [InlineData("Natural gas", true)]
    [InlineData("natural gas", false)]
    [InlineData("LPG", true)]
    [InlineData("Mild hybrid", true)]
    [InlineData("mild hybrid", false)]
    [InlineData("MHEV", false)]
    [InlineData("Plug-in hybrid", true)]
    [InlineData("plug-in hybrid", false)]
    [InlineData("HVO", true)]
    [InlineData("Other", true)]
    [InlineData(null, false)]
    [InlineData("Water", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, FuelType.IsNormalized(input));
    }

    [Fact]
    public void ToNormalizedString_ReturnsCanonicalValue()
    {
        var ft = FuelType.Parse("Bensin");
        Assert.Equal("Petrol", ft.ToNormalizedString());
    }

    [Fact]
    public void ToString_ReturnsEitherEnglishOrLocalizedName()
    {
        var ft = FuelType.Parse("Bensin");
        var display = ft.ToString();
        Assert.True(display == "Petrol" || display == "Bensin",
            $"Expected 'Petrol' or 'Bensin' but got '{display}'");
    }

    [Fact]
    public void Equality_SameType()
    {
        var a = FuelType.Parse("Petrol");
        var b = FuelType.Parse("Bensin");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.Same(a, b);
    }

    [Fact]
    public void Equality_DifferentTypes()
    {
        var a = FuelType.Parse("Petrol");
        var b = FuelType.Parse("Diesel");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        Assert.True(FuelType.Petrol.CompareTo(FuelType.Diesel) < 0);
        Assert.True(FuelType.Diesel.CompareTo(FuelType.Petrol) > 0);
        Assert.Equal(0, FuelType.Electric.CompareTo(FuelType.Electric));
        Assert.Equal(1, FuelType.Petrol.CompareTo(null));
    }

    [Fact]
    public void ComparisonOperators_Work()
    {
        Assert.True(FuelType.Petrol < FuelType.Diesel);
        Assert.True(FuelType.Diesel > FuelType.Petrol);
        Assert.True(FuelType.Petrol <= FuelType.Diesel);
        Assert.True(FuelType.Diesel >= FuelType.Petrol);

        var samePetrol = FuelType.Parse("BE");
        Assert.True(samePetrol <= FuelType.Petrol);
        Assert.True(samePetrol >= FuelType.Petrol);
    }

    [Fact]
    public void TryParse_HandlesWhitespace()
    {
        Assert.True(FuelType.TryParse("  Diesel  ", out var ft));
        Assert.Same(FuelType.Diesel, ft);
    }

    [Fact]
    public void TryParse_HandlesCaseInsensitively()
    {
        Assert.True(FuelType.TryParse("VÄTGAS", out var ft));
        Assert.Same(FuelType.Hydrogen, ft);

        Assert.True(FuelType.TryParse("fotogen", out var ft2));
        Assert.Same(FuelType.Kerosene, ft2);
    }

    [Fact]
    public void TryParse_HandlesHyphenNormalization()
    {
        Assert.True(FuelType.TryParse("Plug-in hybrid", out var ft));
        Assert.Same(FuelType.PlugInHybrid, ft);

        Assert.True(FuelType.TryParse("plug in hybrid", out var ft2));
        Assert.Same(FuelType.PlugInHybrid, ft2);
    }

    [Theory]
    [InlineData("Petrol",         "Petrol")]
    [InlineData("Diesel",         "Diesel")]
    [InlineData("Electric",       "Electric")]
    [InlineData("Natural gas",    "Natural gas")]
    [InlineData("LPG",            "LPG")]
    [InlineData("Mild hybrid",    "Mild hybrid")]
    [InlineData("Plug-in hybrid", "Plug-in hybrid")]
    [InlineData("HVO",            "HVO")]
    [InlineData("Other",          "Other")]
    public void StaticInstances_HaveCorrectValue(string expectedValue, string parseInput)
    {
        var ft = FuelType.Parse(parseInput);
        Assert.Equal(expectedValue, ft.Value);
    }
}
