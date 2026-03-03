using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.Operations;
using GryphonUtilities.Time;
using GryphonUtilityBot.Money;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GryphonUtilityBot.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class AddReceipt : Operation<List<Transaction>>
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

    protected override bool IsInvokingBy(Message message, User? sender, out List<Transaction>? data)
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
            if (expense is not null)
            {
                expense.RegisterData();
                data = new List<Transaction> { expense };
            }
        }
        else
        {
            dateTimeFull = _bot.Core.Clock.GetDateTimeFull(message.ForwardDate.Value);
            data = _manager.TryParseReceipt(message.Text, dateTimeFull.DateOnly, texts, _bot.Core.Clock,
                _defaultCurrency, _defaultCity);
        }

        return data is not null;
    }

    protected override async Task ExecuteAsync(List<Transaction> data, Message message, User sender)
    {
        foreach (Transaction transaction in data)
        {
            switch (transaction)
            {
                case TransactionExpense expense:
                    await _manager.AddExpenseAsync(expense, message.Chat, message.MessageId);
                    break;
                case TransactionDebt debt:
                    await _manager.AddDebtAsync(debt, message.Chat, message.MessageId);
                    break;
                default: throw new InvalidOperationException("Unknown transaction type.");
            }
        }
    }

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly string _defaultCity;
    private readonly string _defaultCurrency;
    private readonly Manager _manager;
}