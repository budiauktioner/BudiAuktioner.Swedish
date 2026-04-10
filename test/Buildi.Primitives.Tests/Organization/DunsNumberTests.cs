using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class DunsNumberTests
{
    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = DunsNumber.Parse("350827673");
        var b = DunsNumber.Parse("350827673");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = DunsNumber.Parse("350827673");
        var b = DunsNumber.Parse("123456789");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = DunsNumber.Parse("350827673");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = DunsNumber.Parse("123456789");
        var b = DunsNumber.Parse("350827673");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = DunsNumber.Parse("350827673");
        Assert.Equal(1, a.CompareTo(null));
    }
}
