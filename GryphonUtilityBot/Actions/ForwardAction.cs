using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class ForwardAction : SupportedAction
{
    public ForwardAction(Bot bot, Message message) : base(bot, message) { }

    protected override Task ExecuteAsync(Chat chat)
    {
        return Bot.RecordsManager.SaveRecordAsync(Message, chat, Bot.CurrentQuery);
    }

    protected override bool AllowedForMistress => true;
}