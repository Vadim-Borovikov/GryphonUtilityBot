using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Records
{
    internal sealed class Manager
    {
        public Manager(SaveManager<List<Record>> saveManager) => _saveManager = saveManager;

        public void SaveRecord(Message message, MarkQuery query)
        {
            _saveManager.Load();

            Record record = GetRecord(message, query);
            if (record != null)
            {
                _saveManager.Data.Add(record);
            }

            _saveManager.Save();
        }

        public async Task ProcessFindQuery(TelegramBotClient client, ChatId chatId, FindQuery query)
        {
            _saveManager.Load();

            List<Record> records = _saveManager.Data
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

        public Task Mark(TelegramBotClient client, ChatId chatId, Message recordMessage, MarkQuery query)
        {
            _saveManager.Load();

            Record record = _saveManager.Data.FirstOrDefault(
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

        private static Record GetRecord(Message message, MarkQuery query)
        {
            if (!message.ForwardDate.HasValue)
            {
                return null;
            }

            return new Record
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                DateTime = query?.DateTime ?? message.ForwardDate.Value.ToLocal(),
                Tags = query?.Tags ?? new HashSet<string>()
            };
        }

        private readonly SaveManager<List<Record>> _saveManager;
    }
}
