using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilities;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Actions;

internal abstract class SupportedAction
{
    protected SupportedAction(Bot bot, Message message)
    {
        _bot = bot;
        Message = message;
    }

    internal Task ExecuteWrapperAsync(InputOnlineFile forbiddenSticker)
    {
        User user = Message.From.GetValue(nameof(Message.From));
        bool isMistress = user.Id == _bot.Config.MistressId;
        if (isMistress && !AllowedForMistress)
        {
            return _bot.SendTextMessageAsync(Message.Chat,
                "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
        }

        bool shouldExecute = _bot.IsAccessSuffice(user.Id, BotBase.AccessType.Admins);
        return shouldExecute ? ExecuteAsync() : _bot.SendStickerAsync(Message.Chat, forbiddenSticker);
    }

    protected abstract Task ExecuteAsync();

    protected readonly Message Message;

    private static bool AllowedForMistress => false;

    private readonly Bot _bot;
}