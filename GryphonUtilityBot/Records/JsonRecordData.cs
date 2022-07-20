using System;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Records;

internal sealed class JsonRecordData
{
    [JsonProperty]
    public int? MessageId { get; set; }

    [JsonProperty]
    public long? ChatId { get; set; }

    [JsonProperty]
    public DateTime? DateTime { get; set; }

    [JsonProperty]
    public HashSet<string?>? Tags { get; set; }

    public static List<RecordData>? Convert(List<JsonRecordData?>? data)
    {
        return data?.Select(r => r?.Convert()).RemoveNulls().ToList();
    }
    private RecordData? Convert()
    {
        if (MessageId is null)
        {
            return null;
        }

        if (ChatId is null)
        {
            return null;
        }

        if (DateTime is null)
        {
            return null;
        }

        HashSet<string>? tags = Tags is null ? null : new HashSet<string>(Tags.RemoveNulls());
        return new RecordData(MessageId.Value, ChatId.Value, DateTime.Value, tags);
    }
}