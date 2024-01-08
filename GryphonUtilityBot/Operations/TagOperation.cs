﻿using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class TagOperation : Operation
{
    protected override byte MenuOrder => 9;

    protected override Access AccessLevel => Access.Admin;

    public TagOperation(Bot bot, Manager manager) : base(bot)
    {
        MenuDescription = "*ответить на сообщение, которое переслали раньше* – добавить теги к записи";
        _manager = manager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        TagQuery? query = Check(message);
        if (query is null || message.ReplyToMessage?.ForwardFrom is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        await _manager.TagAsync(message.Chat, message.ReplyToMessage, query);
        return ExecutionResult.Success;
    }

    private static TagQuery? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null)
        {
            return null;
        }

        return TagQuery.ParseTagQuery(message.Text);
    }

    private readonly Manager _manager;
}