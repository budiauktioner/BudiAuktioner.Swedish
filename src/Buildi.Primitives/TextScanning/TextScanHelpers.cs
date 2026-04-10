namespace Buildi.Primitives.TextScanning;

/// <summary>
/// Shared helpers for scanning unstructured text around anchor positions.
/// </summary>
internal static class TextScanHelpers
{
    /// <summary>
    /// Scans forward from <paramref name="fromIndex"/>, returning the end position of a
    /// plausible text span. Stops at sentence-ending punctuation (<c>. ! ?</c>), double
    /// newline, or <paramref name="maxDistance"/> characters. A trailing comma followed by
    /// an uppercase word (e.g. a country name) is included before stopping.
    /// </summary>
    internal static int ScanForward(string text, int fromIndex, int maxDistance)
    {
        var limit = Math.Min(text.Length, fromIndex + maxDistance);
        var i = fromIndex;

        while (i < limit && text[i] == ' ') i++;

        while (i < limit)
        {
            var ch = text[i];
            if (ch is '.' or '!' or '?') break;
            if (ch == '\n' && i + 1 < text.Length && text[i + 1] == '\n') break;

            if (ch == ',')
            {
                var afterComma = i + 1;
                while (afterComma < limit && text[afterComma] == ' ') afterComma++;
                if (afterComma < limit && char.IsUpper(text[afterComma]))
                {
                    var wordEnd = afterComma;
                    while (wordEnd < limit && (char.IsLetter(text[wordEnd]) || text[wordEnd] == ' '))
                        wordEnd++;
                    i = wordEnd;
                }
                break;
            }

            i++;
        }

        return i;
    }

    /// <summary>
    /// Scans backward from <paramref name="fromIndex"/>, returning the start position of a
    /// plausible text span. Stops at sentence-ending punctuation (<c>. ! ?</c>), double
    /// newline, or <paramref name="maxDistance"/> characters. Leading whitespace is skipped.
    /// </summary>
    internal static int ScanBackward(string text, int fromIndex, int maxDistance)
    {
        var limit = Math.Max(0, fromIndex - maxDistance);
        var i = fromIndex - 1;

        while (i >= limit)
        {
            var ch = text[i];

            if (ch is '.' or '!' or '?')
            {
                i++;
                break;
            }

            if (ch == '\n' && i > 0 && text[i - 1] == '\n')
            {
                i++;
                break;
            }

            i--;
        }

        if (i < limit) i = limit;

        while (i < fromIndex && text[i] == ' ') i++;

        return i;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="index"/> is at the start of
    /// a word — either the beginning of the string or preceded by whitespace / punctuation.
    /// </summary>
    internal static bool IsWordBoundary(string text, int index) =>
        index == 0 || text[index - 1] is ' ' or '\n' or '\t' or ',' or ';';

    /// <summary>
    /// Returns <see langword="true"/> when the span [<paramref name="start"/>,
    /// <paramref name="end"/>) overlaps any span in <paramref name="spans"/>.
    /// </summary>
    internal static bool IsAlreadyCovered(List<(int Start, int End)> spans, int start, int end)
    {
        for (var i = 0; i < spans.Count; i++)
        {
            var (s, e) = spans[i];
            if (start < e && s < end) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="index"/> sits right after a
    /// clause/sentence boundary (<c>: </c>, <c>. </c>, <c>! </c>, <c>? </c>, or newline),
    /// making it a high-confidence start position for a structured value.
    /// </summary>
    internal static bool IsStrongStart(string text, int index)
    {
        if (index >= 2 && text[index - 1] == ' '
            && text[index - 2] is ':' or '.' or '!' or '?')
            return true;
        if (text[index - 1] == '\n') return true;
        return false;
    }
}
