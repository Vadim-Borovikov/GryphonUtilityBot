using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Actions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot
{
    public sealed class Bot : BotBase<Bot, Config>
    {
        public Bot(Config config) : base(config)
        {
        }

        protected override Task UpdateAsync(Message message, bool fromChat, CommandBase<Bot, Config> command = null,
            string payload = null)
        {
            SupportedAction action = GetAction(message, command);
            return action == null
                ? Client.SendStickerAsync(message.Chat, DontUnderstandSticker)
                : action.ExecuteWrapperAsync(ForbiddenSticker);
        }

        private SupportedAction GetAction(Message message, CommandBase<Bot, Config> command)
        {
            if (command != null)
            {
                return new CommandAction(this, message, command);
            }

            return null;
        }
    }
}
