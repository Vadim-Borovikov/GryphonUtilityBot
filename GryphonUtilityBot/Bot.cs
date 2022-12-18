using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using GryphonUtilities;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using GryphonUtilityBot.Operations;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotWithSheets<Config>
{
    public Bot(Config config) : base(config)
    {
        SaveManager<Data> saveManager = new(config.SavePath, TimeManager);
        RecordsManager = new Records.Manager(this, saveManager);

        Manager articlesManager = new(this, DocumentsManager);
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

    protected override Task ProcessInsufficientAccess(Message message, long senderId, Operation operation)
    {
        if (senderId != Config.MistressId)
        {
            return base.ProcessInsufficientAccess(message, senderId, operation);
        }

        return SendTextMessageAsync(message.Chat,
            "Простите, госпожа, но господин заблокировал это действие даже для Вас.",
            replyToMessageId: message.MessageId);
    }

    protected override Task UpdateAsync(CallbackQuery callback)
    {
        if (callback.Data is null)
        {
            throw new NullReferenceException(nameof(callback.Data));
        }
        return _currencyManager.ChangeCurrency(callback.Data);
    }

    internal Records.TagQuery? CurrentQuery;
    internal DateTimeFull CurrentQueryTime;

    internal readonly Records.Manager RecordsManager;
    internal readonly InsuranceManager InsuranceManager;

    private readonly Currency.Manager _currencyManager;
}