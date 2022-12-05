using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class InsuranceAction : SupportedAction
{
    public InsuranceAction(Bot bot, Message message, string messageText) : base(bot, message)
    {
        _messageText = messageText;
    }

    protected override Task ExecuteAsync(Chat chat) => Bot.InsuranceManager.Accept(chat, _messageText);

    private readonly string _messageText;
}