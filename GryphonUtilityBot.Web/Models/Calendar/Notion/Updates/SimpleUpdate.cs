namespace GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;

internal sealed class SimpleUpdate : Update
{
    public enum Type
    {
        Created,
        Deleted,
        Undeleted
    }

    public readonly Type UpdateType;

    public SimpleUpdate(string webhookId, string entityId, Type type) : base(webhookId, entityId) => UpdateType = type;
}