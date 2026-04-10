namespace Buildi.Primitives.Web;

/// <summary>
/// Extension methods for masking sensitive web-related information in display strings.
/// </summary>
public static class WebMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked email showing the first character of the local part and the full domain,
    /// e.g. <c>peter.orneholm@example.com</c> → <c>p***@example.com</c>.
    /// Set <paramref name="maskDomain"/> to <see langword="true"/> to also mask the domain name:
    /// <c>p***@e***.com</c>.
    /// </summary>
    public static string ToMaskedString(this EmailAddress email, bool maskDomain = false)
    {
        var localPart = email.LocalPart;
        var maskedLocal = localPart.Length <= 1
            ? localPart
            : $"{localPart[0]}{new string(MaskChar, Math.Min(localPart.Length - 1, 3))}";

        if (maskDomain)
        {
            var domain = email.Domain;
            var firstDot = domain.IndexOf('.');
            if (firstDot > 0)
            {
                var domainLabel = domain[..firstDot];
                var suffix = domain[firstDot..];
                var maskedDomain = domainLabel.Length <= 1
                    ? domainLabel
                    : $"{domainLabel[0]}{new string(MaskChar, Math.Min(domainLabel.Length - 1, 3))}";
                return $"{maskedLocal}@{maskedDomain}{suffix}";
            }
        }

        return $"{maskedLocal}@{email.Domain}";
    }

    /// <summary>
    /// Returns a masked URL showing the scheme and host but obscuring the path, query, and fragment,
    /// e.g. <c>https://www.example.com/my/secret/page?key=val</c> → <c>https://www.example.com/***</c>.
    /// For non-hierarchical URIs (e.g. <c>mailto:</c>, <c>tel:</c>), returns <c>scheme:***</c>.
    /// When the URL has only a root path, it is returned unchanged.
    /// </summary>
    public static string ToMaskedString(this Url url)
    {
        var schemePrefix = url.Scheme + "://";
        if (!url.Value.StartsWith(schemePrefix, StringComparison.OrdinalIgnoreCase))
            return $"{url.Scheme}:{new string(MaskChar, 3)}";

        var sb = new System.Text.StringBuilder();
        sb.Append(url.Scheme);
        sb.Append("://");
        sb.Append(url.Host);

        if (url.Port.HasValue)
        {
            sb.Append(':');
            sb.Append(url.Port.Value);
        }

        if (url.Path is "/" or "")
        {
            sb.Append('/');
        }
        else
        {
            sb.Append('/');
            sb.Append(new string(MaskChar, 3));
        }

        if (url.Query != null)
        {
            sb.Append('?');
            sb.Append(new string(MaskChar, 3));
        }

        if (url.Fragment != null)
        {
            sb.Append('#');
            sb.Append(new string(MaskChar, 3));
        }

        return sb.ToString();
    }
}
