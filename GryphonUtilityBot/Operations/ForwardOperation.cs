using System;
using AbstractBot.Operations;
using System.Threading.Tasks;
using AbstractBot.Configs;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class ForwardOperation : OperationSimple
{
    protected override byte Order => 7;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public ForwardOperation(Bot bot) : base(bot)
    {
        Description = new MessageTemplate("*переслать сообщение* – добавить запись в таймлайн", true);
        _bot = bot;
    }

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