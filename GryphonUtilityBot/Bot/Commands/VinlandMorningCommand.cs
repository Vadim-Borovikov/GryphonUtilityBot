using System.Threading.Tasks;
using AbstractBot;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class VinlandAfternoonCommand : CommandBase<Bot, Config>
    {
        protected override string Name => "vinland_afternoon";
        protected override string Description => null;

        public VinlandAfternoonCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat, string payload)
        {
            return Bot.VinlandManager.RecommendAsync(message.Chat, false);
        }
    }
}
