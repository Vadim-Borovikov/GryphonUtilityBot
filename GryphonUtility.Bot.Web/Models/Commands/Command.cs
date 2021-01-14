using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    public abstract class Command
    {
        protected abstract string Name { get; }

        internal bool IsInvokingBy(Message message)
        {
            return (message.Type == MessageType.Text) && (message.Text == $"/{Name}");
        }

        internal abstract Task ExecuteAsync(ITelegramBotClient client, ChatId chatId);
    }
}
