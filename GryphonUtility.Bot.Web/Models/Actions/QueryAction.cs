using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class QueryAction : SupportedAction
    {
        public QueryAction(IBot bot, Message message, RecordsQuery query) : base(bot, message) => _query = query;

        protected override Task ExecuteAsync() => Bot.RecordsManager.ProcessQuery(Bot.Client, ChatId, _query);

        protected override bool AllowedForMistress => true;

        private readonly RecordsQuery _query;
    }
}
