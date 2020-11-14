using System.Collections.Generic;
using GryphonUtility.Bot.Web.Models.Commands;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }
        public IEnumerable<Command> Commands { get; }
        public ShopCommand ShopCommand { get; }

        public Config.Config Config { get; }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            Client = new TelegramBotClient(Config.Token);

            ShopCommand = new ShopCommand(Config.Items);

            var saveManager = new Save.Manager(Config.SavePath);

            Commands = new List<Command>
            {
                ShopCommand,
                new ArticlesCommand(Config.Articles, Config.ArticlesChannelChatId, Config.ArticlesFirstMessageId,
                saveManager, Config.Delay)
            };
        }
    }
}
