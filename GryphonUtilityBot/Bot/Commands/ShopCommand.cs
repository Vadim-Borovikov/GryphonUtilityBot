using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ShopCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "shop";
        protected override string Description => null;

        public ShopCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return Bot.ShopManager.ResetAndStartAskingAsync(message.Chat);
        }
    }
}
