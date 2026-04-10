using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// EU energy efficiency class (<c>energimärkning</c>) for appliance labels, including the pre-2021 A+++ scale and the rescaled A–G (2021) context.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://energy.ec.europa.eu/topics/energy-efficiency/energy-label-and-ecodesign/energy-label_en">European Commission — EU energy labels</see></description></item>
/// </list>
/// </remarks>
public sealed class EuEnergyEfficiencyClass : IComparable<EuEnergyEfficiencyClass>, IEquatable<EuEnergyEfficiencyClass>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Energy Efficiency Class", "Energimärkning", "🏷️", ["https://energy.ec.europa.eu/topics/energy-efficiency/energy-label-and-ecodesign/energy-label_en"]);

    private EuEnergyEfficiencyClass(string label, EnergyScale scale, int numericRank)
    {
        Label = label;
        Scale = scale;
        NumericRank = numericRank;
    }

    /// <summary>Display label, e.g. <c>A+++</c>, <c>B</c>, <c>G</c>.</summary>
    public string Label { get; }

    /// <summary>Whether this value is on the pre-2021 A+++ scale or the rescaled A–G (2021) scale.</summary>
    public EnergyScale Scale { get; }

    /// <summary>Ordering rank where lower is better (old scale: A+++=0 … G=9; new scale: A=0 … G=6).</summary>
    public int NumericRank { get; }

    /// <summary>Same as <see cref="Label"/>.</summary>
    public string Value => Label;

    public static readonly EuEnergyEfficiencyClass APlusPlusPlus = new("A+++", EnergyScale.Old, 0);
    public static readonly EuEnergyEfficiencyClass APlusPlus = new("A++", EnergyScale.Old, 1);
    public static readonly EuEnergyEfficiencyClass APlus = new("A+", EnergyScale.Old, 2);
    public static readonly EuEnergyEfficiencyClass A = new("A", EnergyScale.Old, 3);
    public static readonly EuEnergyEfficiencyClass B = new("B", EnergyScale.Old, 4);
    public static readonly EuEnergyEfficiencyClass C = new("C", EnergyScale.Old, 5);
    public static readonly EuEnergyEfficiencyClass D = new("D", EnergyScale.Old, 6);
    public static readonly EuEnergyEfficiencyClass E = new("E", EnergyScale.Old, 7);
    public static readonly EuEnergyEfficiencyClass F = new("F", EnergyScale.Old, 8);
    public static readonly EuEnergyEfficiencyClass G = new("G", EnergyScale.Old, 9);

    /// <summary>Rescaled EU label (2021): <c>A</c> is rank 0, <c>G</c> is rank 6.</summary>
    public static readonly EuEnergyEfficiencyClass NewA = new("A", EnergyScale.New, 0);

    public static readonly EuEnergyEfficiencyClass NewB = new("B", EnergyScale.New, 1);
    public static readonly EuEnergyEfficiencyClass NewC = new("C", EnergyScale.New, 2);
    public static readonly EuEnergyEfficiencyClass NewD = new("D", EnergyScale.New, 3);
    public static readonly EuEnergyEfficiencyClass NewE = new("E", EnergyScale.New, 4);
    public static readonly EuEnergyEfficiencyClass NewF = new("F", EnergyScale.New, 5);
    public static readonly EuEnergyEfficiencyClass NewG = new("G", EnergyScale.New, 6);

    public static IReadOnlyList<EuEnergyEfficiencyClass> All { get; } =
    [
        APlusPlusPlus, APlusPlus, APlus, A, B, C, D, E, F, G
    ];

    public static bool TryParse(string? input, out EuEnergyEfficiencyClass? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var t = InputSanitization.SanitizeInput(input!).Trim();
        if (t.Length is < 1 or > 4) return false;

        var u = t.ToUpperInvariant();
        switch (u)
        {
            case "A+++":
                result = APlusPlusPlus;
                return true;
            case "A++":
                result = APlusPlus;
                return true;
            case "A+":
                result = APlus;
                return true;
            case "A":
                result = A;
                return true;
            case "B":
                result = B;
                return true;
            case "C":
                result = C;
                return true;
            case "D":
                result = D;
                return true;
            case "E":
                result = E;
                return true;
            case "F":
                result = F;
                return true;
            case "G":
                result = G;
                return true;
            default:
                return false;
        }
    }

    public static EuEnergyEfficiencyClass Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid energy efficiency class.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns the canonical label, e.g. <c>A+++</c> or <c>B</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Label;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>Returns the canonical label (same as <see cref="Format"/> without fallback). Returns <see langword="null"/> when invalid.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.Label;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals its normalized label (case-sensitive).</summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns <see cref="Label"/>, e.g. <c>A+++</c>.</summary>
    public string ToNormalizedString() => Label;

    /// <summary>Returns <see cref="Label"/>, e.g. <c>B</c>.</summary>
    public override string ToString() => Label;

    public int CompareTo(EuEnergyEfficiencyClass? other) =>
        other is null ? 1 : NumericRank.CompareTo(other.NumericRank);

    public bool Equals(EuEnergyEfficiencyClass? other) =>
        other is not null && NumericRank == other.NumericRank;

    public override bool Equals(object? obj) => obj is EuEnergyEfficiencyClass other && Equals(other);

    public override int GetHashCode() => NumericRank;

    public static bool operator ==(EuEnergyEfficiencyClass? a, EuEnergyEfficiencyClass? b) =>
        a is null ? b is null : b is not null && a.NumericRank == b.NumericRank;

    public static bool operator !=(EuEnergyEfficiencyClass? a, EuEnergyEfficiencyClass? b) => !(a == b);

    public static bool operator <(EuEnergyEfficiencyClass a, EuEnergyEfficiencyClass b) =>
        a.NumericRank < b.NumericRank;

    public static bool operator >(EuEnergyEfficiencyClass a, EuEnergyEfficiencyClass b) =>
        a.NumericRank > b.NumericRank;

    public static bool operator <=(EuEnergyEfficiencyClass a, EuEnergyEfficiencyClass b) =>
        a.NumericRank <= b.NumericRank;

    public static bool operator >=(EuEnergyEfficiencyClass a, EuEnergyEfficiencyClass b) =>
        a.NumericRank >= b.NumericRank;
}

/// <summary>Which EU label scale a value belongs to.</summary>
public enum EnergyScale
{
    Unknown = 0,
    Old,
    New
}
