using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models.Calendar.Notion;

public sealed class WebhookEvent
{
    public sealed class EntityType
    {
        [Required]
        [UsedImplicitly]
        public string Id { get; set; } = null!;
    }

    public sealed class PageType
    {
        [Required]
        [UsedImplicitly]
        public EntityType Parent { get; set; } = null!;

        [UsedImplicitly]
        public List<string>? UpdatedProperties { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventType
    {
        [JsonStringEnumMemberName("page.created")]
        Created,

        [JsonStringEnumMemberName("page.properties_updated")]
        PropertiesUpdated,

        [JsonStringEnumMemberName("page.moved")]
        Moved,

        [JsonStringEnumMemberName("page.undeleted")]
        Deleted,

        [JsonStringEnumMemberName("page.deleted")]
        Undeleted
    }

    [Required]
    public EventType Type { get; set; }

    [Required]
    public EntityType Entity { get; set; } = null!;

    [Required]
    public PageType Data { get; set; } = null!;
}