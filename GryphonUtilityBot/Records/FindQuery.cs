using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records
{
    internal sealed class FindQuery : MarkQuery
    {
        public DateTime From;
        public DateTime To;

        public static bool TryParseFindQuery(string text, out FindQuery query)
        {
            query = ParseFindQuery(text);
            return query != null;
        }

        private static FindQuery ParseFindQuery(string text)
        {
            var parts = new List<string>(text.Split(' '));
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

            return new FindQuery
            {
                From = from.Value,
                To = to.Value,
                Tags = new HashSet<string>(parts)
            };
        }
    }
}
