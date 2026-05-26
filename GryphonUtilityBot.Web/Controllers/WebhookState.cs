using GryphonUtilities.Time;

namespace GryphonUtilityBot.Web.Controllers;

internal readonly struct WebhookState
{
    public enum WebhookStatus
    {
        Queued,
        Processing,
        Processed
    }

    public readonly WebhookStatus Status;
    public readonly DateTimeFull UpdatedAt;

    public WebhookState(WebhookStatus status, DateTimeFull updatedAt)
    {
        Status = status;
        UpdatedAt = updatedAt;
    }
}