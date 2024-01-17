using System;
using AbstractBot.Operations;
using System.Threading.Tasks;
using GryphonUtilities.Time;
using GryphonUtilityBot.Money;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class AddReceipt : Operation<Transaction>
{
    protected override byte Order => 5;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.OtherFeatures;

    public AddReceipt(Bot bot, Manager manager) : base(bot)
    {
        Description = bot.Config.Texts.AddReceiptDescription;
        _bot = bot;
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out Transaction? data)
    {
        data = null;

        if (message.ForwardDate is null)
        {
            return false;
        }

        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        DateTimeFull dateTimeFull = _bot.Clock.GetDateTimeFull(message.ForwardDate.Value);
        data = Transaction.TryParseReceipt(message.Text, dateTimeFull.DateOnly, _bot.Config.Texts,
            _bot.Config.DefaultCurrency);
        return data is not null;
    }

    protected override async Task ExecuteAsync(Transaction data, Message message, User sender)
    {
        await _manager.AddTransactionAsync(data, message.Chat, message.MessageId);
    }

    private readonly Bot _bot;
    private readonly Manager _manager;
}