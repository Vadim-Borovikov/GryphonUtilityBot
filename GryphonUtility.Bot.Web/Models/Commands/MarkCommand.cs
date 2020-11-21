﻿using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class MarkCommand : Command
    {
        protected override string Name => "mark";

        public MarkCommand(ArticlesManager articlesManager) { _articlesManager = articlesManager; }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            _articlesManager.DeleteFirstArticle();
            return _articlesManager.SendFirstArticleAsync(message.Chat, client);
        }

        private readonly ArticlesManager _articlesManager;
    }
}