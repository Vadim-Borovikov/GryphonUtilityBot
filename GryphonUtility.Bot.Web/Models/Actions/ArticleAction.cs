using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class ArticleAction : SupportedAction
    {
        public ArticleAction(IBot bot, Message message, Article article) : base(bot, message) => _article = article;

        protected override Task ExecuteAsync()
        {
            return Bot.ArticlesManager.ProcessNewArticleAsync(Bot.Client, ChatId, _article);
        }

        private readonly Article _article;
    }
}
