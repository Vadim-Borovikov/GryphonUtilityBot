using System;
using System.Collections.Generic;

namespace GryphonUtilityBot.Records;

internal sealed class RecordData
{
    internal int MessageId { get; init; }
    internal long ChatId { get; init; }
    internal DateTime DateTime { get; set; }
    internal HashSet<string> Tags { get; set; } = new();
}