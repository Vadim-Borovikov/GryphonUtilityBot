using GoogleSheetsManager.Documents;
using GryphonUtilityBot.Configs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GryphonUtilities;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Timeline;

internal sealed class Manager : IDisposable
{
    public readonly Chat InputChannel;

    public Manager(Bot bot, Config config, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _config = config;

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdTimeline);
        _sheet = document.GetOrAddSheet(_config.GoogleTitleTimeline);
        InputChannel = new Chat
        {
            Id = _config.TimelineInputChannelId,
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

    private Task TickAsync(CancellationToken cancellationToken)
    {
        List<Record>? updates;

        lock (_locker)
        {
            if (_recentlyAdded)
            {
                _recentlyAdded = false;
                return Task.CompletedTask;
            }

            if (_currentRecords.Count == 0)
            {
                return Task.CompletedTask;
            }

            updates = new List<Record>(_currentRecords);
            _currentRecords.Clear();
        }

        return _sheet.AddAsync(_config.GoogleRangeTimeline, updates);
    }

    private bool _recentlyAdded;

    private readonly object _locker = new();

    private readonly SortedSet<Record> _currentRecords = new();

    private readonly Invoker _invoker;

    private readonly Config _config;
    private readonly Sheet _sheet;
}