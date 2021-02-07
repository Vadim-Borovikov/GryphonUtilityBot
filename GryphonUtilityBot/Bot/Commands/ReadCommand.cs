using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Articles;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ReadCommand : CommandBase
    {
        protected override string Name => "read";
        protected override string Description => null;

        public ReadCommand(Manager articlesManager) => _articlesManager = articlesManager;

        public override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            return _articlesManager.DeleteFirstArticleAsync(client, chatId);
        }

        private readonly Manager _articlesManager;
    }
}
