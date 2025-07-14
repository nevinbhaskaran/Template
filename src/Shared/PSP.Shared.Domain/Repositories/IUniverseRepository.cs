using PSP.Shared.Domain.Entities;

namespace PSP.Shared.Domain.Repositories;

/// <summary>
/// Repository interface for Universe entities
/// Will be implemented with SQL Server Entity Framework later
/// </summary>
public interface IUniverseRepository
{
    Task<Universe?> GetByIdAsync(int id);
    Task<Universe?> GetByCodeAsync(string code);
    Task<Universe?> GetByCodeAndClientAsync(string code, int clientId);
    Task<IEnumerable<Universe>> GetByClientIdAsync(int clientId);
    Task<IEnumerable<Universe>> GetByFirmIdAsync(int firmId);
    Task<IEnumerable<Universe>> GetAllAsync();
    Task<Universe> AddAsync(Universe universe);
    Task<Universe> UpdateAsync(Universe universe);
    Task DeleteAsync(int id);
}
