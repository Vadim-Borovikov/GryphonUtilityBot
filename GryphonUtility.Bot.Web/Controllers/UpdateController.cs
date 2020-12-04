using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models;
using GryphonUtility.Bot.Web.Models.Actions;
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

                SupportedAction action = null;
                if (message.ForwardFrom != null)
                {
                    action = new ForwardAction(_bot, message);
                }
                else if (_bot.TryParseCommand(message, out Command command))
                {
                    action = new CommandAction(_bot, message, command);
                }
                else if (int.TryParse(message.Text, out int number))
                {
                    action = new NumberAction(_bot, message, number);
                }
                else if (ArticlesManager.TryParseArticle(message.Text, out Article article))
                {
                    action = new ArticleAction(_bot, message, article);
                }
                else if (RecordsQuery.TryParseQuery(message.Text, out RecordsQuery query))
                {
                    action = new QueryAction(_bot, message, query);
                }

                if (action != null)
                {
                    await action.ExecuteWrapperAsync();
                }
                else
                {
                    await _bot.Client.SendTextMessageAsync(message.Chat, "Неизвестное действие.");
                }
            }

            return Ok();
        }

        private readonly IBot _bot;
    }
}
