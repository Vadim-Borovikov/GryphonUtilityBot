using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBase<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
        CurrencyManager = new Currency.Manager(this);
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

        if (decimal.TryParse(message.Text, out decimal number))
        {
            return new NumberAction(this, message, number);
        }

        return null;
    }

    internal readonly Currency.Manager CurrencyManager;
}