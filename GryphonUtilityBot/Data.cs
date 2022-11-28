﻿using System.Collections.Generic;
using GryphonUtilities;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records = new();

    public DateTimeFull? LastUpdated;

    [UsedImplicitly]
    public Dictionary<string, DateTimeFull> Meetings = new();
}