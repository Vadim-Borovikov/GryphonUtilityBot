using System.Collections.Generic;
using GryphonUtilities;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Records;

[PublicAPI]
public sealed class RecordData
{
    public int MessageId { get; init; }
    public long ChatId { get; init; }
    public DateTimeFull DateTime { get; set; }
    public HashSet<string> Tags { get; set; } = new();
}