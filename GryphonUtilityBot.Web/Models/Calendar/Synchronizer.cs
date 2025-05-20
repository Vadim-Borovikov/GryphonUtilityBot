using GryphonUtilityBot.Web.Models.Calendar.Notion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using GryphonUtilities;
using GryphonUtilities.Time;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Synchronizer : IUpdatesSubscriber
{
    public Synchronizer(Config config, Provider notionProvider, GoogleCalendarProvider googleCalendarProvider,
        Logger logger)
        : this(config.RelevantProperties.Values, config.NotionDatabaseId, notionProvider, googleCalendarProvider,
            logger)
    { }

    private Synchronizer(IEnumerable<string> relevantProperties, string releventParentId, Provider notionProvider,
        GoogleCalendarProvider googleCalendarProvider, Logger logger)
    {
        _relevantProperties = new HashSet<string>(relevantProperties);
        _releventParentId = releventParentId;
        _notionProvider = notionProvider;
        _googleCalendarProvider = googleCalendarProvider;
        _logger = logger;
    }

    public async Task OnCreatedAsync(string id)
    {
        PageInfo page = await GetPageInfoAsync(id);
        if (page.IsRelevantMeeting())
        {
            await CreateEventAndUpdatePageAsync(page, page.Dates!.Value);
        }
    }

    public async Task OnPropertiesUpdatedAsync(string id, IEnumerable<string> properties)
    {
        if (!_relevantProperties.Overlaps(properties))
        {
            return;
        }

        PageInfo page = await GetPageInfoAsync(id);

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
                await DeleteEventAndClearPageAsync(page);
            }
        }
    }

    public Task OnMovedAsync(string id, string newParentId)
    {
        return newParentId.Equals(_releventParentId, StringComparison.OrdinalIgnoreCase)
            ? OnCreatedAsync(id)
            : OnDeletedAsync(id);
    }

    public async Task OnDeletedAsync(string id)
    {
        PageInfo page = await GetPageInfoAsync(id);
        if (page.IsRelevantMeeting() && !string.IsNullOrWhiteSpace(page.GoogleEventId))
        {
            await DeleteEventAndClearPageAsync(page);
        }
    }

    public Task OnUndeletedAsync(string id) => OnCreatedAsync(id);

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

    private async Task DeleteEventAndClearPageAsync(PageInfo page)
    {
        await _googleCalendarProvider.DeleteEventAsync(page.GoogleEventId);
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

    private readonly HashSet<string> _relevantProperties;
    private readonly string _releventParentId;
    private readonly Provider _notionProvider;
    private readonly GoogleCalendarProvider _googleCalendarProvider;
    private readonly Logger _logger;
}