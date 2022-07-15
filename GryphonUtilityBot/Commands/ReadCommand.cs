﻿using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ReadCommand : CommandBase<Bot, Config>
{
    protected override string Name => "read";
    protected override string Description => "";

    public ReadCommand(Bot bot) : base(bot) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        return Bot.ArticlesManager.DeleteFirstArticleAsync(message.Chat);
    }
}