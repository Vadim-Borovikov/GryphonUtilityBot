using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Bot.Commands;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Bot
{
    public sealed class Bot : BotBaseGoogleSheets<Config.Config>
    {
        public Bot(Config.Config config) : base(config)
        {
            var saveManager = new SaveManager<List<Record>>(Config.SavePath);
            RecordsManager = new Records.Manager(saveManager);
            ArticlesManager = new Articles.Manager(GoogleSheetsProvider, Config.GoogleRange);

            ShopCommand = new ShopCommand(Config.Items);
            Commands.Add(ShopCommand);
            Commands.Add(new ArticleCommand(ArticlesManager));
            Commands.Add(new ReadCommand(ArticlesManager));

            _forbiddenSticker = new InputOnlineFile(Config.ForbiddenStickerFileId);
        }

        protected override Task UpdateAsync(Message message)
        {
            SupportedAction action = GetAction(message);
            return action == null
                ? Client.SendStickerAsync(message, DontUnderstandSticker)
                : action.ExecuteWrapperAsync(_forbiddenSticker);
        }

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

            if (TryParseCommand(message, out CommandBase command))
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

        private bool TryParseCommand(Message message, out CommandBase command)
        {
            command = Commands.FirstOrDefault(c => c.IsInvokingBy(message.Text));
            return command != null;
        }

        internal readonly Articles.Manager ArticlesManager;
        internal readonly Records.Manager RecordsManager;
        internal readonly ShopCommand ShopCommand;

        internal MarkQuery CurrentQuery;
        internal DateTime CurrentQueryTime;
        private readonly InputOnlineFile _forbiddenSticker;
    }
}
