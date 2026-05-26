namespace GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;

public abstract class Update
{
    internal readonly string WebhookId;
    internal readonly string EntityId;

    protected Update(string webhookId, string entityId)
    {
        WebhookId = webhookId;
        EntityId = entityId;
    }
}