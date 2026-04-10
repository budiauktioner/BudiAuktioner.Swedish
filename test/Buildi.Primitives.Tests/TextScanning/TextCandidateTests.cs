using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.TextScanning;

public class TextCandidateTests
{
    private static TextCandidate<string> MakeCandidate(int start, int length, TextMatchConfidence confidence = TextMatchConfidence.High)
        => new(start, length, "x", "Test", TextCandidateCategory.Contact, "n", "f", "m", confidence, "v");

    [Fact]
    public void EndIndex_IsStartPlusLength()
    {
        var c = MakeCandidate(5, 10);
        Assert.Equal(15, c.EndIndex);
    }

    [Fact]
    public void Overlaps_ReturnsTrueForOverlapping()
    {
        var a = MakeCandidate(0, 10);
        var b = MakeCandidate(5, 10);
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_ReturnsFalseForAdjacent()
    {
        var a = MakeCandidate(0, 5);
        var b = MakeCandidate(5, 5);
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Contains_ReturnsTrueWhenFullyContained()
    {
        var outer = MakeCandidate(0, 20);
        var inner = MakeCandidate(5, 5);
        Assert.True(outer.Contains(inner));
        Assert.False(inner.Contains(outer));
    }

    [Fact]
    public void Contains_ReturnsTrueForExactSameSpan()
    {
        var a = MakeCandidate(3, 7);
        var b = MakeCandidate(3, 7);
        Assert.True(a.Contains(b));
        Assert.True(b.Contains(a));
    }

    [Fact]
    public void Properties_AreCorrectlySet()
    {
        var c = new TextCandidate<string>(
            10, 5, "hello", "TestType", TextCandidateCategory.Financial,
            "norm", "fmt", "m***", TextMatchConfidence.Medium, "val");

        Assert.Equal(10, c.StartIndex);
        Assert.Equal(5, c.Length);
        Assert.Equal(15, c.EndIndex);
        Assert.Equal("hello", c.OriginalText);
        Assert.Equal("TestType", c.TypeName);
        Assert.Equal(TextCandidateCategory.Financial, c.Category);
        Assert.Equal("norm", c.NormalizedForm);
        Assert.Equal("fmt", c.FormattedForm);
        Assert.Equal("m***", c.MaskedForm);
        Assert.Equal(TextMatchConfidence.Medium, c.Confidence);
        Assert.Equal("val", c.Value);
    }
}
