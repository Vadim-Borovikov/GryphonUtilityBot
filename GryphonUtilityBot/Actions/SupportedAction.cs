﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GryphonUtilityBot.Actions
{
    internal abstract class SupportedAction
    {
        protected SupportedAction(Bot.Bot bot, Message message)
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

            return Bot.FromAdmin(Message)
                ? ExecuteAsync()
                : Bot.Client.SendStickerAsync(Message.Chat, forbiddenSticker);
        }

        protected abstract Task ExecuteAsync();

        protected virtual bool AllowedForMistress => false;

        protected readonly Bot.Bot Bot;
        protected readonly Message Message;
    }
}
