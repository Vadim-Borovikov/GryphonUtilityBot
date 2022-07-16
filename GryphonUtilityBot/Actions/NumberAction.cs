using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class NumberAction : SupportedAction
{
    public NumberAction(Bot bot, Message message, decimal number) : base(bot, message) => _number = number;

    protected override Task ExecuteAsync() => Bot.CurrencyManager.ProcessNumberAsync(Message.Chat, _number);

    private readonly decimal _number;
}