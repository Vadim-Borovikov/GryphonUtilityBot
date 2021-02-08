using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions
{
    internal sealed class CommandAction : SupportedAction
    {
        public CommandAction(Bot.Bot bot, Message message, CommandBase<Bot.Bot, Bot.Config> command)
            : base(bot, message)
        {
            _command = command;
        }

        protected override Task ExecuteAsync() => _command.ExecuteAsync(Message);

        private readonly CommandBase<Bot.Bot, Bot.Config> _command;
    }
}
