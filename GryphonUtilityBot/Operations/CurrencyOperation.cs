using AbstractBot.Operations;
using System.Threading.Tasks;
using GryphonUtilityBot.Currency;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class CurrencyOperation : Operation
{
    protected override byte MenuOrder => 5;

    protected override Access AccessLevel => Access.Admins;

    public CurrencyOperation(Bot bot, Manager manager, InsuranceManager insuranceManager) : base(bot)
    {
        MenuDescription = "*число* – перевести сумму из одной валюты в несколько других";
        _manager = manager;
        _insuranceManager = insuranceManager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        decimal? number = Check(message);
        if (number is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _manager.ProcessNumberAsync(chat, number.Value);
        return ExecutionResult.Success;
    }

    private decimal? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || _insuranceManager.Active || message.ReplyToMessage is not null)
        {
            return null;
        }

        return decimal.TryParse(message.Text, out decimal number) ? number : null;
    }

    private readonly Manager _manager;
    private readonly InsuranceManager _insuranceManager;
}