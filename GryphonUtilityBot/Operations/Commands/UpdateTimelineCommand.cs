using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using GryphonUtilityBot.Timeline;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class UpdateTimelineCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdateTimelineCommand(Bot bot, ITextsProvider<ITexts> textsProvider, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "timeline", textsProvider, bot.Core.SelfUsername)
    {
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User _) => _manager.UpdateOutputChannelAsync();

    private readonly Manager _manager;
}