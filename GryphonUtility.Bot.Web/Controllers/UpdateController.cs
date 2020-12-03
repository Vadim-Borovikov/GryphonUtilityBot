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

                bool fromMaster = message.From.Id == _bot.Config.MasterId;
                bool fromMistress = message.From.Id == _bot.Config.MistressId;

                if (fromMaster || fromMistress)
                {
                    if (message.ForwardFrom != null)
                    {
                        _bot.RecordsManager.SaveRecord(message);
                    }
                    else
                    {
                        if (fromMaster && _bot.TryParseCommand(message, out Command command))
                        {
                            await command.ExecuteAsync(message, _bot.Client);
                        }
                        else if (fromMaster && int.TryParse(message.Text, out int number))
                        {
                            await _bot.ShopCommand.ProcessNumberAsync(message.Chat, _bot.Client, number);
                        }
                        else if (fromMaster && ArticlesManager.TryParseArticle(message.Text, out Article article))
                        {
                            await _bot.ArticlesManager.ProcessNewArticleAsync(article, message, _bot.Client);
                        }
                        else if (RecordsQuery.TryParseQuery(message.Text, out RecordsQuery query))
                        {
                            await _bot.RecordsManager.ProcessQuery(query, message.Chat, _bot.Client);
                        }
                        else
                        {
                            await _bot.Client.SendTextMessageAsync(message.Chat, "Неизвестная команда!");
                        }
                    }
                }
            }

            return Ok();
        }

        private readonly IBot _bot;
    }
}
