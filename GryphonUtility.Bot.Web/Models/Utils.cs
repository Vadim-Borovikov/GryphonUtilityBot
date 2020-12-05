using System;
using System.IO;

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

        private const string ExceptionsLogPath = "errors.txt";

        private static TimeZoneInfo _timeZoneInfo;
    }
}
