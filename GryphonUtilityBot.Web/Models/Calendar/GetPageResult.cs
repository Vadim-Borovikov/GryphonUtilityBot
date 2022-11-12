using Notion.Client;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class GetPageResult
{
    public readonly PageInfo? Page;
    public readonly bool Successfull;

    public GetPageResult(Page page) : this(page, true) { }

    public GetPageResult(bool successfull) : this(null, successfull) { }

    private GetPageResult(Page? page, bool successfull)
    {
        Page = page is null ? null : new PageInfo(page);
        Successfull = successfull;
    }
}