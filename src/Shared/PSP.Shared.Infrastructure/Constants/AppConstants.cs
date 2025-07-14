namespace PSP.Shared.Infrastructure.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Configuration section names
    /// </summary>
    public static class ConfigurationSections
    {
        public const string ConnectionStrings = "ConnectionStrings";
        public const string Logging = "Logging";
        public const string WorkerSettings = "WorkerSettings";
        public const string ApiSettings = "ApiSettings";
        public const string CacheSettings = "CacheSettings";
    }

    /// <summary>
    /// Connection string names
    /// </summary>
    public static class ConnectionStringNames
    {
        public const string Default = "DefaultConnection";
        public const string ReadOnly = "ReadOnlyConnection";
        public const string Cache = "CacheConnection";
    }

    /// <summary>
    /// Common HTTP headers
    /// </summary>
    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string RequestId = "X-Request-ID";
        public const string ApiVersion = "X-API-Version";
        public const string UserAgent = "User-Agent";
    }

    /// <summary>
    /// Common claim types
    /// </summary>
    public static class ClaimTypes
    {
        public const string UserId = "user_id";
        public const string Email = "email";
        public const string Role = "role";
        public const string TenantId = "tenant_id";
    }
}
