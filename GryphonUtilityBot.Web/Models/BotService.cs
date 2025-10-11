using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GryphonUtilityBot.Web.Models;

public sealed class BotService : IHostedService, IDisposable
{
    public BotService(Bot bot) => _bot = bot;

    public void Dispose() => _bot.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) => _bot.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => _bot.StopAsync(cancellationToken);

    private readonly Bot _bot;
}