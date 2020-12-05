using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class RememberTagsAction : SupportedAction
    {
        public RememberTagsAction(IBot bot, Message message, HashSet<string> tags) : base(bot, message)
        {
            _message = message;
            _tags = tags;
        }

        protected override Task ExecuteAsync()
        {
            Bot.CurrentTags = _tags;
            Bot.CurrentTagsTime = _message.Date;
            return Task.CompletedTask;
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _message;
        private readonly HashSet<string> _tags;
    }
}
