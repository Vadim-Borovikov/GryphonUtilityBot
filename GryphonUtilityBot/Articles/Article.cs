using System;
using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Articles
{
    internal sealed class Article : IComparable<Article>, ISavable, ILoadable
    {
        IList<string> ISavable.Titles => Titles;

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

        public IDictionary<string, object> Save()
        {
            return new Dictionary<string, object>
            {
                {DateTitle, Date.ToShortDateString() },
                {UriTitle, Uri }
            };
        }

        public void Load(IDictionary<string, object> valueSet)
        {
            Uri = valueSet[UriTitle]?.ToUri();
            Date = valueSet[DateTitle]?.ToDateTime() ?? throw new ArgumentNullException($"Empty date for \"{Uri}\"");
        }

        private static readonly IList<string> Titles = new List<string>
        {
            DateTitle,
            UriTitle
        };

        private const string DateTitle = "Дата";
        private const string UriTitle = "Ссылка";

    }
}