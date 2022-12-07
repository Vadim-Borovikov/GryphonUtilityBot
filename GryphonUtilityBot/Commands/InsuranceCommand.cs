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

    protected override Task ExecuteAsync(Message _, Chat chat, string? __) => _manager.StartDiscussion(chat);

    private readonly InsuranceManager _manager;
}