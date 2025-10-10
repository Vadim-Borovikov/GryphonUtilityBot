using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.Operations.Commands;
using GryphonUtilityBot.Timeline;
using System;
using System.Threading.Tasks;
using GryphonUtilityBot.Configs;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Operations.Commands;

internal sealed class UpdateTimelineCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdateTimelineCommand(Bot bot, ITextsProvider<Texts> textsProvider, Manager manager)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "timeline", textsProvider, bot.Core.SelfUsername)
    {
        _textsProvider = textsProvider;
        _manager = manager;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _manager.UpdateChannelAsync(message.Chat, sender, _textsProvider);
    }

    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly Manager _manager;
}