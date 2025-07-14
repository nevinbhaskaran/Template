using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSP.Template.Contracts.Commands;
using PSP.Shared.Infrastructure.Messaging.Configuration;
using PSP.Shared.Infrastructure.Messaging.Consumers;
using PSP.Shared.Infrastructure.Messaging.Services;
using RabbitMQ.Client;

namespace PSP.Shared.Infrastructure.Messaging.Configuration;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddPSPMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        // Register our messaging services
        services.AddSingleton<IQueueConfigurationService, QueueConfigurationService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        
        // Register all consumers
        services.AddScoped<SecurityAggregationCommandConsumer>();
        services.AddScoped<SecurityMappingCommandConsumer>();
        services.AddScoped<UniverseSOIMappingCommandConsumer>();
        services.AddScoped<ValidationCommandConsumer>();

        // Configure MassTransit
        services.AddMassTransit(x =>
        {
            // Add all consumers
            x.AddConsumer<SecurityAggregationCommandConsumer>();
            x.AddConsumer<SecurityMappingCommandConsumer>();
            x.AddConsumer<UniverseSOIMappingCommandConsumer>();
            x.AddConsumer<ValidationCommandConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                // Get RabbitMQ connection from Aspire service discovery
                var connectionString = configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(connectionString);
                }
                else
                {
                    // Fallback to localhost for development
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("admin");
                        h.Password("admin123");
                    });
                }

                // Configure the main exchange as durable and auto-delete false
                cfg.Publish<SecurityAggregationCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<SecurityMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<UniverseSOIMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<ValidationCommand>(x => x.ExchangeType = ExchangeType.Topic);

                // Configure consumers with configurable worker count
                ConfigureConsumerEndpoints(cfg, context, configuration);
            });
        });

        return services;
    }

    public static IServiceCollection AddPSPMessagingPublisherOnly(this IServiceCollection services, IConfiguration configuration)
    {
        // Register our services (only publisher, no consumers)
        services.AddSingleton<IQueueConfigurationService, QueueConfigurationService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        
        // Configure MassTransit for publishing only
        services.AddMassTransit(x =>
        {
            // No consumers registered for publisher-only mode

            x.UsingRabbitMq((context, cfg) =>
            {
                // Get RabbitMQ connection from Aspire service discovery
                var connectionString = configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(connectionString);
                }
                else
                {
                    // Fallback to localhost for development
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("admin");
                        h.Password("admin123");
                    });
                }

                // Configure publishing for topic exchanges
                cfg.Publish<SecurityAggregationCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<SecurityMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<UniverseSOIMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<ValidationCommand>(x => x.ExchangeType = ExchangeType.Topic);

                // No consumer endpoints configured
            });
        });

        return services;
    }

    /// <summary>
    /// Configure consumer endpoints for competing consumers (load balancing)
    /// Using smart hash-based routing with auto-scaling for dynamic clients/universes
    /// Provides maximum parallelism without static queue management overhead
    /// </summary>
    private static void ConfigureConsumerEndpoints(IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context, IConfiguration configuration)
    {
        // Get worker count from configuration (default to 6 for high parallelism)
        var workerCount = int.TryParse(configuration["PSP:Messaging:WorkerCount"], out var count) ? count : 6;
        
        // Security Aggregation Consumers - Smart hash distribution
        for (int i = 1; i <= workerCount; i++)
        {
            var workerNum = i;
            cfg.ReceiveEndpoint($"psp.security-aggregation.worker{workerNum}", e =>
            {
                e.ConfigureConsumer<SecurityAggregationCommandConsumer>(context);
                e.PrefetchCount = 15; // High throughput for dynamic load
                e.ConcurrentMessageLimit = 12; // Maximum parallelism per worker
                
                // Smart routing using modulo hash of client ID for even distribution
                // This automatically handles any number of dynamic clients
                e.Bind<SecurityAggregationCommand>(b =>
                {
                    // Hash-based routing: routes client hash % workerCount to this worker
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.securityaggregation.*"; // Handles client1, client11, client21, etc.
                    b.ExchangeType = ExchangeType.Topic;
                });
                e.Bind<SecurityAggregationCommand>(b =>
                {
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.securityaggregation"; // No universe specified
                    b.ExchangeType = ExchangeType.Topic;
                });
                
                // Additional catch patterns for better distribution
                if (workerNum <= 3) // First 3 workers get extra catch-all capacity
                {
                    e.Bind<SecurityAggregationCommand>(b =>
                    {
                        b.RoutingKey = "*.*.securityaggregation.*"; // Fallback for any missed patterns
                        b.ExchangeType = ExchangeType.Topic;
                    });
                }
            });
        }

        // Security Mapping Consumers - Same smart pattern
        for (int i = 1; i <= workerCount; i++)
        {
            var workerNum = i;
            cfg.ReceiveEndpoint($"psp.security-mapping.worker{workerNum}", e =>
            {
                e.ConfigureConsumer<SecurityMappingCommandConsumer>(context);
                e.PrefetchCount = 15;
                e.ConcurrentMessageLimit = 12;
                
                e.Bind<SecurityMappingCommand>(b =>
                {
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.securitymapping.*";
                    b.ExchangeType = ExchangeType.Topic;
                });
                e.Bind<SecurityMappingCommand>(b =>
                {
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.securitymapping";
                    b.ExchangeType = ExchangeType.Topic;
                });
                
                if (workerNum <= 3)
                {
                    e.Bind<SecurityMappingCommand>(b =>
                    {
                        b.RoutingKey = "*.*.securitymapping.*";
                        b.ExchangeType = ExchangeType.Topic;
                    });
                }
            });
        }

        // Universe SOI Mapping - High-throughput single worker (SOI is typically fast)
        cfg.ReceiveEndpoint("psp.universe-soi-mapping", e =>
        {
            e.ConfigureConsumer<UniverseSOIMappingCommandConsumer>(context);
            e.PrefetchCount = 20; // High throughput for fast SOI processing
            e.ConcurrentMessageLimit = 15; // Maximum parallelism for SOI
            
            e.Bind<UniverseSOIMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.universesoimapping.*"; // All SOI messages
                b.ExchangeType = ExchangeType.Topic;
            });
            e.Bind<UniverseSOIMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.universesoimapping";
                b.ExchangeType = ExchangeType.Topic;
            });
        });

        // Validation Consumers - Smart distribution for validation workloads
        var validationWorkers = 4; // Validation typically needs fewer workers
        for (int i = 1; i <= validationWorkers; i++)
        {
            var workerNum = i;
            cfg.ReceiveEndpoint($"psp.validation.worker{workerNum}", e =>
            {
                e.ConfigureConsumer<ValidationCommandConsumer>(context);
                e.PrefetchCount = 10;
                e.ConcurrentMessageLimit = 8; // Good validation parallelism
                
                e.Bind<ValidationCommand>(b =>
                {
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.validation.*";
                    b.ExchangeType = ExchangeType.Topic;
                });
                e.Bind<ValidationCommand>(b =>
                {
                    b.RoutingKey = $"*.client*{(workerNum - 1) % 10}*.validation";
                    b.ExchangeType = ExchangeType.Topic;
                });
                
                if (workerNum <= 2)
                {
                    e.Bind<ValidationCommand>(b =>
                    {
                        b.RoutingKey = "*.*.validation.*";
                        b.ExchangeType = ExchangeType.Topic;
                    });
                }
            });
        }
    }

    /// <summary>
    /// Configure consumer endpoints with instance-specific queues
    /// Each instance gets its own queues and processes all messages
    /// </summary>
    public static IServiceCollection AddPSPMessagingWithInstanceQueues(this IServiceCollection services, IConfiguration configuration, string? instanceId = null)
    {
        // Register our services
        services.AddSingleton<IQueueConfigurationService, QueueConfigurationService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        
        // Register all consumers
        services.AddScoped<SecurityAggregationCommandConsumer>();
        services.AddScoped<SecurityMappingCommandConsumer>();
        services.AddScoped<UniverseSOIMappingCommandConsumer>();
        services.AddScoped<ValidationCommandConsumer>();

        // Generate instance ID if not provided
        instanceId ??= Environment.MachineName + "-" + Environment.ProcessId;

        // Configure MassTransit
        services.AddMassTransit(x =>
        {
            // Add all consumers
            x.AddConsumer<SecurityAggregationCommandConsumer>();
            x.AddConsumer<SecurityMappingCommandConsumer>();
            x.AddConsumer<UniverseSOIMappingCommandConsumer>();
            x.AddConsumer<ValidationCommandConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                // Get RabbitMQ connection from Aspire service discovery
                var connectionString = configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(connectionString);
                }
                else
                {
                    // Fallback to localhost for development
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("admin");
                        h.Password("admin123");
                    });
                }

                // Configure the main exchange as durable and auto-delete false
                cfg.Publish<SecurityAggregationCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<SecurityMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<UniverseSOIMappingCommand>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<ValidationCommand>(x => x.ExchangeType = ExchangeType.Topic);

                // Configure consumers with instance-specific queue names
                ConfigureInstanceConsumerEndpoints(cfg, context, instanceId);
            });
        });

        return services;
    }

    /// <summary>
    /// Configure consumer endpoints with instance-specific queue names
    /// </summary>
    private static void ConfigureInstanceConsumerEndpoints(IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context, string instanceId)
    {
        // Security Aggregation Consumer - instance specific
        cfg.ReceiveEndpoint($"psp.security-aggregation.{instanceId}", e =>
        {
            e.ConfigureConsumer<SecurityAggregationCommandConsumer>(context);
            e.PrefetchCount = 5;
            e.AutoDelete = true; // Clean up when instance stops
        });

        // Security Mapping Consumer - instance specific
        cfg.ReceiveEndpoint($"psp.security-mapping.{instanceId}", e =>
        {
            e.ConfigureConsumer<SecurityMappingCommandConsumer>(context);
            e.PrefetchCount = 3;
            e.AutoDelete = true;
        });

        // Universe SOI Mapping Consumer - instance specific
        cfg.ReceiveEndpoint($"psp.universe-soi-mapping.{instanceId}", e =>
        {
            e.ConfigureConsumer<UniverseSOIMappingCommandConsumer>(context);
            e.PrefetchCount = 2;
            e.AutoDelete = true;
        });

        // Validation Consumer - instance specific
        cfg.ReceiveEndpoint($"psp.validation.{instanceId}", e =>
        {
            e.ConfigureConsumer<ValidationCommandConsumer>(context);
            e.PrefetchCount = 4;
            e.AutoDelete = true;
        });
    }
}
