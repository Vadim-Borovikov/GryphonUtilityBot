using System.Threading.Tasks;
using AbstractBot;
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

    internal Task ExecuteWrapperAsync(InputOnlineFile forbiddenSticker, Chat senderChat)
    {
        bool isMistress = senderChat.Id == _bot.Config.MistressId;
        if (isMistress && !AllowedForMistress)
        {
            return _bot.SendTextMessageAsync(Message.Chat,
                "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
        }

        return _bot.GetMaximumAccessFor(senderChat.Id) >= BotBase.AccessType.Admins
            ? ExecuteAsync(Message.Chat)
            : _bot.SendStickerAsync(Message.Chat, forbiddenSticker);
    }

    protected abstract Task ExecuteAsync(Chat chat);

    protected readonly Message Message;

    private static bool AllowedForMistress => false;

    private readonly Bot _bot;
}