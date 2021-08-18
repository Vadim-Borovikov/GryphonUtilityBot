using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class PingService : IHostedService, IDisposable
    {
        public PingService(IOptions<Config> options)
        {
            Config config = options.Value;
            _peroid = config.PingPeriod;

            var host = new Uri(config.Host);
            _uris = new List<Uri> { host };

            if ((config.PingUris == null) || (config.PingUris.Count == 0))
            {
                config.PingUris = JsonConvert.DeserializeObject<List<Uri>>(config.PingUrisJson);
            }
            _uris.AddRange(config.PingUris);
        }

        public Task StartAsync(CancellationToken _)
        {
            PingSites(0);

            _periodicCancellationSource = new CancellationTokenSource();
            StartPeriodicPing();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _)
        {
            ClearPeriodicCancellationSource();
            return Task.CompletedTask;
        }

        public void Dispose() => ClearPeriodicCancellationSource();

        private void StartPeriodicPing()
        {
            IObservable<long> observable = Observable.Interval(_peroid);
            observable.Subscribe(PingSites, _periodicCancellationSource.Token);
        }

        private void ClearPeriodicCancellationSource()
        {
            try
            {
                _periodicCancellationSource?.Cancel();
                _periodicCancellationSource?.Dispose();
            }
            catch (ObjectDisposedException) { }
        }

        private void PingSites(long _)
        {
            foreach (Uri uri in _uris)
            {
                PingSite(uri);
            }
        }

        private static void PingSite(Uri uri)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.GetResponse();
            }
            catch (Exception ex)
            {
                Utils.LogException(ex, $"{uri}: ");
            }
        }

        private readonly TimeSpan _peroid;
        private readonly List<Uri> _uris;
        private CancellationTokenSource _periodicCancellationSource;
    }
}
