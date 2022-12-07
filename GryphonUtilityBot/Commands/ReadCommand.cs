using System.Threading.Tasks;
using AbstractBot.Operations;
using GryphonUtilityBot.Articles;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class ReadCommand : CommandOperation
{
    protected override byte MenuOrder => 3;

    protected override Access AccessLevel => Access.SuperAdmin;

    public ReadCommand(Bot bot, Manager manager) : base(bot, "read", "удалить статью и выдать следующую")
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message _, Chat chat, string? __) => _manager.DeleteFirstArticleAsync(chat);

    private readonly Manager _manager;
}