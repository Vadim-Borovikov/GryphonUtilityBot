using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class ForwardAction : SupportedAction
    {
        public ForwardAction(IBot bot, Message message, HashSet<string> tags) : base(bot, message)
        {
            _message = message;
            _tags = tags;
        }

        protected override Task ExecuteAsync()
        {
            Bot.RecordsManager.SaveRecord(_message, _tags);
            return Task.CompletedTask;
        }

        protected override bool AllowedForMistress => true;

        private readonly Message _message;
        private readonly HashSet<string> _tags;
    }
}
