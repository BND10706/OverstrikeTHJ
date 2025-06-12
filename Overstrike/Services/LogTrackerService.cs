using Microsoft.Extensions.Logging;
using Overstrike.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace Overstrike.Services;

/// <summary>
/// Service for tracking EverQuest log files and parsing damage events
/// </summary>
public class LogTrackerService : ILogTrackerService, IDisposable
{
    private readonly ILogger<LogTrackerService> _logger;
    private readonly IDpsCalculatorService _dpsCalculator;
    private FileSystemWatcher? _fileWatcher;
    private StreamReader? _streamReader;
    private FileStream? _fileStream;
    private bool _isTracking;
    private string? _currentLogPath;
    private long _lastPosition;

    // Regex patterns for parsing EverQuest log lines
    private static readonly Regex TimeRegex = new(@"\[(.*?)\]", RegexOptions.Compiled);
    private static readonly Regex ZoneRegex = new(@"You have entered (.*)", RegexOptions.Compiled);

    // Damage parsing patterns
    private static readonly Regex MeleeDamageRegex = new(
        @"(\w+) (?:hit|hits|slash|slashes|pierce|pierces|crush|crushes|bite|bites|claw|claws|punch|punches|kick|kicks) (\w+) for (\d+) points of damage",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SpellDamageRegex = new(
        @"(\w+) is struck by (.+?) for (\d+) points of damage",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HealRegex = new(
        @"(\w+) has been healed over time for (\d+) points of damage|(\w+) healed (\w+) for (\d+) points of damage",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsTracking => _isTracking;
    public string? CurrentLogPath => _currentLogPath;

    public event EventHandler<string>? LogLineReceived;
    public event EventHandler<string>? ZoneChanged;

    public LogTrackerService(ILogger<LogTrackerService> logger, IDpsCalculatorService dpsCalculator)
    {
        _logger = logger;
        _dpsCalculator = dpsCalculator;
    }

    public async Task StartTrackingAsync(string logPath)
    {
        if (_isTracking)
        {
            await StopTrackingAsync();
        }

        if (!File.Exists(logPath))
        {
            throw new FileNotFoundException($"Log file not found: {logPath}");
        }

        if (!Path.GetFileName(logPath).StartsWith("eqlog_"))
        {
            throw new ArgumentException("Invalid log file (expected eqlog_ prefix)");
        }

        try
        {
            _currentLogPath = logPath;

            // Open file for reading
            _fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _streamReader = new StreamReader(_fileStream);

            // Move to end of file to start tracking new entries
            _streamReader.BaseStream.Seek(0, SeekOrigin.End);
            _lastPosition = _streamReader.BaseStream.Position;

            // Set up file watcher
            var directory = Path.GetDirectoryName(logPath) ?? throw new InvalidOperationException("Invalid log path");
            var fileName = Path.GetFileName(logPath);

            _fileWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;

            _isTracking = true;
            _logger.LogInformation("Started tracking log file: {LogPath}", logPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start tracking log file: {LogPath}", logPath);
            await StopTrackingAsync();
            throw;
        }
    }

    public async Task StopTrackingAsync()
    {
        if (!_isTracking) return;

        _isTracking = false;

        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed -= OnFileChanged;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }

        _streamReader?.Dispose();
        _streamReader = null;

        _fileStream?.Dispose();
        _fileStream = null;

        _currentLogPath = null;
        _lastPosition = 0;

        _logger.LogInformation("Stopped tracking log file");

        // Add await to prevent compiler warning about async method lacking await operators
        await Task.CompletedTask;
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!_isTracking || _streamReader == null) return;

        try
        {
            // Check if file has grown
            var currentLength = _streamReader.BaseStream.Length;
            if (currentLength <= _lastPosition) return;

            // Read new content
            _streamReader.BaseStream.Seek(_lastPosition, SeekOrigin.Begin);
            string? line;

            while ((line = await _streamReader.ReadLineAsync()) != null)
            {
                ProcessLogLine(line);
                LogLineReceived?.Invoke(this, line);
            }

            _lastPosition = _streamReader.BaseStream.Position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing log file changes");
        }
    }

    private void ProcessLogLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;

        try
        {
            // Extract timestamp
            var timeMatch = TimeRegex.Match(line);
            if (!timeMatch.Success) return;

            if (!DateTime.TryParse(timeMatch.Groups[1].Value, out var timestamp))
            {
                timestamp = DateTime.Now;
            }

            // Check for zone changes
            var zoneMatch = ZoneRegex.Match(line);
            if (zoneMatch.Success)
            {
                var zoneName = zoneMatch.Groups[1].Value.Trim();
                ZoneChanged?.Invoke(this, zoneName);
                _logger.LogInformation("Zone changed to: {ZoneName}", zoneName);
                return;
            }

            // Parse damage events
            var damageEvent = ParseDamageEvent(line, timestamp);
            if (damageEvent != null)
            {
                _dpsCalculator.AddDamageEvent(damageEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process log line: {Line}", line);
        }
    }

    private DamageEvent? ParseDamageEvent(string line, DateTime timestamp)
    {
        // Try melee damage
        var meleeMatch = MeleeDamageRegex.Match(line);
        if (meleeMatch.Success)
        {
            return new DamageEvent
            {
                Timestamp = timestamp,
                Source = meleeMatch.Groups[1].Value,
                Target = meleeMatch.Groups[2].Value,
                Amount = int.Parse(meleeMatch.Groups[3].Value),
                Type = DamageType.Melee,
                IsCritical = line.Contains("critical", StringComparison.OrdinalIgnoreCase),
                RawLine = line,
                IsOutgoing = IsOutgoingDamage(line)
            };
        }

        // Try spell damage
        var spellMatch = SpellDamageRegex.Match(line);
        if (spellMatch.Success)
        {
            return new DamageEvent
            {
                Timestamp = timestamp,
                Target = spellMatch.Groups[1].Value,
                SpellName = spellMatch.Groups[2].Value,
                Amount = int.Parse(spellMatch.Groups[3].Value),
                Type = DamageType.Spell,
                IsCritical = line.Contains("critical", StringComparison.OrdinalIgnoreCase),
                RawLine = line,
                IsOutgoing = IsOutgoingDamage(line)
            };
        }

        // Try heal parsing
        var healMatch = HealRegex.Match(line);
        if (healMatch.Success)
        {
            if (healMatch.Groups[1].Success && healMatch.Groups[2].Success)
            {
                // Heal over time format
                return new DamageEvent
                {
                    Timestamp = timestamp,
                    Target = healMatch.Groups[1].Value,
                    Amount = int.Parse(healMatch.Groups[2].Value),
                    Type = DamageType.Heal,
                    IsCritical = line.Contains("critical", StringComparison.OrdinalIgnoreCase),
                    RawLine = line,
                    IsOutgoing = IsOutgoingDamage(line)
                };
            }
            else if (healMatch.Groups[3].Success && healMatch.Groups[4].Success && healMatch.Groups[5].Success)
            {
                // Direct heal format
                return new DamageEvent
                {
                    Timestamp = timestamp,
                    Source = healMatch.Groups[3].Value,
                    Target = healMatch.Groups[4].Value,
                    Amount = int.Parse(healMatch.Groups[5].Value),
                    Type = DamageType.Heal,
                    IsCritical = line.Contains("critical", StringComparison.OrdinalIgnoreCase),
                    RawLine = line,
                    IsOutgoing = IsOutgoingDamage(line)
                };
            }
        }

        // Check for misses
        if (line.Contains("miss", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("dodge", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("parry", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("block", StringComparison.OrdinalIgnoreCase))
        {
            return new DamageEvent
            {
                Timestamp = timestamp,
                Amount = 0,
                Type = DamageType.Miss,
                RawLine = line,
                IsOutgoing = IsOutgoingDamage(line)
            };
        }

        return null;
    }

    private bool IsOutgoingDamage(string line)
    {
        // Simple heuristic - check if "You" is the subject
        return line.Contains("You ") || line.Contains("Your ");
    }

    public void Dispose()
    {
        StopTrackingAsync().Wait();
    }
}
