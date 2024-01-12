using System;
using AbstractBot.Operations;
using System.Threading.Tasks;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Money;
using Telegram.Bot.Types;
using GryphonUtilityBot.Operations.Info;

namespace GryphonUtilityBot.Operations;

internal sealed class AcceptPurchase : Operation<PurchaseInfo>
{
    protected override byte Order => 13;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Admin;

    public AcceptPurchase(Bot bot, Manager manager) : base(bot)
    {
        _bot = bot;
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out PurchaseInfo? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out PurchaseInfo? data)
    {
        data = PurchaseInfo.TryParse(callbackQueryDataCore);
        return data is not null;
    }

    protected override async Task ExecuteAsync(PurchaseInfo data, Message message, User sender)
    {
        DateOnly date = _bot.Clock.GetDateTimeFull(message.Date).DateOnly;

        foreach (byte id in data.ProductIds)
        {
            Product product = _bot.Config.Products[id];
            foreach (Transaction transaction in product.Transactions)
            {
                transaction.To = _bot.Config.Texts.Agents[transaction.To].To;
            }
            string note = string.Format(_bot.Config.Texts.ProductSoldNoteFormat, data.Name, product.Name);
            await _manager.AddSimultaneousTransactionsAsync(product.Transactions, date, note, message.Chat,
                message.MessageId);
        }
    }

    private readonly Bot _bot;
    private readonly Manager _manager;
}