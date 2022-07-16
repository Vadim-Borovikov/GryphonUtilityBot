using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal sealed class FindQuery : MarkQuery
{
    public readonly DateTime From;
    public readonly DateTime To;

    private FindQuery(DateTime from, DateTime to, IEnumerable<string> tags) : base(null, tags)
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

        DateTime? from = Utils.ParseFirstDateTime(parts);
        if (!from.HasValue)
        {
            return null;
        }

        DateTime? to = Utils.ParseFirstDateTime(parts);
        if (!to.HasValue)
        {
            return null;
        }

        return new FindQuery(from.Value, to.Value, parts);
    }
}