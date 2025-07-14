using Serilog;

var builder = DistributedApplication.CreateBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Add RabbitMQ as external connection string from configuration
var rabbitmq = builder.AddConnectionString("rabbitmq");

// Add the Consumer Worker service with RabbitMQ reference - 3 replicas for load balancing
// Configure each worker with Firm context via environment variables
var consumerWorker = builder.AddProject<Projects.PSP_Template_Worker>("consumer-worker")
    .WithReference(rabbitmq)
    .WithEnvironment("PSP__FirmCode", "PSP_FIRM") // Firm context for workers
    .WithEnvironment("PSP__Messaging__WorkerCount", "6") // Configurable worker count
    .WithReplicas(3);

// Add the Producer Worker service with RabbitMQ reference
var producerWorker = builder.AddProject<Projects.PSP_Template1_Worker>("producer-worker")
    .WithReference(rabbitmq)
    .WithEnvironment("PSP__FirmCode", "PSP_FIRM") // Same firm context
    .WithReplicas(1);

// Example: Add another set of workers for a different firm
// var firmBWorkers = builder.AddProject<Projects.PSP_Template_Worker>("firm-b-workers")
//     .WithReference(rabbitmq)
//     .WithEnvironment("PSP__FirmCode", "FIRM_B") // Different firm
//     .WithEnvironment("PSP__Messaging__WorkerCount", "4") // Different worker count
//     .WithReplicas(2);

// Optionally add other services like Redis, SQL Server, etc.
// var redis = builder.AddRedis("cache");
// var sql = builder.AddSqlServer("sql").WithEnvironment("FIRM_CONTEXT", "PSP_FIRM");

// You can also add external services
// var externalApi = builder.AddProject<Projects.SomeApi>("external-api")
//     .WithEnvironment("RABBITMQ_CONNECTION", rabbitmq.GetConnectionString())
//     .WithEnvironment("PSP__FirmCode", "PSP_FIRM");

builder.Build().Run();
