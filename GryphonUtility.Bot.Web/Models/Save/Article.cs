using System;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Article : IComparable<Article>
    {
        [JsonProperty]
        public Uri Uri { get; set; }
        [JsonProperty]
        public DateTime Date { get; set; }

        public Article(DateTime date, Uri uri)
        {
            Date = date;
            Uri = uri;
        }

        public int CompareTo(Article other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            int datesCompare = Date.CompareTo(other.Date);
            return datesCompare != 0
                ? datesCompare
                : string.Compare(Uri.AbsoluteUri, other.Uri.AbsoluteUri, StringComparison.Ordinal);
        }
    }
}