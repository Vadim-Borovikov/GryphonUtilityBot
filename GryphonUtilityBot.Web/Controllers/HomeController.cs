using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GryphonUtilityBot.Web.Controllers;

[Route("")]
public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromServices] BotSingleton singleton) => View(singleton.Bot.User);
}