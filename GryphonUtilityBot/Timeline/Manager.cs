using AbstractBot.Interfaces.Modules;
using AbstractBot.Models;
using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using GryphonUtilities;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GryphonUtilities.Time;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Timeline;

internal sealed class Manager : IDisposable
{
    public readonly Chat Channel;

    public Manager(Bot bot, Config config, ITextsProvider<Texts> textsProvider,
        GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _config = config;
        _textsProvider = textsProvider;

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdTimeline);
        _sheetInput = document.GetOrAddSheet(_config.GoogleTitleTimelineInput);
        _sheetStreamlined = document.GetOrAddSheet(_config.GoogleTitleTimelineStreamlined);
        Channel = new Chat
        {
            Id = _config.TimelineChannelId,
            Type = ChatType.Channel
        };
        _channelLinksId = Channel.Id.ToString().Remove(0, _config.TimelineChannelIdPrefix.Length);

        TimeSpan interval = TimeSpan.FromSeconds(config.TimelineWriteIntervalSeconds);
        _invoker = new Invoker(bot.Core.Logging.Logger);
        _invoker.DoPeriodically(TickAsync, interval, false);

        _deletionWindow = TimeSpan.FromDays(config.TimelinePostsDeletionAvailabilityDays)
                          - TimeSpan.FromSeconds(config.TimelinePostsDeletionThresholdSeconds);
    }

    public void Dispose() => _invoker.Dispose();

    public void AddRecord(int id, DateTimeFull added, DateOnly? date = null, string? groupId = null,
        long? authorId = null, int? replyToId = null)
    {
        RecordInput record = new(id, added, date, groupId, authorId, replyToId);
        lock (_locker)
        {
            _inputRecords.Add(record);
            _recentlyAdded = true;
        }
    }

    public async Task UpdateChannelAsync(Chat chat, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender.Id);

        List<RecordInput> input = await _sheetInput.LoadAsync<RecordInput>(_config.GoogleRangeTimeline);
        if (input.Count == 0)
        {
            await texts.NoTimelineUpdates.SendAsync(_bot.Core.UpdateSender, chat);
            return;
        }

        StrongBox<MessageTemplateText> prefixBox = new(texts.TimelineUpdatedFormat);
        await using (await StatusMessage.CreateAsync(_bot.Core.UpdateSender, chat, texts.UpdatingTimeline,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat, () => prefixBox.Value!))
        {
            List<RecordStreamlined> streamlined =
                await _sheetStreamlined.LoadAsync<RecordStreamlined>(_config.GoogleRangeTimeline);

            IList<int> excessInput = StreamlineRecords(input, streamlined);

            if (streamlined.Count > 0)
            {
                if (excessInput.Count > 0)
                {
                    await _bot.Core.UpdateSender.DeleteMessagesAsync(Channel, excessInput);
                }

                await _sheetStreamlined.SaveAsync(_config.GoogleRangeTimeline, streamlined);

                int? moveFrom = FindIndexToMoveFrom(streamlined);
                if (moveFrom.HasValue)
                {
                    List<RecordStreamlined> toMove = streamlined.Skip(moveFrom.Value).ToList();
                    IList<int> newIds = await RepostMessages(streamlined, toMove);
                    await SendAlmostUpdatedMessageAsync(chat, texts, moveFrom.Value, GetIdsList(toMove), newIds);
                }
            }

            await _sheetInput.ClearAsync(_config.GoogleRangeTimelineClear);
        }
    }

    private Task SendAlmostUpdatedMessageAsync(Chat chat, Texts texts, int deleteFrom, ICollection<int> oldIds,
        ICollection<int> newIds)
    {
        MessageTemplateText firstNew = texts.TimelineMessageHypertextFormat.Format(newIds.Min(), _channelLinksId);

        MessageTemplateText deleteFromMessage =
            texts.TimelineMessageHypertextFormat.Format(oldIds.Min(), _channelLinksId);
        MessageTemplateText deleteToMessage =
            texts.TimelineMessageHypertextFormat.Format(oldIds.Max(), _channelLinksId);

        MessageTemplateText almostUpdated =
            texts.ConfirmTimelineDuplicatesDeletionFormat.Format(newIds.Count, firstNew, oldIds.Count,
                deleteFromMessage, deleteToMessage);

        almostUpdated.KeyboardProvider = CreateConfirmationKeyboard(texts, deleteFrom, oldIds.Count);

        return almostUpdated.SendAsync(_bot.Core.UpdateSender, chat);
    }

    private Task SendManualDeletionRequiredMessageAsync(Chat chat, Texts texts, ICollection<int> ids)
    {
        MessageTemplateText deleteFromMessage =
            texts.TimelineMessageHypertextFormat.Format(ids.Min(), _channelLinksId);
        MessageTemplateText deleteToMessage =
            texts.TimelineMessageHypertextFormat.Format(ids.Max(), _channelLinksId);

        MessageTemplateText almostUpdated =
            texts.TimelineDuplicatesRequireManualDeletion.Format(ids.Count, deleteFromMessage, deleteToMessage);

        return almostUpdated.SendAsync(_bot.Core.UpdateSender, chat);
    }

    public async Task TryToDeleteOldTimelinePart(Chat chat, User sender, int deleteFrom, int deleteAmount)
    {
        List<RecordStreamlined> streamlined =
            await _sheetStreamlined.LoadAsync<RecordStreamlined>(_config.GoogleRangeTimeline);

        DateTimeFull deletionLimit = DateTimeFull.CreateUtcNow() - _deletionWindow;

        Dictionary<bool, IList<int>> grouped = streamlined.Skip(deleteFrom)
                                                          .Take(deleteAmount)
                                                          .GroupBy(r => r.Added >= deletionLimit)
                                                          .ToDictionary(g => g.Key, GetIdsList);

        if (grouped.ContainsKey(true))
        {
            IList<int> deleteAutomaticly = grouped[true];
            await _bot.Core.UpdateSender.DeleteMessagesAsync(Channel, deleteAutomaticly);
        }

        streamlined.RemoveRange(deleteFrom, deleteAmount);
        await _sheetStreamlined.SaveAsync(_config.GoogleRangeTimeline, streamlined);

        Texts texts = _textsProvider.GetTextsFor(sender.Id);
        if (grouped.ContainsKey(false))
        {
            IList<int> deleteManually = grouped[false];
            await SendManualDeletionRequiredMessageAsync(chat, texts, deleteManually);
        }
        else
        {
            await texts.TimelineDuplicatesDeleted.SendAsync(_bot.Core.UpdateSender, chat);
        }
    }

    private static int? FindIndexToMoveFrom(IReadOnlyList<Record> records)
    {
        for (int i = 0; i < (records.Count - 1); ++i)
        {
            if (records[i].Id < records[i + 1].Id)
            {
                continue;
            }

            for (int j = i - 1; j >= 0; --j)
            {
                if (records[j].Id < records[i + 1].Id)
                {
                    return j + 1;
                }
            }
            return 0;
        }
        return null;
    }

    private async Task<IList<int>> RepostMessages(List<RecordStreamlined> streamlined,
        IReadOnlyList<RecordStreamlined> toMove)
    {
        List<int> newIds = new();
        foreach ((IList<RecordStreamlined> batch, bool copy) in Split(toMove))
        {
            IList<int> oldIds = GetIdsList(batch);

            DateTimeFull now = DateTimeFull.CreateUtcNow();
            Dictionary<int, int> oldToNew = await SendBatchAsync(oldIds, copy);
            List<RecordStreamlined> newRecords = new();
            foreach (RecordStreamlined oldRecord in batch)
            {
                string? groupId = oldRecord.GroupId;
                int? threadId = groupId?.ToInt();
                if (threadId.HasValue && oldToNew.ContainsKey(threadId.Value))
                {
                    groupId = oldToNew[threadId.Value].ToString();
                }

                int? replyToId = oldRecord.ReplyToId;
                if (replyToId.HasValue && oldToNew.ContainsKey(replyToId.Value))
                {
                    replyToId = oldToNew[replyToId.Value];
                }

                int id = oldToNew[oldRecord.Id];
                newIds.Add(id);
                RecordStreamlined newRecord = new(oldRecord, oldRecord.Date, groupId, id, now, replyToId);
                newRecords.Add(newRecord);
            }
            streamlined.AddRange(newRecords);
            await _sheetStreamlined.AddAsync(_config.GoogleRangeTimeline, newRecords);
        }
        return newIds;
    }

    private static IList<int> GetIdsList(IEnumerable<Record> records) => records.Select(r => r.Id).ToList();

    private static IList<int> StreamlineRecords(List<RecordInput> input, List<RecordStreamlined> streamlined)
    {
        if (input.Count == 0)
        {
            return Array.Empty<int>();
        }

        HashSet<DateOnly> dates = new(streamlined.Select(r => r.Date));
        DateOnly date = GetDate(dates, input.First());

        string? groupId = null;

        List<int> excessIds = new();

        foreach (RecordInput data in input)
        {
            if (data.TextDate.HasValue)
            {
                date = data.TextDate.Value;
                groupId = null;
                if (dates.Contains(date))
                {
                    excessIds.Add(data.Id);
                    continue;
                }
                dates.Add(date);
            }

            if (data.AuthorId.HasValue)
            {
                groupId ??= data.Id.ToString();
            }
            else
            {
                groupId = null;
            }
            RecordStreamlined record = new(data, date, groupId);
            streamlined.Add(record);
        }
        streamlined.Sort();

        return excessIds;
    }

    private static DateOnly GetDate(IReadOnlyCollection<DateOnly> dates, RecordInput first)
    {
        if (dates.Count > 0)
        {
            return dates.Max();
        }

        return first.TextDate is null || first.AuthorId.HasValue
            ? throw new InvalidOperationException("The first input record must be a date")
            : first.TextDate.Value;
    }

    private static IEnumerable<(IList<RecordStreamlined> Ids, bool Copy)> Split(IReadOnlyList<RecordStreamlined> records)
    {
        List<RecordStreamlined> batch = new();
        RecordStreamlined previous = records.First();
        string? groupId = records.First().GroupId;
        bool copy = records.First().AuthorId is null;
        foreach (RecordStreamlined record in records)
        {
            if ((record.Id < previous.Id) || (record.GroupId != groupId))
            {
                yield return (new List<RecordStreamlined>(batch), copy);
                batch.Clear();
                groupId = record.GroupId;
                copy = record.AuthorId is null;
            }

            batch.Add(record);
            previous = record;
        }
        if (batch.Count > 0)
        {
            yield return (batch, copy);
        }
    }

    private async Task<Dictionary<int, int>> SendBatchAsync(IList<int> batch, bool copy)
    {
        Dictionary<int, int> oldToNew = new();

        if (batch.Count == 0)
        {
            return oldToNew;
        }
        MessageId[] ids = copy
            ? await _bot.Core.UpdateSender.CopyMessagesAsync(Channel, Channel, batch)
            : await _bot.Core.UpdateSender.ForwardMessagesAsync(Channel, Channel, batch);

        if (ids.Length != batch.Count)
        {
            throw new InvalidOperationException("Sent message count does not match the batch count!");
        }

        for (int i = 0; i < ids.Length; ++i)
        {
            oldToNew[batch[i]] = ids[i].Id;
        }

        return oldToNew;
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        List<RecordInput>? updates;

        lock (_locker)
        {
            if (_recentlyAdded)
            {
                _recentlyAdded = false;
                return;
            }

            if (_inputRecords.Count == 0)
            {
                return;
            }

            updates = new List<RecordInput>(_inputRecords);
            _inputRecords.Clear();
        }

        await _sheetInput.AddAsync(_config.GoogleRangeTimeline, updates);
    }

    private static InlineKeyboardMarkup CreateConfirmationKeyboard(Texts texts, int deleteFrom, int deleteAmount)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<ConfirmTimelineDuplicatesDeletion>(texts.TimelineDuplicatesDeletionConfirmationButton, deleteFrom,
                deleteAmount)
        };
        return new InlineKeyboardMarkup(keyboard);
    }
    private static List<InlineKeyboardButton> CreateOneButtonRow<TData>(string caption, params object[] args)
    {
        return new List<InlineKeyboardButton> { CreateButton<TData>(caption, args) };
    }
    private static InlineKeyboardButton CreateButton<TCallback>(string caption, params object[] fields)
    {
        return new InlineKeyboardButton(caption)
        {
            CallbackData = typeof(TCallback).Name + string.Join(FieldSeparator, fields)
        };
    }

    public const string FieldSeparator = ";";

    private bool _recentlyAdded;

    private readonly object _locker = new();

    private readonly SortedSet<RecordInput> _inputRecords = new();

    private readonly Invoker _invoker;

    private readonly string _channelLinksId;

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly Sheet _sheetInput;
    private readonly Sheet _sheetStreamlined;
    private readonly TimeSpan _deletionWindow;
}