using System.Linq;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models;
using GryphonUtility.Bot.Web.Models.Commands;
using GryphonUtility.Bot.Web.Models.Save;
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
            if ((update != null) && (update.Type == UpdateType.Message))
            {
                Message message = update.Message;
                if (message.From.Id == _bot.Config.MasterId)
                {
                    Command command = _bot.Commands.FirstOrDefault(c => c.Contains(message));
                    if (command != null)
                    {
                        await command.ExecuteAsync(message, _bot.Client);
                    }
                    else if (int.TryParse(message.Text, out int number))
                    {
                        await _bot.ShopCommand.ProcessNumberAsync(message.Chat, _bot.Client, number);
                    }
                    else if (ArticlesManager.TryParseArticle(message.Text, out Article article))
                    {
                        await _bot.ArticlesManager.ProcessNewArticleAsync(article, message, _bot.Client);
                    }
                    else
                    {
                        await _bot.Client.SendTextMessageAsync(message.Chat, "Неизвестная команда!");
                    }
                }
            }

            return Ok();
        }

        private readonly IBot _bot;
    }
}
