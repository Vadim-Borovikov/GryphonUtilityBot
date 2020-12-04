using System;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class RecordsQuery
    {
        public DateTime From;
        public DateTime To;
        public List<string> Tags = new List<string>();

        public static bool TryParseQuery(string text, out RecordsQuery query)
        {
            query = null;
            string[] parts = text.Split(' ');

            if ((parts.Length == 0) || !DateTime.TryParse(parts[0], out DateTime from))
            {
                return false;
            }
            query = new RecordsQuery
            {
                From = from,
                To = from
            };

            if (parts.Length > 1)
            {
                int datesAmount = 1;
                if (DateTime.TryParse(parts[1], out DateTime to))
                {
                    query.To = to;
                    ++datesAmount;
                }

                query.Tags = parts.Skip(datesAmount).ToList();
            }

            return true;
        }
    }
}
