using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands
{
    internal sealed class RollCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "roll";
        protected override string Description => null;

        public RollCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat, string payload)
        {
            return Bot.RollsManager.Roll(message.Chat);
        }
    }
}
