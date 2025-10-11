using AbstractBot.Models.Operations;
using GryphonUtilityBot.Operations.Data;
using GryphonUtilityBot.Timeline;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class ConfirmTimelineDuplicatesDeletion : Operation<ConfirmTimelineDuplicatesDeletionData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ConfirmTimelineDuplicatesDeletion(Bot bot, Manager manager) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore,
        out ConfirmTimelineDuplicatesDeletionData? data)
    {
        data = ConfirmTimelineDuplicatesDeletionData.From(callbackQueryDataCore);
        return sender is not null && data is not null;
    }

    protected override Task ExecuteAsync(ConfirmTimelineDuplicatesDeletionData data, Message message, User sender)
    {
        return _manager.DeleteOldTimelinePart(message.Chat, sender, data.DeleteFrom, data.DeleteAmount);
    }

    private readonly Manager _manager;
}