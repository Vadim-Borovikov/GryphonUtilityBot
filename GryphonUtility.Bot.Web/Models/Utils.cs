using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GryphonUtility.Bot.Web.Models
{
    internal static class Utils
    {
        public static void LogException(Exception ex)
        {
            File.AppendAllText(ExceptionsLogPath, $"{ex}{Environment.NewLine}");
        }

        public static DateTime ToLocal(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _timeZoneInfo);
        }

        public static void SetupTimeZoneInfo(string id) => _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id);


        public static DateTime? ParseFirstDateTime(List<string> parts)
        {
            if ((parts.Count == 0) || !DateTime.TryParse(parts.First(), out DateTime dateTime))
            {
                return null;
            }

            parts.RemoveAt(0);
            return dateTime;
        }

        private const string ExceptionsLogPath = "errors.txt";

        private static TimeZoneInfo _timeZoneInfo;
    }
}
