using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Bot.Commands
{
    internal abstract class Command
    {
        protected abstract string Name { get; }

        public bool IsInvokingBy(Message message)
        {
            return (message.Type == MessageType.Text) && (message.Text == $"/{Name}");
        }

        public abstract Task ExecuteAsync(ITelegramBotClient client, ChatId chatId);
    }
}
