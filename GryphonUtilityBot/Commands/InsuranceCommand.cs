using AbstractBot.Commands;
using AbstractBot;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class InsuranceCommand : CommandBase
{
    public override BotBase.AccessType Access => BotBase.AccessType.SuperAdmin;

    public InsuranceCommand(Bot bot, InsuranceManager manager)
        : base(bot, "insurance", "составить обращение в страховую")
    {
        _manager = manager;
    }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload) => _manager.StartDiscussion(chat);

    private readonly InsuranceManager _manager;
}