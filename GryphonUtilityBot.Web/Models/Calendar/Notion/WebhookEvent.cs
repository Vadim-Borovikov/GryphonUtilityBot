using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GryphonUtilityBot.Web.Models.Calendar.Notion;

public sealed class WebhookEvent
{
    public sealed class EntityType
    {
        [Required]
        public string Id { get; set; } = null!;
    }

    public sealed class PageType
    {
        [Required]
        public EntityType Parent { get; set; } = null!;

        public List<string>? UpdatedProperties { get; set; } = null!;
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