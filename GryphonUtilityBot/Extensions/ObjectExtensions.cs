using GoogleSheetsManager.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtilityBot.Extensions;

internal static class ObjectExtensions
{
    public static List<byte>? ToBytes(this object? o)
    {
        if (o is IEnumerable<byte> l)
        {
            return l.ToList();
        }

        IEnumerable<byte?>? items = o?.ToString()?.Split(BytesSeparator).Select(s => s.ToByte());
        if (items is null)
        {
            return null;
        }

        List<byte> result = new();
        foreach (byte? item in items)
        {
            if (item is null)
            {
                return null;
            }
            result.Add(item.Value);
        }
        return result;
    }

    public const string BytesSeparator = ";";
}