using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using Notion.Client;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class NotionHelper
{
    public NotionHelper(INotionClient client, Config config)
    {
        _client = client;
        _databaseId = config.NotionDatabaseId;
        _updatePeriod = TimeSpan.FromSeconds(config.NotionUpdatesPerSecondLimit);
    }

    public async Task<PageInfo> GetPage(string id)
    {
        DelayIfNeeded();
        Page page = await _client.Pages.RetrieveAsync(id);
        return new PageInfo(page);
    }

    public async Task<List<PageInfo>> GetPages(DateTime updatedSince)
    {
        DatabasesQueryParameters query = new() { Filter = GetQueryFilter(updatedSince.ToUniversalTime()) };
        List<PageInfo> result = new();
        do
        {
            DelayIfNeeded();
            PaginatedList<Page>? chunk = await _client.Databases.QueryAsync(_databaseId, query);
            if (chunk is null)
            {
                break;
            }
            result.AddRange(chunk.Results.Select(p => new PageInfo(p)));
            query.StartCursor = chunk.HasMore ? chunk.NextCursor : null;
        }
        while (query.StartCursor is not null);

        return result;
    }

    public async Task UpdateAsync(string pageId, string? eventId, Uri? eventUri)
    {
        Dictionary<string, PropertyValue> eventIdProperty = new()
        {
            { "Google Event Id", CreateTextValue(eventId) },
        };
        DelayIfNeeded();
        await _client.Pages.UpdatePropertiesAsync(pageId, eventIdProperty);

        Dictionary<string, PropertyValue?> eventProperty = new()
        {
            { "Google Event", eventUri is null ? null : new UrlPropertyValue { Url = eventUri.AbsoluteUri } }
        };
        DelayIfNeeded();
        await _client.Pages.UpdatePropertiesAsync(pageId, eventProperty);
    }

    private static Filter GetQueryFilter(DateTime updatedSince)
    {
        LastEditedTimeFilter lastEditedFilter = new(onOrAfter: updatedSince);
        CheckboxFilter meetingFilter = new("Встреча", true);
        DateFilter dateFilter = new("Дата", onOrAfter: updatedSince);
        List<Filter> filters = new()
        {
            lastEditedFilter,
            meetingFilter,
            dateFilter
        };
        return new CompoundFilter(and: filters);
    }

    private void DelayIfNeeded()
    {
        lock (_delayLocker)
        {
            DateTime now = DateTime.UtcNow;

            TimeSpan? beforeUpdate = TimeManager.GetDelayUntil(_lastUpdate, _updatePeriod, now);
            if (beforeUpdate.HasValue)
            {
                Task.Delay(beforeUpdate.Value).Wait();
                now += beforeUpdate.Value;
            }
            _lastUpdate = now;
        }
    }

    private static RichTextPropertyValue CreateTextValue(string? content)
    {
        RichTextPropertyValue result = new()
        {
            RichText = new List<RichTextBase>()
        };
        Text text = new() { Content = content ?? "" };
        RichTextText item = new() { Text = text };
        result.RichText.Add(item);
        return result;
    }

    private readonly INotionClient _client;
    private readonly string _databaseId;
    private readonly object _delayLocker = new();
    private readonly TimeSpan _updatePeriod;
    private DateTime? _lastUpdate;
}