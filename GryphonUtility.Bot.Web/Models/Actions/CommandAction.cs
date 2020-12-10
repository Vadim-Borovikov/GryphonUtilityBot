using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Commands;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal sealed class CommandAction : SupportedAction
    {
        public CommandAction(IBot bot, Message message, Command command) : base(bot, message) => _command = command;

        protected override Task ExecuteAsync() => _command.ExecuteAsync(Bot.Client, ChatId);

        private readonly Command _command;
    }
}
