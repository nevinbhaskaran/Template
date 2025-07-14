using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PSP.Template.Contracts.Models;

namespace PSP.Shared.Infrastructure.Messaging.Configuration;

public interface IQueueConfigurationService
{
    Task<QueueConfiguration> GetQueueConfigurationAsync(ProcessingContext context);
    Task RegisterQueueAsync(ProcessingContext context);
    Task<IEnumerable<QueueConfiguration>> GetAllActiveQueuesAsync();
}

public record QueueConfiguration
{
    public required string QueueName { get; init; }
    public required string RoutingKey { get; init; }
    public required ProcessingContext Context { get; init; }
    public int MaxConcurrency { get; init; } = 1;
    public TimeSpan? MessageTtl { get; init; }
    public bool AutoDelete { get; init; } = false;
    public Dictionary<string, object>? Arguments { get; init; }
}

public class QueueConfigurationService : IQueueConfigurationService
{
    private readonly ILogger<QueueConfigurationService> _logger;
    private readonly ConcurrentDictionary<string, QueueConfiguration> _configurations = new();

    public QueueConfigurationService(ILogger<QueueConfigurationService> logger)
    {
        _logger = logger;
    }

    public Task<QueueConfiguration> GetQueueConfigurationAsync(ProcessingContext context)
    {
        var routingKey = context.GetRoutingKey();
        
        if (_configurations.TryGetValue(routingKey, out var existing))
        {
            return Task.FromResult(existing);
        }

        var configuration = new QueueConfiguration
        {
            QueueName = context.GetQueueName(),
            RoutingKey = routingKey,
            Context = context,
            MaxConcurrency = GetConcurrencyForProcessType(context.ProcessType),
            MessageTtl = TimeSpan.FromHours(24), // 24 hour TTL
            Arguments = new Dictionary<string, object>
            {
                ["x-max-priority"] = GetPriorityForProcessType(context.ProcessType)
            }
        };

        _configurations.TryAdd(routingKey, configuration);
        return Task.FromResult(configuration);
    }

    public Task RegisterQueueAsync(ProcessingContext context)
    {
        var routingKey = context.GetRoutingKey();
        _logger.LogInformation("Registering queue for routing key: {RoutingKey}", routingKey);
        
        // This would typically persist to a database or configuration store
        return GetQueueConfigurationAsync(context);
    }

    public Task<IEnumerable<QueueConfiguration>> GetAllActiveQueuesAsync()
    {
        return Task.FromResult(_configurations.Values.AsEnumerable());
    }

    private static int GetConcurrencyForProcessType(ProcessType processType) => processType switch
    {
        ProcessType.SecurityAggregation => 5,
        ProcessType.SecurityMapping => 3,
        ProcessType.UniverseSOIMapping => 2,
        ProcessType.Validation => 4,
        _ => 1
    };

    private static int GetPriorityForProcessType(ProcessType processType) => processType switch
    {
        ProcessType.Validation => 10, // Highest priority
        ProcessType.SecurityAggregation => 8,
        ProcessType.UniverseSOIMapping => 6,
        ProcessType.SecurityMapping => 4,
        _ => 1
    };
}
