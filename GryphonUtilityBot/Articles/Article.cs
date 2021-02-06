using System;
using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Articles
{
    internal sealed class Article : IComparable<Article>, ISavable, ILoadable
    {
        public Uri Uri { get; private set; }
        public DateTime Date { get; private set; }

        public Article() { }

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

        public IList<object> Save()
        {
            return new List<object>
            {
                Date.ToShortDateString(),
                Uri
            };
        }

        public void Load(IList<object> values)
        {
            Uri = values.ToUri(1);
            Date = values.ToDateTime(0) ?? throw new ArgumentNullException($"Empty date for \"{Uri}\"");
        }
    }
}