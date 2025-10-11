using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using GryphonUtilityBot.Articles;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class ReadCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ReadCommand(Bot bot, ITextsProvider<ITexts> textsProvider, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "read", textsProvider, bot.Core.SelfUsername)
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User _) => _manager.DeleteFirstArticleAsync(message.Chat);

    private readonly Manager _manager;
}