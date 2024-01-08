using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Configs;
using AbstractBot.Operations;
using AbstractBot.Operations.Data;
using GryphonUtilities;
using GryphonUtilities.Time;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Operations;
using GryphonUtilityBot.Operations.Commands;
using JetBrains.Annotations;
using Telegram.Bot.Types;
namespace GryphonUtilityBot;

public sealed class Bot : BotWithSheets<Config, Texts, Data, CommandDataSimple>
{
    [Flags]
    internal enum AccessType
    {
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

        Manager articlesManager = new(this, DocumentsManager);

        Operations.Add(new ArticleCommand(this, articlesManager));
        Operations.Add(new ReadCommand(this, articlesManager));

        Operations.Add(new ArticleOperation(this, articlesManager));
        Operations.Add(new FindOperation(this, RecordsManager));
        Operations.Add(new ForwardOperation(this));
        Operations.Add(new RememberTagOperation(this));
        Operations.Add(new TagOperation(this, RecordsManager));
    }

    protected override Task ProcessInsufficientAccess(Message message, User sender, OperationBasic operation)
    {
        if (sender.Id != Config.MistressId)
        {
            return base.ProcessInsufficientAccess(message, sender, operation);
        }

        return SendTextMessageAsync(message.Chat,
            "Простите, госпожа, но господин заблокировал это действие даже для Вас.",
            replyToMessageId: message.MessageId);
    }

    internal Records.TagQuery? CurrentQuery;
    internal DateTimeFull CurrentQueryTime;

    internal readonly Records.Manager RecordsManager;
}