using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Commands
{
    internal sealed class ReadCommand : Command
    {
        protected override string Name => "read";

        public ReadCommand(ArticlesManager articlesManager) => _articlesManager = articlesManager;

        public override Task ExecuteAsync(ITelegramBotClient client, ChatId chatId)
        {
            return _articlesManager.DeleteFirstArticleAsync(client, chatId);
        }

        private readonly ArticlesManager _articlesManager;
    }
}
