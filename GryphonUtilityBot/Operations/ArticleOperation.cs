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

    public ArticleOperation(Bot bot, Manager manager, InsuranceManager insuranceManager) : base(bot)
    {
        MenuDescription =
            $"*ссылка* – добавить статью сегодняшним числом{Environment.NewLine}" +
            "*дата и ссылка* – добавить статью";
        _manager = manager;
        _insuranceManager = insuranceManager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        Article? article = Check(message);
        if (article is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _manager.ProcessNewArticleAsync(chat, article);
        return ExecutionResult.Success;
    }

    private Article? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || _insuranceManager.Active || message.ReplyToMessage is not null)
        {
            return null;
        }

        return Article.Parse(message.Text);
    }

    private readonly Manager _manager;
    private readonly InsuranceManager _insuranceManager;
}