using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GryphonUtilities.Logging;
using GryphonUtilities.Time;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Money;
using Telegram.Bot.Types;

namespace GryphonUtilityBot;

public sealed class BotHost : IDisposable
{
    public Clock Clock => _bot.Core.Clock;
    public Logger Logger => _bot.Core.Logging.Logger;

    public static async Task<BotHost?> TryCreateAsync(Config config, CancellationToken cancellationToken)
    {
        Bot? bot = await Bot.TryCreateAsync(config, cancellationToken);
        return bot is not null ? new BotHost(bot) : null;
    }

    public (string Name, string? Username) Self => (_bot.Core.Self.FirstName, _bot.Core.Self.Username);
    private BotHost(Bot bot) => _bot = bot;

    public Task StartAsync(CancellationToken cancellationToken) => _bot.StartAsync(cancellationToken);

    public void Update(Update update) => _bot.Update(update);

    public Task StopAsync(CancellationToken cancellationToken) => _bot.StopAsync(cancellationToken);

    public void Dispose() => _bot.Dispose();

    public Task AddSimultaneousTransactionsAsync(List<TransactionDebt> transactions, DateOnly modelDate, string note)
    {
        return _bot.AddSimultaneousTransactionsAsync(transactions, modelDate, note);
    }

    private readonly Bot _bot;
}