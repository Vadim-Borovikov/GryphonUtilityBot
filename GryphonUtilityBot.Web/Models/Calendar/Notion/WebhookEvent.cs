using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models.Calendar.Notion;

public sealed class WebhookEvent
{
    public sealed class EntityType
    {
        [UsedImplicitly]
        public string? Id { get; set; }
    }

    public sealed class PageType
    {
        [UsedImplicitly]
        public EntityType? Parent { get; set; }

        [UsedImplicitly]
        public List<string>? UpdatedProperties { get; set; }

        [UsedImplicitly]
        public List<EntityType>? UpdatedBlocks { get; set; }
    }

    [UsedImplicitly]
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

    [UsedImplicitly]
    public EventType? Type { get; set; }

    [UsedImplicitly]
    public EntityType? Entity { get; set; }

    [UsedImplicitly]
    public PageType? Data { get; set; }
}