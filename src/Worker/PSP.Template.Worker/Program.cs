using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PSP.Template.Worker;
using PSP.Shared.Infrastructure.Extensions;
using PSP.Shared.Infrastructure.Constants;
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

// Bind configuration using shared constants
builder.Services.Configure<WorkerSettings>(
    builder.Configuration.GetSection(AppConstants.ConfigurationSections.WorkerSettings));

// Register shared infrastructure services including messaging (for consuming only)
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Register the background service
builder.Services.AddHostedService<WorkerService>();

// Add any additional services here
// builder.Services.AddScoped<IMyService, MyService>();
// builder.Services.AddHttpClient();

// Build and run the host
var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("PSP Template Worker is starting...");
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
    logger.LogInformation("PSP Template Worker has stopped.");
}
