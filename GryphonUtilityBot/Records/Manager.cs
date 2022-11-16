using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilities;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Records;

internal sealed class Manager
{
    public Manager(Bot bot, SaveManager<Data> saveManager)
    {
        _saveManager = saveManager;
        _bot = bot;
    }

    public Task SaveRecordAsync(Message message, MarkQuery? query)
    {
        _saveManager.Load();

        RecordData? record = GetRecord(message, query);
        if (record is not null)
        {
            _saveManager.Data.Records.Add(record);
        }

        _saveManager.Save();

        return _bot.SendTextMessageAsync(message.Chat, "Запись добавлена.", replyToMessageId: message.MessageId);
    }

    public async Task ProcessFindQueryAsync(Chat chat, FindQuery query)
    {
        _saveManager.Load();

        List<RecordData> records = _saveManager.Data
                                               .Records
                                               .Where(r => r.DateTime.DateOnly() >= query.From)
                                               .Where(r => r.DateTime.DateOnly() <= query.To)
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

    public Task MarkAsync(Chat chat, Message recordMessage, MarkQuery query)
    {
        _saveManager.Load();

        RecordData? record = _saveManager.Data.Records.FirstOrDefault(r =>
            (r.ChatId == recordMessage.Chat.Id) && (r.MessageId == recordMessage.MessageId));

        if (record is null)
        {
            return _bot.SendTextMessageAsync(chat, "Я не нашёл нужной записи.");
        }

        if (query.DateOnly.HasValue)
        {
            record.DateTime = DateTimeOffsetHelper.FromDateOnly(query.DateOnly.Value);
        }

        record.Tags = query.Tags;
        _saveManager.Save();
        return _bot.SendTextMessageAsync(chat, "Запись обновлена.");
    }

    private RecordData? GetRecord(Message message, MarkQuery? query)
    {
        if (!message.ForwardDate.HasValue)
        {
            return null;
        }

        DateTimeOffset dateTime = query?.DateOnly is null
            ? _bot.TimeManager.ToLocal(message.ForwardDate.Value)
            : DateTimeOffsetHelper.FromDateOnly(query.DateOnly.Value);
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