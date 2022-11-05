using System.Threading.Tasks;
using AbstractBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class CommandAction : SupportedAction
{
    public CommandAction(Bot bot, Message message, CommandBase<Bot, Config> command) : base(bot, message)
    {
        _command = command;
    }

    protected override Task ExecuteAsync() => _command.ExecuteAsync(Message, false, null);

    private readonly CommandBase<Bot, Config> _command;
}