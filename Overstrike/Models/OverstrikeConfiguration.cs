using System.Drawing;

namespace Overstrike.Models;

/// <summary>
/// Main configuration for the Overstrike application
/// </summary>
public class OverstrikeConfiguration
{
    public bool IsNew { get; set; } = true;
    public string LogPath { get; set; } = string.Empty;
    public string EQPath { get; set; } = string.Empty;
    public Rectangle MainWindow { get; set; } = new Rectangle(100, 100, 800, 600);

    // Melee configurations
    public Placement MeleeHitOut { get; set; } = new();
    public Placement MeleeHitIn { get; set; } = new();
    public Placement MeleeCritOut { get; set; } = new();
    public Placement MeleeCritIn { get; set; } = new();
    public Placement MeleeMissOut { get; set; } = new();
    public Placement MeleeMissIn { get; set; } = new();

    // Spell configurations
    public Placement SpellHitOut { get; set; } = new();
    public Placement SpellHitIn { get; set; } = new();
    public Placement SpellCritOut { get; set; } = new();
    public Placement SpellCritIn { get; set; } = new();
    public Placement SpellMissOut { get; set; } = new();
    public Placement SpellMissIn { get; set; } = new();

    // Heal configurations
    public Placement HealHitOut { get; set; } = new();
    public Placement HealHitIn { get; set; } = new();
    public Placement HealCritOut { get; set; } = new();
    public Placement HealCritIn { get; set; } = new();

    // Rune configurations
    public Placement RuneHitOut { get; set; } = new();

    // DPS Window configurations
    public Placement DpsWindow { get; set; } = new();
    public bool DpsWindowEnabled { get; set; } = true;
    public int DpsWindowUpdateInterval { get; set; } = 1000;

    // Audio settings
    public bool AudioEnabled { get; set; } = true;
    public float AudioVolume { get; set; } = 0.5f;
    public string AudioPath { get; set; } = string.Empty;

    // Display settings
    public bool ShowInTaskbar { get; set; } = false;
    public bool TopMost { get; set; } = true;
    public double Opacity { get; set; } = 0.9;

    // Parsing settings
    public bool LiveParseEnabled { get; set; } = true;
    public int MaxDamageEvents { get; set; } = 1000;
    public int DpsCalculationWindow { get; set; } = 30; // seconds

    public OverstrikeConfiguration()
    {
        InitializeDefaultPlacements();
    }

    private void InitializeDefaultPlacements()
    {
        // Set up default categories for each placement
        MeleeHitOut.Category = PopupCategory.MeleeHitOut;
        MeleeHitIn.Category = PopupCategory.MeleeHitIn;
        MeleeCritOut.Category = PopupCategory.MeleeCritOut;
        MeleeCritIn.Category = PopupCategory.MeleeCritIn;
        MeleeMissOut.Category = PopupCategory.MeleeMissOut;
        MeleeMissIn.Category = PopupCategory.MeleeMissIn;

        SpellHitOut.Category = PopupCategory.SpellHitOut;
        SpellHitIn.Category = PopupCategory.SpellHitIn;
        SpellCritOut.Category = PopupCategory.SpellCritOut;
        SpellCritIn.Category = PopupCategory.SpellCritIn;
        SpellMissOut.Category = PopupCategory.SpellMissOut;
        SpellMissIn.Category = PopupCategory.SpellMissIn;

        HealHitOut.Category = PopupCategory.HealHitOut;
        HealHitIn.Category = PopupCategory.HealHitIn;
        HealCritOut.Category = PopupCategory.HealCritOut;
        HealCritIn.Category = PopupCategory.HealCritIn;

        RuneHitOut.Category = PopupCategory.RuneHitOut;
    }
}
