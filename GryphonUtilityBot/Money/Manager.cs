using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using AbstractBot.Configs;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Money;

internal sealed class Manager
{
    public Manager(Bot bot, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_bot.Config.GoogleSheetId);
        _sheet = document.GetOrAddSheet(bot.Config.GoogleTitle);
        _itemVendorChat = new Chat
        {
            Id = bot.Config.ItemVendorId,
            Type = ChatType.Private
        };
    }

    public Task StartAsync() => _sheet.LoadTitlesAsync(_bot.Config.GoogleRange);

    public async Task AddTransactionAsync(Transaction transaction, Chat chat, int replyToMessageId)
    {
        await _sheet.AddAsync(_bot.Config.GoogleRange, transaction.WrapWithList());

        string verb = transaction.From == _bot.Config.Texts.FromDima
            ? _bot.Config.Texts.VerbDima
            : _bot.Config.Texts.VerbRita;
        string date = transaction.Date.ToString(_bot.Config.Texts.DateOnlyFormat);
        MessageTemplate formatted = _bot.Config.Texts.TransactionAddedFormat.Format(date, transaction.From, verb,
            transaction.To, transaction.Amount, transaction.Currency, transaction.Note);
        await formatted.SendAsync(_bot, chat, replyToMessageId: replyToMessageId);
    }

    public async Task ProcessSubmissionAsync(string name, Uri email, string telegram, List<string> items,
        List<Uri> slips)
    {
    }

    private readonly Bot _bot;
    private readonly Sheet _sheet;
    private readonly Chat _itemVendorChat;
}