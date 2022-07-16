using System;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtilityBot.Records;

internal sealed class RecordData
{
    public readonly int MessageId;
    public readonly long ChatId;
    public DateTime DateTime;
    public HashSet<string> Tags;

    public RecordData(int messageId, long chatId, DateTime dateTime, HashSet<string>? tags)
    {
        MessageId = messageId;
        ChatId = chatId;
        DateTime = dateTime;
        Tags = tags ?? new HashSet<string>();
    }

    private JsonRecordData Convert()
    {
        return new JsonRecordData
        {
            MessageId = MessageId,
            ChatId = ChatId,
            DateTime = DateTime,
            Tags = new HashSet<string?>(Tags.Cast<string?>())
        };
    }

    public static List<JsonRecordData?>? Convert(List<RecordData>? data)
    {
        return data?.Select(r => (JsonRecordData?) r.Convert()).ToList();
    }
}