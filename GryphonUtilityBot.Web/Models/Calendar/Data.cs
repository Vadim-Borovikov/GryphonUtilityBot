using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class Data
{
    public DateTime? LastUpdated { get; set; }

    [UsedImplicitly]
    public Dictionary<string, DateTime> Meetings { get; set; } = new();
}