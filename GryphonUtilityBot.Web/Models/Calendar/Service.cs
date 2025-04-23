using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using GryphonUtilities;
using GryphonUtilities.Time;
using GryphonUtilityBot.Web.Models.Calendar.Notion;
using Microsoft.Extensions.Hosting;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Service : IHostedService, IDisposable
{
    public Service(Provider notionProvider, GoogleCalendarProvider googleCalendarProvider, Config config,
        BotSingleton botSingleton)
    {
        _notionProvider = notionProvider;
        _googleCalendarProvider = googleCalendarProvider;
        _config = config;
        _clock = botSingleton.Bot.Clock;
        _saveManager = new SaveManager<Data>(config.SavePath, _clock);
        _logger = botSingleton.Bot.Logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimeSpan interval = TimeSpan.FromMinutes(1.0 / _config.NotionPollsPerMinute);
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Invoker.DoPeriodically(TickAsync, interval, true, _logger, _cancellationTokenSource.Token);
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
        _logger.LogTimedMessage("Calendar sync tick");
        DateTimeFull now = DateTimeFull.CreateUtcNow();

        _saveManager.Load();

        await ProcessOutdatedAndDeletedPages(now);
        await ApplyUpdatesAsync();

        _saveManager.SaveData.LastUpdated = now;
        _saveManager.Save();
    }

    private async Task ProcessOutdatedAndDeletedPages(DateTimeFull now)
    {
        List<string> toRemove = new();
        foreach (string id in _saveManager.SaveData.Meetings.Keys)
        {
            if (_saveManager.SaveData.Meetings[id] < now)
            {
                toRemove.Add(id);
            }
            else
            {
                RequestResult<PageInfo> result = await _notionProvider.TryGetPageAsync(id);
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
            _saveManager.SaveData.Meetings.Remove(id);
        }
    }

    private async Task ApplyUpdatesAsync()
    {
        _saveManager.SaveData.LastUpdated ??=
            _clock.GetDateTimeFull(_config.NotionStartWatchingDate, TimeOnly.MinValue);
        RequestResult<List<PageInfo>> result =
            await _notionProvider.TryGetPagesAsync(_saveManager.SaveData.LastUpdated.Value);

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
        _logger.LogTimedMessage($"Creating event for page \"{page.Title}\".");
        return _googleCalendarProvider.CreateEventAsync(page.Title, start, end, page.Page.Url);
    }

    private async Task<Event?> GetEventAsync(PageInfo page)
    {
        _logger.LogTimedMessage($"Acquiring event for page \"{page.Title}\".");
        return string.IsNullOrWhiteSpace(page.GoogleEventId)
            ? null
            : await _googleCalendarProvider.GetEventAsync(page.GoogleEventId);
    }

    private async Task UpdateEventAsync(Event calendarEvent, PageInfo page, DateTimeFull start, DateTimeFull end)
    {
        _logger.LogTimedMessage($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".");
        await _googleCalendarProvider.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, start, end,
            page.Page.Url);
        _saveManager.SaveData.Meetings[page.Page.Id] = end;
    }

    private async Task UpdatePageAsync(PageInfo page, Event calendarEvent, DateTimeFull end)
    {
        _logger.LogTimedMessage($"Updating page \"{page.Title}\" for event \"{calendarEvent.Id}\"");
        await _notionProvider.UpdateAsync(page, calendarEvent.Id, new Uri(calendarEvent.HtmlLink));
        _saveManager.SaveData.Meetings[page.Page.Id] = end;
    }

    private async Task ClearPageAsync(PageInfo page)
    {
        _logger.LogTimedMessage($"Clearing page \"{page.Title}\" of event \"{page.GoogleEventId}\"");
        await _notionProvider.ClearAsync(page);
        _saveManager.SaveData.Meetings.Remove(page.Page.Id);
    }

    private async Task DeleteEventAsync(Event calendarEvent, PageInfo page)
    {
        _logger.LogTimedMessage($"Deleting event \"{page.GoogleEventId}\" for page \"{page.Title}\".");
        await _googleCalendarProvider.DeleteEventAsync(calendarEvent);
    }

    private readonly Provider _notionProvider;
    private readonly GoogleCalendarProvider _googleCalendarProvider;
    private readonly Config _config;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SaveManager<Data> _saveManager;
    private readonly Clock _clock;
    private readonly Logger _logger;
}