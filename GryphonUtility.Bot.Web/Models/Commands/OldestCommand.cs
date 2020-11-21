using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class OldestCommand : Command
    {
        protected override string Name => "oldest";

        public OldestCommand(ArticlesManager articlesManager) { _articlesManager = articlesManager; }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return _articlesManager.SendOldestArticleAsync(message.Chat, client);
        }

        private readonly ArticlesManager _articlesManager;
    }
}
