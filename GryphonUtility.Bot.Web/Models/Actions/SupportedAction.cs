using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models.Actions
{
    internal abstract class SupportedAction
    {
        protected SupportedAction(IBot bot, Message message)
        {
            Bot = bot;
            ChatId = message.Chat;
            _from = message.From.Id;
        }

        internal Task ExecuteWrapperAsync()
        {
            bool authorized = _from == Bot.Config.MasterId;

            if (_from == Bot.Config.MistressId)
            {
                if (AllowedForMistress)
                {
                    authorized = true;
                }
                else
                {
                    return Bot.Client.SendTextMessageAsync(ChatId,
                        "Простите, госпожа, но господин заблокировал это действие даже для Вас.");
                }
            }

            return authorized
                ? ExecuteAsync()
                : Bot.Client.SendTextMessageAsync(ChatId, "Действие заблокировано.");
        }

        protected abstract Task ExecuteAsync();

        protected virtual bool AllowedForMistress => false;

        protected readonly IBot Bot;
        protected readonly ChatId ChatId;

        private readonly int _from;
    }
}
