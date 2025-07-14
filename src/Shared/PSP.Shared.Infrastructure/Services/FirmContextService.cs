using PSP.Shared.Domain.Entities;
using PSP.Shared.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PSP.Shared.Infrastructure.Services;

/// <summary>
/// Service to provide the current firm context for the worker
/// Will be configured via AppHost launch settings and service discovery
/// </summary>
public interface IFirmContextService
{
    /// <summary>
    /// Gets the current firm that this worker instance is processing for
    /// </summary>
    Task<Firm?> GetCurrentFirmAsync();
    
    /// <summary>
    /// Gets the current firm code from configuration
    /// </summary>
    string GetCurrentFirmCode();
    
    /// <summary>
    /// Sets the current firm context (for testing/development)
    /// </summary>
    void SetCurrentFirm(string firmCode);
}

/// <summary>
/// Implementation of IFirmContextService using configuration and service discovery
/// </summary>
public class FirmContextService : IFirmContextService
{
    private readonly IConfiguration _configuration;
    private readonly IFirmRepository _firmRepository;
    private readonly ILogger<FirmContextService> _logger;
    private Firm? _cachedFirm;
    private string? _currentFirmCode;

    public FirmContextService(
        IConfiguration configuration, 
        IFirmRepository firmRepository,
        ILogger<FirmContextService> logger)
    {
        _configuration = configuration;
        _firmRepository = firmRepository;
        _logger = logger;
        
        // Get firm code from configuration (will be set via AppHost)
        _currentFirmCode = _configuration["PSP:FirmCode"] ?? "PSP_FIRM";
    }

    public async Task<Firm?> GetCurrentFirmAsync()
    {
        if (_cachedFirm != null)
            return _cachedFirm;

        try
        {
            _cachedFirm = await _firmRepository.GetByCodeAsync(_currentFirmCode ?? "PSP_FIRM");
            
            if (_cachedFirm == null)
            {
                _logger.LogWarning("Firm with code '{FirmCode}' not found. Creating default firm.", _currentFirmCode);
                
                // Create default firm if not exists
                _cachedFirm = await _firmRepository.AddAsync(new Firm 
                { 
                    Code = _currentFirmCode ?? "PSP_FIRM" 
                });
            }
            
            _logger.LogInformation("Current firm context: {Firm}", _cachedFirm);
            return _cachedFirm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current firm context");
            return null;
        }
    }

    public string GetCurrentFirmCode()
    {
        return _currentFirmCode ?? "PSP_FIRM";
    }

    public void SetCurrentFirm(string firmCode)
    {
        _currentFirmCode = firmCode;
        _cachedFirm = null; // Clear cache to reload
    }
}
