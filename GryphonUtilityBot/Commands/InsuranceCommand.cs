using System.Threading.Tasks;
using AbstractBot.Operations;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class InsuranceCommand : CommandOperation
{
    protected override byte MenuOrder => 4;

    protected override Access AccessLevel => Access.SuperAdmin;

    public InsuranceCommand(Bot bot, InsuranceManager manager)
        : base(bot, "insurance", "составить обращение в страховую")
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, long _, string? __)
    {
        return _manager.StartDiscussion(message.Chat);
    }

    private readonly InsuranceManager _manager;
}