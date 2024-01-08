using AbstractBot.Operations;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class ForwardOperation : Operation
{
    protected override byte MenuOrder => 7;

    protected override Access AccessLevel => Access.Admin;

    public ForwardOperation(Bot bot) : base(bot)
    {
        MenuDescription = "*переслать сообщение* – добавить запись в таймлайн";
        _bot = bot;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        if (message.ForwardFrom is null || message.ReplyToMessage is not null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        if (_bot.CurrentQuery is not null
            && (Bot.TimeManager.GetDateTimeFull(message.Date.ToUniversalTime()) > _bot.CurrentQueryTime))
        {
            _bot.CurrentQuery = null;
        }
        await _bot.RecordsManager.SaveRecordAsync(message, _bot.CurrentQuery);
        return ExecutionResult.Success;
    }

    private readonly Bot _bot;
}