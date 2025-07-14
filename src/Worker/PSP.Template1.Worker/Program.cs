using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PSP.Template1.Worker;
using PSP.Template1.Worker.Services;
using PSP.Shared.Infrastructure.Extensions;
using Serilog;

// Create the host builder with Aspire Service Defaults
var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Add Aspire Service Defaults (includes telemetry, health checks, service discovery, etc.)
builder.AddServiceDefaults();

// Bind configuration
builder.Services.Configure<ProducerSettings>(
    builder.Configuration.GetSection("ProducerSettings"));

// Register shared infrastructure services for publishing only (no consumers)
builder.Services.AddSharedInfrastructurePublisherOnly(builder.Configuration);

// Register producer-specific services
builder.Services.AddScoped<IMessageProducerService, MessageProducerService>();

// Register the background service
builder.Services.AddHostedService<ProducerWorkerService>();

// Build and run the host
var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("PSP Template1 Producer Worker is starting...");
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Host terminated unexpectedly");
}
finally
{
    logger.LogInformation("PSP Template1 Producer Worker has stopped.");
}
