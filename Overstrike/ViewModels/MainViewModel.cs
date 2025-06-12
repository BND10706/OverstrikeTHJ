using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Overstrike.Models;
using Overstrike.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Overstrike.ViewModels;

/// <summary>
/// Main view model for the Overstrike application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly ILogTrackerService _logTrackerService;
    private readonly IAudioService _audioService;
    private readonly IDpsCalculatorService _dpsCalculatorService;
    private readonly IOverlayService _overlayService;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isLogTracking = false;

    [ObservableProperty]
    private bool _isDpsWindowVisible = false;

    [ObservableProperty]
    private string _currentZone = "Unknown";

    [ObservableProperty]
    private string _selectedLogPath = string.Empty;

    [ObservableProperty]
    private bool _isEditMode = false;

    public ObservableCollection<string> RecentLogFiles { get; } = new();
    public ObservableCollection<DamageEvent> RecentDamageEvents { get; } = new();

    public ICommand StartStopTrackingCommand { get; }
    public ICommand ShowDpsWindowCommand { get; }
    public ICommand ShowConfigurationCommand { get; }
    public ICommand SelectLogFileCommand { get; }
    public ICommand ToggleEditModeCommand { get; }
    public ICommand ExitCommand { get; }

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IConfigurationService configurationService,
        ILogTrackerService logTrackerService,
        IAudioService audioService,
        IDpsCalculatorService dpsCalculatorService,
        IOverlayService overlayService)
    {
        _logger = logger;
        _configurationService = configurationService;
        _logTrackerService = logTrackerService;
        _audioService = audioService;
        _dpsCalculatorService = dpsCalculatorService;
        _overlayService = overlayService;

        // Initialize commands
        StartStopTrackingCommand = new AsyncRelayCommand(StartStopTrackingAsync);
        ShowDpsWindowCommand = new AsyncRelayCommand(ShowDpsWindowAsync);
        ShowConfigurationCommand = new RelayCommand(ShowConfiguration);
        SelectLogFileCommand = new AsyncRelayCommand(SelectLogFileAsync);
        ToggleEditModeCommand = new RelayCommand(ToggleEditMode);
        ExitCommand = new RelayCommand(Exit);

        // Subscribe to events
        _logTrackerService.LogLineReceived += OnLogLineReceived;
        _logTrackerService.ZoneChanged += OnZoneChanged;
        _dpsCalculatorService.DamageEventAdded += OnDamageEventAdded;
        _dpsCalculatorService.DpsDataUpdated += OnDpsDataUpdated;

        // Load configuration
        Task.Run(LoadConfigurationAsync);
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            await _configurationService.LoadConfigurationAsync();
            var config = _configurationService.Configuration;

            SelectedLogPath = config.LogPath;
            _audioService.IsEnabled = config.AudioEnabled;
            _audioService.Volume = config.AudioVolume;

            StatusText = "Configuration loaded";
            _logger.LogInformation("Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading configuration: {ex.Message}";
            _logger.LogError(ex, "Failed to load configuration");
        }
    }

    private async Task StartStopTrackingAsync()
    {
        try
        {
            if (IsLogTracking)
            {
                await _logTrackerService.StopTrackingAsync();
                IsLogTracking = false;
                StatusText = "Log tracking stopped";
            }
            else
            {
                if (string.IsNullOrEmpty(SelectedLogPath))
                {
                    StatusText = "Please select a log file first";
                    return;
                }

                await _logTrackerService.StartTrackingAsync(SelectedLogPath);
                IsLogTracking = true;
                StatusText = $"Tracking: {Path.GetFileName(SelectedLogPath)}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            _logger.LogError(ex, "Failed to start/stop log tracking");
        }
    }

    private async Task ShowDpsWindowAsync()
    {
        try
        {
            if (IsDpsWindowVisible)
            {
                await _overlayService.HideDpsWindowAsync();
                IsDpsWindowVisible = false;
            }
            else
            {
                await _overlayService.ShowDpsWindowAsync();
                IsDpsWindowVisible = true;
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing DPS window: {ex.Message}";
            _logger.LogError(ex, "Failed to show/hide DPS window");
        }
    }

    private void ShowConfiguration()
    {
        try
        {
            // TODO: Implement configuration window
            StatusText = "Configuration window - Not implemented yet";
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing configuration: {ex.Message}";
            _logger.LogError(ex, "Failed to show configuration window");
        }
    }

    private async Task SelectLogFileAsync()
    {
        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "EverQuest Log Files (eqlog_*.txt)|eqlog_*.txt|All files (*.*)|*.*",
                Title = "Select EverQuest Log File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedLogPath = openFileDialog.FileName;

                // Update configuration
                _configurationService.Configuration.LogPath = SelectedLogPath;
                await _configurationService.SaveConfigurationAsync();

                StatusText = $"Selected log file: {Path.GetFileName(SelectedLogPath)}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error selecting log file: {ex.Message}";
            _logger.LogError(ex, "Failed to select log file");
        }
    }

    private void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        StatusText = IsEditMode ? "Edit mode enabled" : "Edit mode disabled";
    }

    private void Exit()
    {
        try
        {
            Task.Run(async () =>
            {
                await _logTrackerService.StopTrackingAsync();
                await _configurationService.SaveConfigurationAsync();
            });

            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application exit");
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void OnLogLineReceived(object? sender, string line)
    {
        StatusText = $"Processing: {line.Substring(0, Math.Min(50, line.Length))}...";
    }

    private void OnZoneChanged(object? sender, string zoneName)
    {
        CurrentZone = zoneName;
        StatusText = $"Entered zone: {zoneName}";
    }

    private async void OnDamageEventAdded(object? sender, DamageEvent damageEvent)
    {
        // Update UI with recent damage event
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            RecentDamageEvents.Insert(0, damageEvent);
            if (RecentDamageEvents.Count > 20)
            {
                RecentDamageEvents.RemoveAt(RecentDamageEvents.Count - 1);
            }
        });

        // Show damage popup
        await _overlayService.ShowDamagePopupAsync(damageEvent);

        // Play audio notification
        await _audioService.PlayNotificationAsync(damageEvent.Type, damageEvent.IsCritical);
    }

    private void OnDpsDataUpdated(object? sender, Dictionary<string, DpsData> dpsData)
    {
        // Update DPS window if visible
        if (IsDpsWindowVisible)
        {
            _overlayService.UpdateDpsWindow(dpsData);
        }
    }
}
