namespace PSP.Template1.Worker;

/// <summary>
/// Configuration settings for the producer worker
/// </summary>
public class ProducerSettings
{
    /// <summary>
    /// How often to publish messages (in seconds)
    /// </summary>
    public int PublishIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// Number of messages to publish in each batch
    /// </summary>
    public int BatchSize { get; set; } = 5;
    
    /// <summary>
    /// Whether to generate random test data
    /// </summary>
    public bool EnableRandomData { get; set; } = true;
}
