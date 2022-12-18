using GryphonUtilities;
using System;
using GoogleSheetsManager.Extensions;

namespace GryphonUtilityBot;

internal static class ObjectExtensions
{
    public static DateOnly? ToDateOnly(this object? o, TimeManager timeManager)
    {
        if (o is DateOnly d)
        {
            return d;
        }

        DateTimeFull? dtf = o.ToDateTimeFull(timeManager);
        return dtf?.DateOnly;
    }

    public static Uri? ToUri(this object? o)
    {
        if (o is Uri uri)
        {
            return uri;
        }
        string? uriString = o?.ToString();
        return string.IsNullOrWhiteSpace(uriString) ? null : new Uri(uriString);
    }
}