using Microsoft.Extensions.Logging;
using Overstrike.Models;
using Overstrike.Views;
using System.Windows;

namespace Overstrike.Services;

/// <summary>
/// Service for managing overlay windows and damage popups
/// </summary>
public class OverlayService : IOverlayService
{
    private readonly ILogger<OverlayService> _logger;
    private readonly IConfigurationService _configurationService;
    private DpsWindow? _dpsWindow;
    private readonly List<DamagePopup> _activePopups = new();

    public OverlayService(ILogger<OverlayService> logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task ShowDamagePopupAsync(DamageEvent damageEvent)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                // Add explicit console message for debugging
                Console.WriteLine($"Showing damage popup for {damageEvent.Amount} {damageEvent.Type} damage from {damageEvent.Source} to {damageEvent.Target}");
                
                var placement = GetPlacementForDamageEvent(damageEvent);
                if (placement == null)
                {
                    Console.WriteLine("Placement was null - no popup shown");
                    return;
                }
                
                // Always force visibility on for debugging overlay issues
                placement.IsVisible = true;
                
                if (!placement.IsVisible)
                {
                    Console.WriteLine($"Placement for {damageEvent.Type} is not visible - no popup shown");
                    return;
                }

                // Make sure we have valid coordinates to use for placement
                if (placement.WindowRect.Width <= 0 || placement.WindowRect.Height <= 0)
                {
                    Console.WriteLine("Fixing invalid window rect dimensions");
                    placement.WindowRect = new System.Drawing.Rectangle(
                        placement.WindowRect.X > 0 ? placement.WindowRect.X : 200,
                        placement.WindowRect.Y > 0 ? placement.WindowRect.Y : 200,
                        Math.Max(200, placement.WindowRect.Width),
                        Math.Max(100, placement.WindowRect.Height)
                    );
                }

                Console.WriteLine($"Creating popup at position {placement.WindowRect.X},{placement.WindowRect.Y}");
                
                var popup = new DamagePopup(damageEvent, placement);
                _activePopups.Add(popup);

                // Remove popup when it completes its animation
                popup.Closed += (s, e) => _activePopups.Remove(popup);

                // Force window to be visible and on top
                popup.Topmost = true;
                popup.Visibility = System.Windows.Visibility.Visible;
                popup.Show();
                popup.Activate();
                
                _logger.LogDebug("Showed damage popup for {Amount} {Type} damage",
                    damageEvent.Amount, damageEvent.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show damage popup");
                Console.WriteLine($"Error showing popup: {ex.Message}");
            }
        });
    }

    public async Task ShowDpsWindowAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                if (_dpsWindow == null)
                {
                    _dpsWindow = new DpsWindow();
                    _dpsWindow.Closed += (s, e) => _dpsWindow = null;
                }

                if (!_dpsWindow.IsVisible)
                {
                    _dpsWindow.Show();
                }

                _dpsWindow.Activate();
                _logger.LogInformation("DPS window shown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show DPS window");
            }
        });
    }

    public async Task HideDpsWindowAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                _dpsWindow?.Hide();
                _logger.LogInformation("DPS window hidden");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hide DPS window");
            }
        });
    }

    public void UpdateDpsWindow(Dictionary<string, DpsData> dpsData)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                _dpsWindow?.UpdateDpsData(dpsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update DPS window");
            }
        });
    }

    public async Task ConfigurePlacementAsync(PopupCategory category, Placement placement)
    {
        await Task.Run(() =>
        {
            try
            {
                // Update the configuration with the new placement
                var config = _configurationService.Configuration;
                var targetPlacement = GetPlacementFromConfiguration(config, category);

                if (targetPlacement != null)
                {
                    CopyPlacementProperties(placement, targetPlacement);
                    _configurationService.SaveConfigurationAsync();
                }

                _logger.LogInformation("Configured placement for {Category}", category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure placement for {Category}", category);
            }
        });
    }

    private Placement? GetPlacementForDamageEvent(DamageEvent damageEvent)
    {
        var config = _configurationService.Configuration;

        return damageEvent.Type switch
        {
            DamageType.Melee when damageEvent.IsOutgoing && damageEvent.IsCritical => config.MeleeCritOut,
            DamageType.Melee when damageEvent.IsOutgoing && damageEvent.Amount > 0 => config.MeleeHitOut,
            DamageType.Melee when damageEvent.IsOutgoing => config.MeleeMissOut,
            DamageType.Melee when !damageEvent.IsOutgoing && damageEvent.IsCritical => config.MeleeCritIn,
            DamageType.Melee when !damageEvent.IsOutgoing && damageEvent.Amount > 0 => config.MeleeHitIn,
            DamageType.Melee when !damageEvent.IsOutgoing => config.MeleeMissIn,

            DamageType.Spell when damageEvent.IsOutgoing && damageEvent.IsCritical => config.SpellCritOut,
            DamageType.Spell when damageEvent.IsOutgoing && damageEvent.Amount > 0 => config.SpellHitOut,
            DamageType.Spell when damageEvent.IsOutgoing => config.SpellMissOut,
            DamageType.Spell when !damageEvent.IsOutgoing && damageEvent.IsCritical => config.SpellCritIn,
            DamageType.Spell when !damageEvent.IsOutgoing && damageEvent.Amount > 0 => config.SpellHitIn,
            DamageType.Spell when !damageEvent.IsOutgoing => config.SpellMissIn,

            DamageType.Heal when damageEvent.IsOutgoing && damageEvent.IsCritical => config.HealCritOut,
            DamageType.Heal when damageEvent.IsOutgoing => config.HealHitOut,
            DamageType.Heal when !damageEvent.IsOutgoing && damageEvent.IsCritical => config.HealCritIn,
            DamageType.Heal when !damageEvent.IsOutgoing => config.HealHitIn,

            DamageType.Rune => config.RuneHitOut,

            _ => null
        };
    }

    private Placement? GetPlacementFromConfiguration(OverstrikeConfiguration config, PopupCategory category)
    {
        return category switch
        {
            PopupCategory.MeleeHitOut => config.MeleeHitOut,
            PopupCategory.MeleeHitIn => config.MeleeHitIn,
            PopupCategory.MeleeCritOut => config.MeleeCritOut,
            PopupCategory.MeleeCritIn => config.MeleeCritIn,
            PopupCategory.MeleeMissOut => config.MeleeMissOut,
            PopupCategory.MeleeMissIn => config.MeleeMissIn,
            PopupCategory.SpellHitOut => config.SpellHitOut,
            PopupCategory.SpellHitIn => config.SpellHitIn,
            PopupCategory.SpellCritOut => config.SpellCritOut,
            PopupCategory.SpellCritIn => config.SpellCritIn,
            PopupCategory.SpellMissOut => config.SpellMissOut,
            PopupCategory.SpellMissIn => config.SpellMissIn,
            PopupCategory.HealHitOut => config.HealHitOut,
            PopupCategory.HealHitIn => config.HealHitIn,
            PopupCategory.HealCritOut => config.HealCritOut,
            PopupCategory.HealCritIn => config.HealCritIn,
            PopupCategory.RuneHitOut => config.RuneHitOut,
            _ => null
        };
    }

    private void CopyPlacementProperties(Placement source, Placement target)
    {
        target.IsVisible = source.IsVisible;
        target.IsTallyEnabled = source.IsTallyEnabled;
        target.WindowRect = source.WindowRect;
        target.FontColor = source.FontColor;
        target.Direction = source.Direction;
        target.Font = source.Font;
    }
}
