using System.Collections.Generic;
using AbstractBot;
using GryphonUtilities.Time;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data : SaveData
{
    [UsedImplicitly]
    public List<RecordData> Records = new();

    public DateTimeFull? LastUpdated;

    [UsedImplicitly]
    public Dictionary<string, DateTimeFull> Meetings = new();
}