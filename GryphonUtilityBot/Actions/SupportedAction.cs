using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Actions;

internal abstract class SupportedAction
{
    protected SupportedAction(Bot bot, Message message)
    {
        Bot = bot;
        Message = message;
    }

    internal Task ExecuteWrapperAsync(InputOnlineFile forbiddenSticker, Chat senderChat)
    {
        bool isMistress = senderChat.Id == Bot.Config.MistressId;
        if (isMistress && !AllowedForMistress)
        {
            return Bot.SendTextMessageAsync(Message.Chat,
                "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
        }

        return Bot.GetMaximumAccessFor(senderChat.Id) >= BotBase.AccessType.Admins
            ? ExecuteAsync(Message.Chat)
            : Bot.SendStickerAsync(Message.Chat, forbiddenSticker);
    }

    protected abstract Task ExecuteAsync(Chat chat);

    protected readonly Message Message;

    protected readonly Bot Bot;

    private static bool AllowedForMistress => false;
}