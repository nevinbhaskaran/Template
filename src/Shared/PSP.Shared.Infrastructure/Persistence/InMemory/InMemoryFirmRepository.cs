using PSP.Shared.Domain.Entities;
using PSP.Shared.Domain.Repositories;
using System.Collections.Concurrent;

namespace PSP.Shared.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of IFirmRepository
/// This will be replaced with SQL Server Entity Framework implementation later
/// Thread-safe using ConcurrentDictionary for development/testing
/// </summary>
public class InMemoryFirmRepository : IFirmRepository
{
    private readonly ConcurrentDictionary<int, Firm> _firms = new();
    private int _nextId = 1;

    public InMemoryFirmRepository()
    {
        // Seed with default firm for development
        SeedDefaultData();
    }

    public Task<Firm?> GetByIdAsync(int id)
    {
        _firms.TryGetValue(id, out var firm);
        return Task.FromResult(firm);
    }

    public Task<Firm?> GetByCodeAsync(string code)
    {
        var firm = _firms.Values.FirstOrDefault(f => f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(firm);
    }

    public Task<IEnumerable<Firm>> GetAllAsync()
    {
        return Task.FromResult(_firms.Values.AsEnumerable());
    }

    public Task<Firm> AddAsync(Firm firm)
    {
        firm.Id = Interlocked.Increment(ref _nextId);
        _firms.TryAdd(firm.Id, firm);
        return Task.FromResult(firm);
    }

    public Task<Firm> UpdateAsync(Firm firm)
    {
        _firms.TryUpdate(firm.Id, firm, _firms[firm.Id]);
        return Task.FromResult(firm);
    }

    public Task DeleteAsync(int id)
    {
        _firms.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        // Seed a default firm for development
        var defaultFirm = new Firm
        {
            Id = 1,
            Code = "PSP_FIRM"
        };
        _firms.TryAdd(defaultFirm.Id, defaultFirm);
        _nextId = 2;
    }
}
