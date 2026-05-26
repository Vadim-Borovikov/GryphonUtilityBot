using Google.Apis.Calendar.v3.Data;
using GryphonUtilities.Logging;
using GryphonUtilities.Time;
using GryphonUtilityBot.Web.Models.Calendar.Notion;
using GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;
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
        _relevantPropertyNames = new HashSet<string>(relevantProperties);
        _relevantParentId = releventParentId;
        _notionProvider = notionProvider;
        _googleCalendarProvider = googleCalendarProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Dictionary<string, string> allIds =
            await _notionProvider.TryGetDatabasePropertyIdsAsync(_relevantParentId)
            ?? throw new Exception($"Failed to acquire property ids from database \"{_relevantParentId}\".");
        foreach (string name in _relevantPropertyNames)
        {
            if (allIds.ContainsKey(name))
            {
                string id = allIds[name];
                _relevantProperties[id] = name;
            }
            else
            {
                _logger.Errors.Log($"Property \"{name}\" not found in database \"{_relevantParentId}\".", true);
            }
        }
    }

    public Task ProcessAsync(Update update)
    {
        switch (update)
        {
            case SimpleUpdate simple:
                return simple.UpdateType switch
                {
                    SimpleUpdate.Type.Created => SyncPageAsync(simple.EntityId, WebhookEvent.EventType.Created, true),
                    SimpleUpdate.Type.Deleted => SyncPageAsync(simple.EntityId, WebhookEvent.EventType.Deleted, false),
                    SimpleUpdate.Type.Undeleted => SyncPageAsync(simple.EntityId, WebhookEvent.EventType.Undeleted, true),
                    _ => throw new ArgumentOutOfRangeException()
                };

            case PropertiesUpdatedUpdate properties:
                List<string> names = properties.Properties
                                               .Where(_relevantProperties.ContainsKey)
                                               .Select(i => _relevantProperties[i])
                                               .ToList();
                return names.Count == 0
                    ? Task.CompletedTask
                    : SyncPageAsync(properties.EntityId, WebhookEvent.EventType.PropertiesUpdated, true, names);

            case MovedUpdate moved:
                bool active = moved.NewParentId.Equals(_relevantParentId, StringComparison.OrdinalIgnoreCase);
                return SyncPageAsync(moved.EntityId, WebhookEvent.EventType.Moved, active);

            default: throw new ArgumentOutOfRangeException(nameof(update));
        }
    }

    private async Task SyncPageAsync(string id, WebhookEvent.EventType eventType, bool active,
        IEnumerable<string>? propertyNames = null)
    {
        PageInfo page = await GetPageInfoAsync(id);
        LogPageInfo(page, eventType, propertyNames);

        if (active && page.IsRelevantMeeting())
        {
            await UpsertEventAsync(page);
        }
        else if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
        {
            bool clearPage = eventType is not WebhookEvent.EventType.Deleted;
            await RemoveEventAsync(page, clearPage);
        }
    }

    private async Task UpsertEventAsync(PageInfo page)
    {
        (DateTimeFull Start, DateTimeFull End) dates = page.Dates!.Value;

        Event? calendarEvent = string.IsNullOrWhiteSpace(page.GoogleEventId)
            ? null
            : await _googleCalendarProvider.GetEventAsync(page.GoogleEventId);

        if (calendarEvent is null)
        {
            await CreateEventAndUpdatePageAsync(page, dates);
        }
        else
        {
            await UpdateEventAsync(calendarEvent, page, dates);
        }
    }

    private async Task RemoveEventAsync(PageInfo page, bool clearPage)
    {
        await _googleCalendarProvider.DeleteEventAsync(page.GoogleEventId);
        if (clearPage)
        {
            await ClearPageAsync(page);
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
        _logger.Messages.Log(message, true);
    }

    private async Task CreateEventAndUpdatePageAsync(PageInfo page, (DateTimeFull Start, DateTimeFull End) dates)
    {
        _logger.Messages.Log($"Creating event for page \"{page.Title}\"...", true);
        Event calendarEvent = await _googleCalendarProvider.CreateEventAsync(page.Title, dates.Start, dates.End,
            page.Page.Url, page.Link?.ToString());

        _logger.Messages.Log($"Updating page \"{page.Title}\" with data from event \"{calendarEvent.Id}\"...", true);
        Uri uri = new(calendarEvent.HtmlLink);
        bool updated = await _notionProvider.TryUpdateEventDataAsync(page, calendarEvent.Id, uri);
        if (!updated)
        {
            _logger.Errors.Log($"Failed to update page \"{page.Title}\" with event data due to conflicts.", true);
        }
    }

    private Task UpdateEventAsync(Event calendarEvent, PageInfo page, (DateTimeFull Start, DateTimeFull End) dates)
    {
        _logger.Messages.Log($"Updating event \"{calendarEvent.Id}\" for page \"{page.Title}\".", true);
        return _googleCalendarProvider.UpdateEventAsync(page.GoogleEventId, calendarEvent, page.Title, dates.Start,
            dates.End, page.Page.Url, page.Link?.ToString());
    }

    private async Task ClearPageAsync(PageInfo page)
    {
        bool cleared = await _notionProvider.TryClearEventDataAsync(page);
        if (!cleared)
        {
            _logger.Errors.Log($"Failed to clear page \"{page.Title}\" event data due to conflicts.", true);
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
    private readonly HashSet<string> _relevantPropertyNames;
    private readonly string _relevantParentId;
    private readonly Provider _notionProvider;
    private readonly GoogleCalendarProvider _googleCalendarProvider;
    private readonly Logger _logger;
}