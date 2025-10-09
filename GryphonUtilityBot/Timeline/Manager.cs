using GoogleSheetsManager.Documents;
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
    public readonly Chat InputChannel;
    private readonly Chat _outputChannel;

    public Manager(Bot bot, Config config, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _config = config;

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdTimeline);
        _sheetInput = document.GetOrAddSheet(_config.GoogleTitleTimelineInput);
        _sheetInputStreamlined = document.GetOrAddSheet(_config.GoogleTitleTimelineInputStreamlined);
        _sheetOutput = document.GetOrAddSheet(_config.GoogleTitleTimelineOutput);
        InputChannel = new Chat
        {
            Id = _config.TimelineInputChannelId,
            Type = ChatType.Channel
        };
        _outputChannel = new Chat
        {
            Id = _config.TimelineOutputChannelId,
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
        Record record = new(id, date, groupId, authorId, replyToId);
        lock (_locker)
        {
            _currentRecords.Add(record);
            _recentlyAdded = true;
        }
    }

    public async Task UpdateOutputChannelAsync()
    {
        List<Record> inputRecords = await _sheetInput.LoadAsync<Record>(_config.GoogleRangeTimelineInput);
        inputRecords = StreamlineRecords(inputRecords);
        if (inputRecords.Count == 0)
        {
            return;
        }

        await _sheetInputStreamlined.SaveAsync(_config.GoogleRangeTimelineInput, inputRecords);

        List<IdLine> idMapLines = await _sheetOutput.LoadAsync<IdLine>(_config.GoogleRangeTimelineOutput);

        (int? deleteFrom, int? postFrom) = СorrelateRecords(inputRecords, idMapLines);

        if (deleteFrom.HasValue)
        {
            List<int> toDelete = idMapLines.Skip(deleteFrom.Value).Select(l => l.Id).ToList();
            await _bot.Core.UpdateSender.DeleteMessagesAsync(_outputChannel, toDelete);
            idMapLines.RemoveRange(deleteFrom.Value, toDelete.Count);
            await _sheetOutput.SaveAsync(_config.GoogleRangeTimelineOutput, idMapLines);
        }

        if (postFrom.HasValue)
        {
            List<Record> toPost = inputRecords.Skip(postFrom.Value).ToList();
            await SendNeededMessagesAsync(toPost);
        }
    }

    private static List<Record> StreamlineRecords(List<Record> records)
    {
        if (records.Count == 0)
        {
            return records;
        }

        List<Record> result = new();

        Record first = records.First();
        if (first.Date is null || first.AuthorId.HasValue)
        {
            throw new InvalidOperationException("The first record must contain a date and must not contain an author");
        }

        DateOnly date = first.Date.Value;
        string? group = null;
        HashSet<DateOnly> dates = new();
        foreach (Record record in records)
        {
            if (record.Date.HasValue)
            {
                date = record.Date.Value;
                group = null;
                if (dates.Contains(date))
                {
                    continue;
                }
                dates.Add(date);
            }

            record.Date = date;
            result.Add(record);

            if (record.AuthorId.HasValue)
            {
                group ??= record.Id.ToString();
                record.GroupId = group;
            }
            else
            {
                group = null;
            }
        }
        return result.OrderBy(r => r.Date).ThenBy(r => r.Id).ToList();
    }

    private static (int? DeleteFrom, int? PostFrom) СorrelateRecords(IReadOnlyList<Record> inputRecords,
        IReadOnlyList<IdLine> idMapLines)
    {
        int? deleteFrom = null;
        int? postFrom = null;
        for (int i = 0; i < inputRecords.Count; ++i)
        {
            if (idMapLines.Count <= i)
            {
                postFrom = i;
                break;
            }

            if (idMapLines[i].InputId != inputRecords[i].Id)
            {
                deleteFrom = i;
                postFrom = i;
                break;
            }
        }

        if (deleteFrom is null && (idMapLines.Count > inputRecords.Count))
        {
            deleteFrom = inputRecords.Count;
        }

        return (deleteFrom, postFrom);
    }

    private async Task SendNeededMessagesAsync(IReadOnlyList<Record> records)
    {
        foreach ((IList<int>? ids, bool copy) in Split(records))
        {
            IEnumerable<IdLine> newLines = await SendBatchAsync(ids, copy);
            await _sheetOutput.AddAsync(_config.GoogleRangeTimelineOutput, newLines);
        }
    }

    private static IEnumerable<(IList<int> Ids, bool Copy)> Split(IReadOnlyList<Record> records)
    {
        List<int> batch = new();
        Record previous = records.First();
        string? groupId = records.First().GroupId;
        bool copy = records.First().AuthorId is null;
        foreach (Record record in records)
        {
            if ((record.Id < previous.Id) || (record.GroupId != groupId))
            {
                yield return (new List<int>(batch), copy);
                batch.Clear();
                groupId = record.GroupId;
                copy = record.AuthorId is null;
            }

            batch.Add(record.Id);
            previous = record;
        }
        if (batch.Count > 0)
        {
            yield return (batch, copy);
        }
    }

    private async Task<IEnumerable<IdLine>> SendBatchAsync(IList<int> batch, bool copy)
    {
        if (batch.Count == 0)
        {
            return Enumerable.Empty<IdLine>();
        }

        MessageId[] ids = copy
            ? await _bot.Core.UpdateSender.CopyMessagesAsync(_outputChannel, InputChannel, batch)
            : await _bot.Core.UpdateSender.ForwardMessagesAsync(_outputChannel, InputChannel, batch);

        List<int> newIds = ids.Select(id => id.Id).Order().ToList();
        return newIds.Count == batch.Count
            ? batch.Zip(newIds, (inputId, id) => new IdLine(inputId, id))
            : throw new InvalidOperationException("Sent message count does not match the batch count!");
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        List<Record>? updates;

        lock (_locker)
        {
            if (_recentlyAdded)
            {
                _recentlyAdded = false;
                return;
            }

            if (_currentRecords.Count == 0)
            {
                return;
            }

            updates = new List<Record>(_currentRecords);
            _currentRecords.Clear();
        }

        await _sheetInput.AddAsync(_config.GoogleRangeTimelineInput, updates);
        await UpdateOutputChannelAsync();
    }

    private bool _recentlyAdded;

    private readonly object _locker = new();

    private readonly SortedSet<Record> _currentRecords = new();

    private readonly Invoker _invoker;

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly Sheet _sheetInput;
    private readonly Sheet _sheetInputStreamlined;
    private readonly Sheet _sheetOutput;
}