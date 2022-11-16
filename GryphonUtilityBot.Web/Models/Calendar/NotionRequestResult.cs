namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class NotionRequestResult<T> where T : class
{
    public readonly T? Instance;
    public readonly bool Successfull;

    public NotionRequestResult(T instance) : this(instance, true) { }

    public NotionRequestResult(bool successfull) : this(null, successfull) { }

    private NotionRequestResult(T? instance, bool successfull)
    {
        Instance = instance;
        Successfull = successfull;
    }
}