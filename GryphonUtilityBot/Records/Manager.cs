﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Records;

internal sealed class Manager
{
    public Manager(Bot bot, SaveManager<List<RecordData>> saveManager)
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
            _saveManager.Data.Add(record);
        }

        _saveManager.Save();

        return _bot.SendTextMessageAsync(message.Chat, "Запись добавлена.", replyToMessageId: message.MessageId);
    }

    public async Task ProcessFindQueryAsync(Chat chat, FindQuery query)
    {
        _saveManager.Load();

        List<RecordData> records = _saveManager.Data
                                               .Where(r => r.DateTime.Date >= query.From)
                                               .Where(r => r.DateTime.Date <= query.To)
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

        RecordData? record = _saveManager.Data.FirstOrDefault(r =>
            (r.ChatId == recordMessage.Chat.Id) && (r.MessageId == recordMessage.MessageId));

        if (record is null)
        {
            return _bot.SendTextMessageAsync(chat, "Я не нашёл нужной записи.");
        }

        if (query.DateTime.HasValue)
        {
            record.DateTime = query.DateTime.Value;
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

        DateTime dateTime = query?.DateTime ?? _bot.TimeManager.ToLocal(message.ForwardDate.Value);
        return new RecordData
        {
            MessageId = message.MessageId,
            ChatId = message.Chat.Id,
            DateTime = dateTime,
            Tags = query?.Tags ?? new HashSet<string>()
        };
    }

    private readonly SaveManager<List<RecordData>> _saveManager;
    private readonly Bot _bot;
}