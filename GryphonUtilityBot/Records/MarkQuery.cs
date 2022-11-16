using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal class MarkQuery
{
    public readonly DateOnly? DateOnly;
    public readonly HashSet<string> Tags;

    protected MarkQuery(DateOnly? dateOnly, IEnumerable<string> tags)
    {
        DateOnly = dateOnly;
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

        DateOnly? dateTime = Utils.ParseFirstDate(parts);

        return new MarkQuery(dateTime, parts);
    }
}