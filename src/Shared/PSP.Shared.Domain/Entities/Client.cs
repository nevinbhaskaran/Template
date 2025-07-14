namespace PSP.Shared.Domain.Entities;

/// <summary>
/// Represents a Client entity belonging to a Firm
/// Will be stored in SQL Server with FirmId foreign key
/// </summary>
public class Client
{
    public int Id { get; set; }
    public int FirmId { get; set; }
    public string Code { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Firm Firm { get; set; } = null!;
    public virtual ICollection<Universe> Universes { get; set; } = new List<Universe>();
    
    public override string ToString() => $"Client({Id}): {Code} [Firm: {Firm?.Code}]";
}
