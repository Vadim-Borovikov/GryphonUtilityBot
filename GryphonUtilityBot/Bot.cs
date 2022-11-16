using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using GryphonUtilityBot.Actions;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseCustom<Config>
{
    public Bot(Config config) : base(config) { }

    protected override Task ProcessTextMessageAsync(Message textMessage, bool fromChat,
        CommandBase? command = null, string? payload = null)
    {
        SupportedAction? action = GetAction(textMessage, command);
        return action is null
            ? SendStickerAsync(textMessage.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker);
    }

    private SupportedAction? GetAction(Message message, CommandBase? command)
    {
        if (command is not null)
        {
            return new CommandAction(this, message, command);
        }
        return null;
    }
}