using System;
using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class RememberTag: Operation<TagQuery>
{
    protected override byte Order => 8;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public RememberTag(Bot bot) : base(bot, bot.Config.Texts.RememberTagDescription) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out TagQuery? data)
    {
        data = null;
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        if (message.ForwardFrom is not null || message.ReplyToMessage is not null)
        {
            return false;
        }

        data = TagQuery.ParseTagQuery(message.Text);
        return data is not null;
    }

    protected override Task ExecuteAsync(TagQuery data, Message message, User sender)
    {
        _bot.CurrentQuery = data;
        _bot.CurrentQueryTime = _bot.Clock.GetDateTimeFull(message.Date);
        return _bot.SendTextMessageAsync(message.Chat, "Запрос пометки зафиксирован.",
            replyToMessageId: message.MessageId);
    }

    private readonly Bot _bot;
}