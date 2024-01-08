using System;
using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class ArticleOperation : Operation
{
    protected override byte MenuOrder => 6;

    protected override Access AccessLevel => Access.SuperAdmin;

    public ArticleOperation(Bot bot, Manager manager) : base(bot)
    {
        MenuDescription =
            $"*ссылка* – добавить статью сегодняшним числом{Environment.NewLine}" +
            "*дата и ссылка* – добавить статью";
        _manager = manager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        Article? article = Check(message);
        if (article is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        await _manager.ProcessNewArticleAsync(message.Chat, article);
        return ExecutionResult.Success;
    }

    private static Article? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || message.ReplyToMessage is not null)
        {
            return null;
        }

        return Article.Parse(message.Text);
    }

    private readonly Manager _manager;
}