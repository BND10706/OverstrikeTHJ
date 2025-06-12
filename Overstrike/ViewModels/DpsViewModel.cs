using CommunityToolkit.Mvvm.ComponentModel;
using Overstrike.Models;
using System.Collections.ObjectModel;

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
/// View model for configuration windows
/// </summary>
public partial class ConfigurationViewModel : ObservableObject
{
    [ObservableProperty]
    private OverstrikeConfiguration _configuration = new();

    [ObservableProperty]
    private Placement _selectedPlacement = new();

    [ObservableProperty]
    private PopupCategory _selectedCategory = PopupCategory.MeleeHitOut;

    public Array PopupCategories => Enum.GetValues<PopupCategory>();
    public Array DirectionValues => Enum.GetValues<Direction>();

    public void LoadConfiguration(OverstrikeConfiguration configuration)
    {
        Configuration = configuration;
        UpdateSelectedPlacement();
    }

    private void UpdateSelectedPlacement()
    {
        SelectedPlacement = SelectedCategory switch
        {
            PopupCategory.MeleeHitOut => Configuration.MeleeHitOut,
            PopupCategory.MeleeHitIn => Configuration.MeleeHitIn,
            PopupCategory.MeleeCritOut => Configuration.MeleeCritOut,
            PopupCategory.MeleeCritIn => Configuration.MeleeCritIn,
            PopupCategory.MeleeMissOut => Configuration.MeleeMissOut,
            PopupCategory.MeleeMissIn => Configuration.MeleeMissIn,
            PopupCategory.SpellHitOut => Configuration.SpellHitOut,
            PopupCategory.SpellHitIn => Configuration.SpellHitIn,
            PopupCategory.SpellCritOut => Configuration.SpellCritOut,
            PopupCategory.SpellCritIn => Configuration.SpellCritIn,
            PopupCategory.SpellMissOut => Configuration.SpellMissOut,
            PopupCategory.SpellMissIn => Configuration.SpellMissIn,
            PopupCategory.HealHitOut => Configuration.HealHitOut,
            PopupCategory.HealHitIn => Configuration.HealHitIn,
            PopupCategory.HealCritOut => Configuration.HealCritOut,
            PopupCategory.HealCritIn => Configuration.HealCritIn,
            PopupCategory.RuneHitOut => Configuration.RuneHitOut,
            _ => new Placement()
        };
    }

    partial void OnSelectedCategoryChanged(PopupCategory value)
    {
        UpdateSelectedPlacement();
    }
}
