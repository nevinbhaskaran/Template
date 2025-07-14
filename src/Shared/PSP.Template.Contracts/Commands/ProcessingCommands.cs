namespace PSP.Template.Contracts.Commands;

/// <summary>
/// Command for security aggregation processing
/// </summary>
public record SecurityAggregationCommand : ProcessingCommandBase
{
    public required string SecurityId { get; init; }
    public required DateTime ProcessingDate { get; init; }
    public List<string>? SecurityGroups { get; init; }
}

/// <summary>
/// Command for security mapping processing
/// </summary>
public record SecurityMappingCommand : ProcessingCommandBase
{
    public required string SourceSecurityId { get; init; }
    public required string TargetSecurityId { get; init; }
    public required string MappingType { get; init; }
}

/// <summary>
/// Command for universe SOI mapping processing
/// </summary>
public record UniverseSOIMappingCommand : ProcessingCommandBase
{
    public required string UniverseId { get; init; }
    public required List<string> SecurityIds { get; init; }
    public required string SOIType { get; init; }
}

/// <summary>
/// Command for validation processing
/// </summary>
public record ValidationCommand : ProcessingCommandBase
{
    public required string ValidationRuleSet { get; init; }
    public required Dictionary<string, object> ValidationData { get; init; }
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}

/// <summary>
/// Validation severity levels
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
