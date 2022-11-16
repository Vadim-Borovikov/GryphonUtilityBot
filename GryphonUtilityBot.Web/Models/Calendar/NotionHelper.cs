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

    public Task<NotionRequestResult<PageInfo>> TryGetPageAsync(string id) => Wrapper(GetPageAsync, id);

    public Task<NotionRequestResult<List<PageInfo>>> TryGetPagesAsync(DateTimeOffset updatedSince)
    {
        return Wrapper(GetPagesAsync, updatedSince);
    }

    public async Task UpdateAsync(PageInfo page, string eventId, Uri eventUri)
    {
        if (page.GoogleEventId != eventId)
        {
            await UpdateEventIdAsync(page.Page.Id, eventId);
        }

        if (page.GoogleEvent != eventUri)
        {
            await UpdateEventUriAsync(page.Page.Id, eventUri);
        }
    }

    public async Task ClearAsync(PageInfo page)
    {
        if (!string.IsNullOrWhiteSpace(page.GoogleEventId))
        {
            await UpdateEventIdAsync(page.Page.Id);
        }

        if (page.GoogleEvent is not null)
        {
            await UpdateEventUriAsync(page.Page.Id);
        }
    }

    private async Task<PageInfo> GetPageAsync(string id)
    {
        DelayIfNeeded();
        Page page = await _client.Pages.RetrieveAsync(id);
        return new PageInfo(page);
    }

    private static async Task<NotionRequestResult<TResult>> Wrapper<TParam, TResult>(Func<TParam,
        Task<TResult>> method, TParam param)
        where TResult : class
    {
        try
        {
            TResult result = await method(param);
            return new NotionRequestResult<TResult>(result);
        }
        catch (NotionApiException ex) when (ex.NotionAPIErrorCode == NotionAPIErrorCode.ObjectNotFound)
        {
            Utils.LogManager.LogError($"Method with parameter {param} resulted with ObjectNotFound");
            Utils.LogManager.LogException(ex);
            return new NotionRequestResult<TResult>(true);
        }
        catch (NotionApiException ex)
        {
            Utils.LogManager.LogError($"Method with parameter {param} resulted with unspecified NotionApiException");
            Utils.LogManager.LogException(ex);
            return new NotionRequestResult<TResult>(false);
        }
    }

    private async Task<List<PageInfo>> GetPagesAsync(DateTimeOffset updatedSince)
    {
        DatabasesQueryParameters query = new() { Filter = GetQueryFilter(updatedSince) };
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

    private async Task UpdateEventIdAsync(string pageId, string? eventId = null)
    {
        Dictionary<string, PropertyValue> eventIdProperty = new()
        {
            { "Google Event Id", CreateTextValue(eventId) },
        };
        DelayIfNeeded();
        await _client.Pages.UpdatePropertiesAsync(pageId, eventIdProperty);
    }

    private async Task UpdateEventUriAsync(string pageId, Uri? eventUri = null)
    {
        Dictionary<string, PropertyValue?> eventProperty = new()
        {
            { "Google Event", eventUri is null ? null : new UrlPropertyValue { Url = eventUri.AbsoluteUri } }
        };
        DelayIfNeeded();
        await _client.Pages.UpdatePropertiesAsync(pageId, eventProperty);
    }

    private static Filter GetQueryFilter(DateTimeOffset updatedSince)
    {
        LastEditedTimeFilter lastEditedFilter = new(onOrAfter: updatedSince);
        CheckboxFilter meetingFilter = new("Встреча", true);
        DateFilter dateFilter = new("Дата", onOrAfter: updatedSince.UtcDateTime);
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
            DateTimeOffset now = DateTimeOffset.UtcNow;

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
    private DateTimeOffset? _lastUpdate;
}