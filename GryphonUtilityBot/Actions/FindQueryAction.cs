using System.Threading.Tasks;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class FindQueryAction : SupportedAction
    {
        public FindQueryAction(Bot.Bot bot, Message message, FindQuery query) : base(bot, message) => _query = query;

        protected override Task ExecuteAsync()
        {
            return Bot.RecordsManager.ProcessFindQuery(Message.Chat, _query);
        }

        protected override bool AllowedForMistress => true;

        private readonly FindQuery _query;
    }
}
