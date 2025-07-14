using PSP.Shared.Domain.Entities;
using PSP.Shared.Domain.Repositories;
using System.Collections.Concurrent;

namespace PSP.Shared.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of IUniverseRepository
/// This will be replaced with SQL Server Entity Framework implementation later
/// Thread-safe using ConcurrentDictionary for development/testing
/// </summary>
public class InMemoryUniverseRepository : IUniverseRepository
{
    private readonly ConcurrentDictionary<int, Universe> _universes = new();
    private readonly IClientRepository _clientRepository;
    private int _nextId = 1;

    public InMemoryUniverseRepository(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
        // Seed will be called after client repository is available
        _ = Task.Run(SeedDefaultDataAsync);
    }

    public Task<Universe?> GetByIdAsync(int id)
    {
        _universes.TryGetValue(id, out var universe);
        return Task.FromResult(universe);
    }

    public Task<Universe?> GetByCodeAsync(string code)
    {
        var universe = _universes.Values.FirstOrDefault(u => u.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(universe);
    }

    public Task<Universe?> GetByCodeAndClientAsync(string code, int clientId)
    {
        var universe = _universes.Values.FirstOrDefault(u => 
            u.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && u.ClientId == clientId);
        return Task.FromResult(universe);
    }

    public Task<IEnumerable<Universe>> GetByClientIdAsync(int clientId)
    {
        var universes = _universes.Values.Where(u => u.ClientId == clientId);
        return Task.FromResult(universes);
    }

    public async Task<IEnumerable<Universe>> GetByFirmIdAsync(int firmId)
    {
        var clients = await _clientRepository.GetByFirmIdAsync(firmId);
        var clientIds = clients.Select(c => c.Id).ToHashSet();
        var universes = _universes.Values.Where(u => clientIds.Contains(u.ClientId));
        return universes;
    }

    public Task<IEnumerable<Universe>> GetAllAsync()
    {
        return Task.FromResult(_universes.Values.AsEnumerable());
    }

    public async Task<Universe> AddAsync(Universe universe)
    {
        universe.Id = Interlocked.Increment(ref _nextId);
        
        // Load client reference if not already loaded
        if (universe.Client == null)
        {
            universe.Client = await _clientRepository.GetByIdAsync(universe.ClientId) ?? throw new InvalidOperationException($"Client with ID {universe.ClientId} not found");
        }
        
        _universes.TryAdd(universe.Id, universe);
        return universe;
    }

    public Task<Universe> UpdateAsync(Universe universe)
    {
        _universes.TryUpdate(universe.Id, universe, _universes[universe.Id]);
        return Task.FromResult(universe);
    }

    public Task DeleteAsync(int id)
    {
        _universes.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private async Task SeedDefaultDataAsync()
    {
        try
        {
            // Wait for dependencies to be ready
            await Task.Delay(200);
            
            var clients = await _clientRepository.GetAllAsync();
            var clientList = clients.ToList();
            
            if (!clientList.Any()) return;

            // Seed some default universes for development
            var universes = new List<Universe>();
            
            foreach (var client in clientList.Take(3)) // First 3 clients
            {
                universes.AddRange(new[]
                {
                    new Universe { ClientId = client.Id, Code = "FXUNIVERSE", Client = client },
                    new Universe { ClientId = client.Id, Code = "SOIUNIVERSE", Client = client },
                    new Universe { ClientId = client.Id, Code = "CRYPTOUNIVERSE", Client = client }
                });
            }

            foreach (var universe in universes)
            {
                universe.Id = Interlocked.Increment(ref _nextId);
                _universes.TryAdd(universe.Id, universe);
            }
        }
        catch
        {
            // Ignore seeding errors in development
        }
    }
}
