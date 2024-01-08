using System;
using System.Threading.Tasks;
using AbstractBot.Configs;
using AbstractBot.Operations;
using GryphonUtilityBot.Operations.Infos;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class TagOperation : Operation<TagOperationInfo>
{
    protected override byte Order => 9;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public TagOperation(Bot bot, Manager manager) : base(bot)
    {
        Description =
            new MessageTemplate("*ответить на сообщение, которое переслали раньше* – добавить теги к записи", true);
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out TagOperationInfo? data)
    {
        data = null;
        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        if (message.ForwardFrom is not null)
        {
            return false;
        }

        if (message.ReplyToMessage?.ForwardFrom is null)
        {
            return false;
        }

        TagQuery? query = TagQuery.ParseTagQuery(message.Text);
        if (query is not null)
        {
            data = new TagOperationInfo(query, message.ReplyToMessage);
        }
        return data is not null;
    }

    protected override Task ExecuteAsync(TagOperationInfo data, Message message, User sender)
    {
        return _manager.TagAsync(message.Chat, data.ChatId, data.MessageId, data.TagQuery);
    }

    private readonly Manager _manager;
}