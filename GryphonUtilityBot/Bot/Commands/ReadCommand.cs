using System.Threading.Tasks;
using GryphonUtilityBot.Articles;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ReadCommand : Command
    {
        protected override string Name => "read";

        public ReadCommand(Manager articlesManager) => _articlesManager = articlesManager;

        public override Task ExecuteAsync(ITelegramBotClient client, ChatId chatId)
        {
            return _articlesManager.DeleteFirstArticleAsync(client, chatId);
        }

        private readonly Manager _articlesManager;
    }
}
