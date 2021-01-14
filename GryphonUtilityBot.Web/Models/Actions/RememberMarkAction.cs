using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class RememberMarkAction : SupportedAction
    {
        public RememberMarkAction(IBot bot, Message message, RecordsMarkQuery query) : base(bot, message)
        {
            _query = query;
        }

        protected override Task ExecuteAsync()
        {
            Bot.CurrentQuery = _query;
            Bot.CurrentQueryTime = Message.Date;
            return Task.CompletedTask;
        }

        protected override bool AllowedForMistress => true;

        private readonly RecordsMarkQuery _query;
    }
}
