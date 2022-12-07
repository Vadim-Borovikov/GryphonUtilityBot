using AbstractBot.Operations;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations;

internal sealed class ForwardOperation : Operation
{
    protected override byte MenuOrder => 7;

    protected override Access AccessLevel => Access.Admins;

    public ForwardOperation(Bot bot) : base(bot)
    {
        MenuDescription = "*переслать сообщение* – добавить запись в таймлайн";
        _bot = bot;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        if (message.ForwardFrom is null || _bot.InsuranceManager.Active || message.ReplyToMessage is not null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        if (_bot.CurrentQuery is not null
            && (_bot.TimeManager.GetDateTimeFull(message.Date.ToUniversalTime()) > _bot.CurrentQueryTime))
        {
            _bot.CurrentQuery = null;
        }
        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _bot.RecordsManager.SaveRecordAsync(message, chat, _bot.CurrentQuery);
        return ExecutionResult.Success;
    }

    private readonly Bot _bot;
}