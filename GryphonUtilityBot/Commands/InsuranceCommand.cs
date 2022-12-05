using AbstractBot.Commands;
using AbstractBot;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Commands;

internal sealed class InsuranceCommand : CommandBaseCustom<Bot, Config>
{
    public override BotBase.AccessType Access => BotBase.AccessType.SuperAdmin;

    public InsuranceCommand(Bot bot) : base(bot, "insurance", "составить обращение в страховую") { }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload)
    {
        return Bot.InsuranceManager.StartDiscussion(chat);
    }
}