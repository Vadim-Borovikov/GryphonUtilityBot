using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleSheetsManager;
using GryphonUtilityBot.Web.Models.Actions;
using GryphonUtilityBot.Web.Models.Commands;
using GryphonUtilityBot.Web.Models.Save;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Web.Models
{
    internal sealed class Bot : IDisposable
    {
        public Bot(Config.Config config)
        {
            Config = config;

            Client = new TelegramBotClient(Config.Token);

            string googleCredentialsJson = Config.GoogleCredentialsJson;
            if (string.IsNullOrWhiteSpace(googleCredentialsJson))
            {
                googleCredentialsJson = JsonConvert.SerializeObject(Config.GoogleCredentials);
            }
            _googleSheetsProvider = new Provider(googleCredentialsJson, ApplicationName, Config.GoogleSheetId);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Config.CultureInfoName);

            Utils.SetupTimeZoneInfo(Config.SystemTimeZoneId);

            var saveManager = new Manager(Config.SavePath);
            RecordsManager = new RecordsManager(saveManager);
            ArticlesManager = new ArticlesManager(_googleSheetsProvider, Config.GoogleRange);

            ShopCommand = new ShopCommand(Config.Items);
            _commands = new List<Command>
            {
                ShopCommand,
                new ArticleCommand(ArticlesManager),
                new ReadCommand(ArticlesManager)
            };

            _dontUnderstandSticker = new InputOnlineFile(Config.DontUnderstandStickerFileId);
            _forbiddenSticker = new InputOnlineFile(Config.ForbiddenStickerFileId);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Client.SetWebhookAsync(Config.Url, cancellationToken: cancellationToken);
        }

        public Task UpdateAsync(Update update)
        {
            if (update?.Type != UpdateType.Message)
            {
                return Task.CompletedTask;
            }

            Message message = update.Message;
            SupportedAction action = GetAction(message);
            return action == null
                ? Client.SendStickerAsync(message, _dontUnderstandSticker)
                : action.ExecuteWrapperAsync(_forbiddenSticker);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Client.DeleteWebhookAsync(cancellationToken);

        public void Dispose() => _googleSheetsProvider?.Dispose();

        public Task<User> GetUserAsunc() => Client.GetMeAsync();

        private SupportedAction GetAction(Message message)
        {
            if (message.ForwardFrom != null)
            {
                if ((CurrentQuery != null) && (message.Date > CurrentQueryTime))
                {
                    CurrentQuery = null;
                }
                return new ForwardAction(this, message);
            }

            if (TryParseCommand(message, out Command command))
            {
                return new CommandAction(this, message, command);
            }

            if (int.TryParse(message.Text, out int number))
            {
                return new NumberAction(this, message, number);
            }

            if (ArticlesManager.TryParseArticle(message.Text, out Article article))
            {
                return new ArticleAction(this, message, article);
            }

            if (RecordsFindQuery.TryParseFindQuery(message.Text, out RecordsFindQuery findQuery))
            {
                return new FindQueryAction(this, message, findQuery);
            }

            if (RecordsMarkQuery.TryParseMarkQuery(message.Text, out RecordsMarkQuery markQuery))
            {
                if (message.ReplyToMessage == null)
                {
                    return new RememberMarkAction(this, message, markQuery);
                }

                if (message.ReplyToMessage.ForwardFrom != null)
                {
                    return new MarkAction(this, message, message.ReplyToMessage, markQuery);
                }
            }

            return null;
        }

        private bool TryParseCommand(Message message, out Command command)
        {
            command = _commands.FirstOrDefault(c => c.IsInvokingBy(message));
            return command != null;
        }

        public readonly Config.Config Config;
        public readonly TelegramBotClient Client;
        public readonly ArticlesManager ArticlesManager;
        public readonly RecordsManager RecordsManager;
        public readonly ShopCommand ShopCommand;

        public RecordsMarkQuery CurrentQuery;
        public DateTime CurrentQueryTime;

        private readonly IEnumerable<Command> _commands;
        private readonly Provider _googleSheetsProvider;
        private readonly InputOnlineFile _dontUnderstandSticker;
        private readonly InputOnlineFile _forbiddenSticker;

        private const string ApplicationName = "GryphonUtilityBot";
    }
}
