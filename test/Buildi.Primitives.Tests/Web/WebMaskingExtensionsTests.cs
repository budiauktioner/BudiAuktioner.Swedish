using Buildi.Primitives.Web;

namespace Buildi.Primitives.Tests.Web;

public class WebMaskingExtensionsTests
{
    [Theory]
    [InlineData("peter@example.com", false, "p***@example.com")]
    [InlineData("peter.orneholm@example.com", false, "p***@example.com")]
    [InlineData("ab@example.com", false, "a*@example.com")]
    [InlineData("a@example.com", false, "a@example.com")]
    [InlineData("peter@example.com", true, "p***@e***.com")]
    [InlineData("user@mail.co.uk", true, "u***@m***.co.uk")]
    public void Email_ToMaskedString_ReturnsExpected(string input, bool maskDomain, string expected)
    {
        var email = EmailAddress.Parse(input);
        Assert.Equal(expected, email.ToMaskedString(maskDomain));
    }

    [Fact]
    public void Email_ToMaskedString_DefaultShowsDomain()
    {
        var email = EmailAddress.Parse("peter@example.com");
        var masked = email.ToMaskedString();
        Assert.Contains("@example.com", masked);
        Assert.DoesNotContain("peter", masked);
    }
}
