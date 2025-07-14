using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PSP.Template1.Worker.Services;

namespace PSP.Template1.Worker;

/// <summary>
/// Background service that periodically publishes messages to demonstrate producer functionality
/// </summary>
public class ProducerWorkerService : BackgroundService
{
    private readonly ILogger<ProducerWorkerService> _logger;
    private readonly ProducerSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public ProducerWorkerService(
        ILogger<ProducerWorkerService> logger,
        IOptions<ProducerSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Producer Worker Service started");
        _logger.LogInformation("Publish interval: {IntervalSeconds} seconds", _settings.PublishIntervalSeconds);
        _logger.LogInformation("Batch size: {BatchSize}", _settings.BatchSize);
        _logger.LogInformation("Random data enabled: {EnableRandomData}", _settings.EnableRandomData);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var producerService = scope.ServiceProvider.GetRequiredService<IMessageProducerService>();

                _logger.LogInformation("Producer Worker running at: {Time}", DateTimeOffset.Now);
                
                if (_settings.EnableRandomData)
                {
                    await producerService.PublishBatchAsync(_settings.BatchSize, stoppingToken);
                    _logger.LogInformation("Successfully published batch of {BatchSize} messages", _settings.BatchSize);
                }
                else
                {
                    _logger.LogInformation("Random data generation is disabled. Skipping message publication.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while publishing messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.PublishIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Producer Worker Service stopped");
    }
}
