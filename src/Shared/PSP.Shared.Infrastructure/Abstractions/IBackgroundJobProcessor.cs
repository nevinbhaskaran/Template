namespace PSP.Shared.Infrastructure.Abstractions;

/// <summary>
/// Interface for handling background job processing
/// </summary>
public interface IBackgroundJobProcessor
{
    Task<string> EnqueueJobAsync<T>(T jobData, CancellationToken cancellationToken = default) where T : class;
    Task<string> ScheduleJobAsync<T>(T jobData, TimeSpan delay, CancellationToken cancellationToken = default) where T : class;
    Task<string> ScheduleJobAsync<T>(T jobData, DateTimeOffset scheduleTime, CancellationToken cancellationToken = default) where T : class;
    Task<bool> CancelJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task<JobStatus> GetJobStatusAsync(string jobId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Job status enumeration
/// </summary>
public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Interface for job handlers
/// </summary>
/// <typeparam name="T">The job data type</typeparam>
public interface IJobHandler<T> where T : class
{
    Task HandleAsync(T jobData, CancellationToken cancellationToken = default);
}
