using System.Threading.Tasks;
using AbstractBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class CommandAction : SupportedAction
{
    public CommandAction(Bot bot, Message message, CommandBase command) : base(bot, message)
    {
        _command = command;
    }

    protected override Task ExecuteAsync(Chat chat) => _command.ExecuteAsync(Message, chat, null);

    private readonly CommandBase _command;
}