using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Notion.Client;

namespace GryphonUtilityBot.Web.Models.Calendar;

public sealed class LastEditedTimeFilter : Filter
{
    [JsonProperty("timestamp")]
    public string Timestamp = "last_edited_time";

    [JsonProperty("last_edited_time")]
    public DateFilter.Condition LastEditedTime { get; set; }

    public LastEditedTimeFilter(
        DateTimeOffset? equal = null,
        DateTimeOffset? before = null,
        DateTimeOffset? after = null,
        DateTimeOffset? onOrBefore = null,
        DateTimeOffset? onOrAfter = null,
        Dictionary<string, object>? pastWeek = null,
        Dictionary<string, object>? pastMonth = null,
        Dictionary<string, object>? pastYear = null,
        Dictionary<string, object>? nextWeek = null,
        Dictionary<string, object>? nextMonth = null,
        Dictionary<string, object>? nextYear = null,
        bool? isEmpty = null,
        bool? isNotEmpty = null)
    {
        LastEditedTime = new DateFilter.Condition(
            equal?.UtcDateTime,
            before?.UtcDateTime,
            after?.UtcDateTime,
            onOrBefore?.UtcDateTime,
            onOrAfter?.UtcDateTime,
            pastWeek,
            pastMonth,
            pastYear,
            nextWeek,
            nextMonth,
            nextYear,
            isEmpty,
            isNotEmpty
        );
    }
}