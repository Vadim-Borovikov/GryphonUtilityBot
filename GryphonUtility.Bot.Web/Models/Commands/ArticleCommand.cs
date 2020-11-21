using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class ArticleCommand : Command
    {
        protected override string Name => "article";

        public ArticleCommand(ArticlesManager articlesManager) { _articlesManager = articlesManager; }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return _articlesManager.SendFirstArticleAsync(message.Chat, client);
        }

        private readonly ArticlesManager _articlesManager;
    }
}
