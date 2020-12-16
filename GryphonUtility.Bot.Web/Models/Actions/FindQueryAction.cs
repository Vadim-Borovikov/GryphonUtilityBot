using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class FindQueryAction : SupportedAction
    {
        public FindQueryAction(IBot bot, Message message, RecordsFindQuery query) : base(bot, message) => _query = query;

        protected override Task ExecuteAsync() => Bot.RecordsManager.ProcessFindQuery(Bot.Client, ChatId, _query);

        protected override bool AllowedForMistress => true;

        private readonly RecordsFindQuery _query;
    }
}
