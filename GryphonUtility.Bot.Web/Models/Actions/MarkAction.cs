using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class MarkAction : SupportedAction
    {
        public MarkAction(IBot bot, Message message, Message recordMessage, RecordsMarkQuery query)
            : base(bot, message)
        {
            _recordMessage = recordMessage;
            _query = query;
        }

        protected override Task ExecuteAsync()
        {
            return Bot.RecordsManager.Mark(Bot.Client, ChatId, _recordMessage, _query);
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _recordMessage;
        private readonly RecordsMarkQuery _query;
    }
}
