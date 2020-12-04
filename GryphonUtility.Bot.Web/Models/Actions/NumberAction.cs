using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class NumberAction : SupportedAction
    {
        public NumberAction(IBot bot, Message message, int number) : base(bot, message) { _number = number; }

        protected override Task ExecuteAsync() => Bot.ShopCommand.ProcessNumberAsync(Bot.Client, ChatId, _number);

        private readonly int _number;
    }
}
