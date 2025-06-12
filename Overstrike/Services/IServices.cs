using Overstrike.Models;

namespace Overstrike.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public interface IConfigurationService
{
    OverstrikeConfiguration Configuration { get; }
    Task LoadConfigurationAsync();
    Task SaveConfigurationAsync();
    event EventHandler<OverstrikeConfiguration>? ConfigurationChanged;
}

/// <summary>
/// Service for tracking EverQuest log files
/// </summary>
public interface ILogTrackerService
{
    bool IsTracking { get; }
    string? CurrentLogPath { get; }
    Task StartTrackingAsync(string logPath);
    Task StopTrackingAsync();
    event EventHandler<string>? LogLineReceived;
    event EventHandler<string>? ZoneChanged;
}

/// <summary>
/// Service for playing audio notifications
/// </summary>
public interface IAudioService
{
    bool IsEnabled { get; set; }
    float Volume { get; set; }
    Task PlaySoundAsync(string soundPath);
    Task PlayNotificationAsync(DamageType damageType, bool isCritical);
}

/// <summary>
/// Service for calculating DPS metrics
/// </summary>
public interface IDpsCalculatorService
{
    void AddDamageEvent(DamageEvent damageEvent);
    DpsData GetPlayerDps(string playerName, TimeSpan? window = null);
    Dictionary<string, DpsData> GetAllPlayerDps(TimeSpan? window = null);
    void ClearData();
    void SetCalculationWindow(TimeSpan window);
    event EventHandler<DamageEvent>? DamageEventAdded;
    event EventHandler<Dictionary<string, DpsData>>? DpsDataUpdated;
}

/// <summary>
/// Service for managing overlay windows and popups
/// </summary>
public interface IOverlayService
{
    Task ShowDamagePopupAsync(DamageEvent damageEvent);
    Task ShowDpsWindowAsync();
    Task HideDpsWindowAsync();
    void UpdateDpsWindow(Dictionary<string, DpsData> dpsData);
    Task ConfigurePlacementAsync(PopupCategory category, Placement placement);
}
