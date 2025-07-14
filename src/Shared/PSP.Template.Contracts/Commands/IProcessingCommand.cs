using PSP.Template.Contracts.Models;

namespace PSP.Template.Contracts.Commands;

/// <summary>
/// Base interface for all processing commands
/// </summary>
public interface IProcessingCommand
{
    Guid CommandId { get; }
    ProcessingContext Context { get; }
    DateTime CreatedAt { get; }
    Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Base implementation for processing commands
/// </summary>
public abstract record ProcessingCommandBase : IProcessingCommand
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
    public required ProcessingContext Context { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}
