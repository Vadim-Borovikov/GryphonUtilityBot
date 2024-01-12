using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
        Operations.Add(new AcceptPurchase(this, _manager));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        await _manager.StartAsync();
    }

    public Task OnSubmissionReceivedAsync(string name, MailAddress email, string telegram, List<string> items,
        List<Uri> slips)
    {
        List<byte> productIds = Config.Products.Where(p => items.Contains(p.Value.Name)).Select(p => p.Key).ToList();
        return _manager.ProcessSubmissionAsync(name, email, telegram, productIds, slips);
    }

    private readonly Manager _manager;
}