using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot;

public sealed class Bot : BotBase<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
        SaveManager<List<RecordData>, List<JsonRecordData?>> saveManager =
            new(Config.SavePath, JsonRecordData.Convert, RecordData.Convert);
        RecordsManager = new Records.Manager(this, saveManager);
        CurrencyManager = new Currency.Manager(this);
    }

    protected override Task UpdateAsync(Message message, bool fromChat, CommandBase<Bot, Config>? command = null,
        string? payload = null)
    {
        SupportedAction? action = GetAction(message, command);
        return action is null
            ? SendStickerAsync(message.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker);
    }

    protected override Task ProcessCallbackAsync(CallbackQuery callback)
    {
        if (callback.Data is null)
        {
            throw new NullReferenceException(nameof(callback.Data));
        }
        return CurrencyManager.ChangeCurrency(callback.Data);
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

        if (decimal.TryParse(message.Text, out decimal number))
        {
            return new NumberAction(this, message, number);
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

    internal readonly Records.Manager RecordsManager;

    internal MarkQuery? CurrentQuery;
    internal DateTime CurrentQueryTime;

    internal readonly Currency.Manager CurrencyManager;
}