using System.Collections.Generic;
using GryphonUtility.Bot.Web.Models.Commands;
using Telegram.Bot;

namespace GryphonUtility.Bot.Web.Models
{
    public interface IBot
    {
        TelegramBotClient Client { get; }
        IEnumerable<Command> Commands { get; }
        ShopCommand ShopCommand { get; }
        ArticlesManager ArticlesManager { get; }
        Config.Config Config { get; }
    }
}
