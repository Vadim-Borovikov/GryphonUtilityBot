using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using AbstractBot.Operations.Data;
using GryphonUtilities;
using GryphonUtilities.Time;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Money;
using GryphonUtilityBot.Operations;
using GryphonUtilityBot.Operations.Commands;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Records = 2,
        OtherFeatures = 4,

        Mistress = Default | Records, // 3
        [UsedImplicitly]
        Admin = Mistress | OtherFeatures // 7
    }

    public Bot(Config config) : base(config)
    {
        SaveManager<Data> saveManager = new(config.SavePath, Clock);
        RecordsManager = new Records.Manager(this, saveManager);

        Articles.Manager articlesManager = new(this, DocumentsManager);

        _financemanager = new Manager(this, DocumentsManager);
        Operations.Add(new AddReceipt(this, _financemanager));

        Operations.Add(new ArticleCommand(this, articlesManager));
        Operations.Add(new ReadCommand(this, articlesManager));

        Operations.Add(new AddArticle(this, articlesManager));

        Operations.Add(new FindRecord(this, RecordsManager));
        Operations.Add(new AddRecord(this));
        Operations.Add(new RememberTag(this));
        Operations.Add(new TagRecord(this, RecordsManager));
    }

    public Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note)
    {
        return _financemanager.AddSimultaneousTransactionsAsync(transactions, date, note);
    }

    protected override Task ProcessInsufficientAccess(Message message, User sender, OperationBasic operation)
    {
        if (sender.Id != Config.MistressId)
        {
            return base.ProcessInsufficientAccess(message, sender, operation);
        }

        Config.Texts.ForbiddenForMistress.ReplyToMessageId = message.MessageId;
        return Config.Texts.ForbiddenForMistress.SendAsync(this, message.Chat);
    }

    internal Records.TagQuery? CurrentQuery;
    internal DateTimeFull CurrentQueryTime;

    internal readonly Records.Manager RecordsManager;

    private readonly Manager _financemanager;
}