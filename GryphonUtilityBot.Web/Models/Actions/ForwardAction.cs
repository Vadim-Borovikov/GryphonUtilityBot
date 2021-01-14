using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class ForwardAction : SupportedAction
    {
        public ForwardAction(IBot bot, Message message) : base(bot, message) { }

        protected override Task ExecuteAsync()
        {
            Bot.RecordsManager.SaveRecord(Message, Bot.CurrentQuery);
            return Task.CompletedTask;
        }

        protected override bool AllowedForMistress => true;
    }
}
