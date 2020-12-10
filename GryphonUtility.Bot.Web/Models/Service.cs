using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GryphonUtility.Bot.Web.Models
{
    internal sealed class Service : IHostedService
    {
        public Service(IBot bot) => _bot = bot;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _bot.Client.SetWebhookAsync(_bot.Config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _bot.Client.DeleteWebhookAsync(cancellationToken);
        }

        private readonly IBot _bot;
    }
}