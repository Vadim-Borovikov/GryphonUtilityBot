using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Shop;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ShopCommand : CommandBase
    {
        protected override string Name => "shop";
        protected override string Description => null;

        public ShopCommand(Manager manager) => _manager = manager;


        public override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            return _manager.ResetAndStartAskingAsync(chatId, client);
        }

        private readonly Manager _manager;
    }
}
