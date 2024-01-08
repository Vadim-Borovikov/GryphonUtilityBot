using System;
using System.ComponentModel.DataAnnotations;
using GoogleSheetsManager;
using GryphonUtilities.Time;
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
    public DateOnly Date;

    public Article() { }

    public Article(DateOnly date, Uri uri, bool current = false)
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

    public static Article? Parse(string text)
    {
        string[] parts = text.Split(' ');

        Uri? uri;
        switch (parts.Length)
        {
            case 1:
                uri = CreateUri(parts[0]);
                return uri is null ? null : new Article(DateTimeFull.CreateUtcNow().DateOnly, uri);
            case 2:
                DateOnly? date = ParseDate(parts[0]);
                if (!date.HasValue)
                {
                    return null;
                }
                uri = CreateUri(parts[1]);
                return uri is null ? null : new Article(date.Value, uri);
            default: return null;
        }
    }

    private static DateOnly? ParseDate(string dateString)
    {
        if (DateOnly.TryParse(dateString, out DateOnly date))
        {
            return date;
        }

        if (!int.TryParse(dateString, out int day))
        {
            return null;
        }

        try
        {
            DateTimeFull now = DateTimeFull.CreateUtcNow();
            return new DateOnly(now.DateOnly.Year, now.DateOnly.Month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static Uri? CreateUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri) ? uri : null;
    }
}