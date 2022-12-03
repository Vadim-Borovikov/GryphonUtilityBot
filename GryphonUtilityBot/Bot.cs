using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using GoogleSheetsManager.Providers;
using GryphonUtilities;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseCustom<Config>, IDisposable
{
    internal readonly SheetsProvider GoogleSheetsProvider;
    internal readonly Dictionary<Type, Func<object?, object?>> AdditionalConverters;

    public Bot(Config config) : base(config)
    {
        GoogleSheetsProvider = new SheetsProvider(config, config.GoogleSheetId);
        AdditionalConverters = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(DateOnly), o => GetDateOnly(o) },
            { typeof(DateOnly?), o => GetDateOnly(o) },
            { typeof(Uri), Utils.ToUri }
        };

        SaveManager<Data> saveManager = new(config.SavePath, TimeManager);
        RecordsManager = new Records.Manager(this, saveManager);

        ArticlesManager = new Articles.Manager(this);
        CurrencyManager = new Currency.Manager(this);
    }

    public void Dispose() => GoogleSheetsProvider.Dispose();

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new ArticleCommand(this));
        Commands.Add(new ReadCommand(this));

        await base.StartAsync(cancellationToken);
    }

    protected override Task ProcessTextMessageAsync(Message textMessage, Chat senderChat, CommandBase? command = null,
        string? payload = null)
    {
        SupportedAction? action = GetAction(textMessage, command);
        return action is null
            ? SendStickerAsync(textMessage.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker, senderChat);
    }

    protected override Task ProcessCallbackAsync(CallbackQuery callback)
    {
        if (callback.Data is null)
        {
            throw new NullReferenceException(nameof(callback.Data));
        }
        return CurrencyManager.ChangeCurrency(callback.Data);
    }

    private DateOnly? GetDateOnly(object? o)
    {
        if (o is DateOnly d)
        {
            return d;
        }

        DateTimeFull? dtf = GoogleSheetsManager.Utils.GetDateTimeFull(o, GoogleSheetsProvider.TimeManager);
        return dtf?.DateOnly;
    }

    private SupportedAction? GetAction(Message message, CommandBase? command)
    {
        if (message.ForwardFrom is not null)
        {
            if (CurrentQuery is not null
                && (TimeManager.GetDateTimeFull(message.Date.ToUniversalTime()) > CurrentQueryTime))
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

        if (Articles.Manager.TryParseArticle(message.Text, out Article? article))
        {
            if (article is null)
            {
                throw new NullReferenceException(nameof(article));
            }
            return new ArticleAction(this, message, article);
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

    internal MarkQuery? CurrentQuery;
    internal DateTimeFull CurrentQueryTime;

    internal readonly Articles.Manager ArticlesManager;
    internal readonly Records.Manager RecordsManager;
    internal readonly Currency.Manager CurrencyManager;
}