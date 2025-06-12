namespace Overstrike.Models;

/// <summary>
/// Represents a damage event from the EverQuest log
/// </summary>
public class DamageEvent
{
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Amount { get; set; }
    public DamageType Type { get; set; }
    public bool IsCritical { get; set; }
    public string SpellName { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public bool IsOutgoing { get; set; }
    public string RawLine { get; set; } = string.Empty;
}

public enum DamageType
{
    Melee,
    Spell,
    Heal,
    Rune,
    Miss
}

/// <summary>
/// DPS calculation data
/// </summary>
public class DpsData
{
    public string PlayerName { get; set; } = string.Empty;
    public double TotalDamage { get; set; }
    public double Dps { get; set; }
    public int HitCount { get; set; }
    public int CritCount { get; set; }
    public int MissCount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Duration => (EndTime - StartTime).TotalSeconds;
    public double CritRate => HitCount > 0 ? (double)CritCount / HitCount * 100 : 0;
    public double HitRate => (HitCount + MissCount) > 0 ? (double)HitCount / (HitCount + MissCount) * 100 : 0;
    public double AverageDamage => HitCount > 0 ? TotalDamage / HitCount : 0;
}
