using System;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Money;
using GryphonUtilityBot.Operations;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Admin = 2,
    }

    public Bot(Config config) : base(config)
    {
        _manager = new Manager(this, DocumentsManager);

        Operations.Add(new AddReceipt(this, _manager));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        await _manager.StartAsync();
    }

    private readonly Manager _manager;
}