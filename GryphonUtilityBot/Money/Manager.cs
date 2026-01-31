using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Extensions;
using GryphonUtilityBot.Configs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Money;

internal sealed class Manager
{
    public Manager(Bot bot, Config config, ITextsProvider<Texts> textsProvider,
        GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _config = config;
        _textsProvider = textsProvider;

        GoogleSheetsManager.Documents.Document documentDebts = documentsManager.GetOrAdd(_config.GoogleSheetIdDebts);
        _sheetDebts = documentDebts.GetOrAddSheet(_config.GoogleTitleDebts);

        GoogleSheetsManager.Documents.Document documentExpences =
            documentsManager.GetOrAdd(_config.GoogleSheetIdExpenses);
        _sheetExpences = documentExpences.GetOrAddSheet(_config.GoogleTitleExpenses);
    }

    public async Task InitializeExpenseCategoriesAndPlacesAsync()
    {
        List<TransactionExpense> expenses =
            await _sheetExpences.LoadAsync<TransactionExpense>(_config.GoogleRangeExpenses);
        TransactionExpense.Categories.Clear();
        TransactionExpense.Venues.Clear();
        TransactionExpense.SmsNames.Clear();
        foreach (TransactionExpense expense in expenses)
        {
            expense.RegisterData();
        }
    }

    public async Task AddSimultaneousTransactionsAsync(List<TransactionDebt> transactions, DateOnly date, string note)
    {
        foreach (TransactionDebt t in transactions)
        {
            t.Date = date;
            t.Note = note;
        }

        await _sheetDebts.AddAsync(_config.GoogleRangeDebts, transactions);

        Texts texts = _textsProvider.GetDefaultTexts();

        string dateString = date.ToString(texts.DateOnlyFormat);

        List<MessageTemplateText> items = new();
        foreach (TransactionDebt t in transactions)
        {
            MessageTemplateText core = GetCore(t);
            MessageTemplateText item = texts.ListItemFormat.Format(core);
            items.Add(item);
        }
        MessageTemplateText list = MessageTemplateText.JoinTexts(items);

        MessageTemplateText formatted = texts.TransactionAddedFormat.Format(dateString, list, note);
        await formatted.SendAsync(_bot.Core.UpdateSender, _bot.Core.ReportsDefault);
    }

    public async Task AddExpenseAsync(TransactionExpense transaction, Chat chat, int replyToMessageId)
    {
        await _sheetExpences.AddAsync(_config.GoogleRangeDebts, transaction.WrapWithList());

        string dateString = transaction.Date.ToString(_config.Texts.DateOnlyFormat);

        string amount =
            string.Format(_config.Texts.TransactionExpenseAddedAmountFormat, transaction.Amount, transaction.Currency);

        MessageTemplateText? tail;
        if (string.IsNullOrWhiteSpace(transaction.Category))
        {
            tail = string.IsNullOrWhiteSpace(transaction.Note) ? null : new MessageTemplateText(transaction.Note);
        }
        else
        {
            string purpose = string.IsNullOrWhiteSpace(transaction.To)
                ? transaction.Category
                : string.Format(_config.Texts.TransactionExpenseAddedPlaceFormat, transaction.To,
                    transaction.Category);

            tail = string.IsNullOrWhiteSpace(transaction.Note)
                ? new MessageTemplateText(purpose)
                : _config.Texts.TransactionExpenseAddedTailFormat.Format(purpose, transaction.Note);
        }

        MessageTemplateText message =
            _config.Texts.TransactionAddedFormat.Format(dateString, amount, tail);
        message.ReplyParameters = new ReplyParameters { MessageId = replyToMessageId };
        await message.SendAsync(_bot.Core.UpdateSender, chat);
    }

    public async Task AddDebtAsync(TransactionDebt transaction, Chat chat, int replyToMessageId)
    {
        await _sheetDebts.AddAsync(_config.GoogleRangeDebts, transaction.WrapWithList());

        string dateString = transaction.Date.ToString(_config.Texts.DateOnlyFormat);
        MessageTemplateText core = GetCore(transaction);
        MessageTemplateText message =
            _config.Texts.TransactionAddedFormat.Format(dateString, core, transaction.Note);
        message.ReplyParameters = new ReplyParameters { MessageId = replyToMessageId };
        await message.SendAsync(_bot.Core.UpdateSender, chat);
    }

    private MessageTemplateText GetCore(TransactionDebt transaction)
    {
        Texts texts = _textsProvider.GetDefaultTexts();
        string name = transaction.From;
        Agent agent = texts.Agents[name];
        return texts.TransactionCoreFormat.Format(name, agent.Verb, transaction.To, transaction.Amount,
            transaction.Currency);
    }

    private readonly Bot _bot;
    private readonly Config _config;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly Sheet _sheetDebts;
    private readonly Sheet _sheetExpences;
}