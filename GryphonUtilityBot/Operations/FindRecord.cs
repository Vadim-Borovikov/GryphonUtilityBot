using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot.Configs;
using AbstractBot.Operations;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class FindRecord : Operation<FindQuery>
{
    protected override byte Order => 7;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.Records;

    public FindRecord(Bot bot, Manager manager) : base(bot)
    {
        Description = new MessageTemplate
        {
            Text = new List<string>
            {
                "*пара дат, например \"1\\.02\\.20 1\\.03\\.22\"* – найти записи в этот период",
                "*пара дат и теги, например \"1\\.02\\.20 1\\.03\\.22 творчество вкусности\"* – найти записи в этот период с этими тегами"
            },
            MarkdownV2 = true
        };

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