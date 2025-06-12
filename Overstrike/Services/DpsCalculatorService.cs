using Microsoft.Extensions.Logging;
using Overstrike.Models;
using System.Collections.Concurrent;

namespace Overstrike.Services;

/// <summary>
/// Service for calculating DPS metrics and managing damage events
/// </summary>
public class DpsCalculatorService : IDpsCalculatorService
{
    private readonly ILogger<DpsCalculatorService> _logger;
    private readonly ConcurrentDictionary<string, List<DamageEvent>> _damageEvents;
    private readonly object _lockObject = new();
    private TimeSpan _calculationWindow = TimeSpan.FromSeconds(30);
    private Timer? _updateTimer;

    public event EventHandler<DamageEvent>? DamageEventAdded;
    public event EventHandler<Dictionary<string, DpsData>>? DpsDataUpdated;

    public DpsCalculatorService(ILogger<DpsCalculatorService> logger)
    {
        _logger = logger;
        _damageEvents = new ConcurrentDictionary<string, List<DamageEvent>>();

        // Start periodic DPS calculation updates
        _updateTimer = new Timer(CalculateAndNotifyDps, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public void AddDamageEvent(DamageEvent damageEvent)
    {
        if (damageEvent == null) return;

        lock (_lockObject)
        {
            var playerName = GetPlayerName(damageEvent);
            if (string.IsNullOrEmpty(playerName)) return;

            if (!_damageEvents.ContainsKey(playerName))
            {
                _damageEvents[playerName] = new List<DamageEvent>();
            }

            _damageEvents[playerName].Add(damageEvent);

            // Clean up old events to prevent memory bloat
            CleanupOldEvents(playerName);
        }

        DamageEventAdded?.Invoke(this, damageEvent);
    }

    public DpsData GetPlayerDps(string playerName, TimeSpan? window = null)
    {
        var calculationWindow = window ?? _calculationWindow;
        var cutoffTime = DateTime.Now - calculationWindow;

        lock (_lockObject)
        {
            if (!_damageEvents.TryGetValue(playerName, out var events) || !events.Any())
            {
                return new DpsData { PlayerName = playerName };
            }

            var recentEvents = events.Where(e => e.Timestamp >= cutoffTime).ToList();
            return CalculateDpsData(playerName, recentEvents);
        }
    }

    public Dictionary<string, DpsData> GetAllPlayerDps(TimeSpan? window = null)
    {
        var result = new Dictionary<string, DpsData>();

        lock (_lockObject)
        {
            foreach (var playerName in _damageEvents.Keys)
            {
                var dpsData = GetPlayerDps(playerName, window);
                if (dpsData.TotalDamage > 0 || dpsData.HitCount > 0)
                {
                    result[playerName] = dpsData;
                }
            }
        }

        return result;
    }

    public void ClearData()
    {
        lock (_lockObject)
        {
            _damageEvents.Clear();
        }
        _logger.LogInformation("DPS data cleared");
    }

    public void SetCalculationWindow(TimeSpan window)
    {
        _calculationWindow = window;
        _logger.LogInformation("DPS calculation window set to {Window} seconds", window.TotalSeconds);
    }

    private string GetPlayerName(DamageEvent damageEvent)
    {
        // Determine player name from source or target based on damage direction
        if (damageEvent.IsOutgoing)
        {
            return !string.IsNullOrEmpty(damageEvent.Source) ? damageEvent.Source : "You";
        }
        else
        {
            return !string.IsNullOrEmpty(damageEvent.Target) ? damageEvent.Target : "Unknown";
        }
    }

    private void CleanupOldEvents(string playerName)
    {
        if (!_damageEvents.TryGetValue(playerName, out var events)) return;

        // Keep only events from the last hour to prevent memory bloat
        var cutoffTime = DateTime.Now - TimeSpan.FromHours(1);
        var recentEvents = events.Where(e => e.Timestamp >= cutoffTime).ToList();

        if (recentEvents.Count != events.Count)
        {
            _damageEvents[playerName] = recentEvents;
        }
    }

    private DpsData CalculateDpsData(string playerName, List<DamageEvent> events)
    {
        if (!events.Any())
        {
            return new DpsData { PlayerName = playerName };
        }

        var damageEvents = events.Where(e => e.Type != DamageType.Miss).ToList();
        var missEvents = events.Where(e => e.Type == DamageType.Miss).ToList();

        var totalDamage = damageEvents.Sum(e => e.Amount);
        var hitCount = damageEvents.Count;
        var critCount = damageEvents.Count(e => e.IsCritical);
        var missCount = missEvents.Count;

        var startTime = events.Min(e => e.Timestamp);
        var endTime = events.Max(e => e.Timestamp);
        var duration = Math.Max((endTime - startTime).TotalSeconds, 1); // Minimum 1 second

        var dps = totalDamage / duration;

        return new DpsData
        {
            PlayerName = playerName,
            TotalDamage = totalDamage,
            Dps = dps,
            HitCount = hitCount,
            CritCount = critCount,
            MissCount = missCount,
            StartTime = startTime,
            EndTime = endTime
        };
    }

    private void CalculateAndNotifyDps(object? state)
    {
        try
        {
            var allDpsData = GetAllPlayerDps();
            if (allDpsData.Any())
            {
                DpsDataUpdated?.Invoke(this, allDpsData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic DPS calculation");
        }
    }

    public void Dispose()
    {
        _updateTimer?.Dispose();
        _updateTimer = null;
    }
}
