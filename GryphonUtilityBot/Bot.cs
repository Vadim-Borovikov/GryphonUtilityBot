using AbstractBot;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Operations.Commands.Start;
using AbstractBot.Models;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Models.Operations.Commands.Start;
using AbstractBot.Modules;
using AbstractBot.Modules.TextProviders;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Operations;
using GryphonUtilityBot.Operations.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GryphonUtilityBot.Money;

namespace GryphonUtilityBot;

public sealed class Bot : AbstractBot.Bot, IDisposable
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Admin = 3
    }

    public static async Task<Bot?> TryCreateAsync(Config config, CancellationToken cancellationToken)
    {
        BotCore? core = await BotCore.TryCreateAsync(config, cancellationToken);
        if (core is null)
        {
            return null;
        }

        core.UpdateSender.DefaultKeyboardProvider = KeyboardProvider.Same;

        // TODO
        // SaveManager<BotState, BotData> saveManager = new(config.SavePath, core.Clock);

        // Dictionary<long, UserState> userStates = new();

        Common<Texts> commonTexts = new(config.Texts);

        ICommands commands = new Commands(core.Client, core.Accesses, core.UpdateReceiver, commonTexts);

        Greeter greeter = new(core.UpdateSender, commonTexts);
        Start start = new(core.Accesses, core.UpdateSender, commands, commonTexts, core.SelfUsername, greeter);

        Help help = new(core.Accesses, core.UpdateSender, core.UpdateReceiver, commonTexts, core.SelfUsername);

        return new Bot(core, commands, start, help, config, commonTexts);
    }

    private Bot(BotCore core, ICommands commands, IStartCommand start, Help help, Config config,
        ITextsProvider<Texts> textsProvider)
        : base(core, commands, start, help)
    {
        _core = core;
        _config = config;
        _textsProvider = textsProvider;

        _sheetsManager = new GoogleSheetsManager.Documents.Manager(_config);

        _financemanager = new Manager(this, _config, _textsProvider, _sheetsManager);

        _timelineManager = new Timeline.Manager(this, _config, _sheetsManager);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _core.Connection.StartAsync(cancellationToken);
        await _core.Logging.StartAsync(cancellationToken);

        _core.UpdateReceiver.Operations.Add(new AcceptTimelineMessage(this, _timelineManager));

        Articles.Manager articlesManager = new(this, _config, _textsProvider, _sheetsManager);

        _core.UpdateReceiver.Operations.Add(new AddReceipt(this, _textsProvider, _config.DefaultCurrency,
            _financemanager));

        _core.UpdateReceiver.Operations.Add(new ArticleCommand(this, _textsProvider, articlesManager));
        _core.UpdateReceiver.Operations.Add(new ReadCommand(this, _textsProvider, articlesManager));
        _core.UpdateReceiver.Operations.Add(new UpdateTimelineCommand(this, _textsProvider, _timelineManager));

        _core.UpdateReceiver.Operations.Add(new AddArticle(this, articlesManager));

        await Commands.UpdateForAll(cancellationToken);
    }

    public void Dispose()
    {
        _timelineManager.Dispose();
        _core.Dispose();
    }

    public Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note)
    {
        return _financemanager.AddSimultaneousTransactionsAsync(transactions, date, note);
    }

    private readonly GoogleSheetsManager.Documents.Manager _sheetsManager;

    private readonly Manager _financemanager;
    private readonly Timeline.Manager _timelineManager;

    private readonly ITextsProvider<Texts> _textsProvider;

    private readonly BotCore _core;
    private readonly Config _config;
}