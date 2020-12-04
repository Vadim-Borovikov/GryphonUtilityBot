using System;
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

                SupportedAction action = null;
                bool tagsJustSetted = false;
                if (message.ForwardFrom != null)
                {
                    if ((_currentTags != null) && (message.Date > _currentTagsTime))
                    {
                        _currentTags = null;
                    }
                    action = new ForwardAction(_bot, message, _currentTags);
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
                else if (TryParseTags(message.Text, out _currentTags))
                {
                    _currentTagsTime = message.Date;
                    tagsJustSetted = true;
                }

                if (action != null)
                {
                    await action.ExecuteWrapperAsync();
                }
                else if (!tagsJustSetted)
                {
                    await _bot.Client.SendTextMessageAsync(message.Chat, "Неизвестное действие.");
                }
            }

            return Ok();
        }

        private static bool TryParseTags(string text, out HashSet<string> tags)
        {
            string[] parts = text?.Split(' ');
            tags = (parts != null) && (parts.Length > 0) ? new HashSet<string>(parts) : null;
            return tags != null;
        }

        private static HashSet<string> _currentTags;
        private static DateTime _currentTagsTime;

        private readonly IBot _bot;
    }
}
