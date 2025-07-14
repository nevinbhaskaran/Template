using Microsoft.Extensions.DependencyInjection;
using PSP.Shared.Domain.Repositories;
using PSP.Shared.Infrastructure.Persistence.InMemory;
using PSP.Shared.Infrastructure.Services;

namespace PSP.Shared.Infrastructure.Configuration;

/// <summary>
/// Extension methods for registering PSP repositories and services
/// This will be extended later for SQL Server Entity Framework
/// </summary>
public static class PSPRepositoryConfiguration
{
    /// <summary>
    /// Add PSP repositories using in-memory implementation
    /// This will be replaced with SQL Server implementation later
    /// </summary>
    public static IServiceCollection AddPSPInMemoryRepositories(this IServiceCollection services)
    {
        // Register in-memory repositories (will be replaced with EF Core later)
        services.AddSingleton<IFirmRepository, InMemoryFirmRepository>();
        services.AddSingleton<IClientRepository, InMemoryClientRepository>();
        services.AddSingleton<IUniverseRepository, InMemoryUniverseRepository>();
        
        // Register firm context service for worker configuration
        services.AddScoped<IFirmContextService, FirmContextService>();
        
        return services;
    }
    
    /// <summary>
    /// Add PSP repositories using SQL Server Entity Framework (future implementation)
    /// </summary>
    public static IServiceCollection AddPSPSqlServerRepositories(this IServiceCollection services, string connectionString)
    {
        // TODO: Implement SQL Server repositories with Entity Framework
        // services.AddDbContext<PSPDbContext>(options => options.UseSqlServer(connectionString));
        // services.AddScoped<IFirmRepository, SqlServerFirmRepository>();
        // services.AddScoped<IClientRepository, SqlServerClientRepository>();
        // services.AddScoped<IUniverseRepository, SqlServerUniverseRepository>();
        // services.AddScoped<IFirmContextService, FirmContextService>();
        
        throw new NotImplementedException("SQL Server repositories not yet implemented. Use AddPSPInMemoryRepositories() for now.");
    }
}
