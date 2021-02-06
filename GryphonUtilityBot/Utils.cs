using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot
{
    internal static class Utils
    {
        public static Task<Message> SendStickerAsync(this ITelegramBotClient client, Message message,
            InputOnlineFile sticker)
        {
            return client.SendStickerAsync(message.Chat, sticker, replyToMessageId: message.MessageId);
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

        private static TimeZoneInfo _timeZoneInfo;
    }
}
