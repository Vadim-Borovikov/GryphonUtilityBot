using System;
using System.Collections.Generic;
using GoogleSheetsManager;
using GryphonUtilities;

namespace GryphonUtilityBot.Articles;

internal sealed class Article : IComparable<Article>, ISavable
{
    IList<string> ISavable.Titles => Titles;

    public readonly Uri Uri;
    public readonly DateTime Date;

    public Article(DateTime date, Uri uri)
    {
        Date = date;
        Uri = uri;
    }

    public int CompareTo(Article? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        int datesCompare = Date.CompareTo(other.Date);
        return datesCompare != 0
            ? datesCompare
            : string.Compare(Uri.AbsoluteUri, other.Uri.AbsoluteUri, StringComparison.Ordinal);
    }

    public IDictionary<string, object?> Convert()
    {
        return new Dictionary<string, object?>
        {
            { DateTitle, Date.ToShortDateString() },
            { UriTitle, Uri }
        };
    }

    public static Article Load(IDictionary<string, object?> valueSet)
    {
        Uri uri = valueSet[UriTitle].ToUri().GetValue();
        DateTime date = valueSet[DateTitle].ToDateTime().GetValue($"Empty date for \"{uri}\"");
        return new Article(date, uri);
    }

    private static readonly IList<string> Titles = new List<string>
    {
        DateTitle,
        UriTitle
    };

    private const string DateTitle = "Дата";
    private const string UriTitle = "Ссылка";

}