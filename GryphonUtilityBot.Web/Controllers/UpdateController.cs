using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update, [FromServices]BotSingleton singleton)
        {
            if (update?.Type == UpdateType.CallbackQuery)
            {
                await singleton.Bot.ProcessQueryAsync(update.CallbackQuery);
            }
            else
            {
                await singleton.Bot.UpdateAsync(update);
            }
            return Ok();
        }
    }
}
