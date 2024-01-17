using System;
using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class FindRecord : Operation<FindQuery>
{
    protected override byte Order => 7;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public FindRecord(Bot bot, Manager manager) : base(bot, bot.Config.Texts.FindRecordDescription)
    {
        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out FindQuery? data)
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

        data = FindQuery.ParseFindQuery(message.Text);
        return data is null;
    }

    protected override Task ExecuteAsync(FindQuery data, Message message, User sender)
    {
        return _manager.ProcessFindQueryAsync(message.Chat, data);
    }

    private readonly Manager _manager;
}