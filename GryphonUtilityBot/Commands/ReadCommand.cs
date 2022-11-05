using System.Threading.Tasks;
using AbstractBot.Commands;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ReadCommand : CommandBase<Bot, Config>
{
    public ReadCommand(Bot bot) : base(bot, "read", "удалить статью и выдать следующую") { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        return Bot.ArticlesManager.DeleteFirstArticleAsync(message.Chat);
    }
}