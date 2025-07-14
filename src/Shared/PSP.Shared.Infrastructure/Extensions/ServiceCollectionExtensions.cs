using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSP.Shared.Infrastructure.Abstractions;
using PSP.Shared.Infrastructure.Services;
using PSP.Shared.Infrastructure.Messaging.Configuration;

namespace PSP.Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all infrastructure services including messaging
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        // services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        
        // Register other services as needed
        // services.AddScoped<IBackgroundJobProcessor, BackgroundJobProcessor>();
        
        // Add messaging infrastructure
        services.AddPSPMessaging(configuration);
        
        return services;
    }

    /// <summary>
    /// Register just the messaging infrastructure without other components
    /// </summary>
    public static IServiceCollection AddPSPMessagingOnly(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPSPMessaging(configuration);
    }

    /// <summary>
    /// Register a repository for a specific entity
    /// </summary>
    public static IServiceCollection AddRepository<TEntity, TKey, TImplementation>(this IServiceCollection services)
        where TEntity : class
        where TImplementation : class, IRepository<TEntity, TKey>
    {
        services.AddScoped<IRepository<TEntity, TKey>, TImplementation>();
        return services;
    }

    /// <summary>
    /// Register a job handler
    /// </summary>
    public static IServiceCollection AddJobHandler<TJobData, THandler>(this IServiceCollection services)
        where TJobData : class
        where THandler : class, IJobHandler<TJobData>
    {
        services.AddScoped<IJobHandler<TJobData>, THandler>();
        return services;
    }

    /// <summary>
    /// Register infrastructure services for publisher-only scenarios (no consumers)
    /// </summary>
    public static IServiceCollection AddSharedInfrastructurePublisherOnly(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        // services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        
        // Register other services as needed
        // services.AddScoped<IBackgroundJobProcessor, BackgroundJobProcessor>();
        
        // Add messaging infrastructure for publishing only
        services.AddPSPMessagingPublisherOnly(configuration);
        
        return services;
    }


}
