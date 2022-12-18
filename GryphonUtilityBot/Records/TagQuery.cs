using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal class TagQuery
{
    public readonly DateOnly? DateOnly;
    public readonly HashSet<string> Tags;

    protected TagQuery(DateOnly? dateOnly, IEnumerable<string> tags)
    {
        DateOnly = dateOnly;
        Tags = new HashSet<string>(tags);
    }

    public static TagQuery? ParseTagQuery(string text)
    {
        List<string> parts = new(text.Split(' '));
        if (parts.Count == 0)
        {
            return null;
        }

        DateOnly? dateTime = Manager.ParseFirstDate(parts);

        return new TagQuery(dateTime, parts);
    }
}