using Microsoft.Extensions.Logging;

namespace PSP.Shared.Infrastructure.Services;

/// <summary>
/// Base service class with common functionality
/// </summary>
public abstract class BaseService
{
    protected readonly ILogger Logger;

    protected BaseService(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Log and handle exceptions in a consistent manner
    /// </summary>
    protected void LogAndThrow(Exception exception, string operation)
    {
        Logger.LogError(exception, "Error occurred during {Operation}", operation);
        throw exception;
    }

    /// <summary>
    /// Execute an operation with logging and error handling
    /// </summary>
    protected async Task<T> ExecuteWithLoggingAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Starting {Operation}", operationName);
            var result = await operation();
            Logger.LogInformation("Completed {Operation} successfully", operationName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute {Operation}", operationName);
            throw;
        }
    }

    /// <summary>
    /// Execute an operation with logging and error handling (void return)
    /// </summary>
    protected async Task ExecuteWithLoggingAsync(
        Func<Task> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Starting {Operation}", operationName);
            await operation();
            Logger.LogInformation("Completed {Operation} successfully", operationName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute {Operation}", operationName);
            throw;
        }
    }
}
