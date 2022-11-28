using System.Collections.Generic;
using GryphonUtilities;
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