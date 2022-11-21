using System.Collections.Generic;
using GryphonUtilities;
using JetBrains.Annotations;
using Notion.Client;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class LastEditedTimeFilter : Filter
{
    [UsedImplicitly]
    [JsonProperty("timestamp")]
    public string Timestamp = "last_edited_time";

    [UsedImplicitly]
    [JsonProperty("last_edited_time")]
    public DateFilter.Condition LastEditedTime { get; set; }

    public LastEditedTimeFilter(
        DateTimeFull? equal = null,
        DateTimeFull? before = null,
        DateTimeFull? after = null,
        DateTimeFull? onOrBefore = null,
        DateTimeFull? onOrAfter = null,
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
            equal?.ToDateTimeOffset().UtcDateTime,
            before?.ToDateTimeOffset().UtcDateTime,
            after?.ToDateTimeOffset().UtcDateTime,
            onOrBefore?.ToDateTimeOffset().UtcDateTime,
            onOrAfter?.ToDateTimeOffset().UtcDateTime,
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