using System;
using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class ReadCommand : CommandSimple
{
    protected override byte Order => 3;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.OtherFeatures;

    public ReadCommand(Bot bot, Manager manager) : base(bot, "read", "удалить статью и выдать следующую")
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User _) => _manager.DeleteFirstArticleAsync(message.Chat);

    private readonly Manager _manager;
}