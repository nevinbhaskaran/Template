namespace PSP.Template.Contracts.Models;

/// <summary>
/// Represents the processing context for a command
/// </summary>
public record ProcessingContext
{
    public required string Firm { get; init; }
    public required string Client { get; init; }
    public required ProcessType ProcessType { get; init; }
    public string? Universe { get; init; }
    
    /// <summary>
    /// Generates a unique routing key for message routing
    /// Format: firm.client.processtype[.universe]
    /// </summary>
    public string GetRoutingKey()
    {
        var key = $"{Firm.ToLowerInvariant()}.{Client.ToLowerInvariant()}.{ProcessType.ToString().ToLowerInvariant()}";
        if (!string.IsNullOrEmpty(Universe))
        {
            key += $".{Universe.ToLowerInvariant()}";
        }
        return key;
    }
    
    /// <summary>
    /// Generates queue name for this processing context
    /// </summary>
    public string GetQueueName()
    {
        return $"psp.{GetRoutingKey().Replace('.', '-')}";
    }
}

/// <summary>
/// Available process types in the system
/// </summary>
public enum ProcessType
{
    SecurityAggregation,
    SecurityMapping,
    UniverseSOIMapping,
    Validation
}
