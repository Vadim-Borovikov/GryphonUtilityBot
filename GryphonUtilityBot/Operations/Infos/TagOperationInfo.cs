using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Infos;

internal sealed class TagOperationInfo
{
    public readonly TagQuery TagQuery;
    public readonly long ChatId;
    public readonly long MessageId;

    public TagOperationInfo(TagQuery query, Message message)
    {
        TagQuery = query;
        ChatId = message.Chat.Id;
        MessageId = message.MessageId;
    }
}