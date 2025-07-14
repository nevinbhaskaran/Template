using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PSP.Template.Worker;

/// <summary>
/// Consumer Worker Service - Only consumes messages via MassTransit consumers
/// </summary>
public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly WorkerSettings _settings;

    public WorkerService(
        ILogger<WorkerService> logger, 
        IOptions<WorkerSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consumer Worker service is starting...");
        _logger.LogInformation("This worker only consumes messages - no publishing functionality");
        _logger.LogInformation("Processing interval: {IntervalSeconds} seconds", _settings.ProcessingIntervalSeconds);
        _logger.LogInformation("Background processing enabled: {Enabled}", _settings.EnableBackgroundProcessing);

        if (!_settings.EnableBackgroundProcessing)
        {
            _logger.LogWarning("Background processing is disabled. Worker will exit.");
            return;
        }

        // Wait a bit for the messaging system to be ready
        await Task.Delay(5000, stoppingToken);

        var iterationCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Consumer Worker running at: {time} (Iteration: {iteration})", DateTimeOffset.Now, ++iterationCount);
            
            try
            {
                // Only background monitoring work - no message publishing
                await ExecuteWithLoggingAsync(
                    () => DoMonitoringWorkAsync(stoppingToken),
                    "Background monitoring execution");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during background monitoring");
            }
            
            // Wait for the configured interval before the next iteration
            var delay = TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds);
            _logger.LogDebug("Waiting {DelaySeconds} seconds before next iteration", delay.TotalSeconds);
            await Task.Delay(delay, stoppingToken);
        }
        
        _logger.LogInformation("Consumer Worker service is stopping...");
    }

    private async Task DoMonitoringWorkAsync(CancellationToken cancellationToken)
    {
        // Simulate monitoring work (health checks, metrics, etc.)
        _logger.LogInformation("Performing background monitoring work...");
        _logger.LogInformation("Consumers are active and ready to process incoming messages");
        
        // Simulate work with configurable processing mode
        var workDuration = _settings.ProcessingMode switch
        {
            "Fast" => 2000,
            "Standard" => 5000,
            "Thorough" => 10000,
            _ => 5000
        };
        
        await Task.Delay(workDuration, cancellationToken);
        
        _logger.LogInformation("Background monitoring completed in {Duration}ms", workDuration);
    }

    /// <summary>
    /// Execute an operation with logging and error handling
    /// </summary>
    private async Task ExecuteWithLoggingAsync(
        Func<Task> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting {Operation}", operationName);
            await operation();
            _logger.LogInformation("Completed {Operation} successfully", operationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute {Operation}", operationName);
            throw;
        }
    }
}
