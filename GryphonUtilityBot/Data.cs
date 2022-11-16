using System;
using System.Collections.Generic;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records { get; set; } = new();

    public DateTimeOffset? LastUpdated { get; set; }

    [UsedImplicitly]
    public Dictionary<string, DateTimeOffset> Meetings { get; set; } = new();
}