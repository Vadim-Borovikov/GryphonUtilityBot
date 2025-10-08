using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using GoogleSheetsManager.Extensions;
using GryphonUtilityBot.Timeline;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class AcceptTimelineMessage : Operation
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public override bool EnabledInChannels => true;

    public AcceptTimelineMessage(Bot bot, Manager manager) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User? sender)
    {
        return sender is null && (message.Chat.Id == _manager.InputChannel.Id)
                              && (message.Photo is not null || message.Video is not null || message.Voice is not null
                                  || message.VideoNote is not null || !string.IsNullOrWhiteSpace(message.Text)
                                  || message.Sticker is not null);
    }

    protected override Task ExecuteAsync(Message message)
    {
        DateOnly? date = message.Text?.ToDateOnly(_bot.Core.Clock);
        _manager.AddRecord(message.MessageId, date, message.MediaGroupId, message.ForwardFrom?.Id,
            message.ReplyToMessage?.MessageId);
        return Task.CompletedTask;
    }

    private readonly Bot _bot;
    private readonly Manager _manager;
}