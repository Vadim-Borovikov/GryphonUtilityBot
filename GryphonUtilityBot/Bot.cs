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
        _insuranceManager = new InsuranceManager(this, config.InsuranceMessageFormat, config.DefaultAddress,
            config.ArrivalDate);
        InsuranceCommand insuranceCommand = new(this, _insuranceManager);

        Commands.Add(insuranceCommand);
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
            _insuranceManager.Reset();
            return new CommandAction(this, message, command);
        }

        if (_insuranceManager.Active)
        {
            return new InsuranceAction(this, message, _insuranceManager);
        }

        return null;
    }

    private readonly InsuranceManager _insuranceManager;
}