using PSP.Shared.Domain.Entities;

namespace PSP.Shared.Domain.Repositories;

/// <summary>
/// Repository interface for Firm entities
/// Will be implemented with SQL Server Entity Framework later
/// </summary>
public interface IFirmRepository
{
    Task<Firm?> GetByIdAsync(int id);
    Task<Firm?> GetByCodeAsync(string code);
    Task<IEnumerable<Firm>> GetAllAsync();
    Task<Firm> AddAsync(Firm firm);
    Task<Firm> UpdateAsync(Firm firm);
    Task DeleteAsync(int id);
}
