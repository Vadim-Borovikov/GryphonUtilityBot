using System.Threading.Tasks;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class RememberMarkAction : SupportedAction
    {
        public RememberMarkAction(Bot bot, Message message, MarkQuery query) : base(bot, message)
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

        private readonly MarkQuery _query;
    }
}
