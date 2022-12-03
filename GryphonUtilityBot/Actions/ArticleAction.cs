using System.Threading.Tasks;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class ArticleAction : SupportedAction
{
    public ArticleAction(Bot bot, Message message, Article article) : base(bot, message) => _article = article;

    protected override Task ExecuteAsync(Chat chat) => Bot.ArticlesManager.ProcessNewArticleAsync(chat, _article);

    private readonly Article _article;
}