using System.Collections.Generic;
using GryphonUtilityBot.Records;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Data
{
    [UsedImplicitly]
    public List<RecordData> Records = new();
}