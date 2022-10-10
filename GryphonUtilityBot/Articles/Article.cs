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

    [Required]
    [SheetField("Дата")]
    public DateTime Date;

    public Article() { }

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
            return -1;
        }

        int datesCompare = Date.CompareTo(other.Date);
        return datesCompare != 0
            ? datesCompare
            : string.Compare(Uri.AbsoluteUri, other.Uri.AbsoluteUri, StringComparison.Ordinal);
    }
}