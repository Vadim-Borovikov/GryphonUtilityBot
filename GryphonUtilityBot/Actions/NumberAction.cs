using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class NumberAction : SupportedAction
    {
        public NumberAction(Bot bot, Message message, decimal number) : base(bot, message) => _number = number;

        protected override Task ExecuteAsync()
        {
            if (Bot.ShopManager.HasCurrentItem && (_number == Math.Floor(_number)))
            {
                return Bot.ShopManager.ProcessNumberAsync(Message.Chat, (int) _number);
            }
            return Bot.CurrencyManager.ProcessNumberAsync(Message.Chat, _number);
        }

        private readonly decimal _number;
    }
}
