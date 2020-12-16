using System;
using System.Collections.Generic;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class RecordsFindQuery : RecordsMarkQuery
    {
        public DateTime From;
        public DateTime To;

        public static bool TryParseFindQuery(string text, out RecordsFindQuery query)
        {
            query = ParseFindQuery(text);
            return query != null;
        }

        private static RecordsFindQuery ParseFindQuery(string text)
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

            return new RecordsFindQuery
            {
                From = from.Value,
                To = to.Value,
                Tags = new HashSet<string>(parts)
            };
        }
    }
}
