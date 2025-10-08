using System;
using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class AddArticle : Operation<Article>
{
    protected override byte Order => 4;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Admin;

    public AddArticle(Bot bot, Manager manager)
        : base(bot, bot.Config.Texts.AddArticleDescription)
    {
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out Article? data)
    {
        data = null;
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        if (message.ForwardFrom is not null || message.ReplyToMessage is not null)
        {
            return false;
        }

        data = Article.Parse(message.Text);
        return data is not null;
    }

    protected override Task ExecuteAsync(Article data, Message message, User sender)
    {
        return _manager.ProcessNewArticleAsync(message.Chat, data);
    }

    private readonly Manager _manager;
}