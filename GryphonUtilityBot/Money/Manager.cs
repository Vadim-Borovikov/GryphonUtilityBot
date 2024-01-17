using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using AbstractBot.Configs;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Extensions;
using GryphonUtilityBot.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Money;

internal sealed class Manager
{
    public Manager(Bot bot, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        GoogleSheetsManager.Documents.Document document =
            documentsManager.GetOrAdd(_bot.Config.GoogleSheetIdTransactions);
        _sheet = document.GetOrAddSheet(bot.Config.GoogleTitleTransactions);
        _transactionLogsChat = new Chat
        {
            Id = bot.Config.TransactionLogsChatId,
            Type = ChatType.Private
        };
    }

    public Task StartAsync() => _sheet.LoadTitlesAsync(_bot.Config.GoogleRangeTransactions);

    public async Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note)
    {
        foreach (Transaction t in transactions)
        {
            t.Date = date;
            t.Note = note;
        }

        await _sheet.AddAsync(_bot.Config.GoogleRangeTransactions, transactions);

        string dateString = date.ToString(_bot.Config.Texts.DateOnlyFormat);

        List<MessageTemplate> items = new();
        foreach (Transaction t in transactions)
        {
            MessageTemplate core = GetCore(t);
            MessageTemplate item = _bot.Config.Texts.ListItemFormat.Format(core);
            items.Add(item);
        }
        MessageTemplate list = MessageTemplate.JoinTexts(items).Denull();

        MessageTemplate formatted = _bot.Config.Texts.TransactionAddedFormat.Format(dateString, list, note);
        await formatted.SendAsync(_bot, _transactionLogsChat);
    }

    public async Task AddTransactionAsync(Transaction transaction, Chat chat, int replyToMessageId)
    {
        await _sheet.AddAsync(_bot.Config.GoogleRangeTransactions, transaction.WrapWithList());

        string dateString = transaction.Date.ToString(_bot.Config.Texts.DateOnlyFormat);
        MessageTemplate core = GetCore(transaction);
        MessageTemplate formatted =
            _bot.Config.Texts.TransactionAddedFormat.Format(dateString, core, transaction.Note);
        await formatted.SendAsync(_bot, chat, replyToMessageId: replyToMessageId);
    }

    private MessageTemplate GetCore(Transaction transaction)
    {
        string name = transaction.From;
        Agent agent = _bot.Config.Texts.Agents[name];
        return _bot.Config.Texts.TransactionCoreFormat.Format(name, agent.Verb, transaction.To, transaction.Amount,
            transaction.Currency);
    }

    private readonly Bot _bot;
    private readonly Sheet _sheet;
    private readonly Chat _transactionLogsChat;
}