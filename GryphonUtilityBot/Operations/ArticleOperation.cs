﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot.Configs;
using AbstractBot.Operations;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Operations;

internal sealed class ArticleOperation : Operation<Article>
{
    protected override byte Order => 6;

    public override Enum AccessRequired => GryphonUtilityBot.Bot.AccessType.OtherFeatures;

    public ArticleOperation(Bot bot, Manager manager) : base(bot)
    {
        Description = new MessageTemplate
        {
            Text = new List<string>
            {
                "*ссылка* – добавить статью сегодняшним числом",
                "*дата и ссылка* – добавить статью"
            },
            MarkdownV2 = true
        };

        _manager = manager;
    }

    protected override bool IsInvokingBy(Message message, User sender, out Article? data)
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

        data = Article.Parse(message.Text);
        return data is not null;
    }

    protected override Task ExecuteAsync(Article data, Message message, User sender)
    {
        return _manager.ProcessNewArticleAsync(message.Chat, data);
    }

    private readonly Manager _manager;
}