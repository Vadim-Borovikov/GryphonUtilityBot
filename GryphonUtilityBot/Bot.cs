using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Records;
using GryphonUtilityBot.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot;

public sealed class Bot : BotBase<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
        _saveManager = new SaveManager<List<RecordData>, List<JsonRecordData?>>(Config.SavePath,
            JsonRecordData.Convert, RecordData.Convert);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new StartCommand(this));

        await base.StartAsync(cancellationToken);
    }

    protected override Task UpdateAsync(Message message, bool fromChat, CommandBase<Bot, Config>? command = null,
        string? payload = null)
    {
        SupportedAction? action = GetAction(message, command);
        return action is null
            ? SendStickerAsync(message.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker);
    }

    private SupportedAction? GetAction(Message message, CommandBase<Bot, Config>? command)
    {
        if (message.ForwardFrom is not null)
        {
            if (CurrentQuery is not null && (message.Date > CurrentQueryTime))
            {
                CurrentQuery = null;
            }
            return new ForwardAction(this, message);
        }

        if (command is not null)
        {
            return new CommandAction(this, message, command);
        }

        if (message.Type is not MessageType.Text)
        {
            return null;
        }

        if (message.Text is null)
        {
            return null;
        }

        if (FindQuery.TryParseFindQuery(message.Text, out FindQuery? findQuery))
        {
            if (findQuery is null)
            {
                throw new NullReferenceException(nameof(findQuery));
            }
            return new FindQueryAction(this, message, findQuery);
        }

        if (MarkQuery.TryParseMarkQuery(message.Text, out MarkQuery? markQuery))
        {
            if (markQuery is null)
            {
                throw new NullReferenceException(nameof(findQuery));
            }

            if (message.ReplyToMessage is null)
            {
                return new RememberMarkAction(this, message, markQuery);
            }

            if (message.ReplyToMessage.ForwardFrom is not null)
            {
                return new MarkAction(this, message, message.ReplyToMessage, markQuery);
            }
        }

        return null;
    }

    internal MarkQuery? CurrentQuery;
    internal DateTime CurrentQueryTime;

    internal Records.Manager RecordsManager => _recordsManager ??= new Records.Manager(this, _saveManager);

    private Records.Manager? _recordsManager;

    private readonly SaveManager<List<RecordData>, List<JsonRecordData?>> _saveManager;
}