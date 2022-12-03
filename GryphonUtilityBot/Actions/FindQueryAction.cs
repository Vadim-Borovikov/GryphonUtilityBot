using System.Threading.Tasks;
using GryphonUtilityBot.Records;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Actions;

internal sealed class FindQueryAction : SupportedAction
{
    public FindQueryAction(Bot bot, Message message, FindQuery query) : base(bot, message) => _query = query;

    protected override Task ExecuteAsync(Chat chat) => Bot.RecordsManager.ProcessFindQueryAsync(chat, _query);

    protected override bool AllowedForMistress => true;

    private readonly FindQuery _query;
}