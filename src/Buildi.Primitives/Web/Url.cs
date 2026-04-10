using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Web;

/// <summary>
/// A URL or URI (<c>webbadress</c>). Accepts any valid URI scheme — <c>http</c>, <c>https</c>,
/// <c>mailto</c>, <c>tel</c>, <c>ssh</c>, custom schemes, etc. Parsing delegates to
/// <see cref="System.Uri"/> for validation and normalization (lowercase scheme/host, default-port
/// removal). For hierarchical URIs (those with <c>://</c>), exposes the host, top-level domain,
/// and the country associated with a country-code TLD. Bare domains like <c>example.com</c> are
/// auto-prefixed with <c>https://</c>. Reserved example/test domains per RFC 2606 / RFC 6761 are
/// flagged via <see cref="IsExampleDomain"/> but are still considered valid.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://datatracker.ietf.org/doc/html/rfc3986">RFC 3986</see> — Uniform Resource Identifier (URI): Generic Syntax</description></item>
/// <item><description><see href="https://datatracker.ietf.org/doc/html/rfc2606">RFC 2606</see> — Reserved Top Level DNS Names</description></item>
/// <item><description><see href="https://datatracker.ietf.org/doc/html/rfc6761">RFC 6761</see> — Special-Use Domain Names</description></item>
/// </list>
/// </remarks>
public sealed class Url : IEquatable<Url>, IComparable<Url>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("URL", "Webbadress", "🔗", ["https://datatracker.ietf.org/doc/html/rfc3986", "https://datatracker.ietf.org/doc/html/rfc2606", "https://datatracker.ietf.org/doc/html/rfc6761"]);

    private const int MaxInputLength = 2048;

    private static readonly HashSet<string> ReservedTlds = new(StringComparer.OrdinalIgnoreCase)
    {
        "test", "example", "invalid", "localhost"
    };

    private static readonly HashSet<string> ReservedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "example.com", "example.org", "example.net"
    };

    private static readonly Lazy<Dictionary<string, Country>> TldCountryMap = new(() =>
    {
        var map = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);
        foreach (var country in Country.All)
        {
            if (!string.IsNullOrEmpty(country.TopLevelDomain))
                map[country.TopLevelDomain] = country;
        }
        return map;
    });

    /// <summary>The normalized URL string, e.g. <c>https://www.example.com/path</c>.</summary>
    public string Value { get; }

    /// <summary>The URI scheme without <c>://</c>, e.g. <c>https</c>.</summary>
    public string Scheme { get; }

    /// <summary>The full hostname, e.g. <c>www.example.com</c>. Empty for non-hierarchical URIs like <c>mailto:</c> or <c>tel:</c>.</summary>
    public string Host { get; }

    /// <summary>The explicitly specified port, or <see langword="null"/> when the default port for the scheme is used.</summary>
    public int? Port { get; }

    /// <summary>The path component, e.g. <c>/path/to/page</c>. Root paths are <c>/</c>.</summary>
    public string Path { get; }

    /// <summary>The query string without the leading <c>?</c>, or <see langword="null"/> when absent.</summary>
    public string? Query { get; }

    /// <summary>The fragment without the leading <c>#</c>, or <see langword="null"/> when absent.</summary>
    public string? Fragment { get; }

    /// <summary>The top-level domain without the dot, e.g. <c>com</c>, <c>se</c>.</summary>
    public string TopLevelDomain { get; }

    /// <summary>
    /// The country associated with the TLD when it is a country-code TLD (e.g. <c>.se</c> → Sweden),
    /// or <see langword="null"/> for generic TLDs like <c>.com</c> or <c>.org</c>.
    /// </summary>
    public Country? Country { get; }

    /// <summary><see langword="true"/> when the scheme is <c>https</c>.</summary>
    public bool IsSecure { get; }

    /// <summary>
    /// <see langword="true"/> when the host is a reserved example or test domain per
    /// RFC 2606 / RFC 6761 (e.g. <c>example.com</c>, <c>.test</c>, <c>.invalid</c>).
    /// </summary>
    public bool IsExampleDomain { get; }

    /// <summary><see langword="true"/> when the host is an IP address rather than a domain name.</summary>
    public bool IsIpAddress { get; }

    private readonly Uri _uri;

    private Url(string value, Uri uri, string tld, Country? country, bool isExample, bool isIp)
    {
        _uri = uri;
        Value = value;
        Scheme = uri.Scheme;
        Host = uri.Host;
        Port = uri.Port <= 0 || uri.IsDefaultPort ? null : uri.Port;
        Path = uri.AbsolutePath;
        Query = string.IsNullOrEmpty(uri.Query) ? null : uri.Query[1..];
        Fragment = string.IsNullOrEmpty(uri.Fragment) ? null : uri.Fragment[1..];
        TopLevelDomain = tld;
        Country = country;
        IsSecure = string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        IsExampleDomain = isExample;
        IsIpAddress = isIp;
    }

    /// <summary>Returns the URL as a <see cref="System.Uri"/> instance.</summary>
    public Uri ToUri() => _uri;

    /// <summary>Creates a <see cref="Url"/> from a <see cref="System.Uri"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when the URI cannot be represented as a <see cref="Url"/>.</exception>
    public static Url FromUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!TryParse(uri.AbsoluteUri, out var result))
            throw new ArgumentException("Invalid URL.", nameof(uri));
        return result!;
    }

    public static implicit operator Uri(Url url) => url.ToUri();
    public static implicit operator Url(Uri uri) => FromUri(uri);

    public static bool TryParse(string? input, out Url? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme.Contains('.'))
        {
            if (!trimmed.Contains('.')) return false;
            if (!Uri.TryCreate("https://" + trimmed, UriKind.Absolute, out uri))
                return false;
        }

        var host = uri.Host;
        var isIp = !string.IsNullOrEmpty(host) &&
                   uri.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6;

        var tld = string.Empty;
        if (!isIp && !string.IsNullOrEmpty(host))
        {
            var lastDot = host.LastIndexOf('.');
            tld = lastDot >= 0 ? host[(lastDot + 1)..] : string.Empty;
        }

        Country? country = null;
        if (!isIp && tld.Length > 0)
            TldCountryMap.Value.TryGetValue("." + tld, out country);

        var isExample = !isIp && !string.IsNullOrEmpty(host) && (
            ReservedTlds.Contains(tld) ||
            ReservedDomains.Contains(host) ||
            host.EndsWith(".example.com", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".example.org", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".example.net", StringComparison.OrdinalIgnoreCase));

        var normalized = uri.AbsoluteUri;

        result = new Url(normalized, uri, tld, country, isExample, isIp);
        return true;
    }

    public static Url Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid URL.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the normalized URL, for example <c>https://www.example.com/path</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized URL, for example <c>https://www.example.com/path</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized URL, for example <c>https://www.example.com/path</c>.
    /// </summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the normalized URL, for example <c>https://www.example.com/path</c>.
    /// </summary>
    public override string ToString() => Value;

    private static readonly Regex ScanPattern = new(
        @"(?:[a-zA-Z][\w+.-]*://[^\s<>""]+|(?:mailto|tel|sms|callto):[^\s<>""]+|\bwww\.[^\s<>""]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for potential URLs — any <c>scheme://</c> pattern, common
    /// non-hierarchical schemes (<c>mailto:</c>, <c>tel:</c>, <c>sms:</c>, <c>callto:</c>),
    /// and <c>www.</c>-prefixed domains.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real URL in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Url>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Url>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            var candidate = TrimTrailingPunctuation(match.Value);
            if (!TryParse(candidate, out var url)) continue;

            var startIndex = match.Index;
            results.Add(new TextCandidate<Url>(
                startIndex,
                candidate.Length,
                candidate,
                nameof(Url),
                TextCandidateCategory.Contact,
                url!.ToNormalizedString(),
                url.ToString(),
                url.ToMaskedString(),
                TextMatchConfidence.High,
                url));
        }
        return results;
    }

    private static string TrimTrailingPunctuation(string value)
    {
        var end = value.Length;
        while (end > 0)
        {
            var ch = value[end - 1];
            if (ch is '.' or ',' or ';' or '!' or '?' or ')' or ']' or '>' or '\'' or '"')
            {
                if (ch == ')' && value.Contains('(')) break;
                if (ch == ']' && value.Contains('[')) break;
                end--;
            }
            else break;
        }
        return end < value.Length ? value[..end] : value;
    }

    public bool Equals(Url? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Url other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Url? a, Url? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Url? a, Url? b) => !(a == b);
    public int CompareTo(Url? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Url left, Url right) => left.CompareTo(right) < 0;
    public static bool operator >(Url left, Url right) => left.CompareTo(right) > 0;
    public static bool operator <=(Url left, Url right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Url left, Url right) => left.CompareTo(right) >= 0;
}
