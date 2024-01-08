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

    protected override Access AccessLevel => Access.Admin;

    public FindOperation(Bot bot, Manager manager) : base(bot)
    {
        MenuDescription =
            $"*{AbstractBot.Bots.Bot.EscapeCharacters("пара дат, например \"1.02.20 1.03.22\"")}* – найти записи в этот период{Environment.NewLine}" +
            $"*{AbstractBot.Bots.Bot.EscapeCharacters("пара дат и теги, например \"1.02.20 1.03.22\" творчество вкусности")}* – найти записи в этот период с этими тегами";
        _manager = manager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        FindQuery? query = Check(message);
        if (query is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        await _manager.ProcessFindQueryAsync(message.Chat, query);
        return ExecutionResult.Success;
    }

    private static FindQuery? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || message.ReplyToMessage is not null)
        {
            return null;
        }

        return FindQuery.ParseFindQuery(message.Text);
    }

    private readonly Manager _manager;
}