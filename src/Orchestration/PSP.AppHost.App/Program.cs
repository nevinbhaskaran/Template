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
var consumerWorker = builder.AddProject<Projects.PSP_Template_Worker>("consumer-worker")
    .WithReference(rabbitmq)
    .WithReplicas(3);

// Add the Producer Worker service with RabbitMQ reference
var producerWorker = builder.AddProject<Projects.PSP_Template1_Worker>("producer-worker")
    .WithReference(rabbitmq)
    .WithReplicas(1);

// Optionally add other services like Redis, SQL Server, etc.
// var redis = builder.AddRedis("cache");
// var sql = builder.AddSqlServer("sql");

// You can also add external services
// var externalApi = builder.AddProject<Projects.SomeApi>("external-api")
//     .WithEnvironment("RABBITMQ_CONNECTION", rabbitmq.GetConnectionString());

builder.Build().Run();
