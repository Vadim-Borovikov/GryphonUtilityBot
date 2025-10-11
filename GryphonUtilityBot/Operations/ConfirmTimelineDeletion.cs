using AbstractBot.Models.Operations;
using GryphonUtilityBot.Operations.Data;
using GryphonUtilityBot.Timeline;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class ConfirmTimelineDeletion : Operation<ConfirmTimelineDeletionData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ConfirmTimelineDeletion(Bot bot, Manager manager) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore,
        out ConfirmTimelineDeletionData? data)
    {
        data = ConfirmTimelineDeletionData.From(callbackQueryDataCore);
        return sender is not null && data is not null;
    }

    protected override Task ExecuteAsync(ConfirmTimelineDeletionData data, Message message, User sender)
    {
        return _manager.DeleteOldTimelinePart(data.DeleteFrom, data.DeleteAmount);
    }

    private readonly Manager _manager;
}