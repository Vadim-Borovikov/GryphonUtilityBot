using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class ForwardAction : SupportedAction
    {
        public ForwardAction(IBot bot, Message message) : base(bot, message) { _message = message; }

        protected override Task ExecuteAsync()
        {
            Bot.RecordsManager.SaveRecord(_message);
            return Bot.Client.SendTextMessageAsync(ChatId, "Запись сохранена в таймлайн.");
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _message;
    }
}
