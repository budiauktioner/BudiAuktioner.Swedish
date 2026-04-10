using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;
using Buildi.Primitives.Organization;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.TextScanning;

public class TextScanResultTests
{
    private readonly TextScanner _scanner = new();

    [Fact]
    public void MaskAll_ReplacesAllCandidates()
    {
        var text = "Kontakta info@example.com.";
        var result = _scanner.Scan(text);
        var masked = result.MaskAll(text);

        Assert.DoesNotContain("info@example.com", masked);
        Assert.Contains("@", masked);
    }

    [Fact]
    public void RedactAll_ReplacesWithRedacted()
    {
        var text = "Email: info@example.com";
        var result = _scanner.Scan(text);
        var redacted = result.RedactAll(text);

        Assert.Contains("[REDACTED]", redacted);
        Assert.DoesNotContain("info@example.com", redacted);
    }

    [Fact]
    public void RedactAll_CustomReplacement()
    {
        var text = "Email: info@example.com";
        var result = _scanner.Scan(text);
        var redacted = result.RedactAll(text, "***");

        Assert.Contains("***", redacted);
    }

    [Fact]
    public void ReplaceAll_CustomDelegate()
    {
        var text = "Email: info@example.com";
        var result = _scanner.Scan(text);
        var replaced = result.ReplaceAll(text, c => $"[{c.TypeName}]");

        Assert.Contains("[EmailAddress]", replaced);
    }

    [Fact]
    public void MaskAll_MultipleCandidates_AllMasked()
    {
        var text = "Ring 070-174 06 33 eller maila info@example.com";
        var result = _scanner.Scan(text);
        var masked = result.MaskAll(text);

        Assert.DoesNotContain("info@example.com", masked);
    }

    [Fact]
    public void OverlapResolution_ContainedCandidateLoses()
    {
        // "5592460421@example.com" — the local part is an org number
        var text = "kontakt: 5592460421@example.com";
        var result = _scanner.Scan(text);

        var resolved = result.ResolvedCandidates;
        var emailInResolved = resolved.Any(c => c.TypeName == nameof(EmailAddress));
        Assert.True(emailInResolved, "Email should win over contained org number");

        var orgInResolved = resolved
            .Where(c => c.TypeName == nameof(SwedishOrganizationNumber))
            .Any(c =>
            {
                var email = resolved.FirstOrDefault(e => e.TypeName == nameof(EmailAddress));
                return email != null && email.Contains(c);
            });
        Assert.False(orgInResolved, "Org number contained within email should be removed from resolved");
    }

    [Fact]
    public void CountByCategory_ReturnsCorrectCounts()
    {
        var text = "Email: info@example.com Org: 559246-0421";
        var result = _scanner.Scan(text);

        Assert.True(result.CountByCategory(TextCandidateCategory.Contact) >= 1);
        Assert.True(result.CountByCategory(TextCandidateCategory.OrganizationIdentifier) >= 1);
    }

    [Fact]
    public void CountByConfidence_ReturnsCorrectCounts()
    {
        var text = "Email: info@example.com Org: 559246-0421";
        var result = _scanner.Scan(text);

        Assert.True(result.CountByConfidence(TextMatchConfidence.High) >= 1);
    }

    [Fact]
    public void All_IsSortedByStartIndex()
    {
        var text = "559246-0421 info@example.com";
        var result = _scanner.Scan(text);

        for (var i = 1; i < result.All.Count; i++)
            Assert.True(result.All[i].StartIndex >= result.All[i - 1].StartIndex);
    }

    [Fact]
    public void ResolvedCandidates_HasNoOverlaps()
    {
        var text = "559246-0421 info@example.com";
        var result = _scanner.Scan(text);

        for (var i = 0; i < result.ResolvedCandidates.Count; i++)
        for (var j = i + 1; j < result.ResolvedCandidates.Count; j++)
            Assert.False(result.ResolvedCandidates[i].Overlaps(result.ResolvedCandidates[j]),
                $"Overlap between {result.ResolvedCandidates[i].TypeName}@{result.ResolvedCandidates[i].StartIndex} and {result.ResolvedCandidates[j].TypeName}@{result.ResolvedCandidates[j].StartIndex}");
    }
}
