using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Records;

[PublicAPI]
public sealed class RecordData
{
    public int MessageId { get; init; }
    public long ChatId { get; init; }
    public DateTime DateTime { get; set; }
    public HashSet<string> Tags { get; set; } = new();
}