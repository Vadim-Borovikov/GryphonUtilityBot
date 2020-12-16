using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class RememberMarkAction : SupportedAction
    {
        public RememberMarkAction(IBot bot, Message message, RecordsMarkQuery query) : base(bot, message)
        {
            _message = message;
            _query = query;
        }

        protected override Task ExecuteAsync()
        {
            Bot.CurrentQuery = _query;
            Bot.CurrentQueryTime = _message.Date;
            return Task.CompletedTask;
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _message;
        private readonly RecordsMarkQuery _query;
    }
}
