using Microsoft.Extensions.Logging;
using PSP.Template.Contracts.Commands;
using PSP.Template.Contracts.Models;
using PSP.Shared.Infrastructure.Messaging.Services;
using System.Diagnostics;

namespace PSP.Template1.Worker.Services;

/// <summary>
/// Service responsible for producing and publishing messages to the queue
/// </summary>
public interface IMessageProducerService
{
    Task PublishBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    Task PublishSecurityAggregationCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default);
    Task PublishSecurityMappingCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default);
    Task PublishUniverseSOIMappingCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default);
    Task PublishValidationCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default);
}

public class MessageProducerService : IMessageProducerService
{
    private static readonly ActivitySource ActivitySource = new("PSP.Messaging");
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<MessageProducerService> _logger;
    
    private static readonly string[] Firms = { "firm1", "firm2", "firm3" };
    private static readonly string[] Clients = { "client1", "client2", "client3", "client4" };
    private static readonly string?[] Universes = { "fxuniverse1", "soiuniverse1", "equityuniverse1", "bonduniverse1", null };
    private static readonly string[] SecurityIds = { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "META", "NFLX", "NVDA" };

    public MessageProducerService(
        IMessagePublisher messagePublisher,
        ILogger<MessageProducerService> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task PublishBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Publish Message Batch");
        activity?.SetTag("batch.size", batchSize);
        
        _logger.LogInformation("Publishing batch of {BatchSize} messages", batchSize);
        
        var tasks = new List<Task>();
        
        for (int i = 0; i < batchSize; i++)
        {
            var context = GenerateRandomContext();
            
            // Randomly choose a command type to publish
            var commandType = Random.Shared.Next(4);
            
            var task = commandType switch
            {
                0 => PublishSecurityAggregationCommandAsync(context, cancellationToken),
                1 => PublishSecurityMappingCommandAsync(context, cancellationToken),
                2 => PublishUniverseSOIMappingCommandAsync(context, cancellationToken),
                3 => PublishValidationCommandAsync(context, cancellationToken),
                _ => PublishSecurityAggregationCommandAsync(context, cancellationToken)
            };
            
            tasks.Add(task);
        }

        try
        {
            await Task.WhenAll(tasks);
            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation("Successfully published batch of {BatchSize} messages", batchSize);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task PublishSecurityAggregationCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default)
    {
        var command = new SecurityAggregationCommand
        {
            Context = context,
            SecurityId = GetRandomSecurityId(),
            ProcessingDate = DateTime.UtcNow.Date,
            SecurityGroups = GetRandomSecurityGroups(),
            Metadata = new Dictionary<string, object>
            {
                ["Producer"] = "PSP.Template1.Worker",
                ["CreatedBy"] = "MessageProducerService"
            }
        };

        await _messagePublisher.PublishAsync(command, cancellationToken);
        _logger.LogInformation("Published SecurityAggregationCommand for {SecurityId} with routing key {RoutingKey}", 
            command.SecurityId, context.GetRoutingKey());
    }

    public async Task PublishSecurityMappingCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default)
    {
        var command = new SecurityMappingCommand
        {
            Context = context,
            SourceSecurityId = GetRandomSecurityId(),
            TargetSecurityId = GetRandomSecurityId(),
            MappingType = GetRandomMappingType(),
            Metadata = new Dictionary<string, object>
            {
                ["Producer"] = "PSP.Template1.Worker",
                ["CreatedBy"] = "MessageProducerService"
            }
        };

        await _messagePublisher.PublishAsync(command, cancellationToken);
        _logger.LogInformation("Published SecurityMappingCommand from {SourceId} to {TargetId} with routing key {RoutingKey}", 
            command.SourceSecurityId, command.TargetSecurityId, context.GetRoutingKey());
    }

    public async Task PublishUniverseSOIMappingCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default)
    {
        var command = new UniverseSOIMappingCommand
        {
            Context = context,
            UniverseId = context.Universe ?? "defaultuniverse",
            SecurityIds = GetRandomSecurityIds(),
            SOIType = GetRandomSOIType(),
            Metadata = new Dictionary<string, object>
            {
                ["Producer"] = "PSP.Template1.Worker",
                ["CreatedBy"] = "MessageProducerService"
            }
        };

        await _messagePublisher.PublishAsync(command, cancellationToken);
        _logger.LogInformation("Published UniverseSOIMappingCommand for universe {UniverseId} with {SecurityCount} securities and routing key {RoutingKey}", 
            command.UniverseId, command.SecurityIds.Count, context.GetRoutingKey());
    }

    public async Task PublishValidationCommandAsync(ProcessingContext context, CancellationToken cancellationToken = default)
    {
        var command = new ValidationCommand
        {
            Context = context,
            ValidationRuleSet = GetRandomValidationRuleSet(),
            ValidationData = GetRandomValidationData(),
            Severity = GetRandomValidationSeverity(),
            Metadata = new Dictionary<string, object>
            {
                ["Producer"] = "PSP.Template1.Worker",
                ["CreatedBy"] = "MessageProducerService"
            }
        };

        await _messagePublisher.PublishAsync(command, cancellationToken);
        _logger.LogInformation("Published ValidationCommand with ruleset {RuleSet} and severity {Severity} with routing key {RoutingKey}", 
            command.ValidationRuleSet, command.Severity, context.GetRoutingKey());
    }

    private ProcessingContext GenerateRandomContext()
    {
        var firm = Firms[Random.Shared.Next(Firms.Length)];
        var client = Clients[Random.Shared.Next(Clients.Length)];
        var processType = (ProcessType)Random.Shared.Next(4);
        var universe = Universes[Random.Shared.Next(Universes.Length)];

        return new ProcessingContext
        {
            Firm = firm,
            Client = client,
            ProcessType = processType,
            Universe = universe
        };
    }

    private string GetRandomSecurityId() => SecurityIds[Random.Shared.Next(SecurityIds.Length)];

    private List<string> GetRandomSecurityGroups()
    {
        var count = Random.Shared.Next(1, 4);
        return Enumerable.Range(0, count)
            .Select(_ => $"Group{Random.Shared.Next(1, 10)}")
            .Distinct()
            .ToList();
    }

    private List<string> GetRandomSecurityIds()
    {
        var count = Random.Shared.Next(2, 6);
        return SecurityIds.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }

    private string GetRandomMappingType()
    {
        var types = new[] { "ISIN_TO_CUSIP", "BLOOMBERG_TO_REUTERS", "INTERNAL_TO_EXTERNAL" };
        return types[Random.Shared.Next(types.Length)];
    }

    private string GetRandomSOIType()
    {
        var types = new[] { "EQUITY", "BOND", "OPTION", "FUTURE" };
        return types[Random.Shared.Next(types.Length)];
    }

    private string GetRandomValidationRuleSet()
    {
        var ruleSets = new[] { "BASIC_VALIDATION", "EXTENDED_VALIDATION", "COMPLIANCE_CHECK", "DATA_QUALITY" };
        return ruleSets[Random.Shared.Next(ruleSets.Length)];
    }

    private Dictionary<string, object> GetRandomValidationData()
    {
        return new Dictionary<string, object>
        {
            ["SecurityId"] = GetRandomSecurityId(),
            ["Price"] = Math.Round(Random.Shared.NextDouble() * 1000, 2),
            ["Volume"] = Random.Shared.Next(1000, 100000),
            ["Timestamp"] = DateTime.UtcNow
        };
    }

    private ValidationSeverity GetRandomValidationSeverity()
    {
        var severities = Enum.GetValues<ValidationSeverity>();
        return severities[Random.Shared.Next(severities.Length)];
    }
}
