using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class EuEnergyEfficiencyClassTests
{
    public static TheoryData<string, string, EnergyScale, int> ValidParseCases => new()
    {
        { "A+++", "A+++", EnergyScale.Old, 0 },
        { "a+++", "A+++", EnergyScale.Old, 0 },
        { "A++", "A++", EnergyScale.Old, 1 },
        { "a++", "A++", EnergyScale.Old, 1 },
        { "A+", "A+", EnergyScale.Old, 2 },
        { "a+", "A+", EnergyScale.Old, 2 },
        { "A", "A", EnergyScale.Old, 3 },
        { "a", "A", EnergyScale.Old, 3 },
        { "B", "B", EnergyScale.Old, 4 },
        { "b", "B", EnergyScale.Old, 4 },
        { "C", "C", EnergyScale.Old, 5 },
        { "D", "D", EnergyScale.Old, 6 },
        { "E", "E", EnergyScale.Old, 7 },
        { "F", "F", EnergyScale.Old, 8 },
        { "G", "G", EnergyScale.Old, 9 },
    };

    /// <summary>Input and canonical label only (for theories that do not need scale/rank).</summary>
    public static TheoryData<string, string> ParseInputAndLabelCases => new()
    {
        { "A+++", "A+++" },
        { "a+++", "A+++" },
        { "A++", "A++" },
        { "a++", "A++" },
        { "A+", "A+" },
        { "a+", "A+" },
        { "A", "A" },
        { "a", "A" },
        { "B", "B" },
        { "b", "B" },
        { "C", "C" },
        { "D", "D" },
        { "E", "E" },
        { "F", "F" },
        { "G", "G" },
    };

    [Theory]
    [MemberData(nameof(ValidParseCases))]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, string expectedLabel, EnergyScale expectedScale, int expectedRank)
    {
        var ok = EuEnergyEfficiencyClass.TryParse(input, out var r);

        Assert.True(ok);
        Assert.NotNull(r);
        Assert.Same(GetStaticOld(expectedLabel), r);
        Assert.Equal(expectedLabel, r.Label);
        Assert.Equal(expectedScale, r.Scale);
        Assert.Equal(expectedRank, r.NumericRank);
        Assert.Equal(expectedLabel, r.Value);
    }

    private static EuEnergyEfficiencyClass GetStaticOld(string label) =>
        label switch
        {
            "A+++" => EuEnergyEfficiencyClass.APlusPlusPlus,
            "A++" => EuEnergyEfficiencyClass.APlusPlus,
            "A+" => EuEnergyEfficiencyClass.APlus,
            "A" => EuEnergyEfficiencyClass.A,
            "B" => EuEnergyEfficiencyClass.B,
            "C" => EuEnergyEfficiencyClass.C,
            "D" => EuEnergyEfficiencyClass.D,
            "E" => EuEnergyEfficiencyClass.E,
            "F" => EuEnergyEfficiencyClass.F,
            "G" => EuEnergyEfficiencyClass.G,
            _ => throw new ArgumentOutOfRangeException(nameof(label))
        };

    [Fact]
    public void All_ContainsOldScaleInOrder()
    {
        Assert.Equal(10, EuEnergyEfficiencyClass.All.Count);
        Assert.Same(EuEnergyEfficiencyClass.APlusPlusPlus, EuEnergyEfficiencyClass.All[0]);
        Assert.Same(EuEnergyEfficiencyClass.G, EuEnergyEfficiencyClass.All[^1]);
    }

    [Fact]
    public void NewScale_StaticInstances_HaveExpectedRanks()
    {
        Assert.Equal(0, EuEnergyEfficiencyClass.NewA.NumericRank);
        Assert.Equal(6, EuEnergyEfficiencyClass.NewG.NumericRank);
        Assert.Equal(EnergyScale.New, EuEnergyEfficiencyClass.NewB.Scale);
        Assert.Equal("B", EuEnergyEfficiencyClass.NewB.Label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("H")]
    [InlineData("A++++")]
    [InlineData("AB")]
    [InlineData("B+")]
    [InlineData("++")]
    [InlineData("Z")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EuEnergyEfficiencyClass.IsValid(input));
    }

    [Theory]
    [InlineData("A+++")]
    [InlineData("g")]
    [InlineData("A+")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EuEnergyEfficiencyClass.IsValid(input));
    }

    [Theory]
    [MemberData(nameof(ParseInputAndLabelCases))]
    public void TryParse_TrimsLeadingTrailingWhitespace(string input, string expectedLabel)
    {
        var padded = "  " + input + "  ";
        var ok = EuEnergyEfficiencyClass.TryParse(padded, out var r);
        Assert.True(ok);
        Assert.Equal(expectedLabel, r!.Label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("H")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(EuEnergyEfficiencyClass.TryParse(input, out var r));
        Assert.Null(r);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("A++++")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EuEnergyEfficiencyClass.Parse(input));
    }

    [Theory]
    [MemberData(nameof(ParseInputAndLabelCases))]
    public void Format_ReturnsLabel(string input, string expectedLabel)
    {
        Assert.Equal(expectedLabel, EuEnergyEfficiencyClass.Format(input));
    }

    [Theory]
    [MemberData(nameof(ParseInputAndLabelCases))]
    public void Normalize_ReturnsLabel(string input, string expectedLabel)
    {
        Assert.Equal(expectedLabel, EuEnergyEfficiencyClass.Normalize(input));
    }

    [Theory]
    [InlineData("A+++", true)]
    [InlineData("a+++", false)]
    [InlineData("B", true)]
    [InlineData(" b ", false)]
    public void IsNormalized_Expected(string input, bool expected)
    {
        Assert.Equal(expected, EuEnergyEfficiencyClass.IsNormalized(input));
    }

    [Fact]
    public void Format_FallbackWhenInvalid()
    {
        Assert.Null(EuEnergyEfficiencyClass.Format("nope"));
        Assert.Equal("nope", EuEnergyEfficiencyClass.Format("nope", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [MemberData(nameof(ParseInputAndLabelCases))]
    public void ToString_And_ToNormalizedString_ReturnLabel(string input, string expectedLabel)
    {
        var c = EuEnergyEfficiencyClass.Parse(input);
        Assert.Equal(expectedLabel, c.ToString());
        Assert.Equal(expectedLabel, c.ToNormalizedString());
    }

    [Fact]
    public void CompareTo_LowerNumericRank_ComesFirst()
    {
        Assert.True(EuEnergyEfficiencyClass.APlusPlusPlus.CompareTo(EuEnergyEfficiencyClass.A) < 0);
        Assert.True(EuEnergyEfficiencyClass.A.CompareTo(EuEnergyEfficiencyClass.B) < 0);
        Assert.True(EuEnergyEfficiencyClass.B.CompareTo(EuEnergyEfficiencyClass.A) > 0);
        Assert.Equal(0, EuEnergyEfficiencyClass.A.CompareTo(EuEnergyEfficiencyClass.A));
    }

    [Fact]
    public void CompareTo_Null_IsGreater()
    {
        Assert.True(EuEnergyEfficiencyClass.A.CompareTo(null) > 0);
    }

    [Fact]
    public void Operators_OrderByNumericRank()
    {
        Assert.True(EuEnergyEfficiencyClass.APlusPlusPlus < EuEnergyEfficiencyClass.A);
        Assert.True(EuEnergyEfficiencyClass.B > EuEnergyEfficiencyClass.A);
        Assert.True(EuEnergyEfficiencyClass.A <= EuEnergyEfficiencyClass.Parse("A"));
        Assert.True(EuEnergyEfficiencyClass.G >= EuEnergyEfficiencyClass.F);
    }

    [Fact]
    public void Equality_ByNumericRank_Only()
    {
        Assert.True(EuEnergyEfficiencyClass.APlusPlusPlus == EuEnergyEfficiencyClass.NewA);
        Assert.False(EuEnergyEfficiencyClass.A == EuEnergyEfficiencyClass.NewA);
        Assert.True(EuEnergyEfficiencyClass.B != EuEnergyEfficiencyClass.A);
    }

    [Fact]
    public void Equals_And_GetHashCode_UseNumericRank()
    {
        Assert.True(EuEnergyEfficiencyClass.APlusPlusPlus.Equals(EuEnergyEfficiencyClass.NewA));
        Assert.Equal(EuEnergyEfficiencyClass.APlusPlusPlus.GetHashCode(), EuEnergyEfficiencyClass.NewA.GetHashCode());
    }
}
