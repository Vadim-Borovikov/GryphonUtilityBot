using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class NumberAction : SupportedAction
    {
        public NumberAction(Bot bot, Message message, int number) : base(bot, message) => _number = number;

        protected override Task ExecuteAsync() => Bot.ShopManager.ProcessNumberAsync(Message.Chat, _number);

        private readonly int _number;
    }
}
