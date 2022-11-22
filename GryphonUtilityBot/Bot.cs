using System;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using GryphonUtilities;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseGoogleSheets<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
        _saveManager = new SaveManager<Data>(Config.SavePath, TimeManager);
        AdditionalConverters[typeof(DateOnly)] = AdditionalConverters[typeof(DateOnly?)] = o => GetDateOnly(o);
        AdditionalConverters[typeof(Uri)] = Utils.ToUri;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new ArticleCommand(this));
        Commands.Add(new ReadCommand(this));

        await base.StartAsync(cancellationToken);
    }

    protected override Task UpdateAsync(Message message, bool fromChat, CommandBase? command = null,
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

    private DateOnly? GetDateOnly(object? o)
    {
        if (o is DateOnly d)
        {
            return d;
        }

        DateTimeFull? dtf = GetDateTimeFull(o);
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

    internal Articles.Manager ArticlesManager => _articlesManager ??= new Articles.Manager(this);
    internal Records.Manager RecordsManager => _recordsManager ??= new Records.Manager(this, _saveManager);
    internal Currency.Manager CurrencyManager => _currencyManager ??= new Currency.Manager(this);

    private Articles.Manager? _articlesManager;
    private Records.Manager? _recordsManager;
    private Currency.Manager? _currencyManager;

    private readonly SaveManager<Data> _saveManager;
}