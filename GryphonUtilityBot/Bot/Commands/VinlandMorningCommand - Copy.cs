using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class VinlandMorningCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "vinland_morning";
        protected override string Description => null;

        public VinlandMorningCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat, string payload)
        {
            return Bot.VinlandManager.RecommendAsync(message.Chat, true);
        }
    }
}
