using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using Google.Apis.Calendar.v3.Data;
using GryphonUtilities;
using Microsoft.Extensions.Hosting;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Service : IHostedService, IDisposable
{
    public Service(NotionHelper notionHelper, GoogleCalendarHelper googleCalendarHelper, Config config,
        BotSingleton botSingleton)
    {
        _notionHelper = notionHelper;
        _googleCalendarHelper = googleCalendarHelper;
        _config = config;
        _timeManager = botSingleton.Bot.TimeManager;
        _saveManager = new SaveManager<Data>(config.SavePath, _timeManager);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimeSpan interval = TimeSpan.FromMinutes(1.0 / _config.NotionPollsPerMinute);
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Invoker.DoPeriodically(TickAsync, interval, true, _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        Utils.LogManager.LogTimedMessage("Calendar sync tick");
        DateTimeFull now = DateTimeFull.CreateUtcNow();

        _saveManager.Load();

        await ProcessOutdatedAndDeletedPages(now);
        await ApplyUpdatesAsync();

        _saveManager.Data.LastUpdated = now;
        _saveManager.Save();
    }

    private async Task ProcessOutdatedAndDeletedPages(DateTimeFull now)
    {
        List<string> toRemove = new();
        foreach (string id in _saveManager.Data.Meetings.Keys)
        {
            if (_saveManager.Data.Meetings[id] < now)
            {
                toRemove.Add(id);
            }
            else
            {
                NotionRequestResult<PageInfo> result = await _notionHelper.TryGetPageAsync(id);
                if (!result.Successfull)
                {
                    continue;
                }

                PageInfo? info = result.Instance;
                if (info?.Dates is null || info.IsDeleted)
                {
                    if (!string.IsNullOrWhiteSpace(info?.GoogleEventId))
                    {
                        Event? calendarEvent = await GetEventAsync(info);
                        if (calendarEvent is not null)
                        {
                            await DeleteEventAsync(calendarEvent, info);
                        }
                    }
                    toRemove.Add(id);
                }
                if (info is { IsDeleted: false, Dates: null })
                {
                    await ClearPageAsync(info);
                }
            }
        }
        foreach (string id in toRemove)
        {
            _saveManager.Data.Meetings.Remove(id);
        }
    }

    private async Task ApplyUpdatesAsync()
    {
        _saveManager.Data.LastUpdated ??=
            _timeManager.GetDateTimeFull(_config.NotionStartWatchingDate, TimeOnly.MinValue);
        NotionRequestResult<List<PageInfo>> result =
            await _notionHelper.TryGetPagesAsync(_saveManager.Data.LastUpdated.Value);

        if (result.Instance is null)
        {
            return;
        }

        foreach (PageInfo page in result.Instance)
        {
            if (page.Dates is null)
            {
                continue;
            }

            Event? calendarEvent = await GetEventAsync(page);
            if (calendarEvent is null)
            {
                if (page.IsCancelled)
                {
                    await ClearPageAsync(page);
                }
                else
                {
                    calendarEvent = await CreateEventAsync(page, page.Dates.Value.Start, page.Dates.Value.End);
                    await UpdatePageAsync(page, calendarEvent, page.Dates.Value.End);
                }
            }
            else if (page.IsCancelled)
            {
                await DeleteEventAsync(calendarEvent, page);
                await ClearPageAsync(page);
            }
            else
            {
                await UpdateEventAsync(calendarEvent, page, page.Dates.Value.Start, page.Dates.Value.End);
            }
        }
    }

    private Task<Event> CreateEventAsync(PageInfo page, DateTimeFull start, DateTimeFull end)
    {
        Utils.LogManager.LogTimedMessage($"Creating event for page \"{page.Title}\".");
        return _googleCalendarHelper.CreateEventAsync(page.Title, start, end, page.Page.Url);
    }

    private async Task<Event?> GetEventAsync(PageInfo page)
    {
        Utils.LogManager.LogTimedMessage($"Acquiring event for page \"{page.Title}\".");
        return string.IsNullOrWhiteSpace(page.GoogleEventId)
            ? null
            : await _googleCalendarHelper.GetEventAsync(page.GoogleEventId);
    }

    private async Task UpdateEventAsync(Event calendarEvent, PageInfo page, DateTimeFull start, DateTimeFull end)
    {
        Utils.LogManager.LogTimedMessage($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".");
        await _googleCalendarHelper.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, start, end,
            page.Page.Url);
        _saveManager.Data.Meetings[page.Page.Id] = end;
    }

    private async Task UpdatePageAsync(PageInfo page, Event calendarEvent, DateTimeFull end)
    {
        Utils.LogManager.LogTimedMessage($"Updating page \"{page.Title}\" for event \"{calendarEvent.Id}\"");
        await _notionHelper.UpdateAsync(page, calendarEvent.Id, new Uri(calendarEvent.HtmlLink));
        _saveManager.Data.Meetings[page.Page.Id] = end;
    }

    private async Task ClearPageAsync(PageInfo page)
    {
        Utils.LogManager.LogTimedMessage($"Clearing page \"{page.Title}\" of event \"{page.GoogleEventId}\"");
        await _notionHelper.ClearAsync(page);
        _saveManager.Data.Meetings.Remove(page.Page.Id);
    }

    private async Task DeleteEventAsync(Event calendarEvent, PageInfo page)
    {
        Utils.LogManager.LogTimedMessage($"Deleting event \"{page.GoogleEventId}\" for page \"{page.Title}\".");
        await _googleCalendarHelper.DeleteEventAsync(calendarEvent);
    }

    private readonly NotionHelper _notionHelper;
    private readonly GoogleCalendarHelper _googleCalendarHelper;
    private readonly Config _config;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SaveManager<Data> _saveManager;
    private readonly TimeManager _timeManager;
}