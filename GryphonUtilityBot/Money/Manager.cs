using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AbstractBot.Configs;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Extensions;
using GryphonUtilityBot.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using AbstractBot;
using GryphonUtilityBot.Extensions;
using GryphonUtilityBot.Operations;
using Telegram.Bot.Types.ReplyMarkups;

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

    public async Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note,
        Chat chat, int replyToMessageId)
    {
        foreach (Transaction t in transactions)
        {
            t.Date = date;
            t.Note = note;
        }

        await _sheet.AddAsync(_bot.Config.GoogleRange, transactions);

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
        await formatted.SendAsync(_bot, chat, replyToMessageId: replyToMessageId);
    }

    public async Task AddTransactionAsync(Transaction transaction, Chat chat, int replyToMessageId)
    {
        await _sheet.AddAsync(_bot.Config.GoogleRange, transaction.WrapWithList());

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

    public async Task ProcessSubmissionAsync(string name, MailAddress email, string telegram, IList<byte> productIds,
        IReadOnlyList<Uri> slips)
    {
        List<MessageTemplate> productLines =
            productIds.Select(p => _bot.Config.Texts.ListItemFormat.Format(_bot.Config.Products[p].Name)).ToList();
        MessageTemplate productMessages = MessageTemplate.JoinTexts(productLines).Denull();

        MessageTemplate comfirmation = _bot.Config.Texts.PaymentConfirmationFormat.Format(productMessages);

        KeyboardProvider keyboard = CreateConfirmationKeyboard(name, email, telegram, productIds, slips);

        await comfirmation.SendAsync(_bot, _itemVendorChat, keyboard);
    }

    private KeyboardProvider CreateConfirmationKeyboard(string name, MailAddress email, string telegram,
        IEnumerable<byte> productIds, IReadOnlyList<Uri> slips)
    {
        List<List<InlineKeyboardButton>> rows = new();

        if (slips.Count == 1)
        {
            InlineKeyboardButton button = CreateUriButton(_bot.Config.Texts.PaymentSlipButtonCaption, slips.Single());
            rows.Add(button.WrapWithList());
        }
        else
        {
            for (int i = 0; i < slips.Count; ++i)
            {
                string caption = string.Format(_bot.Config.Texts.PaymentSlipButtonFormat,
                    _bot.Config.Texts.PaymentSlipButtonCaption, i + 1);
                InlineKeyboardButton button = CreateUriButton(caption, slips[i]);
                rows.Add(button.WrapWithList());
            }
        }

        InlineKeyboardButton confirm =
            CreateCallbackButton<AcceptPurchase>(_bot.Config.Texts.PaymentConfirmationButton, name, email, telegram,
                string.Join(ObjectExtensions.BytesSeparator, productIds));
        rows.Add(confirm.WrapWithList());

        return new InlineKeyboardMarkup(rows);
    }

    private static InlineKeyboardButton CreateUriButton(string caption, Uri uri)
    {
        return new InlineKeyboardButton(caption)
        {
            Url = uri.AbsoluteUri
        };
    }

    private static InlineKeyboardButton CreateCallbackButton<TCallback>(string caption, params object[]? args)
    {
        string data = typeof(TCallback).Name;
        if (args is not null)
        {
            data += string.Join(QuerySeparator, args.Select(o => o.ToString().Denull()));
        }
        return new InlineKeyboardButton(caption)
        {
            CallbackData = data
        };
    }

    private readonly Bot _bot;
    private readonly Sheet _sheet;
    private readonly Chat _itemVendorChat;

    public const string QuerySeparator = "_";
}