using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class TagAction : SupportedAction
    {
        public TagAction(IBot bot, Message message, Message recordMessage, HashSet<string> tags)
            : base(bot, message)
        {
            _recordMessage = recordMessage;
            _tags = tags;
        }

        protected override Task ExecuteAsync()
        {
            return Bot.RecordsManager.SetTags(Bot.Client, ChatId, _recordMessage, _tags);
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _recordMessage;
        private readonly HashSet<string> _tags;
    }
}
