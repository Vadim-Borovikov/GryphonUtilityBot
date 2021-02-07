using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Articles;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ArticleCommand : CommandBase
    {
        protected override string Name => "article";
        protected override string Description => null;

        public ArticleCommand(Manager articlesManager) => _articlesManager = articlesManager;

        public override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            return _articlesManager.SendFirstArticleAsync(client, chatId);
        }

        private readonly Manager _articlesManager;
    }
}
