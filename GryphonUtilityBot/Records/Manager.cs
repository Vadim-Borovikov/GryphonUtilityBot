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
        public Manager(Bot bot, SaveManager<List<Record>> saveManager)
        {
            _saveManager = saveManager;
            _bot = bot;
        }

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

        public async Task ProcessFindQuery(ChatId chatId, FindQuery query)
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
                    await _bot.Client.ForwardMessageAsync(chatId, record.ChatId, record.MessageId);
                }
            }
            else
            {
                await _bot.Client.SendTextMessageAsync(chatId, "Я не нашёл таких записей.");
            }
        }

        public Task Mark(ChatId chatId, Message recordMessage, MarkQuery query)
        {
            _saveManager.Load();

            Record record = _saveManager.Data.FirstOrDefault(
                r => (r.ChatId == recordMessage.Chat.Id) && (r.MessageId == recordMessage.MessageId));

            if (record == null)
            {
                return _bot.Client.SendTextMessageAsync(chatId, "Я не нашёл нужной записи.");
            }

            if (query.DateTime.HasValue)
            {
                record.DateTime = query.DateTime.Value;
            }

            record.Tags = query.Tags;
            _saveManager.Save();
            return _bot.Client.SendTextMessageAsync(chatId, "Запись обновлена.");
        }

        private Record GetRecord(Message message, MarkQuery query)
        {
            if (!message.ForwardDate.HasValue)
            {
                return null;
            }

            return new Record
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                DateTime = query?.DateTime ?? _bot.TimeManager.ToLocal(message.ForwardDate.Value),
                Tags = query?.Tags ?? new HashSet<string>()
            };
        }

        private readonly SaveManager<List<Record>> _saveManager;
        private readonly Bot _bot;
    }
}
