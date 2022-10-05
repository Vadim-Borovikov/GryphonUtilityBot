using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal sealed class RecordData
{
    public int MessageId { get; init; }
    public long ChatId { get; init; }
    public DateTime DateTime { get; set; }
    public HashSet<string> Tags { get; set; } = new();
}