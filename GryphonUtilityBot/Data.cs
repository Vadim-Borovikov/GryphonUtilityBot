using System;
using System.Collections.Generic;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records { get; set; } = new();

    public DateTime? LastUpdated { get; set; }

    [UsedImplicitly]
    public Dictionary<string, DateTime> Meetings { get; set; } = new();
}