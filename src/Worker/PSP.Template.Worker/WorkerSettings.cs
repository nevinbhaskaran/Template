namespace PSP.Template.Worker;

public class WorkerSettings
{
    public const string SectionName = "WorkerSettings";
    
    public int ProcessingIntervalSeconds { get; set; } = 30;
    public bool EnableBackgroundProcessing { get; set; } = true;
    public string? ProcessingMode { get; set; } = "Standard";
    public int MaxConcurrentJobs { get; set; } = 1;
}
