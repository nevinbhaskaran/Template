using MassTransit;
using Microsoft.Extensions.Logging;
using PSP.Template.Contracts.Commands;
using PSP.Template.Contracts.Models;

namespace PSP.Shared.Infrastructure.Messaging.Consumers;

public class SecurityAggregationCommandConsumer : ProcessingCommandConsumerBase<SecurityAggregationCommand>
{
    public SecurityAggregationCommandConsumer(
        ILogger<SecurityAggregationCommandConsumer> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    public override ProcessType SupportedProcessType => ProcessType.SecurityAggregation;

    protected override async Task ProcessCommandAsync(SecurityAggregationCommand command, ConsumeContext<SecurityAggregationCommand> context, CancellationToken cancellationToken)
    {
        // Method-level scope for ProcessCommandAsync
        using var methodScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Method"] = "ProcessCommandAsync",
            ["SecurityId"] = command.SecurityId,
            ["ProcessingDate"] = command.ProcessingDate
        });

        _logger.LogInformation("Starting Security Aggregation processing");

        // Call Method1 - which will create its own nested scope
        await Method1_ValidateAndPrepare(command, cancellationToken);
        
        // Call Method2 - which will also create its own nested scope  
        await Method2_ProcessAndAggregate(command, cancellationToken);

        _logger.LogInformation("Security Aggregation processing completed successfully");
    }

    private async Task Method1_ValidateAndPrepare(SecurityAggregationCommand command, CancellationToken cancellationToken)
    {
        // Nested scope for Method1 - inherits all parent scope properties
        using var method1Scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Method"] = "Method1_ValidateAndPrepare",
            ["ValidationStep"] = "SecurityValidation"
        });

        _logger.LogInformation("Validating security data");
        
        // Simulate validation work
        await Task.Delay(500, cancellationToken);
        
        _logger.LogInformation("Security validation completed");
    }

    private async Task Method2_ProcessAndAggregate(SecurityAggregationCommand command, CancellationToken cancellationToken)
    {
        // Nested scope for Method2 - inherits all parent scope properties
        using var method2Scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Method"] = "Method2_ProcessAndAggregate",
            ["AggregationType"] = "RiskMetrics"
        });

        _logger.LogInformation("Starting risk metrics aggregation");
        
        // Simulate complex processing
        await Task.Delay(Random.Shared.Next(1000, 3000), cancellationToken);
        
        _logger.LogInformation("Risk metrics aggregation completed");
    }
}

public class SecurityMappingCommandConsumer : ProcessingCommandConsumerBase<SecurityMappingCommand>
{
    public SecurityMappingCommandConsumer(
        ILogger<SecurityMappingCommandConsumer> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    public override ProcessType SupportedProcessType => ProcessType.SecurityMapping;

    protected override async Task ProcessCommandAsync(SecurityMappingCommand command, ConsumeContext<SecurityMappingCommand> context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Security Mapping from {SourceId} to {TargetId} for Universe {Universe}", 
            command.SourceSecurityId, command.TargetSecurityId, command.Context.Universe);

        // Simulate processing work
        await Task.Delay(Random.Shared.Next(1500, 4000), cancellationToken);

        // Your actual business logic here
        // - Map securities between systems
        // - Validate mappings
        // - Update mapping tables

        _logger.LogInformation("Completed Security Mapping from {SourceId} to {TargetId}", 
            command.SourceSecurityId, command.TargetSecurityId);
    }
}

public class UniverseSOIMappingCommandConsumer : ProcessingCommandConsumerBase<UniverseSOIMappingCommand>
{
    public UniverseSOIMappingCommandConsumer(
        ILogger<UniverseSOIMappingCommandConsumer> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    public override ProcessType SupportedProcessType => ProcessType.UniverseSOIMapping;

    protected override async Task ProcessCommandAsync(UniverseSOIMappingCommand command, ConsumeContext<UniverseSOIMappingCommand> context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Universe SOI Mapping for Universe {UniverseId} with {SecurityCount} securities", 
            command.UniverseId, command.SecurityIds.Count);

        // Simulate processing work
        await Task.Delay(Random.Shared.Next(2000, 5000), cancellationToken);

        // Your actual business logic here
        // - Map universe to SOI
        // - Process all securities in universe
        // - Update SOI mappings

        _logger.LogInformation("Completed Universe SOI Mapping for Universe {UniverseId}", command.UniverseId);
    }
}

public class ValidationCommandConsumer : ProcessingCommandConsumerBase<ValidationCommand>
{
    public ValidationCommandConsumer(
        ILogger<ValidationCommandConsumer> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    public override ProcessType SupportedProcessType => ProcessType.Validation;

    protected override async Task ProcessCommandAsync(ValidationCommand command, ConsumeContext<ValidationCommand> context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Validation with rule set {RuleSet} for Universe {Universe}", 
            command.ValidationRuleSet, command.Context.Universe);

        // Simulate processing work
        await Task.Delay(Random.Shared.Next(500, 2000), cancellationToken);

        // Your actual business logic here
        // - Run validation rules
        // - Generate validation reports
        // - Handle validation failures

        _logger.LogInformation("Completed Validation with rule set {RuleSet}", command.ValidationRuleSet);
    }
}
