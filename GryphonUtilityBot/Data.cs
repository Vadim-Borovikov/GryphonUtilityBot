using System.Collections.Generic;
using GryphonUtilities;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records { get; set; } = new();

    public DateTimeFull? LastUpdated { get; set; }

    [UsedImplicitly]
    public Dictionary<string, DateTimeFull> Meetings { get; set; } = new();
}