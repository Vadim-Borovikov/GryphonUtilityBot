using System;
using System.ComponentModel.DataAnnotations;
using GoogleSheetsManager;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Articles;

internal sealed class Article : IComparable<Article>
{
    [UsedImplicitly]
    [Required]
    [SheetField("Ссылка")]
    public Uri Uri = null!;

    [UsedImplicitly]
    [SheetField("Читаю")]
    public bool Current;

    [Required]
    [SheetField("Дата")]
    public DateTime Date;

    public Article() { }

    public Article(DateTime date, Uri uri, bool current = false)
    {
        Date = date;
        Uri = uri;
        Current = current;
    }

    public int CompareTo(Article? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return -1;
        }

        if (Current)
        {
            return -1;
        }

        if (other.Current)
        {
            return 1;
        }

        int datesCompare = Date.CompareTo(other.Date);
        return datesCompare != 0
            ? datesCompare
            : string.Compare(Uri.AbsoluteUri, other.Uri.AbsoluteUri, StringComparison.Ordinal);
    }
}