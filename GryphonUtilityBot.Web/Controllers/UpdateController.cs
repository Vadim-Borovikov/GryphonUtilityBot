using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models;
using GryphonUtilityBot.Web.Models.Actions;
using GryphonUtilityBot.Web.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        public UpdateController(IBot bot)
        {
            _bot = bot;
            _dontUnderstandSticker = new InputOnlineFile(_bot.Config.DontUnderstandStickerFileId);
            _forbiddenSticker = new InputOnlineFile(_bot.Config.ForbiddenStickerFileId);
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            await ProcessAsync(update);
            return Ok();
        }

        private Task ProcessAsync(Update update)
        {
            if (update?.Type != UpdateType.Message)
            {
                return Task.CompletedTask;
            }

            Message message = update.Message;
            SupportedAction action = GetAction(message);
            return action == null
                ? _bot.Client.SendStickerAsync(message, _dontUnderstandSticker)
                : action.ExecuteWrapperAsync(_forbiddenSticker);
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
        private readonly InputOnlineFile _dontUnderstandSticker;
        private readonly InputOnlineFile _forbiddenSticker;
    }
}
