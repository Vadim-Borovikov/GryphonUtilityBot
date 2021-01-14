using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal sealed class CommandAction : SupportedAction
    {
        public CommandAction(IBot bot, Message message, Command command) : base(bot, message) => _command = command;

        protected override Task ExecuteAsync() => _command.ExecuteAsync(Bot.Client, Message.Chat);

        private readonly Command _command;
    }
}
