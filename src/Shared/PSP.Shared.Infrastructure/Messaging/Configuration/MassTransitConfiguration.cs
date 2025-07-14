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
        // Register our services
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

                // Configure consumers with simpler queue binding
                ConfigureConsumerEndpoints(cfg, context);
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
    /// Multiple instances will compete for messages from the same queues
    /// Using topic routing patterns to match published routing keys
    /// </summary>
    private static void ConfigureConsumerEndpoints(IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        // Security Aggregation Consumer - competing consumers
        // Binds to SecurityAggregationCommand topic exchange with routing pattern
        cfg.ReceiveEndpoint("psp.security-aggregation", e =>
        {
            e.ConfigureConsumer<SecurityAggregationCommandConsumer>(context);
            e.PrefetchCount = 5; // Allow 5 unprocessed messages
            e.ConcurrentMessageLimit = 1; // Process one message at a time for safety
            
            // Bind to SecurityAggregationCommand topic exchange with routing patterns
            e.Bind<SecurityAggregationCommand>(b =>
            {
                b.RoutingKey = "*.*.securityaggregation.*"; // firm.client.securityaggregation.universe
                b.ExchangeType = ExchangeType.Topic;
            });
            e.Bind<SecurityAggregationCommand>(b =>
            {
                b.RoutingKey = "*.*.securityaggregation"; // firm.client.securityaggregation (no universe)
                b.ExchangeType = ExchangeType.Topic;
            });
        });

        // Security Mapping Consumer - competing consumers
        cfg.ReceiveEndpoint("psp.security-mapping", e =>
        {
            e.ConfigureConsumer<SecurityMappingCommandConsumer>(context);
            e.PrefetchCount = 3;
            e.ConcurrentMessageLimit = 1;
            
            // Bind to SecurityMappingCommand topic exchange with routing patterns
            e.Bind<SecurityMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.securitymapping.*"; // firm.client.securitymapping.universe
                b.ExchangeType = ExchangeType.Topic;
            });
            e.Bind<SecurityMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.securitymapping"; // firm.client.securitymapping (no universe)
                b.ExchangeType = ExchangeType.Topic;
            });
        });

        // Universe SOI Mapping Consumer - competing consumers
        cfg.ReceiveEndpoint("psp.universe-soi-mapping", e =>
        {
            e.ConfigureConsumer<UniverseSOIMappingCommandConsumer>(context);
            e.PrefetchCount = 2;
            e.ConcurrentMessageLimit = 1;
            
            // Bind to UniverseSOIMappingCommand topic exchange with routing patterns
            e.Bind<UniverseSOIMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.universesoimapping.*"; // firm.client.universesoimapping.universe
                b.ExchangeType = ExchangeType.Topic;
            });
            e.Bind<UniverseSOIMappingCommand>(b =>
            {
                b.RoutingKey = "*.*.universesoimapping"; // firm.client.universesoimapping (no universe)
                b.ExchangeType = ExchangeType.Topic;
            });
        });

        // Validation Consumer - competing consumers
        cfg.ReceiveEndpoint("psp.validation", e =>
        {
            e.ConfigureConsumer<ValidationCommandConsumer>(context);
            e.PrefetchCount = 4;
            e.ConcurrentMessageLimit = 1;
            
            // Bind to ValidationCommand topic exchange with routing patterns
            e.Bind<ValidationCommand>(b =>
            {
                b.RoutingKey = "*.*.validation.*"; // firm.client.validation.universe
                b.ExchangeType = ExchangeType.Topic;
            });
            e.Bind<ValidationCommand>(b =>
            {
                b.RoutingKey = "*.*.validation"; // firm.client.validation (no universe)
                b.ExchangeType = ExchangeType.Topic;
            });
        });
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
