using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal sealed class FindQuery : MarkQuery
{
    public readonly DateOnly From;
    public readonly DateOnly To;

    private FindQuery(DateOnly from, DateOnly to, IEnumerable<string> tags) : base(null, tags)
    {
        From = from;
        To = to;
    }

    public static bool TryParseFindQuery(string text, out FindQuery? query)
    {
        query = ParseFindQuery(text);
        return query is not null;
    }

    private static FindQuery? ParseFindQuery(string text)
    {
        List<string> parts = new(text.Split(' '));
        if (parts.Count == 0)
        {
            return null;
        }

        DateOnly? from = Utils.ParseFirstDate(parts);
        if (!from.HasValue)
        {
            return null;
        }

        DateOnly? to = Utils.ParseFirstDate(parts);
        if (!to.HasValue)
        {
            return null;
        }

        return new FindQuery(from.Value, to.Value, parts);
    }
}