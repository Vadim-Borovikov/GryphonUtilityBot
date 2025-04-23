using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models.Calendar.Notion;

public abstract class Changes
{
    [UsedImplicitly]
    public List<string>? Properties { get; set; }

    [UsedImplicitly]
    [JsonPropertyName("title")]
    public List<string>? Title { get; set; }

    [UsedImplicitly]
    [JsonPropertyName("archived")]
    public List<string>? Archived { get; set; }
}