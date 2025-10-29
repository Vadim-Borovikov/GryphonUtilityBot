using Google.Apis.Calendar.v3.Data;
using GryphonUtilities;
using GryphonUtilities.Time;
using GryphonUtilityBot.Web.Models.Calendar.Notion;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Synchronizer : BackgroundService, IUpdatesSubscriber
{
    public Synchronizer(Config config, Provider notionProvider, GoogleCalendarProvider googleCalendarProvider,
        Logger logger)
        : this(config.RelevantProperties, config.NotionDatabaseId, notionProvider, googleCalendarProvider, logger)
    { }

    private Synchronizer(IEnumerable<string> relevantProperties, string releventParentId, Provider notionProvider,
        GoogleCalendarProvider googleCalendarProvider, Logger logger)
    {
        _relevantPropertiyNames = new HashSet<string>(relevantProperties);
        _releventParentId = releventParentId;
        _notionProvider = notionProvider;
        _googleCalendarProvider = googleCalendarProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Dictionary<string, string> allIds =
            await _notionProvider.TryGetDatabasePropertyIdsAsync(_releventParentId)
            ?? throw new Exception($"Failed to acquire property ids from database \"{_releventParentId}\".");
        foreach (string name in _relevantPropertiyNames)
        {
            if (allIds.ContainsKey(name))
            {
                string id = allIds[name];
                _relevantProperties[id] = name;
            }
            else
            {
                _logger.LogError($"Property \"{name}\" not found in database \"{_releventParentId}\".");
            }
        }
    }

    public Task OnCreatedAsync(string id) => OnCreatedAsync(id, WebhookEvent.EventType.Created);

    public async Task OnPropertiesUpdatedAsync(string id, IEnumerable<string> properties)
    {
        List<string> names = properties.Where(_relevantProperties.ContainsKey)
                                       .Select(i => _relevantProperties[i])
                                       .ToList();

        if (names.Count == 0)
        {
            return;
        }

        PageInfo page = await GetPageInfoAsync(id);

        LogPageInfo(page, WebhookEvent.EventType.PropertiesUpdated, names);

        if (page.IsRelevantMeeting())
        {
            (DateTimeFull Start, DateTimeFull End) dates = page.Dates!.Value;

            Event? calendarEvent = null;
            if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
            {
                calendarEvent = await _googleCalendarProvider.GetEventAsync(page.GoogleEventId);
            }

            if (calendarEvent is null)
            {
                await CreateEventAndUpdatePageAsync(page, dates);
            }
            else
            {
                await UpdateEventAsync(calendarEvent, page, dates);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
            {
                await _googleCalendarProvider.DeleteEventAsync(page.GoogleEventId);
                await ClearPageAsync(page);
            }
        }
    }

    public Task OnMovedAsync(string id, string newParentId)
    {
        return newParentId.Equals(_releventParentId, StringComparison.OrdinalIgnoreCase)
            ? OnCreatedAsync(id, WebhookEvent.EventType.Moved)
            : OnDeletedAsync(id, WebhookEvent.EventType.Moved);
    }

    public Task OnDeletedAsync(string id) => OnDeletedAsync(id, WebhookEvent.EventType.Deleted);

    public Task OnUndeletedAsync(string id) => OnCreatedAsync(id, WebhookEvent.EventType.Undeleted);

    private async Task OnDeletedAsync(string id, WebhookEvent.EventType eventType)
    {
        PageInfo page = await GetPageInfoAsync(id);
        LogPageInfo(page, eventType);
        if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
        {
            await _googleCalendarProvider.DeleteEventAsync(page.GoogleEventId);
        }
    }

    private void LogPageInfo(PageInfo page, WebhookEvent.EventType eventType,
        IEnumerable<string>? propertyNames = null)
    {
        string message = $"Page \"{page.Title}\": {eventType}. {page.Page.Url}.";
        if (propertyNames is not null)
        {
            message += $" Properties: {string.Join(", ", propertyNames)}.";
        }
        _logger.LogTimedMessage(message);
    }

    private async Task OnCreatedAsync(string id, WebhookEvent.EventType eventType)
    {
        PageInfo page = await GetPageInfoAsync(id);
        LogPageInfo(page, eventType);
        if (page.IsRelevantMeeting())
        {
            await CreateEventAndUpdatePageAsync(page, page.Dates!.Value);
        }
    }

    private async Task CreateEventAndUpdatePageAsync(PageInfo page, (DateTimeFull Start, DateTimeFull End) dates)
    {
        _logger.LogTimedMessage($"Creating event for page \"{page.Title}\"...");
        Event calendarEvent = await _googleCalendarProvider.CreateEventAsync(page.Title, dates.Start, dates.End,
            page.Page.Url, page.Link?.ToString());

        _logger.LogTimedMessage($"Updating page \"{page.Title}\" with data from event \"{calendarEvent.Id}\"...");
        Uri uri = new(calendarEvent.HtmlLink);
        bool updated = await _notionProvider.TryUpdateEventDataAsync(page, calendarEvent.Id, uri);
        if (!updated)
        {
            _logger.LogError($"Failed to update page \"{page.Title}\" with event data due to conflicts.");
        }
    }

    private Task UpdateEventAsync(Event calendarEvent, PageInfo page, (DateTimeFull Start, DateTimeFull End) dates)
    {
        _logger.LogTimedMessage($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".");
        return _googleCalendarProvider.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, dates.Start,
            dates.End, page.Page.Url, page.Link?.ToString());
    }

    private async Task ClearPageAsync(PageInfo page)
    {
        bool cleared = await _notionProvider.TryClearEventDataAsync(page);
        if (!cleared)
        {
            _logger.LogError($"Failed to clear page \"{page.Title}\" event data due to conflicts.");
        }
    }

    private async Task<PageInfo> GetPageInfoAsync(string id)
    {
        RequestResult<PageInfo> result = await _notionProvider.TryGetPageAsync(id);
        return result.Successfull && result.Instance is not null
            ? result.Instance
            : throw new Exception($"Failed to acquire page \"{id}\".");
    }

    private readonly Dictionary<string, string> _relevantProperties = new();
    private readonly HashSet<string> _relevantPropertiyNames;
    private readonly string _releventParentId;
    private readonly Provider _notionProvider;
    private readonly GoogleCalendarProvider _googleCalendarProvider;
    private readonly Logger _logger;
}