﻿using System;
using System.Collections.Generic;

namespace GryphonUtility.Bot.Web.Models
{
    public class RecordsMarkQuery
    {
        internal DateTime? DateTime;
        internal HashSet<string> Tags;

        internal static bool TryParseMarkQuery(string text, out RecordsMarkQuery query)
        {
            query = ParseMarkQuery(text);
            return query != null;
        }

        private static RecordsMarkQuery ParseMarkQuery(string text)
        {
            var parts = new List<string>(text.Split(' '));
            if (parts.Count == 0)
            {
                return null;
            }

            DateTime? dateTime = Utils.ParseFirstDateTime(parts);

            return new RecordsMarkQuery
            {
                DateTime = dateTime,
                Tags = new HashSet<string>(parts)
            };
        }
    }
}