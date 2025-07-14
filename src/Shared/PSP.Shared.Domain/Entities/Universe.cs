namespace PSP.Shared.Domain.Entities;

/// <summary>
/// Represents a Universe entity belonging to a Client
/// Will be stored in SQL Server with ClientId foreign key
/// </summary>
public class Universe
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Code { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Client Client { get; set; } = null!;
    
    public override string ToString() => $"Universe({Id}): {Code} [Client: {Client?.Code}, Firm: {Client?.Firm?.Code}]";
}
