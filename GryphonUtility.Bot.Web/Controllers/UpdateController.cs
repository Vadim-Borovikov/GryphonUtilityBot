using System.Linq;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models;
using GryphonUtility.Bot.Web.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        public UpdateController(IBot bot) { _bot = bot; }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update?.Message.From.Id == _bot.Config.MasterId)
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        Message message = update.Message;

                        Command command = _bot.Commands.FirstOrDefault(c => c.Contains(message));
                        if (command != null)
                        {
                            await command.ExecuteAsync(message, _bot.Client);
                        }
                        else if (int.TryParse(message.Text, out int number))
                        {
                            await _bot.ShopCommand.ProcessNumberAsync(message.Chat, _bot.Client, number);
                        }
                        break;
                }
            }

            return Ok();
        }

        private readonly IBot _bot;
    }
}
