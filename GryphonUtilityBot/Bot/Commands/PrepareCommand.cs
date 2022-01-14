using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class PrepareCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "prepare";
        protected override string Description => null;

        public PrepareCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat, string payload)
        {
            return Bot.RollsManager.PrepareToRoll(message.Chat);
        }
    }
}
