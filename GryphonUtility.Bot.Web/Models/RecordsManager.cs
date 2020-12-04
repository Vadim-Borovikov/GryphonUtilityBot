using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models
{
    public sealed class RecordsManager
    {
        internal RecordsManager(Manager saveManager) { _saveManager = saveManager; }

        internal void SaveRecord(Message message)
        {
            _saveManager.Load();

            Record record = GetRecord(message);
            if (record != null)
            {
                _saveManager.Data.Records.Add(record);
            }

            _saveManager.Save();
        }

        internal async Task ProcessQuery(TelegramBotClient client, ChatId chatId, RecordsQuery query)
        {
            _saveManager.Load();

            List<Record> records = _saveManager.Data.Records
                .Where(r => r.DateTime.Date >= query.From)
                .Where(r => r.DateTime.Date <= query.To)
                .ToList();

            if (query.Tags.Any())
            {
                records = records.Where(r => r.Tags.Any(t => query.Tags.Contains(t))).ToList();
            }

            if (records.Any())
            {
                foreach (Record record in records)
                {
                    await client.ForwardMessageAsync(chatId, record.ChatId, record.MessageId);
                }
            }
            else
            {
                await client.SendTextMessageAsync(chatId, "Я не нашёл таких записей.");
            }
        }

        private static Record GetRecord(Message message)
        {
            if (!message.ForwardDate.HasValue)
            {
                return null;
            }

            return new Record
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                DateTime = message.ForwardDate.Value
            };
        }

        private readonly Manager _saveManager;
    }
}
