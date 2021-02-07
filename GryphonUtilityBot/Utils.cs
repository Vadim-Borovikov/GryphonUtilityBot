using System;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtilityBot
{
    internal static class Utils
    {
        public static DateTime? ParseFirstDateTime(List<string> parts)
        {
            if ((parts.Count == 0) || !DateTime.TryParse(parts.First(), out DateTime dateTime))
            {
                return null;
            }

            parts.RemoveAt(0);
            return dateTime;
        }
    }
}
