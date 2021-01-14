using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Web.Models.Actions
{
    internal abstract class SupportedAction
    {
        protected SupportedAction(IBot bot, Message message)
        {
            Bot = bot;
            Message = message;
        }

        internal Task ExecuteWrapperAsync(InputOnlineFile forbiddenSticker)
        {
            bool isMistress = Message.From.Id == Bot.Config.MistressId;
            if (isMistress && !AllowedForMistress)
            {
                return Bot.Client.SendTextMessageAsync(Message.Chat,
                    "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
            }

            bool isMaster = Message.From.Id == Bot.Config.MasterId;
            return isMaster || isMistress
                ? ExecuteAsync()
                : Bot.Client.SendStickerAsync(Message, forbiddenSticker);
        }

        protected abstract Task ExecuteAsync();

        protected virtual bool AllowedForMistress => false;

        protected readonly IBot Bot;
        protected readonly Message Message;
    }
}
