using System;
using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class ArticleCommand : CommandSimple
{
    protected override byte Order => 2;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.OtherFeatures;

    public ArticleCommand(Bot bot, Manager manager)
        : base(bot, "article", bot.Config.Texts.ArticleCommandDescription)
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User _) => _manager.SendFirstArticleAsync(message.Chat);

    private readonly Manager _manager;
}