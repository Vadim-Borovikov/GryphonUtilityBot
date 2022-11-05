using System.Threading.Tasks;
using AbstractBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ArticleCommand : CommandBase<Bot, Config>
{
    public ArticleCommand(Bot bot) : base(bot, "article", "первая статья") { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        return Bot.ArticlesManager.SendFirstArticleAsync(message.Chat);
    }
}