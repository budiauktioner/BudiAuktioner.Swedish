using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Geography;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Web;

/// <summary>
/// Identifies the email service behind a domain when it belongs to a well-known
/// public (consumer/free) email provider.
/// </summary>
public enum PublicEmailProvider
{
    Unknown = 0,

    // International
    Gmail,
    Outlook,
    Yahoo,
    ICloud,
    ProtonMail,
    Aol,
    Gmx,
    MailCom,
    Yandex,
    Zoho,
    Tuta,
    Hey,
    DuckDuckGoEmail,
    SimpleLogin,

    // Regional (Europe)
    WebDe,
    MailRu,
    WirtualnaPolska,
    Onet,
    Interia,
    Tlen,
    Seznam,
    Libero,
    FreemailHu,
    Abv,
    InboxLv,
    UkrNet,
    Naver,

    // Swedish ISP / portal email
    Telia,
    Tele2,
    Comhem,
    Bredbandsbolaget,
    Bredband2,
    Ownit,
    Spray,
    Passagen,
    Bahnhof,
    Glocalnet,

    // Nordic ISP
    OnlineNo,
    Kolumbus,
    Aland,
}

/// <summary>
/// An email address (<c>e-postadress</c>). Normalization trims whitespace and lowercases the entire
/// address — including the local part — as a pragmatic interoperability and deduplication choice.
/// Note: RFC 5321 §2.3.11 states that the local part is formally case-sensitive, but in practice
/// virtually all mail providers treat it as case-insensitive, and lowercasing is the widely adopted
/// convention for storage, comparison, and deduplication.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://datatracker.ietf.org/doc/html/rfc5321#section-2.3.11">RFC 5321 §2.3.11</see> — Mailbox and Address (local-part case sensitivity)</description></item>
/// <item><description><see href="https://datatracker.ietf.org/doc/html/rfc5321">RFC 5321</see> — Simple Mail Transfer Protocol</description></item>
/// </list>
/// </remarks>
public sealed class EmailAddress : IEquatable<EmailAddress>, IComparable<EmailAddress>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Email Address", "E-postadress", "📧", ["https://datatracker.ietf.org/doc/html/rfc5321#section-2.3.11", "https://datatracker.ietf.org/doc/html/rfc5321"]);

    private const int MaxInputLength = 320;

    private static readonly Regex EmailPattern = new(
        @"^(?!\.)[a-zA-Z0-9_](?!.*\.\.)(?:[a-zA-Z0-9._%+-]*[a-zA-Z0-9_%+-])?@(?!-)[a-zA-Z0-9-]+(?<!-)(\.(?!-)[a-zA-Z0-9-]+(?<!-))*\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(200));

    private static readonly IdnMapping IdnMapper = new();

    private static readonly Dictionary<string, PublicEmailProvider> ProviderDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        // International
        ["gmail.com"] = PublicEmailProvider.Gmail,
        ["googlemail.com"] = PublicEmailProvider.Gmail,

        ["outlook.com"] = PublicEmailProvider.Outlook,
        ["outlook.se"] = PublicEmailProvider.Outlook,
        ["outlook.fr"] = PublicEmailProvider.Outlook,
        ["outlook.co"] = PublicEmailProvider.Outlook,
        ["hotmail.com"] = PublicEmailProvider.Outlook,
        ["hotmail.se"] = PublicEmailProvider.Outlook,
        ["hotmail.co.uk"] = PublicEmailProvider.Outlook,
        ["hotmail.fr"] = PublicEmailProvider.Outlook,
        ["hotmail.de"] = PublicEmailProvider.Outlook,
        ["hotmail.no"] = PublicEmailProvider.Outlook,
        ["hotmail.dk"] = PublicEmailProvider.Outlook,
        ["hotmail.fi"] = PublicEmailProvider.Outlook,
        ["hotmail.it"] = PublicEmailProvider.Outlook,
        ["hotmail.es"] = PublicEmailProvider.Outlook,
        ["live.com"] = PublicEmailProvider.Outlook,
        ["live.se"] = PublicEmailProvider.Outlook,
        ["live.fr"] = PublicEmailProvider.Outlook,
        ["live.dk"] = PublicEmailProvider.Outlook,
        ["live.co.uk"] = PublicEmailProvider.Outlook,
        ["live.it"] = PublicEmailProvider.Outlook,
        ["live.no"] = PublicEmailProvider.Outlook,
        ["msn.com"] = PublicEmailProvider.Outlook,
        ["windowslive.com"] = PublicEmailProvider.Outlook,

        ["yahoo.com"] = PublicEmailProvider.Yahoo,
        ["yahoo.se"] = PublicEmailProvider.Yahoo,
        ["yahoo.co.uk"] = PublicEmailProvider.Yahoo,
        ["yahoo.fr"] = PublicEmailProvider.Yahoo,
        ["yahoo.de"] = PublicEmailProvider.Yahoo,
        ["yahoo.it"] = PublicEmailProvider.Yahoo,
        ["yahoo.es"] = PublicEmailProvider.Yahoo,
        ["yahoo.gr"] = PublicEmailProvider.Yahoo,
        ["yahoo.ro"] = PublicEmailProvider.Yahoo,
        ["yahoo.co"] = PublicEmailProvider.Yahoo,
        ["yahoo.no"] = PublicEmailProvider.Yahoo,
        ["ymail.com"] = PublicEmailProvider.Yahoo,
        ["rocketmail.com"] = PublicEmailProvider.Yahoo,

        ["icloud.com"] = PublicEmailProvider.ICloud,
        ["me.com"] = PublicEmailProvider.ICloud,
        ["mac.com"] = PublicEmailProvider.ICloud,

        ["protonmail.com"] = PublicEmailProvider.ProtonMail,
        ["protonmail.ch"] = PublicEmailProvider.ProtonMail,
        ["proton.me"] = PublicEmailProvider.ProtonMail,
        ["pm.me"] = PublicEmailProvider.ProtonMail,

        ["aol.com"] = PublicEmailProvider.Aol,
        ["aol.se"] = PublicEmailProvider.Aol,

        ["gmx.com"] = PublicEmailProvider.Gmx,
        ["gmx.de"] = PublicEmailProvider.Gmx,
        ["gmx.net"] = PublicEmailProvider.Gmx,
        ["gmx.at"] = PublicEmailProvider.Gmx,

        ["mail.com"] = PublicEmailProvider.MailCom,
        ["email.com"] = PublicEmailProvider.MailCom,
        ["post.com"] = PublicEmailProvider.MailCom,
        ["europe.com"] = PublicEmailProvider.MailCom,
        ["iname.com"] = PublicEmailProvider.MailCom,

        ["yandex.com"] = PublicEmailProvider.Yandex,
        ["yandex.ru"] = PublicEmailProvider.Yandex,

        ["zoho.com"] = PublicEmailProvider.Zoho,

        ["tuta.io"] = PublicEmailProvider.Tuta,
        ["tutanota.com"] = PublicEmailProvider.Tuta,

        ["hey.com"] = PublicEmailProvider.Hey,
        ["duck.com"] = PublicEmailProvider.DuckDuckGoEmail,
        ["simplelogin.com"] = PublicEmailProvider.SimpleLogin,

        // Regional (Europe)
        ["web.de"] = PublicEmailProvider.WebDe,
        ["mail.ru"] = PublicEmailProvider.MailRu,

        ["wp.pl"] = PublicEmailProvider.WirtualnaPolska,
        ["vp.pl"] = PublicEmailProvider.WirtualnaPolska,

        ["onet.pl"] = PublicEmailProvider.Onet,
        ["onet.eu"] = PublicEmailProvider.Onet,
        ["op.pl"] = PublicEmailProvider.Onet,
        ["o2.pl"] = PublicEmailProvider.Onet,
        ["poczta.onet.pl"] = PublicEmailProvider.Onet,

        ["interia.pl"] = PublicEmailProvider.Interia,
        ["interia.eu"] = PublicEmailProvider.Interia,

        ["tlen.pl"] = PublicEmailProvider.Tlen,

        ["seznam.cz"] = PublicEmailProvider.Seznam,
        ["libero.it"] = PublicEmailProvider.Libero,
        ["freemail.hu"] = PublicEmailProvider.FreemailHu,
        ["abv.bg"] = PublicEmailProvider.Abv,
        ["inbox.lv"] = PublicEmailProvider.InboxLv,
        ["ukr.net"] = PublicEmailProvider.UkrNet,
        ["naver.com"] = PublicEmailProvider.Naver,

        // Swedish ISP / portal email
        ["telia.com"] = PublicEmailProvider.Telia,
        ["telia.se"] = PublicEmailProvider.Telia,
        ["home.se"] = PublicEmailProvider.Telia,
        ["swipnet.se"] = PublicEmailProvider.Telia,

        ["tele2.se"] = PublicEmailProvider.Tele2,
        ["comhem.se"] = PublicEmailProvider.Comhem,

        ["bredband.net"] = PublicEmailProvider.Bredbandsbolaget,
        ["bredbandsbolaget.se"] = PublicEmailProvider.Bredbandsbolaget,

        ["bredband2.com"] = PublicEmailProvider.Bredband2,
        ["ownit.nu"] = PublicEmailProvider.Ownit,
        ["mail.se"] = PublicEmailProvider.Spray,
        ["spray.se"] = PublicEmailProvider.Spray,
        ["passagen.se"] = PublicEmailProvider.Passagen,
        ["bahnhof.se"] = PublicEmailProvider.Bahnhof,
        ["glocalnet.net"] = PublicEmailProvider.Glocalnet,

        // Nordic ISP
        ["online.no"] = PublicEmailProvider.OnlineNo,
        ["kolumbus.fi"] = PublicEmailProvider.Kolumbus,
        ["aland.net"] = PublicEmailProvider.Aland,
    };

    private static readonly Dictionary<string, string> DomainCorrections = new(StringComparer.OrdinalIgnoreCase)
    {
        // Gmail misspellings
        ["gnail.com"] = "gmail.com",
        ["gamil.com"] = "gmail.com",
        ["gmai.com"] = "gmail.com",
        ["gmil.com"] = "gmail.com",
        ["gmsil.com"] = "gmail.com",
        ["gmal.com"] = "gmail.com",
        ["gmaill.com"] = "gmail.com",
        ["gamail.com"] = "gmail.com",
        ["gmail.con"] = "gmail.com",
        ["gmail.vom"] = "gmail.com",
        ["gmail.cim"] = "gmail.com",
        ["gmail.comi"] = "gmail.com",
        ["gmail.se"] = "gmail.com",
        ["gmail.co"] = "gmail.com",

        // Hotmail / Outlook misspellings
        ["hotmai.com"] = "hotmail.com",
        ["hotmil.com"] = "hotmail.com",
        ["hotmsil.com"] = "hotmail.com",
        ["hotmal.com"] = "hotmail.com",
        ["hotnail.com"] = "hotmail.com",
        ["hitmail.com"] = "hotmail.com",
        ["hptmail.com"] = "hotmail.com",
        ["hmail.com"] = "hotmail.com",
        ["hotmail.con"] = "hotmail.com",
        ["hotmail.co"] = "hotmail.com",
        ["hotmail.vom"] = "hotmail.com",
        ["hotmail.cim"] = "hotmail.com",
        ["hotmail.comm"] = "hotmail.com",

        // iCloud misspellings
        ["icloude.com"] = "icloud.com",
        ["icloud.se"] = "icloud.com",
        ["icloud.co"] = "icloud.com",
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

    public string Value { get; }
    public string LocalPart { get; }
    public string Domain { get; }

    /// <summary>The top-level domain without the dot, e.g. <c>com</c>, <c>se</c>, <c>org</c>.</summary>
    public string TopLevelDomain { get; }

    /// <summary>
    /// The country associated with the TLD when it is a country-code TLD (e.g. <c>.se</c> → Sweden),
    /// or <see langword="null"/> for generic TLDs like <c>.com</c> or <c>.org</c>.
    /// </summary>
    public Country? Country { get; }

    /// <summary>
    /// The identified public email provider when the domain matches a well-known consumer/free email service,
    /// or <see langword="null"/> for organizational or unknown domains.
    /// </summary>
    public PublicEmailProvider? Provider { get; }

    /// <summary>
    /// <see langword="true"/> when the domain belongs to a well-known public (consumer/free) email provider
    /// such as Gmail, Outlook, Yahoo, or a Swedish ISP.
    /// </summary>
    public bool IsPublicEmailProvider => Provider.HasValue;

    /// <summary>
    /// The domain in ASCII-compatible encoding (Punycode), e.g. <c>xn--rneholm-b1a.com</c> for
    /// <c>örneholm.com</c>. Returns the same value as <see cref="Domain"/> when the domain is
    /// already pure ASCII.
    /// </summary>
    public string DomainAscii { get; }

    /// <summary>
    /// <see langword="true"/> when the domain contains non-ASCII characters (e.g. Swedish å, ä, ö)
    /// and was validated via Punycode conversion — for example <c>örneholm.com</c>.
    /// </summary>
    public bool IsInternationalizedDomain { get; }

    /// <summary>
    /// The original domain before typo correction was applied, or <see langword="null"/> when no
    /// correction was needed. Only populated when <c>tryCorrectTypos</c> was <see langword="true"/>
    /// and a correction was applied.
    /// </summary>
    public string? OriginalDomain { get; }

    /// <summary>
    /// <see langword="true"/> when the domain was corrected from a common misspelling
    /// (e.g. <c>gmail.con</c> → <c>gmail.com</c>).
    /// </summary>
    public bool WasCorrected => OriginalDomain != null;

    private EmailAddress(string value, string localPart, string domain, string domainAscii,
        string tld, Country? country, PublicEmailProvider? provider,
        string? originalDomain = null)
    {
        Value = value;
        LocalPart = localPart;
        Domain = domain;
        DomainAscii = domainAscii;
        IsInternationalizedDomain = domain != domainAscii;
        TopLevelDomain = tld;
        Country = country;
        Provider = provider;
        OriginalDomain = originalDomain;
    }

    /// <summary>
    /// Attempts to parse and normalize an email address.
    /// </summary>
    public static bool TryParse(string? input, out EmailAddress? result)
        => TryParse(input, tryCorrectTypos: false, out result);

    /// <summary>
    /// Attempts to parse and normalize an email address. When <paramref name="tryCorrectTypos"/>
    /// is <see langword="true"/>, common domain misspellings such as <c>gmail.con</c>,
    /// <c>gnail.com</c>, or <c>hotmil.com</c> are corrected automatically. The original domain
    /// is preserved in <see cref="OriginalDomain"/> and <see cref="WasCorrected"/> is set to
    /// <see langword="true"/>. Typo correction is opt-in and should only be used when silent
    /// correction is acceptable in the application flow — intentionally unusual domains may
    /// otherwise be changed without the user's knowledge.
    /// </summary>
    public static bool TryParse(string? input, bool tryCorrectTypos, out EmailAddress? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var sanitized = InputSanitization.SanitizeInput(input!);

        var ltIndex = sanitized.IndexOf('<');
        if (ltIndex >= 0)
        {
            var gtIndex = sanitized.IndexOf('>', ltIndex + 1);
            if (gtIndex > ltIndex)
                sanitized = sanitized[(ltIndex + 1)..gtIndex];
        }

        sanitized = sanitized.Trim();
        if (sanitized.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            sanitized = sanitized[7..];
            var qIndex = sanitized.IndexOf('?');
            if (qIndex >= 0)
                sanitized = sanitized[..qIndex];
        }

        var normalized = sanitized.Replace(" ", "").ToLowerInvariant();

        if (normalized.EndsWith('.'))
            normalized = normalized[..^1];

        if (normalized.Length == 0 || normalized.Length > MaxInputLength) return false;

        var atIndex = normalized.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == normalized.Length - 1) return false;

        var localPart = normalized[..atIndex];
        var domain = normalized[(atIndex + 1)..];

        var domainForValidation = domain;
        if (domain.Any(c => c > 127))
        {
            if (domain.Any(c => c > 127 && !char.IsLetter(c)))
                return false;

            try
            {
                domainForValidation = IdnMapper.GetAscii(domain);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        var emailForValidation = $"{localPart}@{domainForValidation}";
        if (!EmailPattern.IsMatch(emailForValidation)) return false;

        string? originalDomain = null;
        if (tryCorrectTypos && DomainCorrections.TryGetValue(domain, out var corrected))
        {
            originalDomain = domain;
            domain = corrected;
            normalized = $"{localPart}@{domain}";
        }

        var lastDot = domain.LastIndexOf('.');
        var tld = domain[(lastDot + 1)..];

        var tldWithDot = "." + tld;
        TldCountryMap.Value.TryGetValue(tldWithDot, out var country);

        ProviderDomains.TryGetValue(domain, out var provider);

        result = new EmailAddress(normalized, localPart, domain, domainForValidation, tld, country,
            provider != default ? provider : null, originalDomain);
        return true;
    }

    public static EmailAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid email address.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a well-formed email address.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var sanitized = InputSanitization.SanitizeInput(input!);

        var ltIndex = sanitized.IndexOf('<');
        if (ltIndex >= 0)
        {
            var gtIndex = sanitized.IndexOf('>', ltIndex + 1);
            if (gtIndex > ltIndex)
                sanitized = sanitized[(ltIndex + 1)..gtIndex];
        }

        sanitized = sanitized.Trim();
        if (sanitized.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            sanitized = sanitized[7..];
            var qIndex = sanitized.IndexOf('?');
            if (qIndex >= 0)
                sanitized = sanitized[..qIndex];
        }

        var normalized = sanitized.Replace(" ", "").ToLowerInvariant();
        if (normalized.EndsWith('.'))
            normalized = normalized[..^1];

        if (normalized.Length == 0 || normalized.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Email address is too long.", "E-postadressen är för lång.");

        var atIndex = normalized.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == normalized.Length - 1)
            return ValidationResult.Invalid(input, ValidationErrorReason.MissingAtSign,
                "Email address must contain an @ sign with text before and after.",
                "E-postadressen måste innehålla ett @-tecken med text före och efter.");

        var localPart = normalized[..atIndex];
        var domain = normalized[(atIndex + 1)..];

        var domainForValidation = domain;
        if (domain.Any(c => c > 127))
        {
            if (domain.Any(c => c > 127 && !char.IsLetter(c)))
                return ValidationResult.Invalid(input, ValidationErrorReason.InvalidDomain,
                    "Email address has an invalid domain.", "E-postadressen har en ogiltig domän.");

            try
            {
                domainForValidation = IdnMapper.GetAscii(domain);
            }
            catch (ArgumentException)
            {
                return ValidationResult.Invalid(input, ValidationErrorReason.InvalidDomain,
                    "Email address has an invalid domain.", "E-postadressen har en ogiltig domän.");
            }
        }

        var emailForValidation = $"{localPart}@{domainForValidation}";
        if (!EmailPattern.IsMatch(emailForValidation))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "Invalid email address format.", "Ogiltigt format för e-postadress.");

        return ValidationResult.Valid(input);
    }

    /// <summary>
    /// Returns the normalized email address in lowercase, for example <c>user@example.com</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// When <paramref name="tryCorrectTypos"/> is <see langword="true"/>, common domain misspellings are corrected.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, bool tryCorrectTypos = false) =>
        TryParse(input, tryCorrectTypos, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized email address in lowercase, for example <c>user@example.com</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="tryCorrectTypos"/> is <see langword="true"/>, common domain misspellings are corrected.
    /// </summary>
    public static string? Normalize(string? input, bool tryCorrectTypos = false, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, tryCorrectTypos, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized email address in lowercase, for example <c>user@example.com</c>.
    /// For internationalized domains the Unicode form is returned, e.g. <c>user@örneholm.com</c>.
    /// </summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the normalized email address in lowercase, for example <c>user@example.com</c>.
    /// For internationalized domains the Unicode form is returned, e.g. <c>user@örneholm.com</c>.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Returns the email address with the domain in ASCII-compatible encoding (Punycode),
    /// for example <c>user@xn--rneholm-b1a.com</c>. Returns the same as <see cref="ToString"/>
    /// when the domain is already pure ASCII.
    /// </summary>
    public string ToAsciiString() => IsInternationalizedDomain ? $"{LocalPart}@{DomainAscii}" : Value;

    private static readonly Regex ScanPattern = new(
        @"(?<![\w.@])(?!\.)[a-zA-Z0-9](?:[a-zA-Z0-9._%+\-]*[a-zA-Z0-9_%+\-])?@(?!-)[a-zA-Z0-9\-]+(?:\.(?!-)[a-zA-Z0-9\-]+)*\.[a-zA-Z]{2,}(?![\w@])",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(500));

    /// <summary>
    /// Scans unstructured text for potential email addresses.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real email address in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EmailAddress>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EmailAddress>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var email)) continue;
            results.Add(new TextCandidate<EmailAddress>(
                match.Index,
                match.Length,
                match.Value,
                nameof(EmailAddress),
                TextCandidateCategory.Contact,
                email!.ToNormalizedString(),
                email.ToString(),
                email.ToMaskedString(),
                TextMatchConfidence.High,
                email));
        }
        return results;
    }

    public bool Equals(EmailAddress? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EmailAddress other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(EmailAddress? a, EmailAddress? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EmailAddress? a, EmailAddress? b) => !(a == b);
    public int CompareTo(EmailAddress? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(EmailAddress left, EmailAddress right) => left.CompareTo(right) < 0;
    public static bool operator >(EmailAddress left, EmailAddress right) => left.CompareTo(right) > 0;
    public static bool operator <=(EmailAddress left, EmailAddress right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EmailAddress left, EmailAddress right) => left.CompareTo(right) >= 0;
}
