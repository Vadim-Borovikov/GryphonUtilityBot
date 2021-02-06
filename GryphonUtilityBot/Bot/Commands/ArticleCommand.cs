using System.Threading.Tasks;
using GryphonUtilityBot.Articles;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ArticleCommand : Command
    {
        protected override string Name => "article";

        public ArticleCommand(Manager articlesManager) => _articlesManager = articlesManager;

        public override Task ExecuteAsync(ITelegramBotClient client, ChatId chatId)
        {
            return _articlesManager.SendFirstArticleAsync(client, chatId);
        }

        private readonly Manager _articlesManager;
    }
}
