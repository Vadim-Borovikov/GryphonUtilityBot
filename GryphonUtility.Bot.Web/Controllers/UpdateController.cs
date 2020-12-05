using System.Collections.Generic;
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
                if ((_bot.CurrentTags != null) && (message.Date > _bot.CurrentTagsTime))
                {
                    _bot.CurrentTags = null;
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

            if (RecordsQuery.TryParseQuery(message.Text, out RecordsQuery query))
            {
                return new QueryAction(_bot, message, query);
            }

            if (TryParseTags(message.Text, out HashSet<string> tags))
            {
                if (message.ReplyToMessage == null)
                {
                    return new RememberTagsAction(_bot, message, tags);
                }

                if (message.ReplyToMessage.ForwardFrom != null)
                {
                    return new TagAction(_bot, message, message.ReplyToMessage, tags);
                }
            }

            return null;
        }

        private static bool TryParseTags(string text, out HashSet<string> tags)
        {
            string[] parts = text?.Split(' ');
            tags = (parts != null) && (parts.Length > 0) ? new HashSet<string>(parts) : null;
            return tags != null;
        }

        private readonly IBot _bot;
    }
}
