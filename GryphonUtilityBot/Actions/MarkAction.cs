using System.Threading.Tasks;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class MarkAction : SupportedAction
    {
        public MarkAction(Bot.Bot bot, Message message, Message recordMessage, MarkQuery query)
            : base(bot, message)
        {
            _recordMessage = recordMessage;
            _query = query;
        }

        protected override Task ExecuteAsync() => Bot.RecordsManager.Mark(Message.Chat, _recordMessage, _query);

        protected override bool AllowedForMistress => true;

        private readonly Message _recordMessage;
        private readonly MarkQuery _query;
    }
}
