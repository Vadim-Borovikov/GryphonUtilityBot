using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Hosting;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Service : IHostedService, IDisposable
{
    public Service(NotionHelper notionHelper, GoogleCalendarHelper googleCalendarHelper, Config config)
    {
        _notionHelper = notionHelper;
        _googleCalendarHelper = googleCalendarHelper;
        _config = config;
        _saveManager = new SaveManager<Data>(config.SavePath);
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
        DateTime now = DateTime.UtcNow;

        _saveManager.Load();

        await ProcessOutdatedAndDeletedPages(now);
        await ApplyUpdatesAsync();

        _saveManager.Data.LastUpdated = now;
        _saveManager.Save();
    }

    private async Task ProcessOutdatedAndDeletedPages(DateTime now)
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
                PageInfo? page = await _notionHelper.GetPage(id);
                if (page?.Dates is null || page.IsDeleted)
                {
                    if (!string.IsNullOrWhiteSpace(page?.GoogleEventId))
                    {
                        Event? calendarEvent = await GetEventAsync(page);
                        if (calendarEvent is not null)
                        {
                            await DeleteEventAsync(calendarEvent, page);
                        }
                    }
                    toRemove.Add(id);
                }
                if (page is { IsDeleted: false, Dates: null })
                {
                    await ClearPageAsync(page);
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
        _saveManager.Data.LastUpdated ??= _config.NotionStartWatchingDate.ToUniversalTime();
        List<PageInfo> pages = await _notionHelper.GetPages(_saveManager.Data.LastUpdated.Value);
        foreach (PageInfo page in pages)
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

    private Task<Event> CreateEventAsync(PageInfo page, DateTime start, DateTime end)
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

    private async Task UpdateEventAsync(Event calendarEvent, PageInfo page, DateTime start, DateTime end)
    {
        Utils.LogManager.LogTimedMessage($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".");
        await _googleCalendarHelper.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, start, end,
            page.Page.Url);
        _saveManager.Data.Meetings[page.Page.Id] = end.ToUniversalTime();
    }

    private async Task UpdatePageAsync(PageInfo page, Event calendarEvent, DateTime end)
    {
        Utils.LogManager.LogTimedMessage($"Updating page \"{page.Title}\" for event \"{calendarEvent.Id}\"");
        await _notionHelper.UpdateAsync(page, calendarEvent.Id, new Uri(calendarEvent.HtmlLink));
        _saveManager.Data.Meetings[page.Page.Id] = end.ToUniversalTime();
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
}