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
        Bot = bot;
        Message = message;
    }

    internal Task ExecuteWrapperAsync(InputOnlineFile forbiddenSticker)
    {
        User user = Message.From.GetValue(nameof(Message.From));
        bool isMistress = user.Id == Bot.Config.MistressId;
        if (isMistress && !AllowedForMistress)
        {
            return Bot.SendTextMessageAsync(Message.Chat,
                "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
        }

        bool shouldExecute = Bot.IsAccessSuffice(user.Id, BotBase.AccessType.Admins);
        return shouldExecute ? ExecuteAsync() : Bot.SendStickerAsync(Message.Chat, forbiddenSticker);
    }

    protected abstract Task ExecuteAsync();

    protected virtual bool AllowedForMistress => false;

    protected readonly Bot Bot;
    protected readonly Message Message;
}