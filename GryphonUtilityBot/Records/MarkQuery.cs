using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal class MarkQuery
{
    public readonly DateTime? DateTime;
    public readonly HashSet<string> Tags;

    protected MarkQuery(DateTime? dateTime, IEnumerable<string> tags)
    {
        DateTime = dateTime;
        Tags = new HashSet<string>(tags);
    }

    public static bool TryParseMarkQuery(string text, out MarkQuery? query)
    {
        query = ParseMarkQuery(text);
        return query is not null;
    }

    private static MarkQuery? ParseMarkQuery(string text)
    {
        List<string> parts = new(text.Split(' '));
        if (parts.Count == 0)
        {
            return null;
        }

        DateTime? dateTime = Utils.ParseFirstDateTime(parts);

        return new MarkQuery(dateTime, parts);
    }
}