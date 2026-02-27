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

    public AddReceipt(Bot bot, Config config, ITextsProvider<Texts> textsProvider, string defaultCurrency,
        string defaultCity, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _config = config;
        _textsProvider = textsProvider;
        _defaultCurrency = defaultCurrency;
        _defaultCity = defaultCity;
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User? sender, out Transaction? data)
    {
        data = null;

        if (sender is null)
        {
            return false;
        }

        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        Texts texts = _textsProvider.GetTextsFor(sender.Id);

        DateTimeFull dateTimeFull;
        if (message.ForwardDate is null)
        {
            dateTimeFull = _bot.Core.Clock.GetDateTimeFull(message.Date);
            TransactionExpense? expense =
                TransactionExpense.TryParseReceipt(message.Text, dateTimeFull.DateOnly, _bot.Core.Clock,
                    _defaultCurrency, _defaultCity)
                ?? TransactionExpense.TryParseSms(message.Text, _bot.Core.Clock, _defaultCurrency, _defaultCity,
                    _config.SmsSeparator);
            expense?.RegisterData();
            data = expense;
        }
        else
        {
            dateTimeFull = _bot.Core.Clock.GetDateTimeFull(message.ForwardDate.Value);
            data = TransactionDebt.TryParseReceipt(message.Text, dateTimeFull.DateOnly, texts, _bot.Core.Clock,
                _defaultCurrency);
        }

        return data is not null;
    }

    protected override Task ExecuteAsync(Transaction data, Message message, User sender)
    {
        return data switch
        {
            TransactionExpense expense => _manager.AddExpenseAsync(expense, message.Chat, message.MessageId),
            TransactionDebt debt       => _manager.AddDebtAsync(debt, message.Chat, message.MessageId),
            _                          => throw new InvalidOperationException("Unknown transaction type.")
        };
    }

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly string _defaultCity;
    private readonly string _defaultCurrency;
    private readonly Manager _manager;
}