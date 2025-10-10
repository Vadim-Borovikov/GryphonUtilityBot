using AbstractBot.Interfaces.Modules;
using AbstractBot.Models;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using GryphonUtilities;
using GryphonUtilityBot.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Timeline;

internal sealed class Manager : IDisposable
{
    public readonly Chat Channel;

    public Manager(Bot bot, Config config, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _config = config;

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdTimeline);
        _sheetInput = document.GetOrAddSheet(_config.GoogleTitleTimelineInput);
        _sheetStreamlined = document.GetOrAddSheet(_config.GoogleTitleTimelineStreamlined);
        Channel = new Chat
        {
            Id = _config.TimelineChannelId,
            Type = ChatType.Channel
        };

        TimeSpan interval = TimeSpan.FromSeconds(config.TimelineWriteIntervalSeconds);
        _invoker = new Invoker(bot.Core.Logging.Logger);
        _invoker.DoPeriodically(TickAsync, interval, false);
    }

    public void Dispose() => _invoker.Dispose();

    public void AddRecord(int id, DateOnly? date = null, string? groupId = null, long? authorId = null,
        int? replyToId = null)
    {
        RecordInput record = new(id, date, groupId, authorId, replyToId);
        lock (_locker)
        {
            _inputRecords.Add(record);
            _recentlyAdded = true;
        }
    }

    public async Task UpdateChannelAsync(Chat chat, User sender, ITextsProvider<Texts> textsProvider)
    {
        Texts texts = textsProvider.GetTextsFor(sender.Id);

        List<RecordInput> input = await _sheetInput.LoadAsync<RecordInput>(_config.GoogleRangeTimeline);
        if (input.Count == 0)
        {
            await texts.NoTimelineUpdates.SendAsync(_bot.Core.UpdateSender, chat);
            return;
        }

        await using (await StatusMessage.CreateAsync(_bot.Core.UpdateSender, chat, texts.UpdatingTimeline,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat))
        {
            List<RecordStreamlined> streamlined =
                await _sheetStreamlined.LoadAsync<RecordStreamlined>(_config.GoogleRangeTimeline);

            StreamlineRecords(input, streamlined);

            if (streamlined.Count > 0)
            {
                await _sheetStreamlined.SaveAsync(_config.GoogleRangeTimeline, streamlined);

                int? moveFrom = null;
                for (int i = 0; i < (streamlined.Count - 1); ++i)
                {
                    if (streamlined[i].Id < streamlined[i + 1].Id)
                    {
                        continue;
                    }

                    moveFrom = 0;
                    for (int j = i - 1; j >= 0; --j)
                    {
                        if (streamlined[j].Id < streamlined[i + 1].Id)
                        {
                            moveFrom = j + 1;
                            break;
                        }
                    }
                    break;
                }

                if (moveFrom.HasValue)
                {
                    await MoveMessages(streamlined, moveFrom.Value);
                }
            }

            if (input.Count > 0)
            {
                await _sheetInput.ClearAsync(_config.GoogleRangeTimelineClear);
            }
        }
    }

    private async Task MoveMessages(List<RecordStreamlined> streamlined, int moveFrom)
    {
        List<RecordStreamlined> toMove = streamlined.Skip(moveFrom).ToList();
        foreach ((IList<RecordStreamlined> batch, bool copy) in Split(toMove))
        {
            IList<int> oldIds = GetIdsList(batch);

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

                RecordStreamlined newRecord =
                    new(oldRecord, oldRecord.Date, groupId, oldToNew[oldRecord.Id], replyToId);
                newRecords.Add(newRecord);
            }
            streamlined.AddRange(newRecords);
            await _sheetStreamlined.AddAsync(_config.GoogleRangeTimeline, newRecords);
        }

        await _bot.Core.UpdateSender.DeleteMessagesAsync(Channel, GetIdsList(toMove));
        streamlined.RemoveRange(moveFrom, toMove.Count);
        await _sheetStreamlined.SaveAsync(_config.GoogleRangeTimeline, streamlined);
    }

    private static IList<int> GetIdsList(IEnumerable<Record> records) => records.Select(r => r.Id).ToList();

    private static void StreamlineRecords(List<RecordInput> input, List<RecordStreamlined> streamlined)
    {
        if (input.Count == 0)
        {
            return;
        }

        HashSet<DateOnly> dates = new(streamlined.Select(r => r.Date));
        DateOnly date = GetDate(dates, input.First());

        string? groupId = null;

        foreach (RecordInput data in input)
        {
            if (data.TextDate.HasValue)
            {
                date = data.TextDate.Value;
                groupId = null;
                if (dates.Contains(date))
                {
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

    private bool _recentlyAdded;

    private readonly object _locker = new();

    private readonly SortedSet<RecordInput> _inputRecords = new();

    private readonly Invoker _invoker;

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly Sheet _sheetInput;
    private readonly Sheet _sheetStreamlined;
}