namespace GryphonUtilityBot.Web.Models.Calendar.Notion;

internal sealed class RequestResult<T> where T : class
{
    public readonly T? Instance;
    public readonly bool Successfull;

    public RequestResult(T instance) : this(instance, true) { }

    public RequestResult(bool successfull) : this(null, successfull) { }

    private RequestResult(T? instance, bool successfull)
    {
        Instance = instance;
        Successfull = successfull;
    }
}