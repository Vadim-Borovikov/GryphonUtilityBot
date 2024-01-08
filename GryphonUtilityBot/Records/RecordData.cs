using System.Collections.Generic;
using GryphonUtilities.Time;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Records;

[PublicAPI]
public sealed class RecordData
{
    public int MessageId;
    public long ChatId;
    public DateTimeFull DateTime;
    public HashSet<string> Tags = new();
}