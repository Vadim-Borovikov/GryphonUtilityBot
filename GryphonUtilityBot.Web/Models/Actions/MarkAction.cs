using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class MarkAction : SupportedAction
    {
        public MarkAction(Bot bot, Message message, Message recordMessage, RecordsMarkQuery query)
            : base(bot, message)
        {
            _recordMessage = recordMessage;
            _query = query;
        }

        protected override Task ExecuteAsync()
        {
            return Bot.RecordsManager.Mark(Bot.Client, Message.Chat, _recordMessage, _query);
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _recordMessage;
        private readonly RecordsMarkQuery _query;
    }
}
