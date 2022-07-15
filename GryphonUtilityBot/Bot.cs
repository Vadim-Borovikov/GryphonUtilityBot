using System;
using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using GryphonUtilityBot.Articles;
using GryphonUtilityBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseGoogleSheets<Bot, Config>
{
    public Bot(Config config) : base(config)
    {
        ArticlesManager = new Articles.Manager(this);

        Commands.Add(new ArticleCommand(this));
        Commands.Add(new ReadCommand(this));
    }

    protected override Task ProcessTextMessageAsync(Message textMessage, bool fromChat,
        CommandBase<Bot, Config>? command = null, string? payload = null)
    {
        SupportedAction? action = GetAction(textMessage, command);
        return action is null
            ? SendStickerAsync(textMessage.Chat, DontUnderstandSticker)
            : action.ExecuteWrapperAsync(ForbiddenSticker);
    }

    private SupportedAction? GetAction(Message message, CommandBase<Bot, Config>? command)
    {
        if (command is not null)
        {
            return new CommandAction(this, message, command);
        }

        if (Articles.Manager.TryParseArticle(message.Text, out Article? article))
        {
            if (article is null)
            {
                throw new NullReferenceException(nameof(article));
            }
            return new ArticleAction(this, message, article);
        }

        return null;
    }

    internal readonly Articles.Manager ArticlesManager;
}