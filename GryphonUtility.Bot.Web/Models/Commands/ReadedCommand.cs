using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class ReadedCommand : Command
    {
        protected override string Name => "readed";

        public ReadedCommand(ArticlesManager articlesManager) { _articlesManager = articlesManager; }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return _articlesManager.DeleteFirstArticleAsync(message, client);
        }

        private readonly ArticlesManager _articlesManager;
    }
}
