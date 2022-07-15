using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using GryphonUtilityBot.Records;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot
{
    public sealed class Bot : BotBaseGoogleSheets<Bot, Config>
    {
        public Bot(Config config) : base(config)
        {
            var saveManager = new SaveManager<List<Record>>(Config.SavePath);
            RecordsManager = new Records.Manager(this, saveManager);
            ArticlesManager = new Articles.Manager(this);

            Commands.Add(new ArticleCommand(this));
            Commands.Add(new ReadCommand(this));
        }

        protected override Task UpdateAsync(Message message, bool fromChat, CommandBase<Bot, Config> command = null,
            string payload = null)
        {
            SupportedAction action = GetAction(message, command);
            return action == null
                ? Client.SendStickerAsync(message.Chat, DontUnderstandSticker)
                : action.ExecuteWrapperAsync(ForbiddenSticker);
        }

        private SupportedAction GetAction(Message message, CommandBase<Bot, Config> command)
        {
            if (message.ForwardFrom != null)
            {
                if ((CurrentQuery != null) && (message.Date > CurrentQueryTime))
                {
                    CurrentQuery = null;
                }
                return new ForwardAction(this, message);
            }

            if (command != null)
            {
                return new CommandAction(this, message, command);
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

        internal readonly Articles.Manager ArticlesManager;
        internal readonly Records.Manager RecordsManager;

        internal MarkQuery CurrentQuery;
        internal DateTime CurrentQueryTime;
    }
}
