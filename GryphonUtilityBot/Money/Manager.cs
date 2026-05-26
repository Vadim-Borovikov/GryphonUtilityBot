using AbstractBot.Interfaces.Modules;
using AbstractBot.Models;
using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Extensions;
using GryphonUtilities.Time;
using GryphonUtilityBot.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task UpdateExpensesDataAsync(Chat chat, long userId)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);

        await using (await StatusMessage.CreateAsync(_bot.Core.UpdateSender, chat, texts.UpdatingExpenses,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat))
        {
            SheetLoadedData<TransactionExpense> expenses =
                await _sheetExpences.LoadAsync<TransactionExpense>(_config.GoogleRangeExpenses);
            TransactionExpense.Categories.Clear();
            TransactionExpense.Venues.Clear();
            TransactionExpense.SmsNames.Clear();
            foreach (TransactionExpense expense in expenses.Instances)
            {
                expense.RegisterData();
            }
        }
    }

    public async Task AddSimultaneousTransactionsAsync(List<TransactionDebt> transactions, DateOnly date, string note)
    {
        foreach (TransactionDebt t in transactions)
        {
            t.Date = date;
            t.Note = note;
        }

        await _sheetDebts.AddAsync(transactions, _config.GoogleRangeDebts);

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
        await _sheetExpences.AddAsync(transaction.WrapWithList(), _config.GoogleRangeDebts);

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
        await _sheetDebts.AddAsync(transaction.WrapWithList(), _config.GoogleRangeDebts);

        string dateString = transaction.Date.ToString(_config.Texts.DateOnlyFormat);
        MessageTemplateText core = GetCore(transaction);
        MessageTemplateText message =
            _config.Texts.TransactionAddedFormat.Format(dateString, core, transaction.Note);
        message.ReplyParameters = new ReplyParameters { MessageId = replyToMessageId };
        await message.SendAsync(_bot.Core.UpdateSender, chat);
    }

    internal List<Transaction>? TryParseReceipt(string s, DateOnly defaultDate, Texts texts, Clock clock,
        string defaultCurrency, string defaultCity)
    {
        List<string> parts = s.Split(null).Where(p => p.Length > 0).ToList();

        int index = 0;
        if (parts.Count <= index)
        {
            return null;
        }

        string tag = parts[index];
        bool foodTransaction = tag.EndsWith(texts.FoodTagPostfix, StringComparison.Ordinal);
        if (foodTransaction)
        {
            tag = tag.Substring(0, tag.Length - texts.FoodTagPostfix.Length);
        }

        string? name = texts.TryGetAgent(tag);
        if (name is null)
        {
            return null;
        }

        string? partner = texts.TryGetPartner(texts.Agents[name]);
        if (partner is null)
        {
            return null;
        }

        ++index;
        if (parts.Count <= index)
        {
            return null;
        }

        decimal? amount = parts[index].ToDecimal();
        if (amount is null)
        {
            return null;
        }
        ++index;

        DateOnly date = defaultDate;
        DateOnly? result = parts[index].ToDateOnly(clock);
        if (result.HasValue)
        {
            date = result.Value;
            ++index;
            if (parts.Count <= index)
            {
                return null;
            }
        }

        string note = string.Join(" ", parts.Skip(index));

        TransactionDebt debt;
        if (foodTransaction)
        {
            decimal primaryAmount = Math.Round(_config.PrimaryFoodAgentShare * amount.Value, 2);

            decimal debtAmount = name == texts.PrimaryFoodAgent ? amount.Value - primaryAmount : primaryAmount;
            debt = new TransactionDebt(name, texts.Agents[partner].To, date, debtAmount, defaultCurrency, note);

            TransactionExpense expense = new(texts.FoodCategory, null, date, primaryAmount, defaultCurrency,
                defaultCity, note);

            return new List<Transaction> { debt, expense };
        }

        debt = new TransactionDebt(name, texts.Agents[partner].To, date, amount.Value, defaultCurrency, note);
        return new List<Transaction> { debt };
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