using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class InsuranceAction : SupportedAction
{
    public InsuranceAction(Bot bot, Message message, InsuranceManager manager) : base(bot, message)
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Chat chat) => _manager.Accept(chat, Message);

    private readonly InsuranceManager _manager;
}