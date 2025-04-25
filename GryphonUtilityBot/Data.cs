using System.Collections.Generic;
using GryphonUtilities.Time;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records = new();

    [UsedImplicitly]
    public Dictionary<string, DateTimeFull> Meetings = new();
}