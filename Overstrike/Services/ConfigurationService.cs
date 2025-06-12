using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Overstrike.Models;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace Overstrike.Services;

/// <summary>
/// Implementation of configuration service using INI file format
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "overstrike.ini";
    private readonly ILogger<ConfigurationService> _logger;
    private OverstrikeConfiguration _configuration;

    public OverstrikeConfiguration Configuration => _configuration;
    public event EventHandler<OverstrikeConfiguration>? ConfigurationChanged;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configuration = new OverstrikeConfiguration();
    }

    public async Task LoadConfigurationAsync()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            if (!File.Exists(configPath))
            {
                _logger.LogInformation("Configuration file not found, creating default configuration");
                _configuration.IsNew = true;
                await SaveConfigurationAsync();
                return;
            }

            var lines = await File.ReadAllLinesAsync(configPath);
            ParseIniFile(lines);
            _configuration.IsNew = false;

            _logger.LogInformation("Configuration loaded successfully from {ConfigPath}", configPath);
            ConfigurationChanged?.Invoke(this, _configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration");
            throw;
        }
    }

    public async Task SaveConfigurationAsync()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            var iniContent = GenerateIniContent();

            await File.WriteAllTextAsync(configPath, iniContent);
            _logger.LogInformation("Configuration saved successfully to {ConfigPath}", configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            throw;
        }
    }

    private void ParseIniFile(string[] lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith(';'))
                continue;

            var equalIndex = line.IndexOf('=');
            if (equalIndex == -1) continue;

            var key = line[..equalIndex].Trim();
            var value = line[(equalIndex + 1)..].Trim();

            ParseConfigurationValue(key, value);
        }
    }

    private void ParseConfigurationValue(string key, string value)
    {
        switch (key.ToLowerInvariant())
        {
            case "log_path":
                _configuration.LogPath = value;
                break;
            case "eq_path":
                _configuration.EQPath = value;
                break;
            case "main_window":
                _configuration.MainWindow = ParseRectangle(value);
                break;
            case "audio_enabled":
                _configuration.AudioEnabled = bool.Parse(value);
                break;
            case "audio_volume":
                _configuration.AudioVolume = float.Parse(value);
                break;
            case "dps_window_enabled":
                _configuration.DpsWindowEnabled = bool.Parse(value);
                break;
            case "live_parse_enabled":
                _configuration.LiveParseEnabled = bool.Parse(value);
                break;
            case "top_most":
                _configuration.TopMost = bool.Parse(value);
                break;
            case "opacity":
                _configuration.Opacity = double.Parse(value);
                break;
            // Add placement parsing for all damage types
            case var placementKey when placementKey.EndsWith("_out") || placementKey.EndsWith("_in"):
                ParsePlacementValue(placementKey, value);
                break;
        }
    }

    private void ParsePlacementValue(string key, string value)
    {
        var placement = GetPlacementByKey(key);
        if (placement != null)
        {
            ParsePlacement(placement, value);
        }
    }

    private Placement? GetPlacementByKey(string key)
    {
        return key.ToLowerInvariant() switch
        {
            "melee_hit_out" => _configuration.MeleeHitOut,
            "melee_hit_in" => _configuration.MeleeHitIn,
            "melee_crit_out" => _configuration.MeleeCritOut,
            "melee_crit_in" => _configuration.MeleeCritIn,
            "melee_miss_out" => _configuration.MeleeMissOut,
            "melee_miss_in" => _configuration.MeleeMissIn,
            "spell_hit_out" => _configuration.SpellHitOut,
            "spell_hit_in" => _configuration.SpellHitIn,
            "spell_crit_out" => _configuration.SpellCritOut,
            "spell_crit_in" => _configuration.SpellCritIn,
            "spell_miss_out" => _configuration.SpellMissOut,
            "spell_miss_in" => _configuration.SpellMissIn,
            "heal_hit_out" => _configuration.HealHitOut,
            "heal_hit_in" => _configuration.HealHitIn,
            "heal_crit_out" => _configuration.HealCritOut,
            "heal_crit_in" => _configuration.HealCritIn,
            "rune_hit_out" => _configuration.RuneHitOut,
            _ => null
        };
    }

    private void ParsePlacement(Placement placement, string value)
    {
        // Parse format: "isVisible,isTallyEnabled,x,y,width,height,r,g,b,a,direction,fontSize"
        var parts = value.Split(',');
        if (parts.Length >= 12)
        {
            placement.IsVisible = int.Parse(parts[0]) == 1;
            placement.IsTallyEnabled = int.Parse(parts[1]) == 1;
            placement.WindowRect = new Rectangle(
                int.Parse(parts[2]), int.Parse(parts[3]),
                int.Parse(parts[4]), int.Parse(parts[5]));
            placement.FontColor = System.Windows.Media.Color.FromArgb(
                byte.Parse(parts[9]), byte.Parse(parts[6]),
                byte.Parse(parts[7]), byte.Parse(parts[8]));
            placement.Direction = (Direction)int.Parse(parts[10]);
            placement.Font.Size = int.Parse(parts[11]);
        }
    }

    private Rectangle ParseRectangle(string value)
    {
        var parts = value.Split(',');
        if (parts.Length >= 4)
        {
            return new Rectangle(
                int.Parse(parts[0]), int.Parse(parts[1]),
                int.Parse(parts[2]), int.Parse(parts[3]));
        }
        return new Rectangle(100, 100, 800, 600);
    }

    private string GenerateIniContent()
    {
        var content = new System.Text.StringBuilder();
        content.AppendLine("# Overstrike Configuration File");
        content.AppendLine();

        content.AppendLine($"log_path={_configuration.LogPath}");
        content.AppendLine($"eq_path={_configuration.EQPath}");
        content.AppendLine($"main_window={_configuration.MainWindow.X},{_configuration.MainWindow.Y},{_configuration.MainWindow.Width},{_configuration.MainWindow.Height}");
        content.AppendLine($"audio_enabled={_configuration.AudioEnabled}");
        content.AppendLine($"audio_volume={_configuration.AudioVolume}");
        content.AppendLine($"dps_window_enabled={_configuration.DpsWindowEnabled}");
        content.AppendLine($"live_parse_enabled={_configuration.LiveParseEnabled}");
        content.AppendLine($"top_most={_configuration.TopMost}");
        content.AppendLine($"opacity={_configuration.Opacity}");

        content.AppendLine();
        content.AppendLine("# Placement configurations");
        content.AppendLine($"melee_hit_out={FormatPlacement(_configuration.MeleeHitOut)}");
        content.AppendLine($"melee_hit_in={FormatPlacement(_configuration.MeleeHitIn)}");
        content.AppendLine($"melee_crit_out={FormatPlacement(_configuration.MeleeCritOut)}");
        content.AppendLine($"melee_crit_in={FormatPlacement(_configuration.MeleeCritIn)}");
        content.AppendLine($"melee_miss_out={FormatPlacement(_configuration.MeleeMissOut)}");
        content.AppendLine($"melee_miss_in={FormatPlacement(_configuration.MeleeMissIn)}");
        content.AppendLine($"spell_hit_out={FormatPlacement(_configuration.SpellHitOut)}");
        content.AppendLine($"spell_hit_in={FormatPlacement(_configuration.SpellHitIn)}");
        content.AppendLine($"spell_crit_out={FormatPlacement(_configuration.SpellCritOut)}");
        content.AppendLine($"spell_crit_in={FormatPlacement(_configuration.SpellCritIn)}");
        content.AppendLine($"spell_miss_out={FormatPlacement(_configuration.SpellMissOut)}");
        content.AppendLine($"spell_miss_in={FormatPlacement(_configuration.SpellMissIn)}");
        content.AppendLine($"heal_hit_out={FormatPlacement(_configuration.HealHitOut)}");
        content.AppendLine($"heal_hit_in={FormatPlacement(_configuration.HealHitIn)}");
        content.AppendLine($"heal_crit_out={FormatPlacement(_configuration.HealCritOut)}");
        content.AppendLine($"heal_crit_in={FormatPlacement(_configuration.HealCritIn)}");
        content.AppendLine($"rune_hit_out={FormatPlacement(_configuration.RuneHitOut)}");

        return content.ToString();
    }

    private string FormatPlacement(Placement placement)
    {
        return $"{(placement.IsVisible ? 1 : 0)},{(placement.IsTallyEnabled ? 1 : 0)}," +
               $"{placement.WindowRect.X},{placement.WindowRect.Y}," +
               $"{placement.WindowRect.Width},{placement.WindowRect.Height}," +
               $"{placement.FontColor.R},{placement.FontColor.G},{placement.FontColor.B},{placement.FontColor.A}," +
               $"{(int)placement.Direction},{placement.Font.Size}";
    }
}
