using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class ArticleAction : SupportedAction
    {
        public ArticleAction(Bot bot, Message message, Article article) : base(bot, message) => _article = article;

        protected override Task ExecuteAsync()
        {
            return Bot.ArticlesManager.ProcessNewArticleAsync(Bot.Client, Message.Chat, _article);
        }

        private readonly Article _article;
    }
}
