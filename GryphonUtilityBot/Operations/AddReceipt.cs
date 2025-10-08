using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.Operations;
using GryphonUtilities.Time;
using GryphonUtilityBot.Money;
using System;
using System.Threading.Tasks;
using GryphonUtilityBot.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class AddReceipt : Operation<Transaction>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public AddReceipt(Bot bot, ITextsProvider<Texts> textsProvider, string defaultCurrency, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _textsProvider = textsProvider;
        _defaultCurrency = defaultCurrency;
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

        Texts texts = _textsProvider.GetTextsFor(sender.Id);

        DateTimeFull dateTimeFull = _bot.Core.Clock.GetDateTimeFull(message.ForwardDate.Value);
        data = Transaction.TryParseReceipt(message.Text, dateTimeFull.DateOnly, texts, _bot.Core.Clock,
            _defaultCurrency);
        return data is not null;
    }

    protected override async Task ExecuteAsync(Transaction data, Message message, User sender)
    {
        await _manager.AddTransactionAsync(data, message.Chat, message.MessageId);
    }

    private readonly Bot _bot;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly string _defaultCurrency;
    private readonly Manager _manager;
}