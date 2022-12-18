using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal sealed class FindQuery : TagQuery
{
    public readonly DateOnly From;
    public readonly DateOnly To;

    private FindQuery(DateOnly from, DateOnly to, IEnumerable<string> tags) : base(null, tags)
    {
        From = from;
        To = to;
    }

    public static FindQuery? ParseFindQuery(string text)
    {
        List<string> parts = new(text.Split(' '));
        if (parts.Count == 0)
        {
            return null;
        }

        DateOnly? from = Manager.ParseFirstDate(parts);
        if (!from.HasValue)
        {
            return null;
        }

        DateOnly? to = Manager.ParseFirstDate(parts);
        if (!to.HasValue)
        {
            return null;
        }

        return new FindQuery(from.Value, to.Value, parts);
    }
}