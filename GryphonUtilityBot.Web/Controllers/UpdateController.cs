using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update, [FromServices]BotSingleton singleton)
        {
            await singleton.Bot.UpdateAsync(update);
            return Ok();
        }
    }
}
