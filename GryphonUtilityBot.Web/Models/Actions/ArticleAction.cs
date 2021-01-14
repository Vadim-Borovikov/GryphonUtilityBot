using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Save;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class ArticleAction : SupportedAction
    {
        public ArticleAction(IBot bot, Message message, Article article) : base(bot, message) => _article = article;

        protected override Task ExecuteAsync()
        {
            return Bot.ArticlesManager.ProcessNewArticleAsync(Bot.Client, Message.Chat, _article);
        }

        private readonly Article _article;
    }
}
