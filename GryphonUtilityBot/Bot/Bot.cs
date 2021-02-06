using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleSheetsManager;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Bot.Commands;
using GryphonUtilityBot.Records;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Bot
{
    public sealed class Bot : IDisposable
    {
        public Bot(Config.Config config)
        {
            Config = config;

            Client = new TelegramBotClient(Config.Token);

            string googleCredentialsJson = JsonConvert.SerializeObject(Config.GoogleCredentials);
            _googleSheetsProvider = new Provider(googleCredentialsJson, ApplicationName, Config.GoogleSheetId);

            Utils.SetupTimeZoneInfo(Config.SystemTimeZoneId);

            var saveManager = new Save.Manager(Config.SavePath);
            RecordsManager = new Records.Manager(saveManager);
            ArticlesManager = new Articles.Manager(_googleSheetsProvider, Config.GoogleRange);

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

            if (Articles.Manager.TryParseArticle(message.Text, out Article article))
            {
                return new ArticleAction(this, message, article);
            }

            if (FindQuery.TryParseFindQuery(message.Text, out FindQuery findQuery))
            {
                return new FindQueryAction(this, message, findQuery);
            }

            if (MarkQuery.TryParseMarkQuery(message.Text, out MarkQuery markQuery))
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

        internal readonly Config.Config Config;
        internal readonly TelegramBotClient Client;
        internal readonly Articles.Manager ArticlesManager;
        internal readonly Records.Manager RecordsManager;
        internal readonly ShopCommand ShopCommand;

        internal MarkQuery CurrentQuery;
        internal DateTime CurrentQueryTime;

        private readonly IEnumerable<Command> _commands;
        private readonly Provider _googleSheetsProvider;
        private readonly InputOnlineFile _dontUnderstandSticker;
        private readonly InputOnlineFile _forbiddenSticker;

        private const string ApplicationName = "GryphonUtilityBot";
    }
}
