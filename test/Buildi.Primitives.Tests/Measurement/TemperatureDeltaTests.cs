using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class TemperatureDeltaTests
{
    [Fact]
    public void Equality_SameValue()
    {
        var a = TemperatureDelta.FromCelsius(5);
        var b = TemperatureDelta.FromKelvin(5);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = TemperatureDelta.FromCelsius(5);
        var b = TemperatureDelta.FromCelsius(10);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = TemperatureDelta.FromCelsius(5);
        Assert.Equal(1, a.CompareTo(null));
    }
}
