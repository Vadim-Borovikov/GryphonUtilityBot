using System.Collections.Generic;

namespace GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;

internal sealed class PropertiesUpdatedUpdate : Update
{
    public readonly IEnumerable<string> Properties;

    public PropertiesUpdatedUpdate(string webhookId, string entityId, IEnumerable<string> properties)
        : base(webhookId, entityId) => Properties = properties;
}