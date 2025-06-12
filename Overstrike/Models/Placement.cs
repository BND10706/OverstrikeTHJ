using System.Drawing;
using System.Windows.Media;

namespace Overstrike.Models;

/// <summary>
/// Represents a placement configuration for overlay elements
/// </summary>
public class Placement
{
    public bool IsVisible { get; set; }
    public bool IsTallyEnabled { get; set; }
    public Rectangle WindowRect { get; set; }
    public System.Windows.Media.Color FontColor { get; set; }
    public Direction Direction { get; set; }
    public FontConfiguration Font { get; set; }

    // Runtime properties not saved to config
    public PopupCategory Category { get; set; }
    public int LastSpawnX { get; set; }
    public int LastSpawnY { get; set; }

    public Placement()
    {
        WindowRect = new Rectangle(220, 307, 200, 100);
        FontColor = Colors.White;
        Direction = Direction.Up;
        Font = new FontConfiguration();
    }
}

public enum Direction
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public enum PopupCategory
{
    MeleeHitOut,
    MeleeHitIn,
    MeleeCritOut,
    MeleeCritIn,
    MeleeMissOut,
    MeleeMissIn,
    SpellHitOut,
    SpellHitIn,
    SpellCritOut,
    SpellCritIn,
    SpellMissOut,
    SpellMissIn,
    HealHitOut,
    HealHitIn,
    HealCritOut,
    HealCritIn,
    RuneHitOut
}

public class FontConfiguration
{
    public string Family { get; set; } = "Segoe UI";
    public int Size { get; set; } = 12;
    public bool IsBold { get; set; } = false;
    public bool IsItalic { get; set; } = false;
}
