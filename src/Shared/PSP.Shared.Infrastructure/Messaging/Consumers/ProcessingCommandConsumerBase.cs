using MassTransit;
using Microsoft.Extensions.Logging;
using PSP.Template.Contracts.Commands;
using PSP.Template.Contracts.Models;
using System.Diagnostics;

namespace PSP.Shared.Infrastructure.Messaging.Consumers;

/// <summary>
/// Base interface for all command consumers
/// </summary>
public interface IProcessingCommandConsumer<in TCommand> : IConsumer<TCommand>
    where TCommand : class, IProcessingCommand
{
    ProcessType SupportedProcessType { get; }
}

/// <summary>
/// Base consumer with common functionality
/// </summary>
public abstract class ProcessingCommandConsumerBase<TCommand> : IProcessingCommandConsumer<TCommand>
    where TCommand : class, IProcessingCommand
{
    private static readonly ActivitySource ActivitySource = new("PSP.Messaging");
    protected readonly ILogger<ProcessingCommandConsumerBase<TCommand>> _logger;
    protected readonly IServiceProvider _serviceProvider;

    protected ProcessingCommandConsumerBase(
        ILogger<ProcessingCommandConsumerBase<TCommand>> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public abstract ProcessType SupportedProcessType { get; }

    public async Task Consume(ConsumeContext<TCommand> context)
    {
        var command = context.Message;
        var correlationId = context.CorrelationId ?? Guid.NewGuid();
        
        // Create distributed trace for message processing
        using var activity = ActivitySource.StartActivity($"{typeof(TCommand).Name} Processing");
        activity?.SetTag("command.id", command.CommandId.ToString());
        activity?.SetTag("command.type", typeof(TCommand).Name);
        activity?.SetTag("process.type", SupportedProcessType.ToString());
        activity?.SetTag("routing.key", command.Context.GetRoutingKey());
        activity?.SetTag("firm", command.Context.Firm);
        activity?.SetTag("client", command.Context.Client);
        activity?.SetTag("universe", command.Context.Universe ?? "N/A");
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CommandId"] = command.CommandId,
            ["CorrelationId"] = correlationId,
            ["ProcessType"] = SupportedProcessType,
            ["Firm"] = command.Context.Firm,
            ["Client"] = command.Context.Client,
            ["Universe"] = command.Context.Universe ?? "N/A"
        });

        try
        {
            _logger.LogInformation("Starting processing of {CommandType} for {RoutingKey}", 
                typeof(TCommand).Name, command.Context.GetRoutingKey());

            await ProcessCommandAsync(command, context, context.CancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation("Successfully completed processing of {CommandType}", typeof(TCommand).Name);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to process {CommandType} for {RoutingKey}", 
                typeof(TCommand).Name, command.Context.GetRoutingKey());
            
            // Implement retry logic or dead letter handling here
            throw;
        }
    }

    protected abstract Task ProcessCommandAsync(TCommand command, ConsumeContext<TCommand> context, CancellationToken cancellationToken);
}
