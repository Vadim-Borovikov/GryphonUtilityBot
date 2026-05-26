namespace GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;

internal sealed class MovedUpdate : Update
{
    public readonly string NewParentId;

    public MovedUpdate(string webhookId, string entityId, string newParentId)
        : base(webhookId, entityId) => NewParentId = newParentId;
}