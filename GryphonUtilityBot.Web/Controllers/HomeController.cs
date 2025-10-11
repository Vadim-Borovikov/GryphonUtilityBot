using Microsoft.AspNetCore.Mvc;

namespace GryphonUtilityBot.Web.Controllers;

[Route("")]
public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromServices] Bot bot) => View(bot.Core.Self);
}