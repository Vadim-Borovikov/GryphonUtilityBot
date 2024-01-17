using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtilities;
using GryphonUtilities.Time;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Records;

internal sealed class Manager
{
    public Manager(Bot bot, SaveManager<Data> saveManager)
    {
        _saveManager = saveManager;
        _bot = bot;
    }

    public Task SaveRecordAsync(Message message, TagQuery? query)
    {
        _saveManager.Load();

        RecordData? record = GetRecord(message, query);
        if (record is not null)
        {
            _saveManager.SaveData.Records.Add(record);
        }

        _saveManager.Save();

        return _bot.SendTextMessageAsync(message.Chat, "Запись добавлена.", replyToMessageId: message.MessageId);
    }

    public async Task ProcessFindQueryAsync(Chat chat, FindQuery query)
    {
        _saveManager.Load();

        List<RecordData> records = _saveManager.SaveData
                                               .Records
                                               .Where(r => r.DateTime.DateOnly >= query.From)
                                               .Where(r => r.DateTime.DateOnly <= query.To)
                                               .ToList();

        if (query.Tags.Any())
        {
            records = records.Where(r => r.Tags.Any(t => query.Tags.Contains(t))).ToList();
        }

        if (records.Any())
        {
            foreach (RecordData record in records)
            {
                await _bot.ForwardMessageAsync(chat, record.ChatId, record.MessageId);
            }
        }
        else
        {
            await _bot.SendTextMessageAsync(chat, "Я не нашёл таких записей.");
        }
    }

    public Task TagAsync(Chat chat, long chatId, long messageId, TagQuery query)
    {
        _saveManager.Load();

        RecordData? record = _saveManager.SaveData.Records.FirstOrDefault(r =>
            (r.ChatId == chatId) && (r.MessageId == messageId));

        if (record is null)
        {
            return _bot.SendTextMessageAsync(chat, "Я не нашёл нужной записи.");
        }

        if (query.DateOnly.HasValue)
        {
            record.DateTime = _bot.Clock.GetDateTimeFull(query.DateOnly.Value, TimeOnly.MinValue);
        }

        record.Tags = query.Tags;
        _saveManager.Save();
        return _bot.SendTextMessageAsync(chat, "Запись обновлена.");
    }

    public static DateOnly? ParseFirstDate(List<string> parts)
    {
        if ((parts.Count == 0) || !DateOnly.TryParse(parts.First(), out DateOnly date))
        {
            return null;
        }

        parts.RemoveAt(0);
        return date;
    }

    private RecordData? GetRecord(Message message, TagQuery? query)
    {
        if (!message.ForwardDate.HasValue)
        {
            return null;
        }

        DateTimeFull dateTime = query?.DateOnly is null
            ? _bot.Clock.GetDateTimeFull(message.ForwardDate.Value.ToUniversalTime())
            : _bot.Clock.GetDateTimeFull(query.DateOnly.Value, TimeOnly.MinValue);
        return new RecordData
        {
            MessageId = message.MessageId,
            ChatId = message.Chat.Id,
            DateTime = dateTime,
            Tags = query?.Tags ?? new HashSet<string>()
        };
    }

    private readonly SaveManager<Data> _saveManager;
    private readonly Bot _bot;
}