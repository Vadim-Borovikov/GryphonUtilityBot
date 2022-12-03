using System.Threading.Tasks;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class RememberMarkAction : SupportedAction
{
    public RememberMarkAction(Bot bot, Message message, MarkQuery query) : base(bot, message) => _query = query;

    protected override Task ExecuteAsync(Chat chat)
    {
        Bot.CurrentQuery = _query;
        Bot.CurrentQueryTime = Bot.TimeManager.GetDateTimeFull(Message.Date);
        return Bot.SendTextMessageAsync(chat, "Запрос пометки зафиксирован.", replyToMessageId: Message.MessageId);
    }

    protected override bool AllowedForMistress => true;

    private readonly MarkQuery _query;
}