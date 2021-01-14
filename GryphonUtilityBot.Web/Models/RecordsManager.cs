using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class RecordsManager
    {
        internal RecordsManager(Manager saveManager) => _saveManager = saveManager;

        internal void SaveRecord(Message message, RecordsMarkQuery query)
        {
            _saveManager.Load();

            Data record = GetRecord(message, query);
            if (record != null)
            {
                _saveManager.Data.Add(record);
            }

            _saveManager.Save();
        }

        internal async Task ProcessFindQuery(TelegramBotClient client, ChatId chatId, RecordsFindQuery query)
        {
            _saveManager.Load();

            List<Data> records = _saveManager.Data
                .Where(r => r.DateTime.Date >= query.From)
                .Where(r => r.DateTime.Date <= query.To)
                .ToList();

            if (query.Tags.Any())
            {
                records = records.Where(r => r.Tags.Any(t => query.Tags.Contains(t))).ToList();
            }

            if (records.Any())
            {
                foreach (Data record in records)
                {
                    await client.ForwardMessageAsync(chatId, record.ChatId, record.MessageId);
                }
            }
            else
            {
                await client.SendTextMessageAsync(chatId, "Я не нашёл таких записей.");
            }
        }

        internal Task Mark(TelegramBotClient client, ChatId chatId, Message recordMessage, RecordsMarkQuery query)
        {
            _saveManager.Load();

            Data record = _saveManager.Data.FirstOrDefault(
                r => (r.ChatId == recordMessage.Chat.Id) && (r.MessageId == recordMessage.MessageId));

            if (record == null)
            {
                return client.SendTextMessageAsync(chatId, "Я не нашёл нужной записи.");
            }

            if (query.DateTime.HasValue)
            {
                record.DateTime = query.DateTime.Value;
            }

            record.Tags = query.Tags;
            _saveManager.Save();
            return client.SendTextMessageAsync(chatId, "Запись обновлена.");
        }

        private static Data GetRecord(Message message, RecordsMarkQuery query)
        {
            if (!message.ForwardDate.HasValue)
            {
                return null;
            }

            return new Data
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                DateTime = query?.DateTime ?? message.ForwardDate.Value.ToLocal(),
                Tags = query?.Tags ?? new HashSet<string>()
            };
        }

        private readonly Manager _saveManager;
    }
}
