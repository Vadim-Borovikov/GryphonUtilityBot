using System.Threading.Tasks;
using AbstractBot;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Commands
{
    internal sealed class ReadCommand : CommandBase<Config>
    {
        protected override string Name => "read";
        protected override string Description => null;

        public ReadCommand(Bot bot) : base(bot) => _articlesManager = bot.ArticlesManager;

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return _articlesManager.DeleteFirstArticleAsync(Bot.Client, message.Chat);
        }

        private readonly Manager _articlesManager;
    }
}
