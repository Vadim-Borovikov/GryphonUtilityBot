using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ArticleCommand : CommandOperation
{
    protected override byte MenuOrder => 2;

    protected override Access AccessLevel => Access.SuperAdmin;

    public ArticleCommand(Bot bot, Manager manager) : base(bot, "article", "первая статья") => _manager = manager;

    protected override Task ExecuteAsync(Message message, long _, string? __)
    {
        return _manager.SendFirstArticleAsync(message.Chat);
    }

    private readonly Manager _manager;
}