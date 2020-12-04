using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class ArticleCommand : Command
    {
        protected override string Name => "article";

        public ArticleCommand(ArticlesManager articlesManager) { _articlesManager = articlesManager; }

        internal override Task ExecuteAsync(ITelegramBotClient client, ChatId chatId)
        {
            return _articlesManager.SendFirstArticleAsync(client, chatId);
        }

        private readonly ArticlesManager _articlesManager;
    }
}
