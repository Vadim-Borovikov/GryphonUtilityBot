using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class TagOperation : Operation
{
    protected override byte MenuOrder => 9;

    protected override Access AccessLevel => Access.Admins;

    public TagOperation(Bot bot, Manager manager, InsuranceManager insuranceManager) : base(bot)
    {
        MenuDescription = "*ответить на сообщение, которое переслали раньше* – добавить теги к записи";
        _manager = manager;
        _insuranceManager = insuranceManager;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        TagQuery? query = Check(message);
        if (query is null || message.ReplyToMessage?.ForwardFrom is null)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _manager.TagAsync(chat, message.ReplyToMessage, query);
        return ExecutionResult.Success;
    }

    private TagQuery? Check(Message message)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return null;
        }

        if (message.ForwardFrom is not null || _insuranceManager.Active)
        {
            return null;
        }

        return TagQuery.ParseTagQuery(message.Text);
    }

    private readonly Manager _manager;
    private readonly InsuranceManager _insuranceManager;
}