using System.Threading.Tasks;
using AbstractBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ArticleCommand : CommandBaseCustom<Bot, Config>
{
    public ArticleCommand(Bot bot) : base(bot, "article", "первая статья") { }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload)
    {
        return Bot.ArticlesManager.SendFirstArticleAsync(chat);
    }
}