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

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdTransactions);
        _sheet = document.GetOrAddSheet(_config.GoogleTitleTransactions);
    }

    public async Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note)
    {
        foreach (Transaction t in transactions)
        {
            t.Date = date;
            t.Note = note;
        }

        await _sheet.AddAsync(_config.GoogleRangeTransactions, transactions);

        Texts texts = _textsProvider.GetDefaultTexts();

        string dateString = date.ToString(texts.DateOnlyFormat);

        List<MessageTemplateText> items = new();
        foreach (Transaction t in transactions)
        {
            MessageTemplateText core = GetCore(t);
            MessageTemplateText item = texts.ListItemFormat.Format(core);
            items.Add(item);
        }
        MessageTemplateText list = MessageTemplateText.JoinTexts(items);

        MessageTemplateText formatted = texts.TransactionAddedFormat.Format(dateString, list, note);
        await formatted.SendAsync(_bot.Core.UpdateSender, _bot.Core.ReportsDefault);
    }

    public async Task AddTransactionAsync(Transaction transaction, Chat chat, int replyToMessageId)
    {
        await _sheet.AddAsync(_config.GoogleRangeTransactions, transaction.WrapWithList());

        string dateString = transaction.Date.ToString(_config.Texts.DateOnlyFormat);
        MessageTemplateText core = GetCore(transaction);
        MessageTemplateText formatted =
            _config.Texts.TransactionAddedFormat.Format(dateString, core, transaction.Note);
        formatted.ReplyParameters = new ReplyParameters { MessageId = replyToMessageId };
        await formatted.SendAsync(_bot.Core.UpdateSender, chat);
    }

    private MessageTemplateText GetCore(Transaction transaction)
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
    private readonly Sheet _sheet;
}