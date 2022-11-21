using System;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtilityBot;

internal static class Utils
{
    public static DateOnly? ParseFirstDate(List<string> parts)
    {
        if ((parts.Count == 0) || !DateOnly.TryParse(parts.First(), out DateOnly date))
        {
            return null;
        }

        parts.RemoveAt(0);
        return date;
    }

    public static Uri? ToUri(object? o)
    {
        if (o is Uri uri)
        {
            return uri;
        }
        string? uriString = o?.ToString();
        return string.IsNullOrWhiteSpace(uriString) ? null : new Uri(uriString);
    }
}