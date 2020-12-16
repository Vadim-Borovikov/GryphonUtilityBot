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
        public UpdateController(IBot bot) => _bot = bot;

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if ((update != null) && (update.Type == UpdateType.Message))
            {
                Message message = update.Message;
                SupportedAction action = GetAction(message);
                if (action == null)
                {
                    await _bot.Client.SendTextMessageAsync(message.Chat, "Неизвестное действие.");
                }
                else
                {
                    await action.ExecuteWrapperAsync();
                }
            }

            return Ok();
        }

        private SupportedAction GetAction(Message message)
        {
            if (message.ForwardFrom != null)
            {
                if ((_bot.CurrentQuery != null) && (message.Date > _bot.CurrentQueryTime))
                {
                    _bot.CurrentQuery = null;
                }
                return new ForwardAction(_bot, message);
            }

            if (_bot.TryParseCommand(message, out Command command))
            {
                return new CommandAction(_bot, message, command);
            }

            if (int.TryParse(message.Text, out int number))
            {
                return new NumberAction(_bot, message, number);
            }

            if (ArticlesManager.TryParseArticle(message.Text, out Article article))
            {
                return new ArticleAction(_bot, message, article);
            }

            if (RecordsFindQuery.TryParseFindQuery(message.Text, out RecordsFindQuery findQuery))
            {
                return new FindQueryAction(_bot, message, findQuery);
            }

            if (RecordsMarkQuery.TryParseMarkQuery(message.Text, out RecordsMarkQuery markQuery))
            {
                if (message.ReplyToMessage == null)
                {
                    return new RememberMarkAction(_bot, message, markQuery);
                }

                if (message.ReplyToMessage.ForwardFrom != null)
                {
                    return new MarkAction(_bot, message, message.ReplyToMessage, markQuery);
                }
            }

            return null;
        }

        private readonly IBot _bot;
    }
}
