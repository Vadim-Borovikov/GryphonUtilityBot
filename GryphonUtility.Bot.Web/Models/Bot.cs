using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GryphonUtility.Bot.Web.Models.Commands;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }
        public ShopCommand ShopCommand { get; }
        public ArticlesManager ArticlesManager { get; }
        public RecordsManager RecordsManager { get; }

        public Config.Config Config { get; }

        public bool TryParseCommand(Message message, out Command command)
        {
            command = _commands.FirstOrDefault(c => c.Contains(message));
            return command != null;
        }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Config.CultureInfoName);

            Client = new TelegramBotClient(Config.Token);

            ShopCommand = new ShopCommand(Config.Items);

            var saveManager = new Save.Manager(Config.SavePath);

            ArticlesManager = new ArticlesManager(saveManager);
            RecordsManager = new RecordsManager(saveManager);

            _commands = new List<Command>
            {
                ShopCommand,
                new ArticleCommand(ArticlesManager),
                new ReadCommand(ArticlesManager)
            };
        }

        private readonly IEnumerable<Command> _commands;
    }
}
