using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseCustom<Config>
{
    public Bot(Config config) : base(config)
    {
        InsuranceManager = new InsuranceManager(this);
        Commands.Add(new InsuranceCommand(this));
    }

    protected override Task ProcessTextMessageAsync(Message textMessage, Chat senderChat, CommandBase? command = null,
        string? payload = null)
    {
        SupportedAction? action = GetAction(textMessage, command);
        return action is null
            ? SendStickerAsync(textMessage.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker, senderChat);
    }

    private SupportedAction? GetAction(Message message, CommandBase? command)
    {
        if (command is not null)
        {
            InsuranceManager.Reset();
            return new CommandAction(this, message, command);
        }

        if (InsuranceManager.Active)
        {
            return new InsuranceAction(this, message, InsuranceManager);
        }

        return null;
    }

    internal readonly InsuranceManager InsuranceManager;
}