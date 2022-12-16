using AbstractBot.Operations;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class InsuranceOperation : Operation
{
    protected override byte MenuOrder => 0;

    protected override Access AccessLevel => Access.SuperAdmin;

    public InsuranceOperation(Bot bot, InsuranceManager manager) : base(bot) => _manager = manager;

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, Chat sender)
    {
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text)
                                               || message.ForwardFrom is not null || !_manager.Active)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(sender.Id))
        {
            return ExecutionResult.InsufficentAccess;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        await _manager.Accept(chat, message.Text);
        return ExecutionResult.Success;
    }

    private readonly InsuranceManager _manager;
}