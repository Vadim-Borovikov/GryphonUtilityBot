using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Shop;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ShopCommand : CommandBase<Config>
    {
        protected override string Name => "shop";
        protected override string Description => null;

        public ShopCommand(Bot bot) : base(bot) => _manager = bot.ShopManager;

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return _manager.ResetAndStartAskingAsync(message.Chat, Bot.Client);
        }

        private readonly Manager _manager;
    }
}
