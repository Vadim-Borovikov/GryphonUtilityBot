﻿using System;
using AbstractBot.Operations;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class AddRecord : OperationSimple
{
    protected override byte Order => 6;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public AddRecord(Bot bot) : base(bot, bot.Config.Texts.AddRecordDescription) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender)
    {
        return message.ForwardFrom is not null && message.ReplyToMessage is null;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        if (_bot.CurrentQuery is not null
            && (Bot.Clock.GetDateTimeFull(message.Date.ToUniversalTime()) > _bot.CurrentQueryTime))
        {
            _bot.CurrentQuery = null;
        }
        return _bot.RecordsManager.SaveRecordAsync(message, _bot.CurrentQuery);
    }

    private readonly Bot _bot;
}