using System.Threading.Tasks;
using GryphonUtilityBot.Bot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class CommandAction : SupportedAction
    {
        public CommandAction(Bot.Bot bot, Message message, Command command) : base(bot, message) => _command = command;

        protected override Task ExecuteAsync() => _command.ExecuteAsync(Bot.Client, Message.Chat);

        private readonly Command _command;
    }
}
