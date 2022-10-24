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
                PageInfo page = await _notionHelper.GetPage(id);
                if (!page.IsDeleted)
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
                {
                    await DeleteEventAsync(page);
                    await ClearPageAsync(page);
                }
                toRemove.Add(id);
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
            if (page.Date.Start is null || page.Date.End is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(page.GoogleEventId))
            {
                if (page.IsCancelled)
                {
                    continue;
                }

                Event calendarEvent = await CreateEventAsync(page, page.Date.Start.Value, page.Date.End.Value);
                await UpdatePageAsync(page, calendarEvent);
                _saveManager.Data.Meetings[page.Page.Id] = page.Date.End.Value.ToUniversalTime();
            }
            else if (page.IsCancelled)
            {
                await DeleteEventAsync(page);
                await ClearPageAsync(page);
                _saveManager.Data.Meetings.Remove(page.Page.Id);
            }
            else
            {
                await UpdateEventAsync(page, page.Date.Start.Value, page.Date.End.Value);
                _saveManager.Data.Meetings[page.Page.Id] = page.Date.End.Value.ToUniversalTime();
            }
        }
    }

    private Task<Event> CreateEventAsync(PageInfo page, DateTime start, DateTime end)
    {
        Utils.LogManager.LogTimedMessage($"Creating event for page \"{page.Title}\".");
        return _googleCalendarHelper.CreateEventAsync(page.Title, start, end, page.Page.Url);
    }

    private async Task UpdateEventAsync(PageInfo page, DateTime start, DateTime end)
    {
        Event calendarEvent = await _googleCalendarHelper.GetEventAsync(page.GoogleEventId);
        Utils.LogManager.LogTimedMessage($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".");
        await _googleCalendarHelper.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, start, end,
            page.Page.Url);
    }

    private Task UpdatePageAsync(PageInfo page, Event calendarEvent)
    {
        Utils.LogManager.LogTimedMessage($"Updating page \"{page.Title}\" for event \"{calendarEvent.Id}\"");
        return _notionHelper.UpdateAsync(page.Page.Id, calendarEvent.Id, new Uri(calendarEvent.HtmlLink));
    }

    private Task ClearPageAsync(PageInfo page)
    {
        Utils.LogManager.LogTimedMessage($"Clearing page \"{page.Title}\" of event \"{page.GoogleEventId}\"");
        return _notionHelper.UpdateAsync(page.Page.Id, null, null);
    }

    private Task DeleteEventAsync(PageInfo page)
    {
        Utils.LogManager.LogTimedMessage($"Deleting event \"{page.GoogleEventId}\" for page \"{page.Title}\".");
        return _googleCalendarHelper.DeleteEventAsync(page.GoogleEventId);
    }

    private readonly NotionHelper _notionHelper;
    private readonly GoogleCalendarHelper _googleCalendarHelper;
    private readonly Config _config;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SaveManager<Data> _saveManager;
}