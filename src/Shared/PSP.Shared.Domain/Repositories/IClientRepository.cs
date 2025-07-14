using PSP.Shared.Domain.Entities;

namespace PSP.Shared.Domain.Repositories;

/// <summary>
/// Repository interface for Client entities
/// Will be implemented with SQL Server Entity Framework later
/// </summary>
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int id);
    Task<Client?> GetByCodeAsync(string code);
    Task<Client?> GetByCodeAndFirmAsync(string code, int firmId);
    Task<IEnumerable<Client>> GetByFirmIdAsync(int firmId);
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client> AddAsync(Client client);
    Task<Client> UpdateAsync(Client client);
    Task DeleteAsync(int id);
}
