using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoogleSheetsManager;
using GryphonUtilityBot.Web.Models.Commands;
using GryphonUtilityBot.Web.Models.Save;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }
        public ShopCommand ShopCommand { get; }
        public ArticlesManager ArticlesManager { get; private set; }
        public RecordsManager RecordsManager { get; }

        public Config.Config Config { get; }

        public RecordsMarkQuery CurrentQuery { get; set; }
        public DateTime CurrentQueryTime { get; set; }

        public bool TryParseCommand(Message message, out Command command)
        {
            command = _commands.FirstOrDefault(c => c.IsInvokingBy(message));
            return command != null;
        }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Config.CultureInfoName);

            Utils.SetupTimeZoneInfo(Config.SystemTimeZoneId);

            Client = new TelegramBotClient(Config.Token);

            ShopCommand = new ShopCommand(Config.Items);

            var recordsSaveManager = new Manager<List<Record>>(Config.RecordsPath);

            RecordsManager = new RecordsManager(recordsSaveManager);
        }

        public void Initialize(Provider googleSheetsProvider)
        {
            ArticlesManager = new ArticlesManager(googleSheetsProvider, Config.GoogleRange);

            _commands = new List<Command>
            {
                ShopCommand,
                new ArticleCommand(ArticlesManager),
                new ReadCommand(ArticlesManager)
            };
        }

        private IEnumerable<Command> _commands;
    }
}
