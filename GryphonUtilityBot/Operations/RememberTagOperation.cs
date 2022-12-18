using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class RememberTagOperation: Operation
{
    protected override byte MenuOrder => 8;

    protected override Access AccessLevel => Access.Admin;

    public RememberTagOperation(Bot bot) : base(bot)
    {
        MenuDescription = "*переслать сообщение и добавить текст* – добавить запись в таймлайн с тегами";
        _bot = bot;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        TagQuery? query = Check(message);
        if (query is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        _bot.CurrentQuery = query;
        _bot.CurrentQueryTime = _bot.TimeManager.GetDateTimeFull(message.Date);
        await _bot.SendTextMessageAsync(message.Chat, "Запрос пометки зафиксирован.",
            replyToMessageId: message.MessageId);
        return ExecutionResult.Success;
    }

    private TagQuery? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || _bot.InsuranceManager.Active || message.ReplyToMessage is not null)
        {
            return null;
        }

        return TagQuery.ParseTagQuery(message.Text);
    }

    private readonly Bot _bot;
}