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

        internal async Task ProcessQuery(RecordsQuery query, ChatId chatId, TelegramBotClient client)
        {
            _saveManager.Load();

            IEnumerable<Record> records = _saveManager.Data.Records.Where(r => r.DateTime >= query.From);
            if (query.To.HasValue)
            {
                records = records.Where(r => r.DateTime <= query.To.Value);
            }
            if (query.Tags.Any())
            {
                records = records.Where(r => r.Tags.Any(t => query.Tags.Contains(t)));
            }

            foreach (Record record in records)
            {
                await client.ForwardMessageAsync(chatId, record.ChatId, record.MessageId);
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
