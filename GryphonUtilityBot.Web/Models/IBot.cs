using System;
using GoogleSheetsManager;
using GryphonUtilityBot.Web.Models.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models
{
    public interface IBot
    {
        TelegramBotClient Client { get; }
        ShopCommand ShopCommand { get; }
        ArticlesManager ArticlesManager { get; }
        RecordsManager RecordsManager { get; }
        Config.Config Config { get; }

        bool TryParseCommand(Message message, out Command command);
        void Initialize(Provider googleSheetsProvider);

        RecordsMarkQuery CurrentQuery { get; set; }
        DateTime CurrentQueryTime { get; set; }
    }
}
