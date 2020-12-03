using System;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class RecordsQuery
    {
        public DateTime From;
        public DateTime? To;

        public static bool TryParseQuery(string text, out RecordsQuery query)
        {
            query = null;
            string[] parts = text.Split(' ');
            if ((parts.Length == 0) && (parts.Length > 2))
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0], out DateTime from))
            {
                return false;
            }
            query = new RecordsQuery { From = from };

            if (parts.Length == 2)
            {
                if (!DateTime.TryParse(parts[1], out DateTime to))
                {
                    query.To = to;
                }
            }

            return true;
        }
    }
}
