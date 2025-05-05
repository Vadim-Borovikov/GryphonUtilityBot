using GryphonUtilities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GryphonUtilityBot.Web;

[UsedImplicitly]
internal sealed class GlobalExceptionFilter : IExceptionFilter
{
    public GlobalExceptionFilter(Logger logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogException(context.Exception);
        context.Result = new StatusCodeResult(500);
        context.ExceptionHandled = true;
    }

    private readonly Logger _logger;
}