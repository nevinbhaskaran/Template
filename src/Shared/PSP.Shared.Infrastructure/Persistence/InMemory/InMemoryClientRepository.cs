using PSP.Shared.Domain.Entities;
using PSP.Shared.Domain.Repositories;
using System.Collections.Concurrent;

namespace PSP.Shared.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of IClientRepository
/// This will be replaced with SQL Server Entity Framework implementation later
/// Thread-safe using ConcurrentDictionary for development/testing
/// </summary>
public class InMemoryClientRepository : IClientRepository
{
    private readonly ConcurrentDictionary<int, Client> _clients = new();
    private readonly IFirmRepository _firmRepository;
    private int _nextId = 1;

    public InMemoryClientRepository(IFirmRepository firmRepository)
    {
        _firmRepository = firmRepository;
        // Seed will be called after firm repository is available
        _ = Task.Run(SeedDefaultDataAsync);
    }

    public Task<Client?> GetByIdAsync(int id)
    {
        _clients.TryGetValue(id, out var client);
        return Task.FromResult(client);
    }

    public Task<Client?> GetByCodeAsync(string code)
    {
        var client = _clients.Values.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(client);
    }

    public Task<Client?> GetByCodeAndFirmAsync(string code, int firmId)
    {
        var client = _clients.Values.FirstOrDefault(c => 
            c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && c.FirmId == firmId);
        return Task.FromResult(client);
    }

    public Task<IEnumerable<Client>> GetByFirmIdAsync(int firmId)
    {
        var clients = _clients.Values.Where(c => c.FirmId == firmId);
        return Task.FromResult(clients);
    }

    public Task<IEnumerable<Client>> GetAllAsync()
    {
        return Task.FromResult(_clients.Values.AsEnumerable());
    }

    public async Task<Client> AddAsync(Client client)
    {
        client.Id = Interlocked.Increment(ref _nextId);
        
        // Load firm reference if not already loaded
        if (client.Firm == null)
        {
            client.Firm = await _firmRepository.GetByIdAsync(client.FirmId) ?? throw new InvalidOperationException($"Firm with ID {client.FirmId} not found");
        }
        
        _clients.TryAdd(client.Id, client);
        return client;
    }

    public Task<Client> UpdateAsync(Client client)
    {
        _clients.TryUpdate(client.Id, client, _clients[client.Id]);
        return Task.FromResult(client);
    }

    public Task DeleteAsync(int id)
    {
        _clients.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private async Task SeedDefaultDataAsync()
    {
        try
        {
            // Wait for firm repository to be ready
            await Task.Delay(100);
            
            var defaultFirm = await _firmRepository.GetByCodeAsync("PSP_FIRM");
            if (defaultFirm == null) return;

            // Seed some default clients for development
            var clients = new[]
            {
                new Client { FirmId = defaultFirm.Id, Code = "CLIENT1", Firm = defaultFirm },
                new Client { FirmId = defaultFirm.Id, Code = "CLIENT2", Firm = defaultFirm },
                new Client { FirmId = defaultFirm.Id, Code = "CLIENT3", Firm = defaultFirm }
            };

            foreach (var client in clients)
            {
                client.Id = Interlocked.Increment(ref _nextId);
                _clients.TryAdd(client.Id, client);
            }
        }
        catch
        {
            // Ignore seeding errors in development
        }
    }
}
