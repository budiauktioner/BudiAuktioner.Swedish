using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class LeiCodeTests
{
    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        var b = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        var b = LeiCode.Parse("549300ONBUTV20237K19");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = LeiCode.Parse("549300ONBUTV20237K19");
        var b = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = LeiCode.Parse("549300T5RZ1HA5HZ3109");
        Assert.Equal(1, a.CompareTo(null));
    }
}
