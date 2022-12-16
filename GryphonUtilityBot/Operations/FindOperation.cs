using System;
using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class FindOperation : Operation
{
    protected override byte MenuOrder => 10;

    protected override Access AccessLevel => Access.Admins;

    public FindOperation(Bot bot, Manager manager, InsuranceManager insuranceManager) : base(bot)
    {
        MenuDescription =
            $"*пара дат, например \"1\\.02\\.20 1\\.03\\.22\"* – найти записи в этот период{Environment.NewLine}" +
            "*пара дат и теги, например \"1\\.02\\.20 1\\.03\\.22 творчество вкусности\"* – найти записи в этот период с этими тегами";
        _manager = manager;
        _insuranceManager = insuranceManager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        FindQuery? query = Check(message);
        if (query is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _manager.ProcessFindQueryAsync(chat, query);
        return ExecutionResult.Success;
    }

    private FindQuery? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || _insuranceManager.Active || message.ReplyToMessage is not null)
        {
            return null;
        }

        return FindQuery.ParseFindQuery(message.Text);
    }

    private readonly Manager _manager;
    private readonly InsuranceManager _insuranceManager;
}