using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Operations;
using GoogleSheetsManager.Providers;
using GryphonUtilities;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using GryphonUtilityBot.Operations;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseCustom<Config>, IDisposable
{
    internal readonly SheetsProvider GoogleSheetsProvider;
    internal readonly Dictionary<Type, Func<object?, object?>> AdditionalConverters;

    public Bot(Config config) : base(config)
    {
        GoogleSheetsProvider = new SheetsProvider(config, config.GoogleSheetId);
        AdditionalConverters = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(Uri), Utils.ToUri }
        };
        AdditionalConverters[typeof(DateOnly)] = AdditionalConverters[typeof(DateOnly?)] = o => GetDateOnly(o);

        SaveManager<Data> saveManager = new(config.SavePath, TimeManager);
        RecordsManager = new Records.Manager(this, saveManager);

        Manager articlesManager = new(this);
        _currencyManager = new Currency.Manager(this);
        InsuranceManager = new InsuranceManager(this);

        Operations.Add(new ArticleCommand(this, articlesManager));
        Operations.Add(new InsuranceCommand(this, InsuranceManager));
        Operations.Add(new ReadCommand(this, articlesManager));

        Operations.Add(new ArticleOperation(this, articlesManager, InsuranceManager));
        Operations.Add(new CurrencyOperation(this, _currencyManager, InsuranceManager));
        Operations.Add(new FindOperation(this, RecordsManager, InsuranceManager));
        Operations.Add(new ForwardOperation(this));
        Operations.Add(new InsuranceOperation(this, InsuranceManager));
        Operations.Add(new RememberTagOperation(this));
        Operations.Add(new TagOperation(this, RecordsManager, InsuranceManager));
    }

    public void Dispose() => GoogleSheetsProvider.Dispose();

    protected override Task ProcessInsufficientAccess(Message message, Chat sender, Operation operation)
    {
        if (sender.Id != Config.MistressId)
        {
            return base.ProcessInsufficientAccess(message, sender, operation);
        }

        Chat chat = GetReplyChatFor(message, sender);
        return SendTextMessageAsync(chat,
            "Простите, госпожа, но господин заблокировал это действие даже для Вас.", replyToMessageId: message.MessageId);
    }

    protected override Task UpdateAsync(CallbackQuery callback)
    {
        if (callback.Data is null)
        {
            throw new NullReferenceException(nameof(callback.Data));
        }
        return _currencyManager.ChangeCurrency(callback.Data);
    }

    private DateOnly? GetDateOnly(object? o)
    {
        if (o is DateOnly d)
        {
            return d;
        }

        DateTimeFull? dtf = GoogleSheetsManager.Utils.GetDateTimeFull(o, GoogleSheetsProvider.TimeManager);
        return dtf?.DateOnly;
    }

    internal Records.TagQuery? CurrentQuery;
    internal DateTimeFull CurrentQueryTime;

    internal readonly Records.Manager RecordsManager;
    internal readonly InsuranceManager InsuranceManager;

    private readonly Currency.Manager _currencyManager;
}