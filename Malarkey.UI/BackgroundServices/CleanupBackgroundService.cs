
using Malarkey.Abstractions.Util;
using Malarkey.Application.Cleanup;
using Malarkey.Persistence.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Malarkey.UI.BackgroundServices
{
    public class CleanupBackgroundService : BackgroundService
    {

        private readonly ILogger<CleanupBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan? _timeToWait;
        private readonly CancellationTokenSource _stopSource = new CancellationTokenSource();

        public CleanupBackgroundService(ILogger<CleanupBackgroundService> logger, IServiceScopeFactory scopeFactory, IOptions<PersistenceConfiguration> config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _timeToWait = config.Value.CleanupIntervalInSeconds?.Pipe(secs => TimeSpan.FromSeconds(secs));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(_timeToWait != null && !_stopSource.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_timeToWait.Value);
                    var timer = Stopwatch.StartNew();
                    using var scope = _scopeFactory.CreateScope();
                    var cleaner = scope.ServiceProvider.GetRequiredService<IPersistenceCleaner>();
                    await cleaner.PerformCleanup();
                    timer.Stop();
                    _logger.LogInformation($"Ran persistence cleanup in {timer.Elapsed.TotalSeconds} seconds");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "During cleanup");
                    await Task.Delay(_timeToWait.Value * 2);
                }
            }
        }
    }
}
