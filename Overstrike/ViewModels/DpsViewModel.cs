using CommunityToolkit.Mvvm.ComponentModel;
using Overstrike.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Overstrike.ViewModels;

/// <summary>
/// View model for the DPS window
/// </summary>
public partial class DpsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = "DPS Meter - Overstrike";

    [ObservableProperty]
    private double _totalEncounterDps = 0.0;

    [ObservableProperty]
    private TimeSpan _encounterDuration = TimeSpan.Zero;

    [ObservableProperty]
    private int _totalDamageEvents = 0;

    public ObservableCollection<DpsData> PlayerDpsData { get; } = new();

    public void UpdateDpsData(Dictionary<string, DpsData> dpsData)
    {
        PlayerDpsData.Clear();

        var sortedData = dpsData.Values
            .OrderByDescending(d => d.Dps)
            .Take(20); // Show top 20 DPS players

        foreach (var data in sortedData)
        {
            PlayerDpsData.Add(data);
        }

        // Calculate totals
        TotalEncounterDps = PlayerDpsData.Sum(d => d.Dps);
        TotalDamageEvents = PlayerDpsData.Sum(d => d.HitCount);

        if (PlayerDpsData.Any())
        {
            var minStart = PlayerDpsData.Min(d => d.StartTime);
            var maxEnd = PlayerDpsData.Max(d => d.EndTime);
            EncounterDuration = maxEnd - minStart;
        }
    }
}

/// <summary>
/// View model for popup configuration
/// </summary>
public partial class PopupConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private OverstrikeConfiguration _popupConfiguration = new();

    [ObservableProperty]
    private Placement _selectedPlacement = new();

    [ObservableProperty]
    private PopupCategory _selectedCategory = PopupCategory.MeleeHitOut;

    public Array PopupCategories => Enum.GetValues<PopupCategory>();
    public Array DirectionValues => Enum.GetValues<Direction>();

    public void LoadConfiguration(OverstrikeConfiguration configuration)
    {
        PopupConfiguration = configuration;
        UpdateSelectedPlacement();
    }

    private void UpdateSelectedPlacement()
    {
        SelectedPlacement = SelectedCategory switch
        {
            PopupCategory.MeleeHitOut => PopupConfiguration.MeleeHitOut,
            PopupCategory.MeleeHitIn => PopupConfiguration.MeleeHitIn,
            PopupCategory.MeleeCritOut => PopupConfiguration.MeleeCritOut,
            PopupCategory.MeleeCritIn => PopupConfiguration.MeleeCritIn,
            PopupCategory.MeleeMissOut => PopupConfiguration.MeleeMissOut,
            PopupCategory.MeleeMissIn => PopupConfiguration.MeleeMissIn,
            PopupCategory.SpellHitOut => PopupConfiguration.SpellHitOut,
            PopupCategory.SpellHitIn => PopupConfiguration.SpellHitIn,
            PopupCategory.SpellCritOut => PopupConfiguration.SpellCritOut,
            PopupCategory.SpellCritIn => PopupConfiguration.SpellCritIn,
            PopupCategory.SpellMissOut => PopupConfiguration.SpellMissOut,
            PopupCategory.SpellMissIn => PopupConfiguration.SpellMissIn,
            PopupCategory.HealHitOut => PopupConfiguration.HealHitOut,
            PopupCategory.HealHitIn => PopupConfiguration.HealHitIn,
            PopupCategory.HealCritOut => PopupConfiguration.HealCritOut,
            PopupCategory.HealCritIn => PopupConfiguration.HealCritIn,
            PopupCategory.RuneHitOut => PopupConfiguration.RuneHitOut,
            _ => new Placement()
        };
    }

    partial void OnSelectedCategoryChanged(PopupCategory value)
    {
        UpdateSelectedPlacement();
    }
}
