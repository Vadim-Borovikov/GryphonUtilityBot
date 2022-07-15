using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records
{
    internal class MarkQuery
    {
        public DateTime? DateTime;
        public HashSet<string> Tags;

        public static bool TryParseMarkQuery(string text, out MarkQuery query)
        {
            query = ParseMarkQuery(text);
            return query != null;
        }

        private static MarkQuery ParseMarkQuery(string text)
        {
            var parts = new List<string>(text.Split(' '));
            if (parts.Count == 0)
            {
                return null;
            }

            DateTime? dateTime = Utils.ParseFirstDateTime(parts);

            return new MarkQuery
            {
                DateTime = dateTime,
                Tags = new HashSet<string>(parts)
            };
        }
    }
}
