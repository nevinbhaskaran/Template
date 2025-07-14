namespace PSP.Shared.Domain.Entities;

/// <summary>
/// Represents a Firm entity that will be stored in SQL Server
/// Currently using in-memory implementation for development
/// </summary>
public class Firm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    
    public override string ToString() => $"Firm({Id}): {Code}";
}
