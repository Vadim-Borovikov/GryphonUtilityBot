using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.Config;
using AbstractBot.Models.Operations.Commands;
using GryphonUtilityBot.Money;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class UpdateExpensesCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdateExpensesCommand(Bot bot, ITextsProvider<Texts> textsProvider, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "expenses", textsProvider, bot.Core.SelfUsername)
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _manager.UpdateExpensesDataAsync(message.Chat, sender.Id);
    }

    private readonly Manager _manager;
}