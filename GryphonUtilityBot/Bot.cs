using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBase<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new StartCommand(this));

        await base.StartAsync(cancellationToken);
    }

    protected override Task ProcessTextMessageAsync(Message textMessage, bool fromChat,
        CommandBase<Bot, Config>? command = null, string? payload = null)
    {
        SupportedAction? action = GetAction(textMessage, command);
        return action is null
            ? SendStickerAsync(textMessage.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker);
    }

    private SupportedAction? GetAction(Message message, CommandBase<Bot, Config>? command)
    {
        if (command is not null)
        {
            return new CommandAction(this, message, command);
        }
        return null;
    }
}