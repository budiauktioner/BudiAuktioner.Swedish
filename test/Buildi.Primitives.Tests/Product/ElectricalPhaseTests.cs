using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ElectricalPhaseTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(3, ElectricalPhase.All.Count);
    }

    [Theory]
    [InlineData("1-phase")]
    [InlineData("2-phase")]
    [InlineData("3-phase")]
    [InlineData("Enfas")]
    [InlineData("Tvåfas")]
    [InlineData("Trefas")]
    [InlineData("Single-phase")]
    [InlineData("Two-phase")]
    [InlineData("Three-phase")]
    [InlineData("1-fas")]
    [InlineData("1fas")]
    [InlineData("1 fas")]
    [InlineData("3-fas")]
    [InlineData("3fas")]
    [InlineData("3 fas")]
    [InlineData("1P")]
    [InlineData("3P")]
    [InlineData("1F")]
    [InlineData("3F")]
    [InlineData("230V 1-fas")]
    [InlineData("400V 3-fas")]
    [InlineData("L1")]
    [InlineData("L1L2L3")]
    [InlineData("1~")]
    [InlineData("3~")]
    [InlineData("Single phase")]
    [InlineData("Three phase")]
    [InlineData("  3-phase  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ElectricalPhase.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("4-phase")]
    [InlineData("0-phase")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ElectricalPhase.IsValid(input));
    }

    [Theory]
    [InlineData("1-phase", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("Enfas", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1-fas", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1fas", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1 fas", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1P", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1F", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("230V 1-fas", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("L1", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("1~", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("Single phase", "1-phase", "Single-phase", "Enfas", 1)]
    [InlineData("2-phase", "2-phase", "Two-phase", "Tvåfas", 2)]
    [InlineData("Tvåfas", "2-phase", "Two-phase", "Tvåfas", 2)]
    [InlineData("2-fas", "2-phase", "Two-phase", "Tvåfas", 2)]
    [InlineData("2P", "2-phase", "Two-phase", "Tvåfas", 2)]
    [InlineData("2~", "2-phase", "Two-phase", "Tvåfas", 2)]
    [InlineData("3-phase", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("Trefas", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("3-fas", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("3fas", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("3P", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("3F", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("400V 3-fas", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("L1L2L3", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("3~", "3-phase", "Three-phase", "Trefas", 3)]
    [InlineData("Three phase", "3-phase", "Three-phase", "Trefas", 3)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedEnglish, string expectedSwedish, int expectedPhaseCount)
    {
        var ok = ElectricalPhase.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedEnglish, result.EnglishName);
        Assert.Equal(expectedSwedish, result.LocalizedName);
        Assert.Equal(expectedPhaseCount, result.PhaseCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("4-phase")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = ElectricalPhase.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("4-phase")]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ElectricalPhase.Parse(input));
    }

    [Theory]
    [InlineData("1-phase", "1-phase")]
    [InlineData("Enfas", "1-phase")]
    [InlineData("3-fas", "3-phase")]
    [InlineData("L1L2L3", "3-phase")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElectricalPhase.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", ElectricalPhase.Normalize(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(ElectricalPhase.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(ElectricalPhase.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", ElectricalPhase.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_ReturnsNull_ForInvalidInput()
    {
        Assert.Null(ElectricalPhase.Format(null));
        Assert.Null(ElectricalPhase.Format(""));
        Assert.Null(ElectricalPhase.Format("nope"));
    }

    [Theory]
    [InlineData("1-phase", true)]
    [InlineData("2-phase", true)]
    [InlineData("3-phase", true)]
    [InlineData("Enfas", false)]
    [InlineData("1-fas", false)]
    [InlineData("nope", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, ElectricalPhase.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        var single = ElectricalPhase.Parse("1-phase");
        var display = single.ToString();
        Assert.True(display == "Single-phase" || display == "Enfas");
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var three = ElectricalPhase.Parse("Trefas");
        Assert.Equal("3-phase", three.ToNormalizedString());
    }

    [Fact]
    public void Equality_SamePhase()
    {
        var a = ElectricalPhase.Parse("1-phase");
        var b = ElectricalPhase.Parse("Enfas");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentPhases()
    {
        var a = ElectricalPhase.Parse("1-phase");
        var b = ElectricalPhase.Parse("3-phase");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_Null()
    {
        var a = ElectricalPhase.Parse("1-phase");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersByPhaseCount()
    {
        var single = ElectricalPhase.Parse("1-phase");
        var three = ElectricalPhase.Parse("3-phase");
        Assert.True(single < three);
        Assert.True(three > single);
        Assert.True(single.CompareTo(three) < 0);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ElectricalPhase.Parse("1-phase");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Fact]
    public void Parse_ReturnsSameInstance()
    {
        var a = ElectricalPhase.Parse("1-phase");
        Assert.Same(ElectricalPhase.SinglePhase, a);

        var b = ElectricalPhase.Parse("3-fas");
        Assert.Same(ElectricalPhase.ThreePhase, b);
    }
}
