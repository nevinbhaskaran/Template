using MassTransit;
using Microsoft.Extensions.Logging;
using PSP.Shared.Infrastructure.Messaging.Configuration;
using PSP.Template.Contracts.Commands;

namespace PSP.Shared.Infrastructure.Messaging.Services;

public interface IMessagePublisher
{
    Task PublishAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, IProcessingCommand;
    
    Task PublishBatchAsync<TCommand>(IEnumerable<TCommand> commands, CancellationToken cancellationToken = default)
        where TCommand : class, IProcessingCommand;
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IQueueConfigurationService _queueConfigurationService;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(
        IPublishEndpoint publishEndpoint,
        IQueueConfigurationService queueConfigurationService,
        ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _queueConfigurationService = queueConfigurationService;
        _logger = logger;
    }

    public async Task PublishAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, IProcessingCommand
    {
        // Create a logging scope with correlation context
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CommandId"] = command.CommandId,
            ["CommandType"] = typeof(TCommand).Name,
            ["Firm"] = command.Context.Firm,
            ["Client"] = command.Context.Client,
            ["ProcessType"] = command.Context.ProcessType,
            ["Universe"] = command.Context.Universe ?? "N/A"
        });

        _logger.LogInformation("Starting command publication");

        await _queueConfigurationService.RegisterQueueAsync(command.Context);
        
        var routingKey = command.Context.GetRoutingKey();
        
        _logger.LogInformation("Publishing with routing key {RoutingKey}", routingKey);

        try
        {
            await _publishEndpoint.Publish(command, context =>
            {
                context.SetRoutingKey(routingKey);
                context.CorrelationId = command.CommandId;
            }, cancellationToken);
            
            _logger.LogInformation("Command published successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish command");
            throw;
        }
    }

    public async Task PublishBatchAsync<TCommand>(IEnumerable<TCommand> commands, CancellationToken cancellationToken = default)
        where TCommand : class, IProcessingCommand
    {
        var commandList = commands.ToList();
        
        // Create a batch-level scope
        using var batchScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["BatchId"] = Guid.NewGuid(),
            ["CommandType"] = typeof(TCommand).Name,
            ["BatchSize"] = commandList.Count
        });

        _logger.LogInformation("Starting batch publication of {Count} commands", commandList.Count);

        try
        {
            var tasks = commandList.Select(command => PublishAsync(command, cancellationToken));
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Batch publication completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch publication failed");
            throw;
        }
    }
}
