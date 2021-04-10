using System;
using File = System.IO.File;

namespace GryphonUtilityBot.Web
{
    internal static class Utils
    {
        public static void LogException(Exception ex, string prefix = "")
        {
            File.AppendAllText(ExceptionsLogPath, $"{prefix}{ex}{Environment.NewLine}");
        }

        private const string ExceptionsLogPath = "errors.txt";
    }
}
